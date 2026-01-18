namespace Messages;

public abstract record Command(string CorrelationId, Enum type) : IAmAMessageWithType
{
    public string Type => type.ToString();
}

public interface IAmACommand : IAmAMessage;