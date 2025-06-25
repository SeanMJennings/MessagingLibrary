using BDD;
using Testcontainers.Azurite;

namespace Testing.Messengers.StorageAccountQueue;

public class AzuriteContainerSpecification : Specification
{
    protected AzuriteContainer AzuriteContainer = null!;
    
    protected override void before_all()
    {
        base.before_all();
        AzuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .WithCommand("--skipApiVersionCheck")
            .Build();
        AzuriteContainer.StartAsync().Await();
    }

    protected override void after_all()
    {
        AzuriteContainer.DisposeAsync().GetAwaiter().GetResult();
        base.after_all();
    }
}