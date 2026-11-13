using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using BookLending.Application.Messaging;

namespace BookLending.Infrastructure.Messaging;

public sealed class ServiceBusEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;

    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

    public ServiceBusEventPublisher(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task PublishAsync<T>(string queueName, T message, CancellationToken ct = default)
    {
        var sender = _senders.GetOrAdd(queueName, q => _client.CreateSender(q));

        var serviceBusMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            ContentType = "application/json",
            Subject = typeof(T).Name
        };

        await sender.SendMessageAsync(serviceBusMessage, ct);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();
    }
}
