using BDD;
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
    
    [Test]
    public void optionally_create_service_bus_queue_if_not_exists()
    {
        Given(a_service_bus_with_no_queues);
        When(creating_the_service_bus_sender_using_a_factory);
        Then(the_queue_is_created);
    }   
}