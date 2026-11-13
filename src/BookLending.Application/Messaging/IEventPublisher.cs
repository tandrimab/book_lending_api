namespace BookLending.Application.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string queueName, T message, CancellationToken ct = default);
}
