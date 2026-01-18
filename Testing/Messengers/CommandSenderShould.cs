using NUnit.Framework;

namespace Testing.Messengers;

[TestFixture]
public partial class CommandSenderShould
{
    [Test]
    public void send_a_command()
    {
        Given(a_command);
        When(sending_command);
        Then(command_is_sent);
    }
}