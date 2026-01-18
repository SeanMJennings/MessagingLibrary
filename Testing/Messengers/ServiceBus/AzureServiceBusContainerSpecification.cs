using BDD;
using Testcontainers.ServiceBus;

namespace Testing.Messengers.ServiceBus;

// service bus emulator doesn't support on-the-fly management operations through a client-side SDK. So admin client is mocked.
public class AzureServiceBusContainerSpecification : Specification
{
    protected ServiceBusContainer ServiceBusContainer = null!;
    
    protected override void before_all()
    {
        base.before_all();
        ServiceBusContainer = new ServiceBusBuilder()
            .WithAcceptLicenseAgreement(true)
            .Build();
        ServiceBusContainer.StartAsync().Await();
    }

    protected override void after_all()
    {
        ServiceBusContainer.DisposeAsync().GetAwaiter().GetResult();
        base.after_all();
    }
}