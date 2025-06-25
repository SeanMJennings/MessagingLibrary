using System.Globalization;
using Alerting;
using BDD;
using Handlers;
using Messages;
using Shouldly;
using Logging.TestingUtilities;

namespace Testing.Dispatchers;

public class CommonSteps : Specification
{
    protected readonly Guid CorrelationId = Guid.NewGuid();
    protected IAmAMessage TheCommand = null!;
    private IAmAMessage CapturedMessage = null!;
    protected List<string> CapturedLogMessages = null!;
    private Exception CapturedLogException = null!;
    private AlertCode CapturedAlertCode;
    protected IAmAMessageHandler TheHandler = null!;
    protected IAmAMessageHandler AnotherHandler = null!;
    
    private enum Wibble
    {
        Wobble,
        Wubble
    }

    private class DoSomething(string correlationId, Wibble wibble) : Command(correlationId, CommandTypes.DoSomething)
    {
        public string SomethingImportant = "wibble";
        public Wibble Wibble = wibble;
    }

    private class DoAnotherSomething(string correlationId, Wibble wibble) : Command(correlationId, CommandTypes.DoAnotherSomething)
    {
        public string SomethingImportant = "wibble";
        public Wibble Wibble = wibble;
    }

    private class DoSomethingElse(string correlationId, Wibble wibble) : Command(correlationId, CommandTypes.DoSomethingElse)
    {
        public string SomethingImportant = "wibble";
        public Wibble Wibble = wibble;
    }

    private class DoSomethingUnexpected(string correlationId, Wibble wibble) : Command(correlationId, UnknownCommandTypes.DoSomethingUnexpected)
    {
        public string SomethingImportant = "wibble";
        public Wibble Wibble = wibble;
    }

    protected enum CommandTypes
    {
        DoSomething,
        DoAnotherSomething,
        DoSomethingElse
    }

    private enum UnknownCommandTypes
    {
        DoSomethingUnexpected
    }

    private class DoSomethingHandler(Action<DoSomething> captureAction) : MessageHandler<DoSomething>
    {
        protected override Task Handle(DoSomething message)
        {
            captureAction(message);
            return Task.CompletedTask;
        }
    }

    private class DoAnotherSomethingHandler : MessageHandler<DoAnotherSomething>
    {
        protected override Task Handle(DoAnotherSomething message)
        {
            throw new ApplicationException("wibble");
        }
    }

    protected override void before_each()
    {
        base.before_each();
        TheCommand = null!;
        CapturedMessage = null!;
        CapturedLogMessages = [];
        CapturedLogException = null!;
        CapturedAlertCode = default;
        TheHandler = new DoSomethingHandler(message => CapturedMessage = message);
        AnotherHandler = new DoAnotherSomethingHandler();
        
        LoggingMocker.SetupLoggingInfoMock((theMessage, _) =>
        {
            CapturedLogMessages.Add(theMessage);
        });
        LoggingMocker.SetupLoggingErrorMock((message, theException, alertCode, _) =>
        {
           CapturedLogMessages.Add(message);
           CapturedLogException = theException;
           CapturedAlertCode = alertCode;
        });
    }

    protected void a_message()
    {
        TheCommand = new DoSomething(CorrelationId.ToString(), Wibble.Wobble);
    }
    
    protected void a_message_for_handler_that_will_fail()
    {
        TheCommand = new DoAnotherSomething(CorrelationId.ToString(), Wibble.Wobble);
    }
    
    protected void an_unexpected_message()
    {
        TheCommand = new DoSomethingUnexpected(CorrelationId.ToString(), Wibble.Wobble);
    }
    
    protected void a_message_we_have_not_registered_a_handler_for()
    {
        TheCommand = new DoSomethingElse(CorrelationId.ToString(), Wibble.Wobble);
    }
    
    protected void the_message_is_handled()
    {
        CapturedMessage.CorrelationId.ShouldBe(TheCommand.CorrelationId);
        CapturedMessage.ShouldBeOfType<DoSomething>();
        CapturedMessage.Type.ShouldBe(CommandTypes.DoSomething.ToString());
        ((DoSomething)CapturedMessage).SomethingImportant.ShouldBe("wibble");
        ((DoSomething)CapturedMessage).Wibble.ShouldBe(Wibble.Wobble);
    }
    
    protected void the_message_is_not_handled()
    {
        CapturedMessage.ShouldBeNull();
    }

    protected void the_message_is_logged_as_received()
    {
        CapturedLogMessages[0].ShouldBe($"Received message with correlation id {CorrelationId} and type {CommandTypes.DoSomething}");
    }
    
    protected void the_message_is_logged_as_completed_with_time_taken()
    {
        CapturedLogMessages[1].ShouldStartWith($"Completed message with correlation id {CorrelationId} and type {CommandTypes.DoSomething} in ");
        TimeSpan.TryParseExact(CapturedLogMessages[1].Split("in ")[1], @"hh\:mm\:ss\.fff", CultureInfo.InvariantCulture, out _).ShouldBeTrue();
    }
    
    protected void the_handling_failure_is_logged()
    {
        CapturedLogMessages[1].ShouldBe($"Error handling message with correlation id {CorrelationId} and type {CommandTypes.DoAnotherSomething}");
    }
    
        
    protected void the_message_type_failure_is_logged()
    {
        CapturedLogException.Message.ShouldBe($"Message type is not defined in the enum {typeof(CommandTypes)}");
    }
    
    protected void the_missing_handler_is_logged()
    {
        CapturedLogException.Message.ShouldBe("No handler registered for message type DoSomethingElse");
    }
    
    protected void the_deserialisation_failure_is_logged()
    {
        CapturedLogMessages[0].ShouldBe("Error handling message with correlation id Unknown and type Unknown");
    }
    
    protected void an_error_that_will_trigger_an_alert_is_logged()
    {
        CapturedAlertCode.ShouldBe(AlertCode.Alert);
    }
    
    protected void an_error_that_will_not_trigger_an_alert_is_logged()
    {
        CapturedAlertCode.ShouldBe(AlertCode.Ignore);
    }
}


