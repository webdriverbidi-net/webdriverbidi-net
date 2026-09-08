namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using Microsoft.Extensions.Time.Testing;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.TestUtilities;

public class CommandTests
{
    [Fact]
    public async Task TestCanSerializeCommand()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
            { "overflowParameterName", "overflowParameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "additionalCommandProperty", "additionalCommandPropertyValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        command.AdditionalCommandProperties["additionalCommandProperty"] = "additionalCommandPropertyValue";
        string json = JsonSerializer.Serialize(command);
        Dictionary<string, object?> dataValue = JObject.Parse(json).ToParsedDictionary();
        Assert.Equivalent(expected, dataValue);
    }

    [Fact]
    public async Task TestCannotDeserializeCommand()
    {
        string json = """
                      {
                        "id": 1,
                        "method": "module.command",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Deserialize<Command>(json));
    }

    [Fact]
    public async Task TestCommandResultType()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        Assert.Equal(typeof(CommandResponseMessage<TestCommandResult>), command.ResponseType);
    }

    [Fact]
    public async Task TestCommandResult()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.Null(command.ThrownException);
        TestCommandResult result = new();
        command.SetResult(result);
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
        hasResult = command.TryGetResult(out commandResult);
        Assert.True(hasResult);
        Assert.NotNull(commandResult);
        Assert.IsType<TestCommandResult>(commandResult);
        Assert.Null(command.ThrownException);
        Assert.Equal(result with { }, commandResult);
    }

    [Fact]
    public async Task TestCommandThrownException()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.Null(command.ThrownException);
        command.SetException(new WebDriverBiDiException("test exception"));
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
        hasResult = command.TryGetResult(out commandResult);
        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.NotNull(command.ThrownException);
        Assert.IsType<WebDriverBiDiException>(command.ThrownException);
        WebDriverBiDiException? thrownException = command.ThrownException as WebDriverBiDiException;
        Assert.NotNull(thrownException);
        Assert.Equal("test exception", thrownException.Message);
    }

    [Fact]
    public async Task TestCommandCancel()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.Null(command.ThrownException);
        Assert.False(command.IsCanceled);
        command.Cancel();
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);
        hasResult = command.TryGetResult(out commandResult);
        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.Null(command.ThrownException);
        Assert.True(command.IsCanceled);
    }

    [Fact]
    public async Task TestSetExceptionFaultsCommand()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        command.SetException(new WebDriverBiDiException("custom fault message"));
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.NotNull(command.ThrownException);
        Assert.IsType<WebDriverBiDiException>(command.ThrownException);
        Assert.Equal("custom fault message", command.ThrownException.Message);
        Assert.False(command.IsCanceled);
    }

    [Fact]
    public async Task TestWaitForCompletionReturnsFalseOnTimeout()
    {
        TestCommandParameters commandParams = new TestCommandParameters("module.command");
        FakeTimeProvider timeProvider = new();
        Command command = new(1, commandParams, timeProvider);

        // The wait arms its timer against the provider before its first suspension, so advancing
        // afterwards elapses the timeout without any real time passing.
        TimeSpan timeout = TimeSpan.FromSeconds(10);
        Task<bool> waitTask = command.WaitForCompletionAsync(timeout, TestContext.Current.CancellationToken);
        timeProvider.Advance(timeout + TimeSpan.FromMilliseconds(1));
        bool completed = await waitTask;
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);

        Assert.False(completed);
        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.Null(command.ThrownException);
        Assert.False(command.IsCanceled);
    }

    [Fact]
    public void TestCannotCreateCommandWithNullTimeProvider()
    {
        Assert.Throws<ArgumentNullException>(() => new Command(1, new TestCommandParameters("module.command"), null!));
    }

    [Fact]
    public async Task TestWaitForCompletionAsyncWithTimeoutExceedingMaximumThrows()
    {
        Command command = new(1, new TestCommandParameters("module.command"));
        ArgumentOutOfRangeException exception = await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(async () => await command.WaitForCompletionAsync(TimeSpan.MaxValue, TestContext.Current.CancellationToken));
        Assert.Equal("timeout", exception.ParamName);
        Assert.Contains("no greater than", exception.Message);
    }

    [Fact]
    public async Task TestWaitForCompletionAsyncWithNegativeTimeoutThrows()
    {
        Command command = new(1, new TestCommandParameters("module.command"));
        ArgumentOutOfRangeException exception = await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(async () => await command.WaitForCompletionAsync(TimeSpan.FromSeconds(-1), TestContext.Current.CancellationToken));
        Assert.Equal("timeout", exception.ParamName);
    }

    [Fact]
    public async Task TestWaitForCompletionAsyncWithMaximumTimeoutCompletes()
    {
        Command command = new(1, new TestCommandParameters("module.command"));
        Task<bool> waitTask = command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(uint.MaxValue - 1), TestContext.Current.CancellationToken);
        command.SetResult(new TestCommandResult());
        Assert.True(await waitTask);
    }

    [Fact]
    public async Task TestWaitForCompletionAsyncWithInfiniteTimeoutCompletes()
    {
        Command command = new(1, new TestCommandParameters("module.command"));
        Task<bool> waitTask = command.WaitForCompletionAsync(Timeout.InfiniteTimeSpan, TestContext.Current.CancellationToken);
        command.SetResult(new TestCommandResult());
        Assert.True(await waitTask);
    }

    [Fact]
    public async Task TestSettingResultAfterAlreadyCompletedDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        TestCommandResult firstResult = new();
        command.SetResult(firstResult);
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        command.SetResult(new TestCommandResult());
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.True(hasResult);
        Assert.Equal(firstResult with { }, commandResult);
    }

    [Fact]
    public async Task TestSetExceptionAfterAlreadyCompletedDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        TestCommandResult result = new();
        command.SetResult(result);
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        command.SetException(new WebDriverBiDiException("late exception"));
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.True(hasResult);
        Assert.NotNull(commandResult);
        Assert.Null(command.ThrownException);
    }

    [Fact]
    public async Task TestCancelAfterAlreadyCompletedDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        TestCommandResult result = new();
        command.SetResult(result);
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        command.Cancel();
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.True(hasResult);
        Assert.NotNull(commandResult);
        Assert.False(command.IsCanceled);
    }

    [Fact]
    public async Task TestSetResultAfterCancelDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        command.Cancel();
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        command.SetResult(new TestCommandResult());
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.True(command.IsCanceled);
    }

    [Fact]
    public async Task TestSetExceptionAfterCancelDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        command.Cancel();
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50), TestContext.Current.CancellationToken);

        command.SetException(new WebDriverBiDiException("late exception"));
        Assert.Null(command.ThrownException);
        Assert.True(command.IsCanceled);
    }

    [Fact]
    public async Task TestWaitForCompletionReturnsTrueWhenCompletedBeforeCancellation()
    {
        TestCommandParameters commandParams = new TestCommandParameters("module.command");
        Command command = new(1, commandParams);

        command.SetResult(new TestCommandResult());
        using CancellationTokenSource cts = new();

        bool completed = await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), cts.Token);
        Assert.True(completed);
    }

    [Fact]
    public async Task TestWaitForCompletionThrowsWhenCancellationTokenIsCanceled()
    {
        TestCommandParameters commandParams = new TestCommandParameters("module.command");
        Command command = new(1, commandParams);

        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), cts.Token));
    }

    [Fact]
    public async Task TestWaitForCompletionThrowsWhenCancellationTokenIsCanceledDuringWait()
    {
        TestCommandParameters commandParams = new TestCommandParameters("module.command");
        Command command = new(1, commandParams);

        FakeTimeProvider timeProvider = new();
        using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(50), timeProvider);
        timeProvider.Advance(TimeSpan.FromSeconds(1));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await command.WaitForCompletionAsync(TimeSpan.FromSeconds(30), cts.Token));
    }

    [Fact]
    public void TestCancelReportsWhetherCancellationTookEffect()
    {
        Command command = new(1, new TestCommandParameters("module.command"));
        Assert.True(command.Cancel());
        Assert.True(command.IsCanceled);

        // Already canceled: nothing left to cancel.
        Assert.False(command.Cancel());
        Assert.True(command.IsCanceled);
    }

    [Fact]
    public void TestCancelOnCompletedCommandLeavesResultIntact()
    {
        Command command = new(1, new TestCommandParameters("module.command"));
        command.SetResult(new TestCommandResult { Value = "done" });

        Assert.False(command.Cancel());
        Assert.False(command.IsCanceled);
        Assert.True(command.TryGetResult(out CommandResult? result));
        Assert.Equal("done", Assert.IsType<TestCommandResult>(result).Value);
    }

    [Fact]
    public void TestElapsedMillisecondsIsZeroBeforeTheCommandIsSent()
    {
        // Timing starts when a Transport sends the command; a command that has never been sent has no
        // interval to report.
        Command command = new(1, new TestCommandParameters("module.command"));
        Assert.Equal(0, command.ElapsedMilliseconds);
    }

    [Fact]
    public void TestElapsedMillisecondsIsZeroWhenTimingNeverStarted()
    {
        // Reading it more than once still reports zero: there is no start timestamp to measure from,
        // and nothing about reading the property starts one.
        Command command = new(1, new TestCommandParameters("module.command"));
        Assert.Equal(0, command.ElapsedMilliseconds);
        Assert.Equal(0, command.ElapsedMilliseconds);
    }

    [Fact]
    public void TestNullCommandParametersThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new Command(1, null!));
    }
}
