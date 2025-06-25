using Azure.Messaging.ServiceBus.Administration;

namespace Messengers.ServiceBus;

internal interface IAmAServiceBusAdministrationClientWrapper
{
    Task<bool> QueueExistsAsync(string queueName);
    Task CreateQueueAsync(string queueName);
    Task<bool> TopicExistsAsync(string topicName);
    Task CreateTopicAsync(string topicName);
}

// some azure classes are internal, so we use wrappers that we can then mock
internal sealed class ServiceBusAdministrationClientWrapper : IAmAServiceBusAdministrationClientWrapper
{
    private ServiceBusAdministrationClient serviceBusAdministrationClient;
    
    public async Task<bool> QueueExistsAsync(string queueName)
    {
        return (await serviceBusAdministrationClient.QueueExistsAsync(queueName)).Value;
    }
    
    public async Task CreateQueueAsync(string queueName)
    {
        await serviceBusAdministrationClient.CreateQueueAsync(queueName);
    }
    
    public async Task<bool> TopicExistsAsync(string topicName)
    {
        return (await serviceBusAdministrationClient.TopicExistsAsync(topicName)).Value;
    }
    
    public async Task CreateTopicAsync(string topicName)
    {
        await serviceBusAdministrationClient.CreateTopicAsync(topicName);
    }
    
    internal ServiceBusAdministrationClientWrapper(ServiceBusAdministrationClient serviceBusAdministrationClient)
    {
        this.serviceBusAdministrationClient = serviceBusAdministrationClient;
    }
    
    internal static ServiceBusAdministrationClientWrapper New(string connectionString)
    {
        return new ServiceBusAdministrationClientWrapper(new ServiceBusAdministrationClient(connectionString));
    }
}