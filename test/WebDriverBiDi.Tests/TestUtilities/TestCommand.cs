namespace WebDriverBiDi.TestUtilities;

using WebDriverBiDi.Protocol;

/// <summary>
/// A test command whose WaitForCompletionAsync always returns true without
/// completing the underlying TaskCompletionSource. This leaves the command
/// in a state where Result, ThrownException, and IsCanceled are all
/// null/false, which is unreachable in production but needed to verify
/// the safety-net branch in BiDiDriver.ExecuteCommandAsync.
/// </summary>
public class TestCommand : Command
{
    public TestCommand(long commandId, CommandParameters commandData)
        : base(commandId, commandData)
    {
    }

    public override Task<bool> WaitForCompletionAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
