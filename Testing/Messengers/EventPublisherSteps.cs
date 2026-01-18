using BDD;
using MassTransit;
using MassTransit.Testing;
using Messages;
using Messengers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Testing.Messengers;

public partial class EventPublisherShould : Specification
{
    private readonly Guid correlationId = Guid.NewGuid();
    private const string TheEventProperty = "wibble";
    private SomethingHappened TheEvent = null!;
    private ServiceProvider ServiceProvider = null!;
    private ITestHarness Harness = null!;

    public record SomethingHappened(string CorrelationId, string EventProperty) : IAmAnEvent;

    public class SomethingHappenedConsumer : IConsumer<SomethingHappened>
    {
        public Task Consume(ConsumeContext<SomethingHappened> context)
        {
            return Task.CompletedTask;
        }
    }
    
    protected override void before_each()
    {
        base.before_each();
        ServiceProvider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<SomethingHappenedConsumer>();
                x.UsingInMemory(
                    (context, cfg) =>
                    {
                        cfg.ReceiveEndpoint("test-events", e =>
                        {
                            e.ConfigureConsumer<SomethingHappenedConsumer>(context);
                        });
                        cfg.ConfigureEndpoints(context);
                    });
            })
            .BuildServiceProvider();
        Harness = ServiceProvider.GetRequiredService<ITestHarness>();
        Harness.Start().Await();
    }
    
    protected override void after_each()
    {
        Harness.Stop().Await();
        ServiceProvider.DisposeAsync().GetAwaiter().GetResult();
        base.after_each();
    }

    private void an_event()
    {
        TheEvent = new SomethingHappened(correlationId.ToString(), TheEventProperty);
    }
    
    private void publishing_event()
    {
        var publisher = new InitialEventPublisher(Harness.Bus);
        publisher.Publish(TheEvent).Await();
    }

    private void event_is_published()
    {
        Harness.Published.Select<SomethingHappened>().Any().ShouldBeTrue("The event should have been published to the bus.");
        Harness.Published.Select<SomethingHappened>().Any(x => x.Context.Message.CorrelationId == correlationId.ToString()).ShouldBeTrue("The event should have the correct correlation ID.");
        Harness.Published.Select<SomethingHappened>().Any(x => x.Context.Message.EventProperty == TheEventProperty).ShouldBeTrue("The event should have the correct property value.");
    }
}