using NUnit.Framework;

namespace Testing.Messengers;

[TestFixture]
public partial class EventPublisherShould
{
    [Test]
    public void publish_an_event()
    {
        Given(an_event);
        When(publishing_event);
        Then(event_is_published);
    }
}