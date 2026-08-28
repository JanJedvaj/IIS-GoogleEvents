using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using IISGoogleEvents.API.Configuration;
using IISGoogleEvents.API.Extensions;
using IISGoogleEvents.API.GraphQL;
using IISGoogleEvents.API.Grpc;
using IISGoogleEvents.API.Middleware;
using IISGoogleEvents.Application.Configuration;
using IISGoogleEvents.Application.Constants;
using IISGoogleEvents.Application.Interfaces;
using IISGoogleEvents.Application.Security;
using IISGoogleEvents.Application.Services;
using IISGoogleEvents.Infrastructure;
using IISGoogleEvents.Infrastructure.Clients.Dhmz;
using IISGoogleEvents.Infrastructure.Clients.Google;
using IISGoogleEvents.Infrastructure.Configuration;
using IISGoogleEvents.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

const string CorsPolicy = "ClientCors";

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.Configure<AppOptions>(configuration.GetSection(AppOptions.SectionName));
builder.Services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<ClientCorsOptions>(configuration.GetSection(ClientCorsOptions.SectionName));
builder.Services.Configure<DhmzOptions>(configuration.GetSection(DhmzOptions.SectionName));
builder.Services.Configure<GoogleOptions>(configuration.GetSection(GoogleOptions.SectionName));

var connectionString = Required(configuration.GetConnectionString("Db"), "ConnectionStrings__Db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        x => x.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name)));

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RefreshTokenRepository>();
builder.Services.AddScoped<CalendarEventRepository>();

builder.Services.AddScoped<PasswordHelper>();
builder.Services.AddScoped<TokenHelper>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DhmzWeatherService>();

builder.Services.AddHttpClient<DhmzClient>((provider, client) =>
    {
        var dhmz = provider.GetRequiredService<IOptions<DhmzOptions>>().Value;
        client.BaseAddress = new Uri(dhmz.BaseUrl);
        client.DefaultRequestHeaders.UserAgent.ParseAdd("IISGoogleEvents/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/xml");
    })
    .ConfigurePrimaryHttpMessageHandler(DhmzClient.CreateIPv4OnlyHandler);

builder.Services.AddSingleton<DhmzFeedCache>();

builder.Services.AddHttpClient<GoogleCalendarClient>((provider, client) =>
{
    var google = provider.GetRequiredService<IOptions<GoogleOptions>>().Value;
    client.BaseAddress = new Uri(google.BaseUrl);
});

builder.Services.AddScoped<LocalCalendarEventService>();
builder.Services.AddScoped<ExternalCalendarEventService>();

builder.Services.AddSingleton<XmlValidationService>();
builder.Services.AddSingleton<JsonValidationService>();
builder.Services.AddScoped<EventImportService>();

builder.Services.AddScoped<ICalendarEventService>(provider =>
{
    var appOptions = provider.GetRequiredService<IOptions<AppOptions>>().Value;

    return appOptions.DataSource == DataSourceType.Local
        ? provider.GetRequiredService<LocalCalendarEventService>()
        : provider.GetRequiredService<ExternalCalendarEventService>();
});

var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Nedostaje sekcija {JwtOptions.SectionName}.");

Required(jwtOptions.Key, "JwtOptions__Key");

if (Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
    throw new InvalidOperationException(
        "JwtOptions__Key mora imati barem 32 bajta za HMAC-SHA256. Generiraj ga s: openssl rand -base64 48");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = CustomClaimTypes.Role,
            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };
    });

builder.Services.AddAuthorization();

var corsOptions = configuration.GetSection(ClientCorsOptions.SectionName).Get<ClientCorsOptions>()
    ?? new ClientCorsOptions();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");

        if (corsOptions.AllowCredentials)
            policy.AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddGrpc();

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<EventQuery>()
    .AddMutationType<EventMutation>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IIS Google Events API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Zalijepi samo pristupni token, bez prefiksa 'Bearer '."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if ((await context.Database.GetPendingMigrationsAsync()).Any())
        await context.Database.MigrateAsync();

    var passwordHelper = scope.ServiceProvider.GetRequiredService<PasswordHelper>();
    await DbSeeder.SeedAsync(context, passwordHelper.HashPassword);
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);

app.UseGrpcWeb();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<WeatherGrpcService>()
    .EnableGrpcWeb()
    .RequireCors(CorsPolicy);
app.MapGraphQL();

app.Run();

static string Required(string? value, string variableName) =>
    !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new InvalidOperationException(
            $"Nedostaje konfiguracija '{variableName}'. Kopiraj .env.example u .env u korijenu repozitorija i postavi ju.");
