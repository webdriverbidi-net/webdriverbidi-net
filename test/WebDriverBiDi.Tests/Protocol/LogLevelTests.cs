namespace WebDriverBiDi.Protocol;

using System.Text;
using WebDriverBiDi.TestUtilities;

/// <summary>
/// Covers the minimum log level shared by the connection, the transport and the driver: its default, the
/// filtering it performs, and the fact that all three layers read and write one value.
/// </summary>
public class LogLevelTests
{
    [Fact]
    public void TestDefaultLogLevelIsInfo()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(1), transport);
        Assert.Equal(WebDriverBiDiLogLevel.Info, connection.LogLevel);
        Assert.Equal(WebDriverBiDiLogLevel.Info, transport.LogLevel);
        Assert.Equal(WebDriverBiDiLogLevel.Info, driver.LogLevel);
    }

    [Fact]
    public void TestTheThreeLayersShareOneValue()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(1), transport);

        driver.LogLevel = WebDriverBiDiLogLevel.Trace;
        Assert.Equal(WebDriverBiDiLogLevel.Trace, transport.LogLevel);
        Assert.Equal(WebDriverBiDiLogLevel.Trace, connection.LogLevel);

        transport.LogLevel = WebDriverBiDiLogLevel.Warn;
        Assert.Equal(WebDriverBiDiLogLevel.Warn, driver.LogLevel);
        Assert.Equal(WebDriverBiDiLogLevel.Warn, connection.LogLevel);

        connection.LogLevel = WebDriverBiDiLogLevel.Error;
        Assert.Equal(WebDriverBiDiLogLevel.Error, driver.LogLevel);
        Assert.Equal(WebDriverBiDiLogLevel.Error, transport.LogLevel);
    }

    [Theory]
    [InlineData(WebDriverBiDiLogLevel.Trace, false)]
    [InlineData(WebDriverBiDiLogLevel.Debug, false)]
    [InlineData(WebDriverBiDiLogLevel.Info, true)]
    [InlineData(WebDriverBiDiLogLevel.Warn, true)]
    [InlineData(WebDriverBiDiLogLevel.Error, true)]
    [InlineData(WebDriverBiDiLogLevel.Fatal, true)]
    public async Task TestMessagesBelowTheMinimumLevelAreNotRaised(WebDriverBiDiLogLevel level, bool expectRaised)
    {
        List<LogMessageEventArgs> received = [];
        TestWebSocketConnection connection = new();
        connection.OnLogMessage.AddObserver(e =>
        {
            received.Add(e);
            return Task.CompletedTask;
        });

        await connection.RaiseFilteredLogMessageAsync("message", level);
        Assert.Equal(expectRaised, received.Count == 1);
    }

    [Fact]
    public async Task TestRaisingTheLevelDeliversTheLowerLevels()
    {
        List<LogMessageEventArgs> received = [];
        TestWebSocketConnection connection = new();
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            received.Add(e);
            return Task.CompletedTask;
        });

        await connection.RaiseFilteredLogMessageAsync("trace", WebDriverBiDiLogLevel.Trace);
        await connection.RaiseFilteredLogMessageAsync("debug", WebDriverBiDiLogLevel.Debug);
        Assert.Equal(2, received.Count);
    }

    [Fact]
    public async Task TestOffSuppressesEveryMessageIncludingFatal()
    {
        List<LogMessageEventArgs> received = [];
        TestWebSocketConnection connection = new();
        connection.LogLevel = WebDriverBiDiLogLevel.Off;
        connection.OnLogMessage.AddObserver(e =>
        {
            received.Add(e);
            return Task.CompletedTask;
        });

        foreach (WebDriverBiDiLogLevel level in Enum.GetValues<WebDriverBiDiLogLevel>())
        {
            // Off is included: it is suppressed as a message level in its own right, so the sweep no
            // longer has to step around it.
            await connection.RaiseFilteredLogMessageAsync("message", level);
        }

        Assert.Empty(received);
    }

    [Fact]
    public void TestIsLogLevelEnabledRequiresBothAnObserverAndAnEnabledLevel()
    {
        TestWebSocketConnection connection = new();

        // No observer: nothing would be raised whatever the level.
        Assert.False(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Fatal));

        connection.OnLogMessage.AddObserver(e => Task.CompletedTask);
        Assert.False(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Trace));
        Assert.False(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug));
        Assert.True(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Info));

        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        Assert.True(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Trace));

        connection.LogLevel = WebDriverBiDiLogLevel.Off;
        Assert.False(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Fatal));
    }

    [Fact]
    public void TestTransportIsLogLevelEnabledFollowsTheSharedLevel()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        // The transport always has an observer once a driver wraps it, so the level is what decides.
        transport.OnLogMessage.AddObserver(e => Task.CompletedTask);
        Assert.False(transport.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug));
        Assert.True(transport.IsLogLevelEnabled(WebDriverBiDiLogLevel.Info));

        connection.LogLevel = WebDriverBiDiLogLevel.Debug;
        Assert.True(transport.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug));
    }

    [Fact]
    public void TestIsLogLevelEnabledAgreesAcrossTheThreeLayers()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(1), transport);

        // The transport observes the connection and the driver observes the transport, so those two
        // layers already have an observer; adding one to the driver completes the chain. With every
        // layer observed, the shared level alone decides, and all three must answer identically.
        driver.OnLogMessage.AddObserver(e => Task.CompletedTask);

        foreach (WebDriverBiDiLogLevel level in Enum.GetValues<WebDriverBiDiLogLevel>())
        {
            Assert.Equal(connection.IsLogLevelEnabled(level), transport.IsLogLevelEnabled(level));
            Assert.Equal(transport.IsLogLevelEnabled(level), driver.IsLogLevelEnabled(level));
        }

        driver.LogLevel = WebDriverBiDiLogLevel.Trace;
        Assert.True(driver.IsLogLevelEnabled(WebDriverBiDiLogLevel.Trace));
        Assert.True(transport.IsLogLevelEnabled(WebDriverBiDiLogLevel.Trace));
        Assert.True(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Trace));

        driver.LogLevel = WebDriverBiDiLogLevel.Off;
        Assert.False(driver.IsLogLevelEnabled(WebDriverBiDiLogLevel.Fatal));
        Assert.False(transport.IsLogLevelEnabled(WebDriverBiDiLogLevel.Fatal));
        Assert.False(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Fatal));
    }

    [Fact]
    public async Task TestProtocolTrafficIsNotLoggedAtTheDefaultLevel()
    {
        // The SEND message is composed by decoding the whole payload, so not raising it at the default
        // level is what keeps that decode from happening for every message.
        List<LogMessageEventArgs> received = [];
        TestWebSocketConnection connection = CreateSendableConnection(received);

        await connection.SendDataAsync(Encoding.UTF8.GetBytes("{\"id\":1}"), TestContext.Current.CancellationToken);
        Assert.Equal("{\"id\":1}", connection.DataSent);
        Assert.DoesNotContain(received, log => log.Message.Contains("SEND >>>"));
    }

    [Fact]
    public async Task TestProtocolTrafficIsLoggedWhenTheLevelIsRaisedToTrace()
    {
        List<LogMessageEventArgs> received = [];
        TestWebSocketConnection connection = CreateSendableConnection(received);
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;

        await connection.SendDataAsync(Encoding.UTF8.GetBytes("{\"id\":1}"), TestContext.Current.CancellationToken);
        LogMessageEventArgs trafficLog = Assert.Single(received, log => log.Message.Contains("SEND >>>"));
        Assert.Equal(WebDriverBiDiLogLevel.Trace, trafficLog.Level);
        Assert.Contains("{\"id\":1}", trafficLog.Message);
    }

    [Fact]
    public async Task TestTransportLogsAnErrorResponseForACommandAtDebugLevel()
    {
        TaskCompletionSource logged = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.LogLevel = WebDriverBiDiLogLevel.Debug;
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            if (e.Message.Contains("Received error response"))
            {
                logged.TrySetResult();
            }

            return Task.CompletedTask;
        });

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        string json = """
                      {
                        "type": "error",
                        "id": 1,
                        "error": "unknown command",
                        "message": "This is a test error message"
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await logged.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        LogMessageEventArgs log = Assert.Single(logs, entry => entry.Message.Contains("Received error response"));
        Assert.Equal(WebDriverBiDiLogLevel.Debug, log.Level);
        Assert.Contains("module.command", log.Message);
    }

    [Fact]
    public async Task TestTransportLogsAReceivedEventAtDebugLevel()
    {
        TaskCompletionSource logged = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.LogLevel = WebDriverBiDiLogLevel.Debug;
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            if (e.Message.Contains("Received event"))
            {
                logged.TrySetResult();
            }

            return Task.CompletedTask;
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);
        await logged.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        LogMessageEventArgs log = Assert.Single(logs, entry => entry.Message.Contains("Received event"));
        Assert.Equal(WebDriverBiDiLogLevel.Debug, log.Level);
        Assert.Contains("protocol.event", log.Message);
    }

    [Fact]
    public async Task TestDriverDoesNotRaiseItsOwnMessagesBelowTheLevel()
    {
        // The driver logs at Warn when disposal cannot stop cleanly. With the level at Off, not even
        // that is raised.
        List<LogMessageEventArgs> received = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection) { ThrowOnDisconnect = true };
        BiDiDriver driver = new(TimeSpan.FromSeconds(1), transport);
        driver.LogLevel = WebDriverBiDiLogLevel.Off;
        driver.OnLogMessage.AddObserver(e =>
        {
            received.Add(e);
            return Task.CompletedTask;
        });

        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        Assert.Empty(received);
    }

    /// <summary>
    /// Creates a connection that routes a send through <see cref="Connection.SendDataAsync"/>, where the
    /// traffic message is composed. The test double otherwise short-circuits to the derived send, which
    /// would skip that code entirely and let a traffic assertion pass without exercising anything.
    /// </summary>
    /// <param name="received">The list to which log messages are recorded.</param>
    /// <returns>The connection.</returns>
    private static TestWebSocketConnection CreateSendableConnection(List<LogMessageEventArgs> received)
    {
        TestWebSocketConnection connection = new() { BypassStart = false, IsActiveOverride = () => true };
        connection.OnLogMessage.AddObserver(e =>
        {
            received.Add(e);
            return Task.CompletedTask;
        });
        return connection;
    }

    [Fact]
    public void TestACustomTransportCanKeepItsOwnLogLevel()
    {
        TestWebSocketConnection connection = new();
        OwnLevelTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(1), transport);
        transport.OnLogMessage.AddObserver(e => Task.CompletedTask);

        transport.LogLevel = WebDriverBiDiLogLevel.Trace;

        // The override keeps its own storage, so the connection is left where it was.
        Assert.Equal(WebDriverBiDiLogLevel.Trace, transport.LogLevel);
        Assert.Equal(WebDriverBiDiLogLevel.Info, connection.LogLevel);

        // The transport's own filtering follows the override, and so does the driver, which reads the
        // property virtually.
        Assert.True(transport.IsLogLevelEnabled(WebDriverBiDiLogLevel.Trace));
        Assert.Equal(WebDriverBiDiLogLevel.Trace, driver.LogLevel);

        // The connection still filters its own messages, the protocol traffic among them, by its own
        // level, which is the decoupling a derived transport takes on by overriding.
        Assert.False(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Trace));
    }

    [Theory]
    [InlineData(WebDriverBiDiLogLevel.Trace)]
    [InlineData(WebDriverBiDiLogLevel.Debug)]
    [InlineData(WebDriverBiDiLogLevel.Info)]
    [InlineData(WebDriverBiDiLogLevel.Warn)]
    [InlineData(WebDriverBiDiLogLevel.Error)]
    [InlineData(WebDriverBiDiLogLevel.Fatal)]
    [InlineData(WebDriverBiDiLogLevel.Off)]
    public void TestOffIsNeverAnEnabledLevelAtAnySetting(WebDriverBiDiLogLevel setting)
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(1), transport);
        driver.OnLogMessage.AddObserver(e => Task.CompletedTask);
        driver.LogLevel = setting;

        // Off is the highest value, so a bare "level >= this.LogLevel" test would report it enabled
        // against every setting, Off included. It is a filter setting, not a message level.
        Assert.False(connection.IsLogLevelEnabled(WebDriverBiDiLogLevel.Off));
        Assert.False(transport.IsLogLevelEnabled(WebDriverBiDiLogLevel.Off));
        Assert.False(driver.IsLogLevelEnabled(WebDriverBiDiLogLevel.Off));
    }

    [Fact]
    public async Task TestTheConnectionNeverRaisesAMessageAtOff()
    {
        List<LogMessageEventArgs> received = [];
        TestWebSocketConnection connection = new();

        // Trace is the most permissive setting there is; if anything would let an Off message through,
        // this would.
        connection.LogLevel = WebDriverBiDiLogLevel.Trace;
        connection.OnLogMessage.AddObserver(e =>
        {
            received.Add(e);
            return Task.CompletedTask;
        });

        await connection.RaiseFilteredLogMessageAsync("message", WebDriverBiDiLogLevel.Off);
        Assert.Empty(received);

        // The observer is wired correctly; it is the level that was rejected.
        await connection.RaiseFilteredLogMessageAsync("message", WebDriverBiDiLogLevel.Trace);
        Assert.Single(received);
    }

    [Fact]
    public async Task TestTheDriverNeverRaisesAMessageAtOff()
    {
        List<LogMessageEventArgs> received = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        LoggingDriver driver = new(transport);
        driver.LogLevel = WebDriverBiDiLogLevel.Trace;
        driver.OnLogMessage.AddObserver(e =>
        {
            received.Add(e);
            return Task.CompletedTask;
        });

        await driver.RaiseFilteredLogMessageAsync("message", WebDriverBiDiLogLevel.Off);
        Assert.Empty(received);

        await driver.RaiseFilteredLogMessageAsync("message", WebDriverBiDiLogLevel.Trace);
        Assert.Single(received);
    }

    /// <summary>
    /// A driver that exposes its protected <c>LogAsync</c>, which is the surface through which a derived
    /// driver could otherwise raise a message at a level the library itself never uses.
    /// </summary>
    private sealed class LoggingDriver : BiDiDriver
    {
        public LoggingDriver(Transport transport)
            : base(TimeSpan.FromSeconds(1), transport)
        {
        }

        public async Task RaiseFilteredLogMessageAsync(string message, WebDriverBiDiLogLevel level)
        {
            await this.LogAsync(message, level);
        }
    }

    /// <summary>
    /// A transport that keeps a log level of its own instead of sharing the connection's, which is what
    /// the virtual property on <see cref="Transport"/> exists to allow.
    /// </summary>
    private sealed class OwnLevelTransport : Transport
    {
        public OwnLevelTransport(Connection connection)
            : base(connection)
        {
        }

        public override WebDriverBiDiLogLevel LogLevel { get; set; } = WebDriverBiDiLogLevel.Info;
    }
}
