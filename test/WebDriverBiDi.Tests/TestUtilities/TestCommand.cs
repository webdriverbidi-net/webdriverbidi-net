namespace WebDriverBiDi.TestUtilities;

using WebDriverBiDi.Protocol;

/// <summary>
/// A test command that replaces either the whole of <see cref="WaitForCompletionAsync"/>, or only the wait for
/// its outcome inside the real <see cref="Command.WaitForCompletionAsync"/>.
/// </summary>
/// <remarks>
/// <para>
/// By default, <see cref="WaitForCompletionAsync"/> returns true without completing the underlying
/// TaskCompletionSource. This leaves the command in a state where Result, ThrownException, and IsCanceled are all
/// null/false, which is unreachable in production but needed to verify the safety-net branch in
/// BiDiDriver.ExecuteCommandAsync.
/// </para>
/// <para>
/// When <see cref="OutcomeWaitHandler"/> is set, the real <see cref="Command.WaitForCompletionAsync"/> runs instead,
/// and only its wait for the outcome is replaced, so that a test can fix the order in which the command completes
/// and the wait ends.
/// </para>
/// </remarks>
public class TestCommand : Command
{
    public TestCommand(long commandId, CommandParameters commandData)
        : base(commandId, commandData)
    {
    }

    /// <summary>
    /// Gets or sets a delegate invoked by <see cref="WaitForCompletionAsync"/> that may manipulate the
    /// command (for example, set its result) and returns the value the wait should report. When not set,
    /// the wait reports that the command completed. It is not used when <see cref="OutcomeWaitHandler"/> is set.
    /// </summary>
    public Func<TestCommand, bool>? CompletionBehavior { get; set; }

    /// <summary>
    /// Gets or sets a delegate that stands in for the wait for the command's outcome, receiving the command and the
    /// token passed to the wait. When it is set, the real <see cref="Command.WaitForCompletionAsync"/> runs around it,
    /// and <see cref="CompletionBehavior"/> is not used.
    /// </summary>
    public Func<TestCommand, CancellationToken, Task>? OutcomeWaitHandler { get; set; }

    public override Task<bool> WaitForCompletionAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (this.OutcomeWaitHandler is not null)
        {
            return base.WaitForCompletionAsync(timeout, cancellationToken);
        }

        return Task.FromResult(this.CompletionBehavior?.Invoke(this) ?? true);
    }

    protected override Task WaitForOutcomeAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (this.OutcomeWaitHandler is not null)
        {
            return this.OutcomeWaitHandler(this, cancellationToken);
        }

        return base.WaitForOutcomeAsync(timeout, cancellationToken);
    }
}
