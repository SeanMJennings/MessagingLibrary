using NUnit.Framework;

namespace Testing.Messengers.StorageAccountQueue;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public partial class StorageAccountQueueCommandSenderShould
{
    [Test]
    public void send_command_to_queue()
    {
        Given(a_command_to_send_to_queue);
        When(sending_command_to_queue);
        Then(the_command_is_sent_to_queue);
    }
}