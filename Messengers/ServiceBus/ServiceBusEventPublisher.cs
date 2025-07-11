using Azure.Messaging.ServiceBus;
using Messages;

namespace Messengers.ServiceBus;

public interface IAmAServiceBusEventPublisher
{
    Task PublishToTopic<T>(T theEvent) where T : Event;
}

public sealed class ServiceBusEventPublisher : IAmAServiceBusEventPublisher
{
    private readonly ServiceBusSender sender;
    
    private ServiceBusEventPublisher(string topicName, IAmAServiceBus serviceBus)
    {
        sender = serviceBus.CreateSender(topicName);
    }
    
    public static ServiceBusEventPublisher New(string connectionString, string topicName)
    {
        return new ServiceBusEventPublisher(topicName, ServiceBus.New(connectionString));
    }
    
    public Task PublishToTopic<T>(T theEvent) where T : Event
    { 
        return sender.SendMessageAsync(new ServiceBusMessage(JsonSerialization.Serialize(theEvent)));
    }
}