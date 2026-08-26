namespace WebDriverBiDi.Protocol;

using TestUtilities;

public class PendingCommandCollectionTests
{
    [Fact]
    public async Task TestCanAddCommand()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        Assert.Equal(0, collection.PendingCommandCount);

        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
        Assert.Equal(1, collection.PendingCommandCount);
        Assert.True(collection.RemovePendingCommand(1, out Command? removedCommand));
        Assert.Equal("module.command", removedCommand.CommandName);
    }

    [Fact]
    public async Task TestCanRemoveCommand()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);

        Assert.True(collection.RemovePendingCommand(1, out Command? removedCommand));
        Assert.Equal("module.command", removedCommand.CommandName);
    }

    [Fact]
    public async Task TestCanClearCollection()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
        await collection.CloseAsync();
        collection.Clear();
        Assert.Equal(0, collection.PendingCommandCount);
    }

    [Fact]
    public async Task TestCanAttemptToRemoveNonExistentCommand()
    {
        PendingCommandCollection collection = new();
        Assert.False(collection.RemovePendingCommand(1, out Command? removedCommand));
    }

    [Fact]
    public async Task TestCannotAddCommandToClosedCollection()
    {
        PendingCommandCollection collection = new();
        await collection.CloseAsync();
        Assert.Equal("Cannot add command; pending command collection is closed", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await collection.AddPendingCommandAsync(new Command(1, new TestCommandParameters("test.command")), TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestCannotAddCommandWithDuplicateId()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
        Assert.Equal("Could not add command with id 1, as id already exists", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await collection.AddPendingCommandAsync(new Command(1, new TestCommandParameters("test.command")), TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestCanRemoveCommandFromClosedCollection()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
        await collection.CloseAsync();
        Assert.Equal(1, collection.PendingCommandCount);
        Assert.True(collection.RemovePendingCommand(1, out Command? removedCommand));
        Assert.Equal("module.command", removedCommand.CommandName);
    }

    [Fact]
    public async Task TestCanFailAllPendingCommands()
    {
        Command testCommand1 = new(1, new TestCommandParameters("module.command1"));
        Command testCommand2 = new(2, new TestCommandParameters("module.command2"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand1, TestContext.Current.CancellationToken);
        await collection.AddPendingCommandAsync(testCommand2, TestContext.Current.CancellationToken);
        await collection.CloseAsync();

        Exception exception = new("connection lost");
        collection.FailAllPendingCommands(exception);
        Assert.Equal(0, collection.PendingCommandCount);
        Assert.Same(exception, testCommand1.ThrownException);
        Assert.Same(exception, testCommand2.ThrownException);
    }

    [Fact]
    public async Task TestCannotFailAllPendingCommandsUnlessClosed()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
        Exception exception = new("connection lost");
        Assert.Equal("Cannot fail commands while the collection can accept new incoming commands; close it with the Close method first", Assert.ThrowsAny<InvalidOperationException>(() => collection.FailAllPendingCommands(exception)).Message);
    }

    [Fact]
    public async Task TestCannotClearCollectionUnlessClosed()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
        Assert.Equal("Cannot clear the collection while it can accept new incoming commands; close it with the Close method first", Assert.ThrowsAny<InvalidOperationException>(() => collection.Clear()).Message);
    }

    [Fact]
    public async Task TestIsAcceptingCommandsPropertyHasCorrectValue()
    {
        PendingCommandCollection collection = new();
        Assert.True(collection.IsAcceptingCommands);
        await collection.CloseAsync();
        Assert.False(collection.IsAcceptingCommands);
    }

    [Fact]
    public async Task TestCanDispose()
    {
        PendingCommandCollection collection = new();
        collection.Dispose();
    }

    [Fact]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        PendingCommandCollection collection = new();
        collection.Dispose();
        collection.Dispose();
    }

    [Fact]
    public async Task TestCanDisposeAfterUse()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
        collection.RemovePendingCommand(1, out _);
        await collection.CloseAsync();
        collection.Clear();
        collection.Dispose();
    }

    [Fact]
    public async Task TestAddPendingCommandThrowsWhenCancellationTokenIsCanceled()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        PendingCommandCollection collection = new();
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await collection.AddPendingCommandAsync(testCommand, cts.Token));
        Assert.Equal(0, collection.PendingCommandCount);
    }

    [Fact]
    public async Task TestDisposeFinalizerPathDoesNotThrow()
    {
        ExposedDisposeCollection collection = new();
        collection.DisposeUnmanaged();
    }

    [Fact]
    public async Task TestDisposeFinalizerPathAfterManagedDisposeDoesNotThrow()
    {
        ExposedDisposeCollection collection = new();
        collection.Dispose();
        collection.DisposeUnmanaged();
    }

    private sealed class ExposedDisposeCollection : PendingCommandCollection
    {
        public void DisposeUnmanaged() => this.Dispose(false);
    }

    [Fact]
    public void TestDefaultCanceledCommandTrackingCapacity()
    {
        using PendingCommandCollection collection = new();
        Assert.Equal(PendingCommandCollection.DefaultMaxTrackedCanceledCommands, collection.MaxTrackedCanceledCommands);
        Assert.Equal(1024u, collection.MaxTrackedCanceledCommands);
        Assert.Equal(0, collection.TrackedCanceledCommandCount);
    }

    [Fact]
    public async Task TestCancelPendingCommandRemovesAndTracksCommand()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        using PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);

        Assert.True(collection.CancelPendingCommand(testCommand, CommandCancellationReason.TimedOut));

        Assert.True(testCommand.IsCanceled);
        Assert.Equal(0, collection.PendingCommandCount);
        Assert.Equal(1, collection.TrackedCanceledCommandCount);
        Assert.False(collection.RemovePendingCommand(1, out _));

        Assert.True(collection.TryRemoveCanceledCommand(1, out CanceledCommandInfo? canceledCommand));
        Assert.Equal(1, canceledCommand.CommandId);
        Assert.Equal("module.command", canceledCommand.CommandName);
        Assert.Equal(testCommand.ResponseType, canceledCommand.ResponseType);
        Assert.Equal(CommandCancellationReason.TimedOut, canceledCommand.Reason);
        Assert.True(canceledCommand.TimeSinceCancellation >= TimeSpan.Zero);
    }

    [Fact]
    public void TestCancelPendingCommandForCompletedCommandThatIsNotPendingReportsNoCancellationAndDoesNotTrack()
    {
        // A command whose response has already been received (and so is no longer pending)
        // cannot be canceled, and can never produce a late response, so nothing is remembered.
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        testCommand.SetResult(new TestCommandResult());
        using PendingCommandCollection collection = new();

        Assert.False(collection.CancelPendingCommand(testCommand, CommandCancellationReason.TimedOut));

        Assert.False(testCommand.IsCanceled);
        Assert.True(testCommand.TryGetResult(out _));
        Assert.Equal(0, collection.TrackedCanceledCommandCount);
        Assert.False(collection.TryRemoveCanceledCommand(1, out CanceledCommandInfo? canceledCommand));
        Assert.Null(canceledCommand);
    }

    [Fact]
    public async Task TestCancelPendingCommandReportsWhetherCancellationTookEffect()
    {
        // The return value reports the cancellation outcome, not whether the command was tracked:
        // a second cancellation of the same command finds nothing to cancel and returns false.
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        using PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);

        Assert.True(collection.CancelPendingCommand(testCommand, CommandCancellationReason.Canceled));
        Assert.False(collection.CancelPendingCommand(testCommand, CommandCancellationReason.Canceled));
        Assert.Equal(1, collection.TrackedCanceledCommandCount);
    }

    [Fact]
    public async Task TestTryRemoveCanceledCommandIsOneShot()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        using PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
        collection.CancelPendingCommand(testCommand, CommandCancellationReason.Canceled);

        Assert.True(collection.TryRemoveCanceledCommand(1, out _));
        Assert.False(collection.TryRemoveCanceledCommand(1, out _));
        Assert.Equal(0, collection.TrackedCanceledCommandCount);
    }

    [Fact]
    public async Task TestCanceledCommandTrackingForgetsOldestBeyondCapacity()
    {
        using PendingCommandCollection collection = new(2);
        for (long id = 1; id <= 3; id++)
        {
            Command testCommand = new(id, new TestCommandParameters("module.command"));
            await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);
            collection.CancelPendingCommand(testCommand, CommandCancellationReason.Canceled);
        }

        Assert.Equal(2, collection.TrackedCanceledCommandCount);
        Assert.False(collection.TryRemoveCanceledCommand(1, out _));
        Assert.True(collection.TryRemoveCanceledCommand(2, out _));
        Assert.True(collection.TryRemoveCanceledCommand(3, out _));
    }

    [Fact]
    public async Task TestZeroCapacityDisablesCanceledCommandTracking()
    {
        Command testCommand = new(1, new TestCommandParameters("module.command"));
        using PendingCommandCollection collection = new(0);
        await collection.AddPendingCommandAsync(testCommand, TestContext.Current.CancellationToken);

        Assert.True(collection.CancelPendingCommand(testCommand, CommandCancellationReason.Canceled));

        Assert.True(testCommand.IsCanceled);
        Assert.Equal(0, collection.TrackedCanceledCommandCount);
        Assert.False(collection.TryRemoveCanceledCommand(1, out _));
    }

    [Fact]
    public async Task TestClearTracksCanceledCommandsWithConnectionClosedReason()
    {
        Command first = new(1, new TestCommandParameters("module.first"));
        Command second = new(2, new TestCommandParameters("module.second"));
        using PendingCommandCollection collection = new();
        await collection.AddPendingCommandAsync(first, TestContext.Current.CancellationToken);
        await collection.AddPendingCommandAsync(second, TestContext.Current.CancellationToken);
        await collection.CloseAsync();

        collection.Clear();

        Assert.True(first.IsCanceled);
        Assert.True(second.IsCanceled);
        Assert.Equal(0, collection.PendingCommandCount);
        Assert.Equal(2, collection.TrackedCanceledCommandCount);
        Assert.True(collection.TryRemoveCanceledCommand(2, out CanceledCommandInfo? canceledCommand));
        Assert.Equal("module.second", canceledCommand.CommandName);
        Assert.Equal(CommandCancellationReason.ConnectionClosed, canceledCommand.Reason);
    }
}
