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
    
    internal ServiceBusEventPublisher(string topicName, IAmAServiceBus serviceBus)
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

public sealed class ServiceBusEventPublisherFactory
{
    private readonly ServiceBus serviceBus;
    
    internal ServiceBusEventPublisherFactory(ServiceBus serviceBus)
    {
        this.serviceBus = serviceBus;
    }

    public ServiceBusEventPublisherFactory(string connectionString)
    {
        serviceBus = ServiceBus.New(connectionString);
    }

    public async Task<ServiceBusEventPublisher> CreateServiceBusEventPublisherEnsuringTopicExists(string topicName)
    {
        await serviceBus.CreateTopicIfNotExistsAsync(topicName);
        return new ServiceBusEventPublisher(topicName, serviceBus);
    }    
    
    public ServiceBusEventPublisher CreateServiceBusEventPublisher(string topicName)
    {
        return new ServiceBusEventPublisher(topicName, serviceBus);
    }
}