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
    
    // async constructors are not supported.
    internal ServiceBusEventPublisher(string connectionString, string topicName, IAmAServiceBusAdministrationClientWrapper serviceBusAdministrationClient)
    {
        var client = new ServiceBusClient(connectionString);
        if (!serviceBusAdministrationClient.TopicExistsAsync(topicName).GetAwaiter().GetResult())
        {
            serviceBusAdministrationClient.CreateTopicAsync(topicName).Wait();
        }
        sender = client.CreateSender(topicName);
    }
    
    public static ServiceBusEventPublisher New(string connectionString, string topicName)
    {
        return new ServiceBusEventPublisher(connectionString, topicName, ServiceBusAdministrationClientWrapper.New(connectionString));
    }
    
    public Task PublishToTopic<T>(T theEvent) where T : Event
    { 
        return sender.SendMessageAsync(new ServiceBusMessage(JsonSerialization.Serialize(theEvent)));
    }
}