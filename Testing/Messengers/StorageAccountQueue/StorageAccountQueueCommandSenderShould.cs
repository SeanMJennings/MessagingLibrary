using NUnit.Framework;

namespace Testing.Messengers.StorageAccountQueue;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public partial class StorageAccountQueueCommandSenderShould
{
    [Test]
    public void create_storage_account_queue_if_not_exists()
    {
        Given(a_storage_account_with_no_queues);
        When(creating_the_storage_account_command_sender);
        Then(the_queue_is_created);
    }
    
    [Test]
    public void send_command_to_queue()
    {
        Given(a_command_to_send_to_queue);
        When(sending_command_to_queue);
        Then(the_command_is_sent_to_queue);
    }
}