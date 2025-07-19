using MassTransit;
using Messages;

namespace Messengers;

public interface IAmAnInitialEventPublisher
{
    Task Publish<T>(T theEvent) where T : IAmAnEvent;
}

public sealed class InitialEventPublisher(IPublishEndpoint publishEndpoint) : IAmAnInitialEventPublisher
{
    public async Task Publish<T>(T theEvent) where T : IAmAnEvent
    {
        await publishEndpoint.Publish(theEvent);
    }
}