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
    
    internal ServiceBusCommandSender(string queueName, IAmAServiceBus serviceBus)
    {
        sender = serviceBus.CreateSender(queueName);
    }
    
    public static ServiceBusCommandSender New(string connectionString, string queueName)
    {
        return new ServiceBusCommandSender(queueName, ServiceBus.New(connectionString));
    }
    
    public Task SendToQueue<T>(T theCommand) where T : Command
    { 
        return sender.SendMessageAsync(new ServiceBusMessage(JsonSerialization.Serialize(theCommand)));
    }
}

public sealed class ServiceBusCommandSenderFactory
{
    private readonly IAmAServiceBus serviceBus;
    
    internal ServiceBusCommandSenderFactory(IAmAServiceBus serviceBus)
    {
        this.serviceBus = serviceBus;
    }

    public ServiceBusCommandSenderFactory(string connectionString)
    {
        serviceBus = ServiceBus.New(connectionString);
    }

    public async Task<ServiceBusCommandSender> CreateServiceBusCommandSenderEnsuringQueueExists(string queueName)
    {
        await serviceBus.CreateQueueIfNotExistsAsync(queueName);
        return new ServiceBusCommandSender(queueName, serviceBus);
    }    
    
    public ServiceBusCommandSender CreateServiceBusCommandSender(string queueName)
    {
        return new ServiceBusCommandSender(queueName, serviceBus);
    }
}