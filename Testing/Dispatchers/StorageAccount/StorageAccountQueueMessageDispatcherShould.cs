using NUnit.Framework;

namespace Testing.Dispatchers.StorageAccount;

[TestFixture]
public partial class StorageAccountQueueMessageDispatcherShould
{
    [Test]
    public void handle_a_message()
    {
        Given(a_message);
        And(a_message_receiver);
        When(handling_the_message);
        Then(the_message_is_handled);
        And(the_poison_queue_is_created);
        And(the_message_is_logged_as_received);
        And(the_message_is_completed);
        And(the_message_is_logged_as_completed_with_time_taken);
    }

    [Test]
    public void handle_a_message_that_is_base64_encoded()
    {
        Given(a_message);
        And(a_message_receiver);
        When(handling_the_message_that_is_sent_base_64_encoded);
        Then(the_message_is_handled);
        And(the_message_is_logged_as_received);
        And(the_message_is_completed);
    }

    [Test]
    public void move_a_message_to_poison_queue_if_fails_for_third_time()
    {
        Given(a_message_for_handler_that_will_fail);
        And(a_message_receiver);
        When(handling_the_message_that_gets_dequeud_for_third_time);
        Then(the_message_is_moved_to_a_poison_queue);
        And(the_message_is_removed_from_the_queue);
        And(the_poisoning_is_explicitly_logged);
        And(an_error_that_will_trigger_an_alert_is_logged);
    }
    
    [Test]
    public void log_a_failure_to_handle()
    {
        Given(a_message_for_handler_that_will_fail);
        And(a_message_receiver);
        When(handling_the_message);
        Then(the_message_is_not_handled);
        And(the_handling_failure_is_logged);
        And(the_message_is_abandoned);
        And(an_error_that_will_not_trigger_an_alert_is_logged);
    }

    [Test]
    public void error_if_message_type_not_recognised()
    {
        Given(an_unexpected_message);
        And(a_message_receiver);
        When(handling_the_message);
        Then(the_message_is_not_handled);
        And(the_message_type_failure_is_logged);
        And(the_message_is_abandoned);
        And(an_error_that_will_not_trigger_an_alert_is_logged);
    }
    
    [Test]
    public void error_if_no_handler_registered()
    {
        Given(a_message_we_have_not_registered_a_handler_for);
        And(a_message_receiver);
        When(handling_the_message);
        And(the_message_is_not_handled);
        And(the_missing_handler_is_logged);
        And(the_message_is_abandoned);
        And(an_error_that_will_not_trigger_an_alert_is_logged);
    }
        
    [Test]
    public void error_if_message_cannot_be_deserialised()
    {
        Given(a_message_that_cannot_be_deserialised);
        And(a_message_receiver);
        When(handling_the_message_that_cannot_be_deserialised);
        Then(the_message_is_not_handled);
        And(the_deserialisation_failure_is_logged);
        And(the_message_is_abandoned);
        And(an_error_that_will_not_trigger_an_alert_is_logged);
    }
    
    [Test]
    public void do_nothing_if_no_message_in_queue()
    {
        Given(a_message_receiver);
        When(handling_the_message);
        Then(the_message_is_not_handled);
    }
}