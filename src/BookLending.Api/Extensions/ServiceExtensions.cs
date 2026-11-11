using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Azure.Messaging.ServiceBus;
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

    /// <summary>
    /// Wires up Azure Service Bus: one shared client, the publisher LoanService uses,
    /// and the background consumer that handles loan events.
    /// </summary>
    public static IServiceCollection AddServiceBusMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ServiceBus")
            ?? throw new InvalidOperationException("Missing connection string 'ServiceBus'.");
        var settings = new ServiceBusSettings(GetRequired(configuration, "ServiceBus:LoanEventsQueue"));

        // One client for the whole app: it owns the AMQP connection. Registered with a
        // factory so the DI container disposes it (and closes the connection) on shutdown.
        services.AddSingleton(_ => new ServiceBusClient(connectionString));
        services.AddSingleton(settings);

        services.AddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();
        services.AddScoped<LoanCreatedHandler>();
        services.AddHostedService<LoanEventsConsumer>(); // starts with the app, stops with it

        return services;
    }

    private static string GetRequired(IConfiguration configuration, string key) =>
        configuration[key] ?? throw new InvalidOperationException($"Missing configuration value '{key}'.");
}
