using Azure.Storage.Queues;
using Messages;

namespace Messengers.StorageAccountQueue;

public interface IAmAStorageAccountQueueCommandSender
{
    Task SendToQueue<T>(T theCommand) where T : Command;
}

public sealed class StorageAccountQueueCommandSender : IAmAStorageAccountQueueCommandSender
{
    private QueueClient QueueClient;
    
    private StorageAccountQueueCommandSender(QueueClient queueClient)
    {
        QueueClient = queueClient;
        if (!QueueClient.Exists().Value)
        {
            QueueClient.CreateIfNotExists();
        }
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