using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BDD;
using Messages;
using Messengers.ServiceBus;
using Moq;
using Shouldly;

namespace Testing.Messengers.ServiceBus;

public partial class ServiceBusCommandSenderShould : AzureServiceBusContainerSpecification
{
    private Guid correlationId = Guid.NewGuid();
    private const string TheCommandProperty = "wibble";
    private DoSomething theCommand = null!;
    private const string DefaultQueueName = "queue.1";
    private Mock<IAmAServiceBusAdministrationClientWrapper> serviceBusAdministrationClientMock = null!;
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
        serviceBusAdministrationClientMock = new Mock<IAmAServiceBusAdministrationClientWrapper>();
        serviceBusAdministrationClientMock.Setup(s => s.QueueExistsAsync(DefaultQueueName))
            .ReturnsAsync(false);
        serviceBusAdministrationClientMock.Setup(s => s.CreateQueueAsync(DefaultQueueName));
        client = new ServiceBusClient(ServiceBusContainer.GetConnectionString());
        receiver = client.CreateReceiver(DefaultQueueName);
    }
    
    protected override void after_each()
    {
        receiver.DisposeAsync().GetAwaiter().GetResult();
        client.DisposeAsync().GetAwaiter().GetResult();
        base.after_each();
    }
    
    private static void a_service_bus_with_no_queues(){}

    private void creating_the_service_bus_sender()
    {
        commandSender = new ServiceBusCommandSender(ServiceBusContainer.GetConnectionString(), DefaultQueueName, serviceBusAdministrationClientMock.Object);
    }

    private void the_queue_is_created()
    {
        serviceBusAdministrationClientMock.Verify(x => x.QueueExistsAsync(DefaultQueueName), Times.Once);
        serviceBusAdministrationClientMock.Verify(x => x.CreateQueueAsync(DefaultQueueName), Times.Once);
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
        receivedMessage.Body.ToString().ShouldContain(Wibble.Wobble.ToString());
        var receivedCommand = JsonSerializer.Deserialize<DoSomething>(receivedMessage.Body.ToString());
        receivedCommand!.CorrelationId.ShouldBe(correlationId.ToString());
        receivedCommand.Type.ShouldBe(CommandTypes.DoSomething.ToString());
        receivedCommand.CommandProperty.ShouldBe(TheCommandProperty);
        receivedCommand.Wobble.ShouldBe(Wibble.Wobble);
    }
}