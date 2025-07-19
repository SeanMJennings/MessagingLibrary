using MassTransit;
using Messages;

namespace Messengers;

public interface IAmAnInitialCommandSender
{
    Task SendToQueue<T>(T theCommand) where T : IAmACommand;
}

// This command sender does not wait for a response.
public sealed class InitialCommandSender(ISendEndpointProvider SendEndpointProvider, Uri SendUri) : IAmAnInitialCommandSender
{
    public async Task SendToQueue<T>(T theCommand) where T : IAmACommand
    {
        var endpoint = await SendEndpointProvider.GetSendEndpoint(SendUri);
        await endpoint.Send(theCommand);
    }
}