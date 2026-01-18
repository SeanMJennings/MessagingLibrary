using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Messengers.ServiceBus;

internal interface IAmAServiceBus
{
    Task CreateQueueIfNotExistsAsync(string queueName);
    Task CreateTopicIfNotExistsAsync(string topicName);
    ServiceBusSender CreateSender(string queueName);
}

// some azure classes are internal, so we use wrappers that we can then mock
internal sealed class ServiceBus : IAmAServiceBus
{
    private readonly ServiceBusAdministrationClient serviceBusAdministrationClient;
    private readonly ServiceBusClient serviceBusClient;

    public async Task CreateQueueIfNotExistsAsync(string queueName)
    {
        if (await serviceBusAdministrationClient.QueueExistsAsync(queueName)) await serviceBusAdministrationClient.CreateQueueAsync(queueName);
    }

    public async Task CreateTopicIfNotExistsAsync(string topicName)
    {
        if (await serviceBusAdministrationClient.TopicExistsAsync(topicName)) await serviceBusAdministrationClient.CreateTopicAsync(topicName);
    }

    public ServiceBusSender CreateSender(string queueName)
    {
        return serviceBusClient.CreateSender(queueName);
    }
    
    internal ServiceBus(ServiceBusAdministrationClient serviceBusAdministrationClient, ServiceBusClient serviceBusClient)
    {
        this.serviceBusAdministrationClient = serviceBusAdministrationClient;
        this.serviceBusClient = serviceBusClient;
    }
    
    public static ServiceBus New(string connectionString)
    {
        return new ServiceBus(new ServiceBusAdministrationClient(connectionString), new  ServiceBusClient(connectionString));
    }
}