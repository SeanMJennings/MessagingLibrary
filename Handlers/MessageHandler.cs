using Messages;

namespace Handlers;

public interface IAmAMessageHandler
{
    public Task HandleMessage(IAmAMessageWithType messageWithType);
    public Type GetMessageType();
}

public abstract class MessageHandler<T> : IAmAMessageHandler where T : IAmAMessageWithType
{
    public async Task HandleMessage(IAmAMessageWithType messageWithType)
    {
        if (messageWithType is not T concreteMessage) throw new ArgumentException("Message is not of the expected type", nameof(messageWithType));
        await Handle(concreteMessage);
    }
    protected abstract Task Handle(T message);
    public Type GetMessageType() => typeof(T);
}