using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BookLending.Api.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Validates incoming "Authorization: Bearer ..." tokens.
    /// Must use the SAME issuer, audience and key that AuthService uses to issue them.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Fail at startup if anything is missing, rather than rejecting every token at runtime.
        var issuer = GetRequired(configuration, "Jwt:Issuer");
        var audience = GetRequired(configuration, "Jwt:Audience");
        var key = GetRequired(configuration, "Jwt:Key");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,            // iss must match: who issued the token
                    ValidIssuer = issuer,
                    ValidateAudience = true,          // aud must match: who the token is for
                    ValidAudience = audience,
                    ValidateLifetime = true,          // exp must be in the future (default 5 min clock skew)
                    ValidateIssuerSigningKey = true,  // signature must verify: nobody tampered with it
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

        services.AddAuthorization();
        return services;
    }

    private static string GetRequired(IConfiguration configuration, string key) =>
        configuration[key] ?? throw new InvalidOperationException($"Missing configuration value '{key}'.");
}
