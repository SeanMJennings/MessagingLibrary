using Messages;

namespace Handlers;

public interface IAmAMessageHandler
{
    public Task HandleMessage(IAmAMessage message);
    public Type GetMessageType();
}

public abstract class MessageHandler<T> : IAmAMessageHandler where T : IAmAMessage
{
    public async Task HandleMessage(IAmAMessage message)
    {
        if (message is not T concreteMessage) throw new ArgumentException("Message is not of the expected type", nameof(message));
        await Handle(concreteMessage);
    }
    protected abstract Task Handle(T message);
    public Type GetMessageType() => typeof(T);
}