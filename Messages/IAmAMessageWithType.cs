namespace Messages;

public interface IAmAMessageWithType : IAmAMessage
{
    public string Type { get; }
}

public interface IAmAMessage
{
    public string CorrelationId { get; }
}