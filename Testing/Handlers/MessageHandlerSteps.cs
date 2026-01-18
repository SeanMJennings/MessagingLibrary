using BDD;
using Handlers;
using Messages;
using Shouldly;

namespace Testing.Handlers;

public partial class MessageHandlerShould : Specification
{
    private readonly Guid correlationId = Guid.NewGuid();
    private Guid capturedCorrelationId = Guid.Empty;
    private ACommand theCommand = null!;
    public class ACommand(string correlationId) : Command(correlationId, CommandTypes.ACommand);
    public enum CommandTypes
    {
        ACommand
    }
    
    public class AMessageHandler(Action<string> captureFunction) : MessageHandler<ACommand>
    {
        protected override Task Handle(ACommand message)
        {
            captureFunction(message.CorrelationId);
            return Task.CompletedTask;
        }
    }

    protected override void before_each()
    {
        base.before_each();
        capturedCorrelationId = Guid.Empty;
        theCommand = null!;
    }

    private void a_message()
    {
        theCommand = new ACommand(correlationId.ToString());
    }

    private void handling_message()
    {
        var handler = new AMessageHandler(theCorrelationId => { capturedCorrelationId = Guid.Parse(theCorrelationId); });
        handler.HandleMessage(theCommand).Wait();
    }

    private void the_message_is_handled()
    {
        capturedCorrelationId.ShouldBe(correlationId);
    }
}