using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BDD;
using Messages;
using Messengers.ServiceBus;
using Shouldly;

namespace Testing.Messengers.ServiceBus;

public partial class ServiceBusCommandSenderShould : AzureServiceBusContainerSpecification
{
    private readonly Guid correlationId = Guid.NewGuid();
    private const string TheCommandProperty = "wibble";
    private DoSomething theCommand = null!;
    private const string DefaultQueueName = "queue.1";
    private ServiceBusClient client = null!;
    private ServiceBusReceiver receiver = null!;
    private ServiceBusCommandSender commandSender = null!;

    private enum Wibble
    {
        Wobble
    }

    private class DoSomething(string correlationId, string commandProperty) : Command(correlationId, CommandTypes.DoSomething)
    {
        public string CommandProperty { get; } = commandProperty;
        public Wibble Wobble { get; } = Wibble.Wobble;
    }
    
    private enum CommandTypes
    {
        DoSomething
    }

    protected override void before_each()
    {
        base.before_each();
        theCommand = null!;
        client = new ServiceBusClient(ServiceBusContainer.GetConnectionString());
        receiver = client.CreateReceiver(DefaultQueueName);
    }
    
    protected override void after_each()
    {
        receiver.DisposeAsync().GetAwaiter().GetResult();
        client.DisposeAsync().GetAwaiter().GetResult();
        base.after_each();
    }
    
    private void creating_the_service_bus_sender()
    {
        commandSender = ServiceBusCommandSender.New(ServiceBusContainer.GetConnectionString(), DefaultQueueName);
    }

    private void a_command()
    {
        theCommand = new DoSomething(correlationId.ToString(), TheCommandProperty);
    }

    private void sending_command_to_service_bus_queue()
    {
        creating_the_service_bus_sender();
        commandSender.SendToQueue(theCommand).Await();
    }

    private void command_is_sent_to_service_bus_queue()
    {
        var receivedMessage = receiver.ReceiveMessageAsync().Await();
        receivedMessage.Body.ToString().ShouldContain(nameof(Wibble.Wobble));
        var receivedCommand = JsonSerializer.Deserialize<DoSomething>(receivedMessage.Body.ToString());
        receivedCommand!.CorrelationId.ShouldBe(correlationId.ToString());
        receivedCommand.Type.ShouldBe(nameof(CommandTypes.DoSomething));
        receivedCommand.CommandProperty.ShouldBe(TheCommandProperty);
        receivedCommand.Wobble.ShouldBe(Wibble.Wobble);
    }
}