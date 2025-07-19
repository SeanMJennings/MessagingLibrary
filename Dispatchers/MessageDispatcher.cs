using System.Text;
using System.Text.Json;
using Handlers;
using Messages;

namespace Dispatchers;

public abstract class MessageDispatcher<E>(Dictionary<E, IAmAMessageHandler> enumToHandlerMappings)
    where E : Enum
{
    protected async Task Handle(BinaryData binaryData, Func<Task> completeMessage, Func<string, Exception, Task> abandonMessage)
    {
        var startTime = DateTime.UtcNow;
        IAmAMessageWithType? message = null;
        try
        {
            var messageType = DeserializeAndRetrieveMessageEnum(binaryData);
            message = DeserializeAndRetrieveMessage(GetMessageJson(binaryData), messageType);
            Logger.LogInformation($"Received message with correlation id {message.CorrelationId} and type {message.Type}");
            await enumToHandlerMappings[messageType].HandleMessage(message);
            await completeMessage();
            Logger.LogInformation($@"Completed message with correlation id {message.CorrelationId} and type {message.Type} in {DateTime.UtcNow - startTime:hh\:mm\:ss\.fff}");
        }
        catch (Exception exception)
        {
            var logMessage = $"Error handling message with correlation id {message?.CorrelationId ?? "Unknown"} and type {message?.Type ?? "Unknown"}";
            await abandonMessage(logMessage, exception);
        }
    }
    
    private static E DeserializeAndRetrieveMessageEnum(BinaryData binaryData)
    {
        var json = GetMessageJson(binaryData);
        TryParseMessageType(json, out var messageEnumType);
        return messageEnumType;
    }    
    
    private IAmAMessageWithType DeserializeAndRetrieveMessage(JsonElement json, E messageEnumType)
    {
        ValidateHandlerRegistered(messageEnumType);
        var handler = enumToHandlerMappings[messageEnumType];
        var theMessageClassType = handler.GetMessageType();
        var message = (IAmAMessageWithType)JsonSerialization.Deserialize(json.ToString(), theMessageClassType)!;
        return message;
    }

    private static JsonElement GetMessageJson(BinaryData binaryData)
    {
        if (IsBase64String(binaryData))
        {
            var base64String = binaryData.ToString();
            var binaryDataBytes = Convert.FromBase64String(base64String);
            binaryData = new BinaryData(binaryDataBytes);
        }
        
        var messageJson = Encoding.UTF8.GetString(binaryData);
        var json = JsonSerializer.Deserialize<JsonElement>(messageJson);
        return json;
    }
    
    private static bool IsBase64String(BinaryData binaryData)
    {
        var buffer = new Span<byte>(new byte[binaryData.ToString().Length]);
        return Convert.TryFromBase64String(binaryData.ToString(), buffer , out _);
    }

    private static void TryParseMessageType(JsonElement messageJson, out E messageType)
    {
        if (Enum.TryParse(typeof(E), messageJson.GetProperty(nameof(IAmAMessageWithType.Type)).ToString(), out var parsedMessageType))
        {
            messageType = (E)parsedMessageType;
            return;
        }
        
        var exception = new ArgumentException($"Message type is not defined in the enum {typeof(E)}");
        throw exception;
    }

    private void ValidateHandlerRegistered(E messageType)
    {
        if (enumToHandlerMappings.ContainsKey(messageType)) return;
        
        var exception = new ArgumentException($"No handler registered for message type {messageType}");
        throw exception;
    }
}