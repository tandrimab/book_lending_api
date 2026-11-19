using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BookLending.Application.Loans;
using BookLending.Application.Messaging;
using BookLending.Infrastructure.Messaging;

namespace BookLending.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = GetRequired(configuration, "Jwt:Issuer");
        var audience = GetRequired(configuration, "Jwt:Audience");
        var key = GetRequired(configuration, "Jwt:Key");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Missing connection string 'Redis'.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "booklending:";
        });

        return services;
    }

    public static IServiceCollection AddServiceBusMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new ServiceBusSettings(GetRequired(configuration, "ServiceBus:LoanEventsQueue"));
        services.AddSingleton(settings);

        services.AddScoped<LoanCreatedHandler>();
        services.AddHostedService<LoanEventsConsumer>(); // starts with the app, stops with it

        return services;
    }

    private static string GetRequired(IConfiguration configuration, string key) =>
        configuration[key] ?? throw new InvalidOperationException($"Missing configuration value '{key}'.");
}
