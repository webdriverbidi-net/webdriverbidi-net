namespace WebDriverBiDi.TestUtilities;

/// <summary>
/// Watches <see cref="TaskScheduler.UnobservedTaskException"/> for a single, identifiable
/// exception while the monitor is alive.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TaskScheduler.UnobservedTaskException"/> is a process-wide event, and the tests
/// that use it force a full garbage collection to make the finalizer raise it. Test classes run
/// in parallel, so that collection also finalizes faulted tasks abandoned by whatever other tests
/// happen to be running at the same time, and a handler that flags every notification will
/// intermittently fail on somebody else's leaked task.
/// </para>
/// <para>
/// Matching on a marker message keeps the assertion scoped to the task actually under test.
/// Notifications that do not match are left alone -- not marked observed -- so that the test
/// that owns them can still see them.
/// </para>
/// </remarks>
public sealed class UnobservedTaskExceptionMonitor : IDisposable
{
    private const int MaxCollectionAttempts = 10;

    private readonly string markerMessage;
    private readonly object matchLockObject = new();
    private readonly List<Exception> matchedExceptions = new();
    private readonly EventHandler<UnobservedTaskExceptionEventArgs> handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnobservedTaskExceptionMonitor"/> class and
    /// begins listening for unobserved task exceptions.
    /// </summary>
    /// <param name="markerMessage">
    /// The <see cref="Exception.Message"/> of the exception this test expects to be able to
    /// identify as its own. It must be unique within the test assembly.
    /// </param>
    public UnobservedTaskExceptionMonitor(string markerMessage)
    {
        this.markerMessage = markerMessage;
        this.handler = this.OnUnobservedTaskException;
        TaskScheduler.UnobservedTaskException += this.handler;
    }

    /// <summary>
    /// Gets a value indicating whether an unobserved task exception matching the marker message was raised.
    /// </summary>
    public bool Raised
    {
        get
        {
            lock (this.matchLockObject)
            {
                return this.matchedExceptions.Count > 0;
            }
        }
    }

    /// <summary>
    /// Gets the first matching unobserved exception, or <see langword="null"/> if none was raised.
    /// </summary>
    public Exception? Exception
    {
        get
        {
            lock (this.matchLockObject)
            {
                return this.matchedExceptions.Count > 0 ? this.matchedExceptions[0] : null;
            }
        }
    }

    /// <summary>
    /// Asynchronously forces full, blocking garbage collections, running pending finalizers after each one,
    /// until the given task has been collected or the attempts are exhausted.
    /// </summary>
    /// <param name="task">A weak reference to the task whose fault the test is checking.</param>
    /// <returns>
    /// <see langword="true"/> if the task was collected and its finalization has run, so that
    /// <see cref="Raised"/> has settled; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <see cref="TaskScheduler.UnobservedTaskException"/> is raised only when a faulted task that nothing
    /// references any longer is finalized. While anything still references the task the event cannot be
    /// raised, whether or not the fault was observed, so a check of <see cref="Raised"/> proves nothing unless
    /// the task was actually collected. Assert this method's result before asserting on <see cref="Raised"/>,
    /// and create the task in a separate, non-inlined method, so that no local of the test itself roots it.
    /// </para>
    /// <para>
    /// A task completes on whatever thread ran its last continuation, and that thread can still be unwinding
    /// out of the task's own frames when the test resumes on another thread. Each attempt therefore begins by
    /// yielding, so such a thread can finish, and the method gives up after a fixed number of attempts rather
    /// than waiting on a clock.
    /// </para>
    /// </remarks>
    public static async Task<bool> CollectAsync(WeakReference<Task> task)
    {
        for (int attempt = 0; attempt < MaxCollectionAttempts; attempt++)
        {
            await Task.Yield();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // The target is discarded rather than stored, because a stored target would be hoisted into this
            // method's state machine and keep the task alive into the next attempt.
            if (!task.TryGetTarget(out _))
            {
                // The collect above is what made the task unreachable, so its exception holder was queued
                // for finalization by that collect and the wait before it cannot have run the finalizer
                // that raises the event. Wait once more, so that a caller reading Raised is reading a
                // settled value rather than racing the finalizer thread.
                GC.WaitForPendingFinalizers();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Stops listening for unobserved task exceptions.
    /// </summary>
    public void Dispose()
    {
        TaskScheduler.UnobservedTaskException -= this.handler;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        AggregateException? aggregateException = e.Exception;
        if (aggregateException is null)
        {
            return;
        }

        foreach (Exception innerException in aggregateException.Flatten().InnerExceptions)
        {
            if (innerException.Message == this.markerMessage)
            {
                lock (this.matchLockObject)
                {
                    this.matchedExceptions.Add(innerException);
                }

                e.SetObserved();
            }
        }
    }
}
