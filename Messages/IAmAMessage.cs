namespace Messages;

public interface IAmAMessage
{
    public string CorrelationId { get; }
    
    public string Type { get; }
}