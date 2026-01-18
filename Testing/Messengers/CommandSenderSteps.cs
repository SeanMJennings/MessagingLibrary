using BDD;
using MassTransit;
using MassTransit.Testing;
using Messages;
using Messengers;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Testing.Messengers;

public partial class CommandSenderShould : Specification
{
    private readonly Guid correlationId = Guid.NewGuid();
    private const string TheCommandProperty = "wibble";
    private DoSomething theCommand = null!;
    private ServiceProvider ServiceProvider = null!;
    private ITestHarness Harness = null!;

    public record DoSomething(string CorrelationId, string CommandProperty) : IAmACommand;

    public class DoSomethingConsumer : IConsumer<DoSomething>
    {
        public Task Consume(ConsumeContext<DoSomething> context)
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
                x.AddConsumer<DoSomethingConsumer>();
                x.UsingInMemory(
                    (context, cfg) =>
                    {
                        cfg.ReceiveEndpoint("test-commands", e =>
                        {
                            e.ConfigureConsumer<DoSomethingConsumer>(context);
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

    private void a_command()
    {
        theCommand = new DoSomething(correlationId.ToString(), TheCommandProperty);
    }

    private void sending_command()
    {
        var commandSender = new InitialCommandSender(Harness.Bus, new Uri("queue:test-commands"));
        commandSender.SendToQueue(theCommand).Await();
    }

    private void command_is_sent()
    {
        Harness.Sent.Select<DoSomething>().Any().ShouldBeTrue("The command should have been sent to the bus.");
        Harness.Sent.Select<DoSomething>().Any(x => x.Context.Message.CorrelationId == correlationId.ToString()).ShouldBeTrue("The command should have the correct correlation ID.");
        Harness.Sent.Select<DoSomething>().Any(x => x.Context.Message.CommandProperty == TheCommandProperty).ShouldBeTrue("The command should have the correct property value.");
    }
}