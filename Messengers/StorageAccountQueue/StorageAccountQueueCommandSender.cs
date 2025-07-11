using Azure.Storage.Queues;
using Messages;

namespace Messengers.StorageAccountQueue;

public interface IAmAStorageAccountQueueCommandSender
{
    Task SendToQueue<T>(T theCommand) where T : Command;
}

public sealed class StorageAccountQueueCommandSender : IAmAStorageAccountQueueCommandSender
{
    private readonly QueueClient QueueClient;
    
    internal StorageAccountQueueCommandSender(QueueClient queueClient)
    {
        QueueClient = queueClient;
    }
    
    public static StorageAccountQueueCommandSender New(string connectionString, string queueName)
    {
        return new StorageAccountQueueCommandSender(new QueueClient(connectionString, queueName));
    }
    
    public Task SendToQueue<T>(T theCommand) where T : Command
    {
        return QueueClient.SendMessageAsync(JsonSerialization.Serialize(theCommand));
    }
}

public sealed class StorageAccountQueueCommandSenderFactory
{
    private readonly QueueClient queueClient;
    
    internal StorageAccountQueueCommandSenderFactory(QueueClient queueClient)
    {
        this.queueClient = queueClient;
    }

    public StorageAccountQueueCommandSenderFactory(string connectionString, string queueName)
    {
        queueClient = new QueueClient(connectionString, queueName);
    }

    public async Task<StorageAccountQueueCommandSender> CreateStorageAccountQueueCommandSenderEnsuringQueueExists(string queueName)
    {
        if (!await queueClient.ExistsAsync()) await queueClient.CreateAsync();
        return new StorageAccountQueueCommandSender(queueClient);
    }    
    
    public StorageAccountQueueCommandSender CreateStorageAccountQueueCommandSender(string queueName)
    {
        return new StorageAccountQueueCommandSender(queueClient);
    }
}