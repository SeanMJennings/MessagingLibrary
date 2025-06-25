namespace Messages;

public abstract class Command(string correlationId, Enum type) : IAmAMessage
{
    public string CorrelationId { get; } = correlationId;

    public string Type => type.ToString();
}