namespace WebDriverBiDi;

using System.Runtime.CompilerServices;
using WebDriverBiDi.TestUtilities;

public class UnobservedTaskExceptionMonitorTests
{
    [Fact]
    public async Task TestMonitorReportsTheUnobservedFaultOfACollectedTask()
    {
        // The positive control for every test that asserts a fault was not left unobserved: a faulted task that
        // nothing observes, once collected, must be reported. Without this, a monitor or collection that never
        // reported anything would make each of those tests pass whatever the library did.
        using UnobservedTaskExceptionMonitor monitor = new("deliberately unobserved monitor control fault");

        WeakReference<Task> faultedTask = CreateUnobservedFaultedTask();

        Assert.True(await UnobservedTaskExceptionMonitor.CollectAsync(faultedTask), "The faulted task was not collected.");
        Assert.True(monitor.Raised);
        InvalidOperationException exception = Assert.IsType<InvalidOperationException>(monitor.Exception);
        Assert.Equal("deliberately unobserved monitor control fault", exception.Message);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static WeakReference<Task> CreateUnobservedFaultedTask()
        {
            return new WeakReference<Task>(Task.FromException(new InvalidOperationException("deliberately unobserved monitor control fault")));
        }
    }

    [Fact]
    public async Task TestMonitorDoesNotReportAnObservedFault()
    {
        using UnobservedTaskExceptionMonitor monitor = new("deliberately observed monitor control fault");

        WeakReference<Task> faultedTask = CreateObservedFaultedTask();

        Assert.True(await UnobservedTaskExceptionMonitor.CollectAsync(faultedTask), "The faulted task was not collected.");
        Assert.False(monitor.Raised, monitor.Exception?.ToString());

        [MethodImpl(MethodImplOptions.NoInlining)]
        static WeakReference<Task> CreateObservedFaultedTask()
        {
            Task task = Task.FromException(new InvalidOperationException("deliberately observed monitor control fault"));
            _ = task.Exception;
            return new WeakReference<Task>(task);
        }
    }
}
