using NUnit.Framework;

namespace Testing.Messengers.ServiceBus;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public partial class ServiceBusCommandSenderShould
{
    [Test]
    public void send_a_command_to_service_bus_queue()
    {
        Given(a_command);
        When(sending_command_to_service_bus_queue);
        Then(command_is_sent_to_service_bus_queue);
    }
}