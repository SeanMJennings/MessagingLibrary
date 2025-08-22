# Messaging

Messaging with services such as Azure Storage queues available as dotnet libraries. I'm generally using MassTransit now for services like Azure Service Bus.

A command is a message sent directly to a queue with the language DoSomething. A command sender knows and cares where the command is going. 
A command recipient describes the shape of the command they respond to. A command recipient should publish a nuget package containing the command class.

An event is a message published to a topic and has the language SomethingHasHappened. An event publisher does not care or know about who receives the event. 
An event publisher describes the shape of the event and should publish a nuget package containing the event class.

A message dispatcher is responsible for taking a message from a source (queue or topic-subscription) and selecting the right message handler to invoke. The message dispatcher will then mark the message as succeeded or failed. For Azure Storage Account Queues, the dispatcher is responsible for deciding what to do on repeated failure.

A message handler is responsible for processing the message and invoking the application logic.

[See this article for command vs event explanation](https://medium.com/@shahrukhkhan_7802/action-and-reaction-understanding-commands-and-events-in-system-design-7bc346604c4a)

![Command vs Event](https://github.com/SeanMJennings/MessagingLibrary/blob/master/CommandsVsEvents.png?raw=true)
