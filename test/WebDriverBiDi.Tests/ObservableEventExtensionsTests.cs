namespace WebDriverBiDi;

using WebDriverBiDi.TestUtilities;

public class ObservableEventExtensionsTests
{
    [Fact]
    public async Task TestToObservableReturnsIObservable()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();
        Assert.NotNull(observable);
    }

    [Fact]
    public async Task TestSubscribeReceivesRaisedEvents()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();

        List<string> received = [];
        TaskCompletionSource<bool> twoReceived = new();
        using IDisposable subscription = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onNext: e =>
            {
                received.Add(e.EventValue);
                if (received.Count == 2)
                {
                    twoReceived.TrySetResult(true);
                }
            }));

        await testEventSource.RaiseTestEventAsync("value1");
        await testEventSource.RaiseTestEventAsync("value2");
        await twoReceived.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(2, received.Count);
        Assert.Equal("value1", received[0]);
        Assert.Equal("value2", received[1]);
    }

    [Fact]
    public async Task TestSubscribeReturnsIDisposable()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();
        IDisposable subscription = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>());
        Assert.NotNull(subscription);
        Assert.IsType<IDisposable>(subscription, exactMatch: false);
        await testEventSource.TestObservableEvent.InvokeNotifyObserversAsync(new TestObservableEventArgs("value"));
        subscription.Dispose();
    }

    [Fact]
    public async Task TestDisposingSubscriptionCallsOnCompleted()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();

        TaskCompletionSource<bool> completed = new();
        IDisposable subscription = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onCompleted: () => completed.TrySetResult(true)));

        subscription.Dispose();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.True(completed.Task.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task TestDisposingSubscriptionStopsDeliveringEvents()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();

        List<string> received = [];
        TaskCompletionSource<bool> completed = new();
        IDisposable subscription = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onNext: e => received.Add(e.EventValue),
            onCompleted: () => completed.TrySetResult(true)));

        await testEventSource.RaiseTestEventAsync("before");
        subscription.Dispose();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        int countAfterDispose = received.Count;
        await testEventSource.RaiseTestEventAsync("after");

        // Give the background task a moment to deliver any extra items (it should not)
        await Task.Delay(TimeSpan.FromMicroseconds(50), TestContext.Current.CancellationToken);
        Assert.Equal(countAfterDispose, received.Count);
    }

    [Fact]
    public async Task TestOnErrorCalledWhenOnNextThrows()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();

        TaskCompletionSource<Exception> errorReceived = new();
        InvalidOperationException thrown = new("observer failure");
        IDisposable subscription = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onNext: _ => throw thrown,
            onError: ex => errorReceived.TrySetResult(ex)));

        await testEventSource.RaiseTestEventAsync("value");
        Exception received = await errorReceived.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Same(thrown, received);
    }

    [Fact]
    public async Task TestMultipleSubscribersEachReceiveAllEvents()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();

        List<string> received1 = [];
        List<string> received2 = [];
        TaskCompletionSource<bool> sub1Done = new();
        TaskCompletionSource<bool> sub2Done = new();

        using IDisposable subscription1 = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onNext: e =>
            {
                received1.Add(e.EventValue);
                if (received1.Count == 2)
                {
                    sub1Done.TrySetResult(true);
                }
            }));

        using IDisposable subscription2 = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onNext: e =>
            {
                received2.Add(e.EventValue);
                if (received2.Count == 2)
                {
                    sub2Done.TrySetResult(true);
                }
            }));

        await testEventSource.RaiseTestEventAsync("value1");
        await testEventSource.RaiseTestEventAsync("value2");
        await Task.WhenAll(
            sub1Done.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            sub2Done.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        Assert.Equal(2, received1.Count);
        Assert.Equal(2, received2.Count);
    }

    [Fact]
    public async Task TestToObservableOnSameSourceProducesIndependentObservables()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable1 = testEventSource.TestObservableEvent.ToObservable();
        IObservable<TestObservableEventArgs> observable2 = testEventSource.TestObservableEvent.ToObservable();

        TaskCompletionSource<string> result1 = new();
        TaskCompletionSource<string> result2 = new();

        using IDisposable sub1 = observable1.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onNext: e => result1.TrySetResult(e.EventValue)));
        using IDisposable sub2 = observable2.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onNext: e => result2.TrySetResult(e.EventValue)));

        await testEventSource.RaiseTestEventAsync("value");
        await Task.WhenAll(
            result1.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            result2.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        Assert.Equal("value", await result1.Task);
        Assert.Equal("value", await result2.Task);
    }

    /// <summary>
    /// Minimal IObserver implementation driven by delegate callbacks for testing.
    /// </summary>
    private sealed class DelegateObserver<T> : IObserver<T>
    {
        private readonly Action<T>? onNext;
        private readonly Action<Exception>? onError;
        private readonly Action? onCompleted;

        public DelegateObserver(
            Action<T>? onNext = null,
            Action<Exception>? onError = null,
            Action? onCompleted = null)
        {
            this.onNext = onNext;
            this.onError = onError;
            this.onCompleted = onCompleted;
        }

        public void OnNext(T value) => this.onNext?.Invoke(value);

        public void OnError(Exception error) => this.onError?.Invoke(error);

        public void OnCompleted() => this.onCompleted?.Invoke();
    }
}
