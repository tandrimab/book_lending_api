namespace BookLending.Infrastructure.Messaging;

/// <summary>Which queue loan events go to. Read from configuration ("ServiceBus:LoanEventsQueue").</summary>
public record ServiceBusSettings(string LoanEventsQueue);
