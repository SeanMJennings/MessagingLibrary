namespace Messages;

public abstract record Event(string CorrelationId, Enum type) : IAmAMessageWithType
{
    public string Type => type.ToString();
}

public interface IAmAnEvent : IAmAMessage;