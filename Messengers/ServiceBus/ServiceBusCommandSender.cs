using Azure.Messaging.ServiceBus;
using Messages;

namespace Messengers.ServiceBus;

public interface IAmAServiceBusCommandSender
{
    Task SendToQueue<T>(T theCommand) where T : Command;
}

public sealed class ServiceBusCommandSender : IAmAServiceBusCommandSender
{
    private string queueName;
    private ServiceBusClient client;
    
    internal ServiceBusCommandSender(string connectionString, string queueName, IAmAServiceBusAdministrationClientWrapper serviceBusAdministrationClient)
    {
        this.queueName = queueName;
        client = new ServiceBusClient(connectionString);
        if (!serviceBusAdministrationClient.QueueExistsAsync(queueName).GetAwaiter().GetResult())
        {
            serviceBusAdministrationClient.CreateQueueAsync(queueName).Wait();
        }
    }
    
    public static ServiceBusCommandSender New(string connectionString, string queueName)
    {
        return new ServiceBusCommandSender(connectionString, queueName, ServiceBusAdministrationClientWrapper.New(connectionString));
    }
    
    public Task SendToQueue<T>(T theCommand) where T : Command
    { 
        var sender = client.CreateSender(queueName);
        return sender.SendMessageAsync(new ServiceBusMessage(JsonSerialization.Serialize(theCommand)));
    }
}