namespace WebDriverBiDi;

using WebDriverBiDi.TestUtilities;

[TestFixture]
public class ObservableEventExtensionsTests
{
    [Test]
    public void TestToObservableReturnsIObservable()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();
        Assert.That(observable, Is.Not.Null);
    }

    [Test]
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
        await twoReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received, Has.Count.EqualTo(2));
            Assert.That(received[0], Is.EqualTo("value1"));
            Assert.That(received[1], Is.EqualTo("value2"));
        }
    }

    [Test]
    public async Task TestSubscribeReturnsIDisposable()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();
        IDisposable subscription = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>());
        Assert.That(subscription, Is.Not.Null);
        Assert.That(subscription, Is.InstanceOf<IDisposable>());
        await testEventSource.TestObservableEvent.InvokeNotifyObserversAsync(new TestObservableEventArgs("value"));
        subscription.Dispose();
    }

    [Test]
    public async Task TestDisposingSubscriptionCallsOnCompleted()
    {
        TestEventSource testEventSource = new();
        IObservable<TestObservableEventArgs> observable = testEventSource.TestObservableEvent.ToObservable();

        TaskCompletionSource<bool> completed = new();
        IDisposable subscription = observable.Subscribe(new DelegateObserver<TestObservableEventArgs>(
            onCompleted: () => completed.TrySetResult(true)));

        subscription.Dispose();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.That(completed.Task.IsCompletedSuccessfully, Is.True);
    }

    [Test]
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
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));

        int countAfterDispose = received.Count;
        await testEventSource.RaiseTestEventAsync("after");

        // Give the background task a moment to deliver any extra items (it should not)
        await Task.Delay(50);
        Assert.That(received, Has.Count.EqualTo(countAfterDispose));
    }

    [Test]
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
        Exception received = await errorReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.That(received, Is.SameAs(thrown));
    }

    [Test]
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
            sub1Done.Task.WaitAsync(TimeSpan.FromSeconds(5)),
            sub2Done.Task.WaitAsync(TimeSpan.FromSeconds(5)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(received1, Has.Count.EqualTo(2));
            Assert.That(received2, Has.Count.EqualTo(2));
        }
    }

    [Test]
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
            result1.Task.WaitAsync(TimeSpan.FromSeconds(5)),
            result2.Task.WaitAsync(TimeSpan.FromSeconds(5)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result1.Task.Result, Is.EqualTo("value"));
            Assert.That(result2.Task.Result, Is.EqualTo("value"));
        }
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
