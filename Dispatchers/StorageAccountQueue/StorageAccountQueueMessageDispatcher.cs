using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Handlers;

namespace Dispatchers.StorageAccountQueue;

public sealed class StorageAccountQueueMessageDispatcher<E>(Dictionary<E, IAmAMessageHandler> enumToHandlerMappings) : MessageDispatcher<E>(enumToHandlerMappings)
    where E : Enum
{
    public async Task DispatchMessageFromQueue(string storageAccountConnectionString, string queueName, TimeSpan? visibilityTimeout = null)
    {
        var storageAccountClient = new QueueServiceClient(storageAccountConnectionString);
        await DispatchMessageFromQueue(storageAccountClient, queueName, visibilityTimeout);
    }
    
    internal async Task DispatchMessageFromQueue(QueueServiceClient queueServiceClient, string queueName, TimeSpan? visibilityTimeout = null)
    {
        visibilityTimeout ??= TimeSpan.FromMinutes(5);
        var queueClient = queueServiceClient.GetQueueClient(queueName);
        var poisonedStorageQueueClient = queueServiceClient.GetQueueClient($"{queueName}-poisoned");
        await poisonedStorageQueueClient.CreateIfNotExistsAsync();
        
        var receivedMessage = await queueClient.ReceiveMessageAsync(visibilityTimeout);
        if (receivedMessage is { HasValue: true, Value: not null })
        {
            var abandonMessage = AbandonMessage(queueClient, poisonedStorageQueueClient, receivedMessage);
            var completeMessage = new Func<Task>(async () => await queueClient.DeleteMessageAsync(receivedMessage.Value.MessageId, receivedMessage.Value.PopReceipt));
            
            await Handle(receivedMessage.Value.Body, completeMessage, abandonMessage);
        }
    }

    private static Func<string, Exception, Task> AbandonMessage(QueueClient queueClient, QueueClient poisonedStorageQueueClient, Response<QueueMessage> receivedMessage)
    {
        var abandonMessage = new Func<string, Exception, Task>(async (message, exception) =>
        {
            if (await PoisonMessage(queueClient, poisonedStorageQueueClient, receivedMessage))
            {
                message = $"Poisoned message moved to {poisonedStorageQueueClient.Name} storage queue in storage account {queueClient.AccountName}.\n" + message;
                Logger.LogErrorThatWillTriggerAnAlert(message, exception);
                return;
            }
            await queueClient.UpdateMessageAsync(receivedMessage.Value.MessageId, receivedMessage.Value.PopReceipt, receivedMessage.Value.Body, TimeSpan.FromSeconds(10), CancellationToken.None);
            Logger.LogErrorThatWillNotTriggerAnAlert(message, exception);
        });
        return abandonMessage;
    }

    private static async Task<bool> PoisonMessage(QueueClient storageQueueClient, QueueClient poisonedStorageQueueClient, Response<QueueMessage> receivedMessage)
    {
        if (receivedMessage.Value.DequeueCount < 3) return false;
        await storageQueueClient.DeleteMessageAsync(receivedMessage.Value.MessageId, receivedMessage.Value.PopReceipt);
        await poisonedStorageQueueClient.SendMessageAsync(receivedMessage.Value.Body, TimeSpan.FromSeconds(10), TimeSpan.FromDays(7));
        return true;

    }
}