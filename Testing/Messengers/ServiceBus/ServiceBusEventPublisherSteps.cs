using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BDD;
using Messages;
using Messengers.ServiceBus;
using Moq;
using Shouldly;

namespace Testing.Messengers.ServiceBus;

public partial class ServiceBusEventPublisherShould : AzureServiceBusContainerSpecification
{
    private readonly Guid correlationId = Guid.NewGuid();
    private const string TheEventProperty = "wibble";
    private SomethingHasHappened theEvent = null!;
    private const string DefaultTopicName = "topic.1";
    private const string DefaultSubscriptionName = "subscription.1";
    private Mock<IAmAServiceBus> serviceBusMock = null!;
    private ServiceBusClient client = null!;
    private ServiceBusProcessor processor = null!;
    private ServiceBusEventPublisher eventPublisher = null!;

    private class SomethingHasHappened(string correlationId, string eventProperty) : Event(correlationId, EventTypes.SomethingHasHappened)
    {
        public string EventProperty { get; } = eventProperty;
        public Wibble Wobble { get; } = Wibble.Wobble;
    }
    
    private enum EventTypes
    {
        SomethingHasHappened
    }
    
        
    private enum Wibble
    {
        Wobble
    }

    protected override void before_each()
    {
        base.before_each();
        theEvent = null!;
        serviceBusMock = new Mock<IAmAServiceBus>();
        serviceBusMock.Setup(s => s.CreateTopicIfNotExistsAsync(DefaultTopicName));
        client = new ServiceBusClient(ServiceBusContainer.GetConnectionString());
        serviceBusMock.Setup(s => s.CreateSender(DefaultTopicName)).Returns(client.CreateSender(DefaultTopicName));
        processor = client.CreateProcessor(DefaultTopicName, DefaultSubscriptionName);
    }
    
    protected override void after_each()
    {
        processor.DisposeAsync().GetAwaiter().GetResult();
        client.DisposeAsync().GetAwaiter().GetResult();
        base.after_each();
    }
    
    private static void a_service_bus_with_no_topics(){}

    private void creating_the_service_bus_event_publisher_using_a_factory()
    {
        eventPublisher = new ServiceBusEventPublisherFactory(serviceBusMock.Object)
            .CreateServiceBusEventPublisherEnsuringTopicExists(DefaultTopicName).Await();
    }

    private void the_topic_is_created()
    {
        serviceBusMock.Verify(x => x.CreateTopicIfNotExistsAsync(DefaultTopicName), Times.Once);
    }

    private void an_event()
    {
        theEvent = new SomethingHasHappened(correlationId.ToString(), TheEventProperty);
    }

    private void publishing_event_to_service_bus_topic()
    {
        eventPublisher = new ServiceBusEventPublisher(DefaultTopicName, serviceBusMock.Object);
        eventPublisher.PublishToTopic(theEvent).Await();
    }

    private void event_is_received_in_subscription_to_service_bus_topic()
    {
        processor.ProcessMessageAsync += receive_and_assert_message;
        processor.ProcessErrorAsync += _ => Task.CompletedTask;
        processor.StartProcessingAsync().Await(); 
        Task.Delay(TimeSpan.FromSeconds(1)).Await();
        processor.StopProcessingAsync().Await();
    }

    private Task receive_and_assert_message(ProcessMessageEventArgs processMessageEventArgs)
    {
        processMessageEventArgs.Message.Body.ToString().ShouldContain(nameof(Wibble.Wobble));
        var receivedEvent = JsonSerializer.Deserialize<SomethingHasHappened>(processMessageEventArgs.Message.Body.ToString())!;
        receivedEvent.CorrelationId.ShouldBe(correlationId.ToString());
        receivedEvent.Type.ShouldBe(nameof(EventTypes.SomethingHasHappened));
        receivedEvent.EventProperty.ShouldBe(TheEventProperty);
        receivedEvent.Wobble.ShouldBe(Wibble.Wobble);
        return Task.CompletedTask;
    }
}