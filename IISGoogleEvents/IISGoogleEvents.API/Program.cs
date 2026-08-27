using System.Text;
using System.Text.Json.Serialization;
using IISGoogleEvents.API.Abstractions.Exceptions;
using IISGoogleEvents.API.Extensions;
using IISGoogleEvents.Application;
using IISGoogleEvents.Application.Configurations;
using IISGoogleEvents.Infrastructure;
using IISGoogleEvents.Infrastructure.Grpc;
using IISGoogleEvents.Repository;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Before CreateBuilder: it snapshots the environment when it builds the configuration.
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAppConfiguration(builder.Configuration);

builder.Services.AddRepository(builder.Configuration.GetDbConnectionString());
builder.Services.AddApplication();

var dhmzConfig = builder.Configuration.GetSection(nameof(DhmzConfig)).Get<DhmzConfig>()!;
builder.Services.AddInfrastructure(dhmzConfig);
builder.Services.AddCalendarServiceSwitch();

builder.Services.AddGrpc();

var grpcConfig = builder.Configuration.GetSection(nameof(GrpcConfig)).Get<GrpcConfig>()!;
builder.Services.AddGrpcClient<WeatherService.WeatherServiceClient>(o => o.Address = new Uri(grpcConfig.WeatherServiceUrl));

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var corsConfig = builder.Configuration.GetSection(nameof(CorsConfig)).Get<CorsConfig>()!;
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsConfig.AllowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader();

        if (corsConfig.AllowCredentials)
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.ApplyMigrationsAsync();
    await app.SeedDataAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<DhmzGrpcService>();

app.Run();
