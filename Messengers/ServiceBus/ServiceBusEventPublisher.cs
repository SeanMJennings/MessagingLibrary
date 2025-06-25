using Azure.Messaging.ServiceBus;
using Messages;

namespace Messengers.ServiceBus;

public interface IAmAServiceBusEventPublisher
{
    Task PublishToTopic<T>(T theEvent) where T : Event;
}

public sealed class ServiceBusEventPublisher : IAmAServiceBusEventPublisher
{
    private string topicName;
    private ServiceBusClient client;
    
    internal ServiceBusEventPublisher(string connectionString, string topicName, IAmAServiceBusAdministrationClientWrapper serviceBusAdministrationClient)
    {
        this.topicName = topicName;
        client = new ServiceBusClient(connectionString);
        if (!serviceBusAdministrationClient.TopicExistsAsync(topicName).GetAwaiter().GetResult())
        {
            serviceBusAdministrationClient.CreateTopicAsync(topicName).Wait();
        }
    }
    
    public static ServiceBusEventPublisher New(string connectionString, string topicName)
    {
        return new ServiceBusEventPublisher(connectionString, topicName, ServiceBusAdministrationClientWrapper.New(connectionString));
    }
    
    public Task PublishToTopic<T>(T theEvent) where T : Event
    { 
        var sender = client.CreateSender(topicName);
        return sender.SendMessageAsync(new ServiceBusMessage(JsonSerialization.Serialize(theEvent)));
    }
}