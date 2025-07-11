using Azure.Storage.Queues;
using BDD;
using Messages;
using Messengers.StorageAccountQueue;
using Shouldly;

namespace Testing.Messengers.StorageAccountQueue;

public partial class StorageAccountQueueCommandSenderShould : AzuriteContainerSpecification
{
    private readonly Guid correlationId = Guid.NewGuid();
    private const string QueueName = "test-queue";
    private const string QueueThatDoesNotExistName = "test-queue-2";
    private ACommand theCommand = null!;
    private class ACommand(string correlationId) : Command(correlationId, CommandTypes.ACommand);
    private enum CommandTypes
    {
        ACommand
    }
    
    protected override void before_each()
    {
        base.before_each();
        theCommand = null!;
    }
    
    private static void a_storage_account_with_no_queues() {}
    
    private void a_command_to_send_to_queue()
    {
        theCommand = new ACommand(correlationId.ToString());
    }

    private void sending_command_to_queue()
    {
        var factory = new StorageAccountQueueCommandSenderFactory(AzuriteContainer.GetConnectionString(), QueueName);
        var sender = factory.CreateStorageAccountQueueCommandSenderEnsuringQueueExists(QueueName).Await();
        sender.SendToQueue(theCommand).Await();
    }    
    
    private void creating_the_storage_account_command_sender_using_a_factory()
    {
        var factory = new StorageAccountQueueCommandSenderFactory(AzuriteContainer.GetConnectionString(), QueueThatDoesNotExistName);
        factory.CreateStorageAccountQueueCommandSenderEnsuringQueueExists(QueueThatDoesNotExistName).Await();
    }

    private void the_queue_is_created()
    {
        var queueClient = new QueueClient(AzuriteContainer.GetConnectionString(), QueueThatDoesNotExistName);
        queueClient.Exists().Value.ShouldBeTrue();
    }

    private void the_command_is_sent_to_queue()
    {
        var queueClient = new QueueClient(AzuriteContainer.GetConnectionString(), QueueName);
        var theMessages = queueClient.ReceiveMessages();
        theMessages.Value.Length.ShouldBe(1);
        JsonSerialization.Deserialize<ACommand>(theMessages.Value[0].MessageText).CorrelationId.ShouldBe(correlationId.ToString());
    }
}