using Azure.Messaging.ServiceBus;
using BookLending.Application.Loans;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace BookLending.Infrastructure.Messaging;

/// <summary>
/// Runs in the background for the whole life of the app, receiving messages from
/// the loan-events queue and passing each one to its Application handler.
/// </summary>
/// <remarks>
/// Delivery is "peek-lock": a received message is locked, not deleted.
///  - Handler succeeds → CompleteMessageAsync → the message is deleted.
///  - Handler throws   → the processor abandons it → the lock is released and it is
///    delivered again. After MaxDeliveryCount attempts (3, set in config.json) Service Bus
///    moves it to the queue's dead-letter sub-queue so it stops retrying.
/// Because a message can arrive more than once, handlers must be safe to run twice.
/// </remarks>
public sealed class LoanEventsConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LoanEventsConsumer> _logger;

    public LoanEventsConsumer(
        ServiceBusClient client,
        ServiceBusSettings settings,
        IServiceScopeFactory scopeFactory,
        ILogger<LoanEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _processor = client.CreateProcessor(settings.LoanEventsQueue, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false, // complete only after the handler succeeds
            MaxConcurrentCalls = 1        // one message at a time; raise for throughput
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken); // keep running until the app shuts down
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }

        await _processor.StopProcessingAsync(CancellationToken.None);
        await _processor.DisposeAsync();
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        var message = args.Message;
        _logger.LogInformation("Received {Subject} {MessageId} (attempt {Attempt})",
            message.Subject, message.MessageId, message.DeliveryCount);

        if (message.Subject != nameof(LoanCreated))
        {
            // Retrying can't help a message nobody knows how to handle. Park it for inspection.
            await args.DeadLetterMessageAsync(message, "UnknownSubject", $"No handler for '{message.Subject}'.");
            return;
        }

        var loanCreated = message.Body.ToObjectFromJson<LoanCreated>()!;

        // This class is a Singleton, but the handler needs Scoped services
        // (IMemberRepository → DbContext). Create a scope per message, like a mini request.
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<LoanCreatedHandler>();
        await handler.HandleAsync(loanCreated, args.CancellationToken);

        await args.CompleteMessageAsync(message, args.CancellationToken);
        _logger.LogInformation("Completed {Subject} {MessageId}", message.Subject, message.MessageId);
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error ({Source}) on {Entity}",
            args.ErrorSource, args.EntityPath);
        return Task.CompletedTask;
    }
}
