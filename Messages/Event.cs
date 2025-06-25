namespace Messages;

public abstract class Event(string correlationId, Enum type) : IAmAMessage
{
    public string CorrelationId { get; } = correlationId;
    
    public string Type => type.ToString();
}