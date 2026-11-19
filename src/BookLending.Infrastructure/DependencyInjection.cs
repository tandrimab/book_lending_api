using Azure.Messaging.ServiceBus;
using BookLending.Application.Messaging;
using BookLending.Domain.Items;
using BookLending.Domain.Loans;
using BookLending.Domain.Members;
using BookLending.Domain.Reminders;
using BookLending.Domain.Reservations;
using BookLending.Infrastructure.Email;
using BookLending.Infrastructure.Messaging;
using BookLending.Infrastructure.Persistence;
using BookLending.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace BookLending.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnection = configuration.GetConnectionString("BookLending")
            ?? throw new InvalidOperationException("Missing connection string 'BookLending'.");

        services.AddDbContext<BookLendingDbContext>(options => options.UseSqlServer(dbConnection));

        services.AddScoped<IBookRepository, EfBookRepository>();
        services.AddScoped<IMemberRepository, EfMemberRepository>();
        services.AddScoped<ILoanRepository, EfLoanRepository>();
        services.AddScoped<IReservationRepository, EfReservationRepository>();

        services.AddEmail(configuration);
        services.AddMessaging(configuration);
        return services;
    }

    /// <summary>
    /// this: marks the method as an extension method of the parameter's type
    /// IServiceCollection: the type being extended. It’s ASP.NET Core’s built-in DI container: the list of registered in Program.cs.
    /// </summary>
    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceBusConnection = configuration["ServiceBusConnection"]
            ?? throw new InvalidOperationException("Missing configuration value 'ServiceBusConnection'.");

        services.AddSingleton(_ => new ServiceBusClient(serviceBusConnection));
        services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
        return services;
    }

    private static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
    {
        var portValue = GetRequired(configuration, "Smtp:Port");
        if (!int.TryParse(portValue, out var port))
            throw new InvalidOperationException($"Configuration value 'Smtp:Port' must be a number, but was '{portValue}'.");

        var smtpSettings = new SmtpSettings(
            GetRequired(configuration, "Smtp:Host"),
            port,
            GetRequired(configuration, "Smtp:Username"),
            GetRequired(configuration, "Smtp:Password"),
            GetRequired(configuration, "Smtp:FromAddress"));

        services.AddSingleton(smtpSettings);
        services.AddSingleton<IReminderSender, SmtpEmailReminder>();
        return services;
    }

    private static string GetRequired(IConfiguration configuration, string key) =>
        configuration[key] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"Missing configuration value '{key}'.");
}
