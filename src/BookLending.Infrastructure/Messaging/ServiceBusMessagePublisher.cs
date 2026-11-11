using Azure.Messaging.ServiceBus;
using BookLending.Application.Messaging;
namespace BookLending.Infrastructure.Messaging;

/// <summary>
/// IMessagePublisher implemented with Azure Service Bus.
/// Registered as a Singleton: a ServiceBusSender is thread-safe and holds an open
/// AMQP link, so creating one per request would be slow and wasteful.
/// </summary>
public sealed class ServiceBusMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusSender _sender;

    public ServiceBusMessagePublisher(ServiceBusClient client, ServiceBusSettings settings)
    {
        _sender = client.CreateSender(settings.LoanEventsQueue);
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        var serviceBusMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            // The body is just JSON bytes. Subject tells the consumer which type to
            // deserialize it into, e.g. "LoanCreated".
            Subject = typeof(T).Name,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString()
        };

        return _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public ValueTask DisposeAsync() => _sender.DisposeAsync();
}
