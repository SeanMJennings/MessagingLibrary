using Azure.Messaging.ServiceBus;
using BDD;
using Dispatchers.ServiceBus;
using Handlers;
using Moq;
using Shouldly;

namespace Testing.Dispatchers.ServiceBus;

public partial class ServiceBusQueueMessageDispatcherShould : CommonSteps
{
    private ServiceBusMessageDispatcher<CommandTypes> theDispatcher = null!;
    private Mock<ServiceBusClient> mockServiceBusClient = null!;
    private Mock<ServiceBusReceiver> mockServiceBusReceiver = null!;

    protected override void before_each()
    {
        base.before_each();
        theDispatcher = null!;
        mockServiceBusClient = new Mock<ServiceBusClient>();
        mockServiceBusReceiver = new Mock<ServiceBusReceiver>();
        mockServiceBusReceiver.SetupGet(x => x.FullyQualifiedNamespace).Returns("wobble.servicebus.windows.net");
    }

    private void a_message_receiver()
    {
        var enumToHandlerMap = new Dictionary<CommandTypes, IAmAMessageHandler>
        {
            {CommandTypes.DoSomething, TheHandler},
            {CommandTypes.DoAnotherSomething, AnotherHandler}
        };
        theDispatcher = new ServiceBusMessageDispatcher<CommandTypes>(enumToHandlerMap);
    }
    
    private static void a_message_that_cannot_be_deserialised(){}

    private void handling_the_message()
    {
        mockServiceBusClient.Setup(x => x.CreateReceiver(It.IsAny<string>())).Returns(mockServiceBusReceiver.Object);
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(BinaryData.FromString(JsonSerialization.Serialize(TheCommand)));
        setup_service_bus_mocks(serviceBusReceivedMessage);
        theDispatcher.DispatchMessageFromQueue(mockServiceBusClient.Object, "wibble").Await();
    }    
    
    private void handling_the_message_that_will_be_dequeued_for_third_time()
    {
        mockServiceBusClient.Setup(x => x.CreateReceiver(It.IsAny<string>())).Returns(mockServiceBusReceiver.Object);
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(BinaryData.FromString(JsonSerialization.Serialize(TheCommand)), 
            null, 
            null, 
            null, 
            null, 
            null, 
            default, 
            null, 
            null, 
            null, 
            null, 
            null, 
            default, 
            null, 
            default, 
            3);
        setup_service_bus_mocks(serviceBusReceivedMessage);
        theDispatcher.DispatchMessageFromQueue(mockServiceBusClient.Object, "wibble").Await();
    }

    private void setup_service_bus_mocks(ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        mockServiceBusReceiver.Setup(x => x.ReceiveMessageAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(serviceBusReceivedMessage);
        mockServiceBusReceiver.Setup(x =>
            x.CompleteMessageAsync(serviceBusReceivedMessage, It.IsAny<CancellationToken>()));
        mockServiceBusReceiver.Setup(x =>
            x.AbandonMessageAsync(serviceBusReceivedMessage,  It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()));
    }

    private void handling_the_message_that_cannot_be_deserialised()
    {
        mockServiceBusClient.Setup(x => x.CreateReceiver(It.IsAny<string>())).Returns(mockServiceBusReceiver.Object);
        var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(BinaryData.FromString("nonsense"));
        setup_service_bus_mocks(serviceBusReceivedMessage);
        theDispatcher.DispatchMessageFromQueue(mockServiceBusClient.Object, "wibble").Await();
    }
    
    private void the_message_is_completed()
    {
        mockServiceBusReceiver.Verify(x => x.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private void the_message_is_abandoned()
    {
        mockServiceBusReceiver.Verify(x => x.AbandonMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
    }    
    
    private void the_message_is_deadlettered()
    {
        mockServiceBusReceiver.Verify(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), "Poisoned message", $"Message dead-lettered for wibble queue in service bus wobble.servicebus.windows.net.\nError handling message with correlation id {CorrelationId.ToString()} and type {CommandTypes.DoAnotherSomething}", It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private void the_deadlettering_is_explicitly_logged()
    {
        CapturedLogMessages[1].ShouldBe( $"Message dead-lettered for wibble queue in service bus wobble.servicebus.windows.net.\nError handling message with correlation id {CorrelationId} and type {CommandTypes.DoAnotherSomething}");
    }
}


