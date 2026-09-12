namespace WebDriverBiDi.TestUtilities;

using Microsoft.Extensions.Time.Testing;

/// <summary>
/// A <see cref="FakeTimeProvider"/> that reports timer creation, so a test can advance virtual
/// time only once the code under test has registered the timer it is about to wait on. Advancing
/// before that point does nothing (the timer does not exist yet) and the wait then never elapses.
/// </summary>
public sealed class TestTimeProvider : FakeTimeProvider
{
    private readonly object signalLock = new();
    private int timerCount;
    private TaskCompletionSource timerCreatedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Gets the number of timers created on this provider so far.
    /// </summary>
    public int TimerCount
    {
        get
        {
            lock (this.signalLock)
            {
                return this.timerCount;
            }
        }
    }

    /// <summary>
    /// Gets a task that completes once more timers have been created than <paramref name="observedTimerCount"/>.
    /// </summary>
    /// <param name="observedTimerCount">The count the caller has already accounted for.</param>
    /// <returns>A task that completes when a further timer has been created.</returns>
    public Task WaitForTimerCreatedAsync(int observedTimerCount)
    {
        lock (this.signalLock)
        {
            return this.timerCount > observedTimerCount ? Task.CompletedTask : this.timerCreatedSignal.Task;
        }
    }

    /// <summary>
    /// Advances virtual time by <paramref name="step"/> each time the code under test registers a
    /// timer, until <paramref name="operation"/> completes. Every timeout the operation takes is
    /// therefore elapsed as soon as it is armed, without any wall-clock delay. The operation's
    /// outcome is not observed here; the caller awaits it afterwards.
    /// </summary>
    /// <param name="operation">The operation whose timeouts are to be elapsed.</param>
    /// <param name="step">The amount to advance per timer; use at least the longest timeout involved.</param>
    /// <param name="cancellationToken">The test's cancellation token.</param>
    /// <returns>A task that completes when <paramref name="operation"/> has completed.</returns>
    public async Task AdvanceUntilCompletedAsync(Task operation, TimeSpan step, CancellationToken cancellationToken)
    {
        int accountedFor = 0;
        while (!operation.IsCompleted)
        {
            // A timer registered between the previous advance and this check counts too, so the
            // signal is keyed on the count rather than on "the next" creation.
            Task timerCreated = this.WaitForTimerCreatedAsync(accountedFor).WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            Task completed = await Task.WhenAny(operation, timerCreated);
            if (completed == operation)
            {
                // The bounded wait is abandoned here, and its guard would fault it with a
                // TimeoutException five seconds later. Observe that fault so it cannot surface as an
                // UnobservedTaskException in whichever test happens to force a collection next.
                _ = timerCreated.ContinueWith(
                    static abandonedWait => _ = abandonedWait.Exception,
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
                break;
            }

            // Surfaces the five-second guard as a TimeoutException if the operation neither
            // completes nor arms a timer, rather than looping forever.
            await timerCreated;
            accountedFor = this.TimerCount;
            this.Advance(step);
        }
    }

    /// <inheritdoc/>
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        ITimer timer = base.CreateTimer(callback, state, dueTime, period);
        TaskCompletionSource completedSignal;
        lock (this.signalLock)
        {
            this.timerCount++;
            completedSignal = this.timerCreatedSignal;
            this.timerCreatedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        completedSignal.TrySetResult();
        return timer;
    }
}
