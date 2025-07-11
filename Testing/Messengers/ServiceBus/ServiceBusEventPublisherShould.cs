using NUnit.Framework;

namespace Testing.Messengers.ServiceBus;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public partial class ServiceBusEventPublisherShould
{
    [Test]
    public void publishing_an_event_to_service_bus_topic()
    {
        Given(an_event);
        When(publishing_event_to_service_bus_topic);
        Then(event_is_received_in_subscription_to_service_bus_topic);
    }
}