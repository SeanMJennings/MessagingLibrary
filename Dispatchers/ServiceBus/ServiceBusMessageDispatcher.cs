using Azure.Messaging.ServiceBus;
using Handlers;

namespace Dispatchers.ServiceBus;

public sealed class ServiceBusMessageDispatcher<E>(Dictionary<E, IAmAMessageHandler> enumToHandlerMappings)
    : MessageDispatcher<E>(enumToHandlerMappings)
    where E : Enum
{
    public async Task DispatchMessageFromQueue(string serviceBusConnectionString, string queueName)
    {
        var serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
        await DispatchMessageFromQueue(serviceBusClient, queueName);
    }
    
    internal async Task DispatchMessageFromQueue(ServiceBusClient serviceBusClient, string queueName)
    {
        var receiver = serviceBusClient.CreateReceiver(queueName);
        var receivedMessage = await receiver.ReceiveMessageAsync();

        if (receivedMessage != null)
        {
            var completeMessage = new Func<Task>(async () => await receiver.CompleteMessageAsync(receivedMessage));
            var abandonMessage = AbandonMessage(receivedMessage, receiver, queueName);
            await Handle(receivedMessage.Body, completeMessage, abandonMessage);
        }
    }

    private static Func<string, Exception, Task> AbandonMessage(ServiceBusReceivedMessage receivedMessage, ServiceBusReceiver receiver, string queueName)
    {
        var abandonMessage = new Func<string, Exception, Task>(async (errorMessage, exception) =>
        {
            if (receivedMessage.DeliveryCount < 3)
            {
                await receiver.AbandonMessageAsync(receivedMessage);
                Logger.LogErrorThatWillNotTriggerAnAlert(errorMessage, exception);
                return;
            }

            errorMessage = $"Message dead-lettered for {queueName} queue in service bus {receiver.FullyQualifiedNamespace}.\n" + errorMessage;
            await receiver.DeadLetterMessageAsync(receivedMessage, "Poisoned message", errorMessage);
            Logger.LogErrorThatWillTriggerAnAlert(errorMessage, exception);

        });
        return abandonMessage;
    }
}