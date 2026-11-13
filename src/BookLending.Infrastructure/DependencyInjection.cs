using Azure.Messaging.ServiceBus;
using BookLending.Application.Messaging;
using BookLending.Domain.Items;
using BookLending.Domain.Loans;
using BookLending.Domain.Members;
using BookLending.Domain.Reminders;
using BookLending.Domain.Reservations;
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

        services.AddSingleton<IReminderSender, EmailReminder>();

        services.AddMessaging(configuration);
        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceBusConnection = configuration["ServiceBusConnection"]
            ?? throw new InvalidOperationException("Missing configuration value 'ServiceBusConnection'.");

        services.AddSingleton(_ => new ServiceBusClient(serviceBusConnection));
        services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
        return services;
    }
}
