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
    
    private void a_command_to_send_to_queue()
    {
        theCommand = new ACommand(correlationId.ToString());
    }

    private void sending_command_to_queue()
    {
        var sender = StorageAccountQueueCommandSender.New(AzuriteContainer.GetConnectionString(), QueueName);
        sender.SendToQueue(theCommand).Await();
    }    
    
    private void the_command_is_sent_to_queue()
    {
        var queueClient = new QueueClient(AzuriteContainer.GetConnectionString(), QueueName);
        var theMessages = queueClient.ReceiveMessages();
        theMessages.Value.Length.ShouldBe(1);
        JsonSerialization.Deserialize<ACommand>(theMessages.Value[0].MessageText).CorrelationId.ShouldBe(correlationId.ToString());
    }
}