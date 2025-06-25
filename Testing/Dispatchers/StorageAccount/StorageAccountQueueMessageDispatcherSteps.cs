using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using BDD;
using Dispatchers.StorageAccountQueue;
using Handlers;
using Moq;
using Shouldly;

namespace Testing.Dispatchers.StorageAccount;

public partial class StorageAccountQueueMessageDispatcherShould : CommonSteps
{
    private const string QueueName = "queue";
    private const string PoisonQueueName = "queue-poisoned";
    private StorageAccountQueueMessageDispatcher<CommandTypes> theDispatcher = null!;
    private Mock<QueueServiceClient> mockStorageAccountClient = null!;
    private Mock<QueueClient> mockQueueClient = null!;
    private Mock<QueueClient> mockPoisonedQueueClient = null!;
    private QueueMessage? queueMessage;

    protected override void before_each()
    {
        base.before_each();
        queueMessage = null!;
        theDispatcher = null!;
        mockStorageAccountClient = new Mock<QueueServiceClient>();
        mockQueueClient = new Mock<QueueClient>();
        mockQueueClient.SetupGet(m => m.AccountName).Returns("theAccount");
        mockPoisonedQueueClient = new Mock<QueueClient>();
        mockPoisonedQueueClient.Setup(x =>
            x.CreateIfNotExistsAsync(It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()));
        mockPoisonedQueueClient.SetupGet(m => m.Name).Returns(PoisonQueueName);
        mockStorageAccountClient.Setup(x => x.GetQueueClient(QueueName)).Returns(mockQueueClient.Object);
        mockStorageAccountClient.Setup(x => x.GetQueueClient(PoisonQueueName)).Returns(mockPoisonedQueueClient.Object);
    }

    private void a_message_receiver()
    {
        var enumToHandlerMap = new Dictionary<CommandTypes, IAmAMessageHandler>
        {
            {CommandTypes.DoSomething, TheHandler},
            {CommandTypes.DoAnotherSomething, AnotherHandler}
        };
        theDispatcher = new StorageAccountQueueMessageDispatcher<CommandTypes>(enumToHandlerMap);
    }
    
    private static void a_message_that_cannot_be_deserialised(){}

    private void handling_the_message()
    {
        queueMessage = QueuesModelFactory.QueueMessage(
            messageId: "id2", 
            popReceipt: "pr2", 
            body: BinaryData.FromString(JsonSerialization.Serialize(TheCommand)), 
            dequeueCount: 1, 
            insertedOn: DateTimeOffset.UtcNow);
        setup_storage_account_mocks();
        theDispatcher.DispatchMessageFromQueue(mockStorageAccountClient.Object, QueueName).Await();
    }
    
    private void handling_the_message_that_gets_dequeud_for_third_time()
    {
        queueMessage = QueuesModelFactory.QueueMessage(
            messageId: "id2", 
            popReceipt: "pr2", 
            body: BinaryData.FromString(JsonSerialization.Serialize(TheCommand)), 
            dequeueCount: 3, 
            insertedOn: DateTimeOffset.UtcNow);
        setup_storage_account_mocks();
        theDispatcher.DispatchMessageFromQueue(mockStorageAccountClient.Object, QueueName).Await();
    }    
    
    private void handling_the_message_that_is_sent_base_64_encoded()
    {
        queueMessage = QueuesModelFactory.QueueMessage(
            messageId: "id2", 
            popReceipt: "pr2", 
            body: BinaryData.FromString(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerialization.Serialize(TheCommand)))),
            dequeueCount: 1, 
            insertedOn: DateTimeOffset.UtcNow);
        setup_storage_account_mocks();
        theDispatcher.DispatchMessageFromQueue(mockStorageAccountClient.Object, QueueName).Await();
    }
    
    private void setup_storage_account_mocks()
    {
        var mockResponse = new Mock<Response<QueueMessage>>();
        mockResponse.Setup(x => x.Value).Returns(queueMessage!);
        mockResponse.Setup(x => x.HasValue).Returns(true);
        mockQueueClient.Setup(x => x.ReceiveMessageAsync(TimeSpan.FromMinutes(5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);
        mockQueueClient.Setup(x => x.UpdateMessageAsync(queueMessage!.MessageId, queueMessage.PopReceipt,
            queueMessage.Body, It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()));
        mockQueueClient.Setup(x => x.DeleteMessageAsync(queueMessage!.MessageId, queueMessage.PopReceipt, It.IsAny<CancellationToken>()));
    }

    private void handling_the_message_that_cannot_be_deserialised()
    {
        queueMessage = QueuesModelFactory.QueueMessage(
            messageId: "id2", 
            popReceipt: "pr2", 
            body: BinaryData.FromString("wibble"), 
            dequeueCount: 1, 
            insertedOn: DateTimeOffset.UtcNow);
        setup_storage_account_mocks();
        theDispatcher.DispatchMessageFromQueue(mockStorageAccountClient.Object, QueueName).Await();
    }

    private void the_poison_queue_is_created()
    {
        mockPoisonedQueueClient.Verify(x => x.CreateIfNotExistsAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private void the_message_is_completed()
    {
        mockQueueClient.Verify(x => x.DeleteMessageAsync(queueMessage!.MessageId, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private void the_message_is_moved_to_a_poison_queue()
    {
        mockPoisonedQueueClient.Verify(x => x.SendMessageAsync(queueMessage!.Body, TimeSpan.FromSeconds(10), TimeSpan.FromDays(7), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private void the_message_is_removed_from_the_queue()
    {
        mockQueueClient.Verify(x => x.DeleteMessageAsync(queueMessage!.MessageId, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private void the_message_is_abandoned()
    {
        mockQueueClient.Verify(x => x.UpdateMessageAsync(queueMessage!.MessageId, It.IsAny<string>(), It.IsAny<BinaryData>(), TimeSpan.FromSeconds(10), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    private void the_poisoning_is_explicitly_logged()
    {
        CapturedLogMessages[1].ShouldBe( $"Poisoned message moved to queue-poisoned storage queue in storage account theAccount.\n"+ $"Error handling message with correlation id {CorrelationId} and type {CommandTypes.DoAnotherSomething}");
    }
}


