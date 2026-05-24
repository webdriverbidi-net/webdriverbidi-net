namespace WebDriverBiDi.Protocol;

using TestUtilities;

public class EventInvokerTests
{
    [Fact]
    public async Task TestCanInvokeEvent()
    {
        bool eventInvoked = false;
        Task action(EventInfo<TestEventArgs> info)
        {
            eventInvoked = true;
            return Task.CompletedTask;
        }
        EventInvoker<TestEventArgs> invoker = new(action);
        await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        Assert.True(eventInvoked);
    }

    [Fact]
    public async Task TestInvokeEventWithInvalidObjectTypeThrows()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await invoker.InvokeEventAsync("this is an invalid object", ReceivedDataDictionary.EmptyDictionary));
    }

    [Fact]
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

        Assert.NotNull(capturedEventData);
        Assert.Same(originalEventArgs, capturedEventData);
    }

    [Fact]
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

        Assert.NotNull(capturedAdditionalData);
        Assert.Same(additionalData, capturedAdditionalData);
        Assert.Equal("value1", capturedAdditionalData["key1"]);
        Assert.Equal(42, capturedAdditionalData["key2"]);
    }

    [Fact]
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

        Assert.NotNull(capturedAdditionalData);
        Assert.Empty(capturedAdditionalData);
    }

    [Fact]
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

        Assert.NotNull(capturedAdditionalData);
        Assert.IsType<ReceivedDataDictionary>(capturedAdditionalData["outer"]);
    }

    [Fact]
    public async Task TestInvokeWithNullEventDataThrows()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await invoker.InvokeEventAsync(null, ReceivedDataDictionary.EmptyDictionary));
    }

    [Fact]
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

        Assert.NotNull(capturedEventData);
        Assert.Equal("testEventName", capturedEventData.EventName);
    }

    [Fact]
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

        Assert.Equal(3, invocationCount);
    }

    [Fact]
    public async Task TestAsyncDelegateWithDelay()
    {
        bool delegateCompleted = false;
        TaskCompletionSource delegateGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        async Task action(EventInfo<TestEventArgs> info)
        {
            taskCompletionSource.TrySetResult();
            await delegateGate.Task;
            delegateCompleted = true;
        }

        EventInvoker<TestEventArgs> invoker = new(action);
        Task invocationTask = invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.False(delegateCompleted);

        delegateGate.SetResult();
        await invocationTask;

        Assert.True(delegateCompleted);
    }

    [Fact]
    public async Task TestExceptionInAsyncDelegateIsPropagated()
    {
        static Task action(EventInfo<TestEventArgs> info)
        {
            throw new InvalidOperationException("Test exception from delegate");
        }

        EventInvoker<TestEventArgs> invoker = new(action);

        Assert.Equal("Test exception from delegate", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary))).Message);
    }

    [Fact]
    public async Task TestExceptionInAsyncTaskIsPropagated()
    {
        static async Task action(EventInfo<TestEventArgs> info)
        {
            await Task.Yield();
            throw new ArgumentException("Async exception from delegate");
        }

        EventInvoker<TestEventArgs> invoker = new(action);

        Assert.Equal("Async exception from delegate", (await Assert.ThrowsAnyAsync<ArgumentException>(async () => await invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary))).Message);
    }

    [Fact]
    public async Task TestReturnedTaskCompletesCorrectly()
    {
        TaskCompletionSource taskCompletionSource = new();
        Task action(EventInfo<TestEventArgs> info)
        {
            return taskCompletionSource.Task;
        }

        EventInvoker<TestEventArgs> invoker = new(action);
        Task invocationTask = invoker.InvokeEventAsync(new TestEventArgs(), ReceivedDataDictionary.EmptyDictionary);

        Assert.False(invocationTask.IsCompleted);

        taskCompletionSource.SetResult();
        await invocationTask;

        Assert.True(invocationTask.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task TestInvalidTypeWithHelpfulErrorMessage()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);

        WebDriverBiDiException caughtException = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(
            async () => await invoker.InvokeEventAsync(123, ReceivedDataDictionary.EmptyDictionary));

        Assert.Contains("TestEventArgs", caughtException.Message);
        Assert.Contains("cast", caughtException.Message);
    }

    [Fact]
    public async Task TestTypeCloseButNotAssignableThrows()
    {
        EventInvoker<TestEventArgs> invoker = new((info) => Task.CompletedTask);
        TestParameterizedEventArgs wrongTypeEventArgs = new(new TestValidEventData("test"));

        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await invoker.InvokeEventAsync(wrongTypeEventArgs, ReceivedDataDictionary.EmptyDictionary));
    }

    [Fact]
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

        Assert.NotNull(capturedEventData);
        Assert.IsType<TestEventArgs>(capturedEventData);
    }

    [Fact]
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

        Assert.NotNull(capturedEventInfo);
        Assert.Same(eventArgs, capturedEventInfo.EventData);
    }

    [Fact]
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

        Assert.NotNull(capturedEventInfo);
        Assert.Same(additionalData, capturedEventInfo.AdditionalData);
        Assert.Equal("testValue", capturedEventInfo.AdditionalData["testKey"]);
    }
}
