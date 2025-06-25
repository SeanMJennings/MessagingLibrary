using NUnit.Framework;

namespace Testing.Handlers;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public partial class MessageHandlerShould
{
    [Test]
    public void handle_message()
    {
        Given(a_message);
        When(handling_message);
        Then(the_message_is_handled);
    }
}