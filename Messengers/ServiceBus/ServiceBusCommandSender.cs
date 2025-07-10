using Azure.Messaging.ServiceBus;
using Messages;

namespace Messengers.ServiceBus;

public interface IAmAServiceBusCommandSender
{
    Task SendToQueue<T>(T theCommand) where T : Command;
}

public sealed class ServiceBusCommandSender : IAmAServiceBusCommandSender
{
    private readonly ServiceBusSender sender;
    
    // async constructors are not supported.
    internal ServiceBusCommandSender(string connectionString, string queueName, IAmAServiceBusAdministrationClientWrapper serviceBusAdministrationClient)
    {
        var client = new ServiceBusClient(connectionString);
        if (!serviceBusAdministrationClient.QueueExistsAsync(queueName).GetAwaiter().GetResult())
        {
            serviceBusAdministrationClient.CreateQueueAsync(queueName).Wait();
        }
        sender = client.CreateSender(queueName);
    }
    
    public static ServiceBusCommandSender New(string connectionString, string queueName)
    {
        return new ServiceBusCommandSender(connectionString, queueName, ServiceBusAdministrationClientWrapper.New(connectionString));
    }
    
    public Task SendToQueue<T>(T theCommand) where T : Command
    { 
        return sender.SendMessageAsync(new ServiceBusMessage(JsonSerialization.Serialize(theCommand)));
    }
}