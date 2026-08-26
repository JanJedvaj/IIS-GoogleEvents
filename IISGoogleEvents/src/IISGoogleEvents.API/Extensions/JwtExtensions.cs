using System.IdentityModel.Tokens.Jwt;
using System.Text;
using IISGoogleEvents.Application.Configurations;
using IISGoogleEvents.Application.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace IISGoogleEvents.API.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection(nameof(JwtConfig)).Get<JwtConfig>()
            ?? throw new InvalidOperationException("JwtConfig section is missing.");

        // The key is a secret and lives only in .env, never in appsettings.json.
        ConfigurationExtensions.Required(jwtConfig.Key, "JwtConfig__Key");

        if (Encoding.UTF8.GetByteCount(jwtConfig.Key) < 32)
            throw new InvalidOperationException(
                "JwtConfig__Key must be at least 32 bytes for HMAC-SHA256. Generate one with: openssl rand -base64 48");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Keep claims exactly as they appear in the token. Left on, the handler
                // rewrites "role" to the long WS-Federation URI and "sub" to nameidentifier,
                // which silently breaks the RoleClaimType below.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
                    ClockSkew = TimeSpan.Zero,

                    // Matches the short claim names emitted by TokenHelper, which survive
                    // because MapInboundClaims is off.
                    RoleClaimType = CustomClaimTypes.Role,
                    NameClaimType = JwtRegisteredClaimNames.UniqueName
                };
            });

        return services;
    }
}
