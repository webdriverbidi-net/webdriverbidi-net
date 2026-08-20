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
