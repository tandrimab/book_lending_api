using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BookLending.Application.Loans.Events;
using BookLending.Application.Messaging;
using BookLending.Application.Reservations;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BookLending.Functions;

public class BookReturnedHandler
{
    private readonly ReservationService _reservationService;
    private readonly ILogger<BookReturnedHandler> _logger;

    public BookReturnedHandler(ReservationService reservationService, ILogger<BookReturnedHandler> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    [Function(nameof(BookReturnedHandler))]
    public async Task Run(
        [ServiceBusTrigger(QueueNames.BookReturned, Connection = "ServiceBusConnection", AutoCompleteMessages = false)]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        BookReturnedEvent? evt;
        try
        {
            evt = message.Body.ToObjectFromJson<BookReturnedEvent>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid BookReturned payload. MessageId {MessageId}", message.MessageId);
            await messageActions.DeadLetterMessageAsync(message,
                deadLetterReason: "InvalidPayload",
                deadLetterErrorDescription: ex.Message,
                cancellationToken: cancellationToken);
            return;
        }

        if (evt is null)
        {
            await messageActions.DeadLetterMessageAsync(message,
                deadLetterReason: "EmptyPayload",
                deadLetterErrorDescription: "Message body deserialized to null.",
                cancellationToken: cancellationToken);
            return;
        }

        _logger.LogInformation(
            "Processing BookReturned: loan {LoanId}, book {BookId}, delivery attempt {DeliveryCount}",
            evt.LoanId, evt.BookId, message.DeliveryCount);

        var outcome = await _reservationService.HandleBookReturnedAsync(evt);

        _logger.LogInformation("BookReturned for loan {LoanId} handled: {Outcome}", evt.LoanId, outcome);

        await messageActions.CompleteMessageAsync(message, cancellationToken);
    }
}
