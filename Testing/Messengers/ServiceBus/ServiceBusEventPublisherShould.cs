using NUnit.Framework;

namespace Testing.Messengers.ServiceBus;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public partial class ServiceBusEventPublisherShould
{
    [Test]
    public void create_service_bus_topic_if_not_exists()
    {
        Given(a_service_bus_with_no_topics);
        When(creating_the_service_bus_sender);
        Then(the_topic_is_created);
    }    
    
    [Test]
    public void publishing_an_event_to_service_bus_topic()
    {
        Given(an_event);
        When(publishing_event_to_service_bus_topic);
        Then(event_is_received_in_subscription_to_service_bus_topic);
    }
}