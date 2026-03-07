namespace WebDriverBiDi.Protocol;

using TestUtilities;

[TestFixture]
public class EventInvokerTests
{
    [Test]
    public async Task TestCanInvokeEvent()
    {
        bool eventInvoked = false;
        Task action(EventInfo<TestEventArgs> info) {
            eventInvoked = true;
            return Task.CompletedTask;
        }
        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        Assert.That(eventInvoked, Is.True);
    }

    [Test]
    public void TestInvokeEventWithInvalidObjectTypeThrows()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);
        Assert.That(async () => await invoker.InvokeEventAsync("this is an invalid object", ReceivedDataDictionary.EmptyDictionary), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public async Task TestEventDataPassedCorrectlyToDelegate()
    {
        TestEventArgs? capturedEventData = null;
        Task action(EventInfo<TestEventArgs> info)
        {
            capturedEventData = info.EventData;
            return Task.CompletedTask;
        }

        TestEventArgs originalEventArgs = new TestEventArgs();
        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(originalEventArgs, ReceivedDataDictionary.EmptyDictionary);

        Assert.That(capturedEventData, Is.Not.Null);
        Assert.That(capturedEventData, Is.SameAs(originalEventArgs));
    }

    [Test]
    public async Task TestAdditionalDataPassedCorrectlyToDelegate()
    {
        ReceivedDataDictionary? capturedAdditionalData = null;
        Task action(EventInfo<TestEventArgs> info)
        {
            capturedAdditionalData = info.AdditionalData;
            return Task.CompletedTask;
        }

        Dictionary<string, object?> additionalDataValues = new() { ["key1"] = "value1", ["key2"] = 42 };
        ReceivedDataDictionary additionalData = new(additionalDataValues);

        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(new TestEventArgs(), additionalData);

        Assert.That(capturedAdditionalData, Is.Not.Null);
        Assert.That(capturedAdditionalData, Is.SameAs(additionalData));
        Assert.That(capturedAdditionalData!["key1"], Is.EqualTo("value1"));
        Assert.That(capturedAdditionalData["key2"], Is.EqualTo(42));
    }

    [Test]
    public async Task TestInvokeWithEmptyAdditionalData()
    {
        ReceivedDataDictionary? capturedAdditionalData = null;
        Task action(EventInfo<TestEventArgs> info)
        {
            capturedAdditionalData = info.AdditionalData;
            return Task.CompletedTask;
        }

        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);

        Assert.That(capturedAdditionalData, Is.Not.Null);
        Assert.That(capturedAdditionalData.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task TestInvokeWithNestedAdditionalData()
    {
        ReceivedDataDictionary? capturedAdditionalData = null;
        Task action(EventInfo<TestEventArgs> info)
        {
            capturedAdditionalData = info.AdditionalData;
            return Task.CompletedTask;
        }

        Dictionary<string, object?> nestedData = new() { ["nested"] = "value" };
        Dictionary<string, object?> additionalDataValues = new() { ["outer"] = new ReceivedDataDictionary(nestedData) };
        ReceivedDataDictionary additionalData = new(additionalDataValues);

        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(new TestEventArgs(), additionalData);

        Assert.That(capturedAdditionalData, Is.Not.Null);
        Assert.That(capturedAdditionalData!["outer"], Is.InstanceOf<ReceivedDataDictionary>());
    }

    [Test]
    public void TestInvokeWithNullEventDataThrows()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);
        Assert.That(async () => await invoker.InvokeEventAsync(null, ReceivedDataDictionary.EmptyDictionary), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public async Task TestInvokeWithComplexEventDataType()
    {
        TestParameterizedEventArgs? capturedEventData = null;
        Task action(EventInfo<TestParameterizedEventArgs> info)
        {
            capturedEventData = info.EventData;
            return Task.CompletedTask;
        }

        TestValidEventData eventData = new("testEventName");
        TestParameterizedEventArgs eventArgs = new(eventData);

        EventInvoker<TestParameterizedEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(eventArgs, ReceivedDataDictionary.EmptyDictionary);

        Assert.That(capturedEventData, Is.Not.Null);
        Assert.That(capturedEventData!.EventName, Is.EqualTo("testEventName"));
    }

    [Test]
    public async Task TestMultipleSequentialInvocations()
    {
        int invocationCount = 0;
        Task action(EventInfo<TestEventArgs> info)
        {
            invocationCount++;
            return Task.CompletedTask;
        }

        EventInvoker<TestEventArgs> invoker = new(action);

        await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);

        Assert.That(invocationCount, Is.EqualTo(3));
    }

    [Test]
    public async Task TestAsyncDelegateWithDelay()
    {
        bool delegateCompleted = false;
        async Task action(EventInfo<TestEventArgs> info)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            delegateCompleted = true;
        }

        EventInvoker<TestEventArgs> invoker = new(action);
        Task invocationTask = invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);

        Assert.That(delegateCompleted, Is.False, "Delegate should not have completed yet");

        await invocationTask;

        Assert.That(delegateCompleted, Is.True, "Delegate should have completed after awaiting");
    }

    [Test]
    public void TestExceptionInAsyncDelegateIsPropagated()
    {
        Task action(EventInfo<TestEventArgs> info)
        {
            throw new InvalidOperationException("Test exception from delegate");
        }

        EventInvoker<TestEventArgs> invoker = new(action);

        Assert.That(
            async () => await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary),
            Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo("Test exception from delegate"));
    }

    [Test]
    public void TestExceptionInAsyncTaskIsPropagated()
    {
        async Task action(EventInfo<TestEventArgs> info)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            throw new ArgumentException("Async exception from delegate");
        }

        EventInvoker<TestEventArgs> invoker = new(action);

        Assert.That(
            async () => await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary),
            Throws.InstanceOf<ArgumentException>().With.Message.EqualTo("Async exception from delegate"));
    }

    [Test]
    public async Task TestReturnedTaskCompletesCorrectly()
    {
        TaskCompletionSource taskCompletionSource = new();
        Task action(EventInfo<TestEventArgs> info)
        {
            return taskCompletionSource.Task;
        }

        EventInvoker<TestEventArgs> invoker = new(action);
        Task invocationTask = invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);

        Assert.That(invocationTask.IsCompleted, Is.False, "Task should not be completed yet");

        taskCompletionSource.SetResult();
        await invocationTask;

        Assert.That(invocationTask.IsCompletedSuccessfully, Is.True, "Task should have completed successfully");
    }

    [Test]
    public void TestInvalidTypeWithHelpfulErrorMessage()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);

        WebDriverBiDiException? caughtException = null;
        try
        {
            invoker.InvokeEventAsync(123, ReceivedDataDictionary.EmptyDictionary).Wait();
        }
        catch (AggregateException ex)
        {
            caughtException = ex.InnerException as WebDriverBiDiException;
        }

        Assert.That(caughtException, Is.Not.Null);
        Assert.That(caughtException!.Message, Does.Contain("TestEventArgs"));
        Assert.That(caughtException.Message, Does.Contain("cast"));
    }

    [Test]
    public void TestTypeCloseButNotAssignableThrows()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);
        TestParameterizedEventArgs wrongTypeEventArgs = new(new TestValidEventData("test"));

        Assert.That(
            async () => await invoker.InvokeEventAsync(wrongTypeEventArgs, ReceivedDataDictionary.EmptyDictionary),
            Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public async Task TestInvokerWithBaseEventArgsType()
    {
        WebDriverBiDiEventArgs? capturedEventData = null;
        Task action(EventInfo<WebDriverBiDiEventArgs> info)
        {
            capturedEventData = info.EventData;
            return Task.CompletedTask;
        }

        TestEventArgs eventArgs = new TestEventArgs();
        EventInvoker<WebDriverBiDiEventArgs> invoker = new(action);

        // TestEventArgs derives from WebDriverBiDiEventArgs, so this should work
        await invoker.InvokeEventAsync(eventArgs, ReceivedDataDictionary.EmptyDictionary);

        Assert.That(capturedEventData, Is.Not.Null);
        Assert.That(capturedEventData, Is.InstanceOf<TestEventArgs>());
    }

    [Test]
    public async Task TestEventInfoCreatedCorrectlyWithEventData()
    {
        EventInfo<TestEventArgs>? capturedEventInfo = null;
        Task action(EventInfo<TestEventArgs> info)
        {
            capturedEventInfo = info;
            return Task.CompletedTask;
        }

        TestEventArgs eventArgs = new TestEventArgs();
        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(eventArgs, ReceivedDataDictionary.EmptyDictionary);

        Assert.That(capturedEventInfo, Is.Not.Null);
        Assert.That(capturedEventInfo!.EventData, Is.SameAs(eventArgs));
    }

    [Test]
    public async Task TestEventInfoCreatedCorrectlyWithAdditionalData()
    {
        EventInfo<TestEventArgs>? capturedEventInfo = null;
        Task action(EventInfo<TestEventArgs> info)
        {
            capturedEventInfo = info;
            return Task.CompletedTask;
        }

        Dictionary<string, object?> additionalDataValues = new() { ["testKey"] = "testValue" };
        ReceivedDataDictionary additionalData = new(additionalDataValues);

        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(new TestEventArgs(), additionalData);

        Assert.That(capturedEventInfo, Is.Not.Null);
        Assert.That(capturedEventInfo!.AdditionalData, Is.SameAs(additionalData));
        Assert.That(capturedEventInfo.AdditionalData["testKey"], Is.EqualTo("testValue"));
    }
}
