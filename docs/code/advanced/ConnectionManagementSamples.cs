// <copyright file="ConnectionManagementSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/connection-management.md

#pragma warning disable CS8600, CS8602, CS1591

namespace WebDriverBiDi.Docs.Code.Advanced;

using WebDriverBiDi;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Protocol;

/// <summary>
/// Snippets for connection management documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class ConnectionManagementSamples
{
    /// <summary>
    /// Simple WebSocket connection - BiDiDriver handles connection automatically.
    /// </summary>
    public static async Task SimpleWebSocketConnection()
    {
        #region SimpleWebSocketConnection
        // BiDiDriver handles connection automatically
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");

        // Use driver...

        await driver.StopAsync();
        #endregion
    }

    /// <summary>
    /// Using a browser launcher for local automation. Implement your own launcher.
    /// </summary>
    public static async Task UsingBrowserLauncher()
    {
        #region UsingaBrowserLauncher
        // Launcher manages browser process and connection
        BrowserLauncher launcher = BrowserLauncher.Configure(Browser.Chrome)
            .WithReleaseChannel(BrowserReleaseChannel.Stable)
            .AtAutomaticallyDownloadedLocation()
            .Build();

        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();

        // Create driver with launcher's preconfigured transport
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), launcher.CreateTransport());
        await driver.StartAsync(launcher.WebSocketUrl);

        try
        {
            // Use driver...
        }
        finally
        {
            await driver.StopAsync();
            await launcher.QuitBrowserAsync();
            await launcher.StopAsync();
        }
        #endregion
    }

    public static void DriverTimeout()
    {
        #region Drivertimeout
        // Simpler and sufficient for most cases
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(60)); // Long timeout
        #endregion
    }

    /// <summary>
    /// Connection timeout settings.
    /// </summary>
    public static void TimeoutSettings()
    #region TimeoutSettings
    {
        WebSocketConnection connection = new WebSocketConnection()
        {
            StartupTimeout = TimeSpan.FromSeconds(15),
            ShutdownTimeout = TimeSpan.FromSeconds(10),
            DataTimeout = TimeSpan.FromSeconds(10)
        };
        #endregion
    }

    /// <summary>
    /// Connection buffer size (read-only).
    /// </summary>
    public static void BufferSize(WebSocketConnection connection)
    {
        #region BufferSize
        int bufferSize = connection.BufferSize; // 1048576 bytes (read-only)
        #endregion
    }

    /// <summary>
    /// OnDataReceived event - monitors raw data received from the browser.
    /// </summary>
    public static void OnDataReceivedEvent(WebSocketConnection connection)
    {
        #region OnDataReceivedEvent
        connection.OnDataReceived.AddObserver((ConnectionDataReceivedEventArgs e) =>
        {
            Console.WriteLine($"Received: {e.Data.Length} bytes");
            // e.Data contains the raw byte array
        });
        #endregion
    }

    /// <summary>
    /// OnConnectionError event - monitors connection errors.
    /// </summary>
    public static void OnConnectionErrorEvent(WebSocketConnection connection)
    {
        #region OnConnectionErrorEvent
        connection.OnConnectionError.AddObserver((ConnectionErrorEventArgs e) =>
        {
            Console.WriteLine($"Connection error: {e.Exception.Message}");
            Logger.Error($"Exception: {e.Exception}");
        });
        #endregion
    }

    /// <summary>
    /// OnLogMessage event - internal connection logging.
    /// </summary>
    public static void OnLogMessageEvent(WebSocketConnection connection)
    {
        #region OnLogMessageEvent
        connection.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            Console.WriteLine($"[{e.Level}] {e.ComponentName}: {e.Message}");
        });
        #endregion
    }

    /// <summary>
    /// Valid WebSocket URL requirements.
    /// </summary>
    public static async Task WebSocketUrlRequirements(BiDiDriver driver)
    {
        #region WebSocketURLRequirements
        // ✅ Valid
        await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");
        await driver.StartAsync("wss://remote-host:9222/devtools/browser/abc-123");

        // ❌ Invalid
        await driver.StartAsync("http://localhost:9222");  // Wrong scheme
        await driver.StartAsync("localhost:9222");         // Not absolute
        #endregion
    }

    /// <summary>
    /// Automatic retry during WebSocket startup.
    /// </summary>
    public static void AutomaticRetry()
    {
        #region AutomaticRetry
        // Will retry every 500ms for up to 30 seconds
        WebSocketConnection connection = new WebSocketConnection()
        {
            StartupTimeout = TimeSpan.FromSeconds(30)
        };
        #endregion
    }

    /// <summary>
    /// Check connection state.
    /// </summary>
    public static void ConnectionState(WebSocketConnection connection)
    {
        #region ConnectionState
        if (connection.IsActive)
        {
            // Connection is open and ready
        }
        #endregion
    }

    /// <summary>
    /// Using pipes with a custom launcher. Implement IPipeServerProcessProvider.
    /// </summary>
    public static async Task UsingPipesWithLauncher()
    {
        #region UsingPipeswithLauncher
        BrowserLauncher launcher = BrowserLauncher.Configure(Browser.Chrome)
            .WithReleaseChannel(BrowserReleaseChannel.Stable)
            .AtAutomaticallyDownloadedLocation()
            .WithConnection(ConnectionType.Pipes)
            .Build();

        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();

        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), launcher.CreateTransport());
        await driver.StartAsync("pipes");

        try
        {
            // Use driver...
        }
        finally
        {
            await driver.StopAsync();
            await launcher.QuitBrowserAsync();
            await launcher.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Common connection error handling.
    /// </summary>
    public static async Task ConnectionErrorHandling(BiDiDriver driver, string url)
    {
        #region ConnectionErrorHandling
        try
        {
            await driver.StartAsync(url);
        }
        catch (WebDriverBiDiTimeoutException ex)
        {
            Console.WriteLine($"Connection timeout: {ex.Message}");
            // Browser may not be ready or URL incorrect
        }
        catch (WebDriverBiDiConnectionException ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
            // Connection already in use or invalid state
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Invalid URL: {ex.Message}");
            // URL format is incorrect
        }
        #endregion
    }

    /// <summary>
    /// Monitoring connection health with diagnostics.
    /// </summary>
    public static void MonitoringConnectionHealth(WebSocketConnection connection)
    {
        #region MonitoringConnectionHealth
        bool connectionHealthy = true;

        connection.OnConnectionError.AddObserver((e) =>
        {
            connectionHealthy = false;
            Logger.Error($"Connection error: {e.Exception.Message}");
        });

        // Check before critical operations
        if (!connectionHealthy || !connection.IsActive)
        {
            // Handle error or reconnect
        }
        #endregion
    }

    /// <summary>
    /// Docker/container WebSocket connection.
    /// </summary>
    public static async Task DockerWebSocketConnection()
    {
        #region DockerWebSocketConnection
        // WebSocket to containerized browser
        string url = $"ws://172.17.0.2:9222/devtools/browser/abc-123";
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync(url);
        #endregion
    }

    /// <summary>
    /// Best practice: simplest pattern - let BiDiDriver handle everything.
    /// </summary>
    public static async Task BestPracticeSimplest(string url)
    {
        #region BestPracticeSimplest
        // ✅ Best: Let BiDiDriver handle everything
        BiDiDriver driver1 = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver1.StartAsync(url);

        // ❌ Avoid unless you have a specific need
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection);
        BiDiDriver driver2 = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
        #endregion
    }

    /// <summary>
    /// Best practice: use a browser launcher for local testing.
    /// </summary>
    public static async Task BestPracticeBrowserLauncher()
    {
        #region BestPracticeBrowserLauncher
        // ✅ Recommended for local testing
        BrowserLauncher launcher = BrowserLauncher.Configure(Browser.Chrome)
            .WithReleaseChannel(BrowserReleaseChannel.Stable)
            .AtAutomaticallyDownloadedLocation()
            .Build();

        await launcher.StartAsync();
        await launcher.LaunchBrowserAsync();

        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), launcher.CreateTransport());
        #endregion
    }

    /// <summary>
    /// Best practice: always clean up.
    /// </summary>
    public static async Task BestPracticeCleanup(string url)
    {
        #region BestPracticeCleanup
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        try
        {
            await driver.StartAsync(url);
            // Use driver...
        }
        finally
        {
            if (driver.IsStarted)
            {
                await driver.StopAsync();
            }
        }
        #endregion
    }

    /// <summary>
    /// Best practice: configure timeouts at driver level.
    /// </summary>
    public static void BestPracticeTimeouts()
    {
        #region BestPracticeTimeouts
        // ✅ Preferred: Simple and sufficient
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(60));

        // ❌ Avoid unless needed: More complex
        WebSocketConnection connection = new WebSocketConnection()
        {
            StartupTimeout = TimeSpan.FromSeconds(60),
            DataTimeout = TimeSpan.FromSeconds(60)
        };
        #endregion
    }

    /// <summary>
    /// Custom connection configuration with timeout overrides.
    /// </summary>
    public static async Task CustomConnectionConfiguration(string url)
    {
        #region CustomConnectionConfiguration
        // Create and configure connection (rare need)
        WebSocketConnection connection = new WebSocketConnection()
        {
            StartupTimeout = TimeSpan.FromSeconds(30),  // Very slow environment
            DataTimeout = TimeSpan.FromSeconds(15)       // Slow network
        };

        // Wrap in transport
        Transport transport = new Transport(connection);

        // Create driver with custom transport
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:9222/devtools/browser/abc-123");

        try
        {
            // Use driver...
        }
        finally
        {
            await driver.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Transport.ShutdownTimeout controls how long DisconnectAsync waits for
    /// the message-processing task to drain before proceeding.
    /// </summary>
    public static async Task TransportShutdownTimeout(string url)
    {
        #region TransportShutdownTimeout
        // Advanced: construct the transport explicitly so we can tune Transport.ShutdownTimeout.
        // This is distinct from Connection.ShutdownTimeout (which controls the underlying
        // WebSocket/pipe close handshake). Transport.ShutdownTimeout governs how long
        // DisconnectAsync waits for the incoming-message processing task to complete.
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection)
        {
            // Default is 10 seconds. Reduce for fail-fast behavior in tests, or
            // increase if you have long-running event handlers that should be given
            // more time to finish during shutdown.
            ShutdownTimeout = TimeSpan.FromSeconds(2),
        };

        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync(url);

        try
        {
            // Use driver...
        }
        finally
        {
            // If the message-processing task has not completed within ShutdownTimeout,
            // DisconnectAsync logs a warning and proceeds; messages still in the queue
            // will not be processed, and pending commands are canceled.
            await driver.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Transport.IncomingQueueDepth exposes the current depth of the incoming message
    /// queue. Intended for operator diagnostics.
    /// </summary>
    public static async Task TransportIncomingQueueDepthDiagnostic(string url)
    {
        #region TransportIncomingQueueDepthDiagnostic
        // Construct the transport explicitly so we can read its diagnostic properties.
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection);
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync(url);

        try
        {
            // A persistently growing value indicates that event handlers are not
            // keeping up with the incoming message rate. Consider using
            // ObservableEventHandlerOptions.RunHandlerAsynchronously for I/O-heavy handlers.
            int depth = transport.IncomingQueueDepth;
            if (depth > 100)
            {
                Logger.Warn($"Incoming message queue depth is high: {depth}");
            }
        }
        finally
        {
            await driver.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Transport.PendingCommandCount exposes the number of commands awaiting a
    /// response. Intended for operator diagnostics alongside IncomingQueueDepth.
    /// </summary>
    public static async Task TransportPendingCommandCountDiagnostic(string url)
    {
        #region TransportPendingCommandCountDiagnostic
        WebSocketConnection connection = new WebSocketConnection();
        Transport transport = new Transport(connection);
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync(url);

        try
        {
            // A persistently high value suggests that the remote end is not
            // responding promptly, or that a burst of commands is in flight.
            int pending = transport.PendingCommandCount;
            if (pending > 50)
            {
                Logger.Warn($"Pending command count is high: {pending}");
            }
        }
        finally
        {
            await driver.StopAsync();
        }
        #endregion
    }

    internal static class Logger
    {
        public static void Error(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }

        public static void Warn(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }
    }
}

/// <summary>
/// Example browser launcher - implement your own to manage browser process and connection.
/// </summary>
public class ExampleBrowserLauncher
{
    public string WebSocketUrl { get; set; } = "ws://localhost:9222/devtools/browser/abc-123";

    public Transport CreateTransport() => new Transport(new WebSocketConnection());

    public Task StartAsync() => Task.CompletedTask;

    public Task LaunchBrowserAsync() => Task.CompletedTask;

    public Task QuitBrowserAsync() => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;
}

/// <summary>
/// Example pipe launcher - implement IPipeServerProcessProvider to launch browser with --remote-debugging-pipe.
/// </summary>
public class ExamplePipeLauncher
{
    /// <summary>
    /// Create transport - in real implementation, implement IPipeServerProcessProvider and return new Transport(new PipeConnection(this)).
    /// </summary>
    public Transport CreateTransport() =>
        throw new NotImplementedException("Implement IPipeServerProcessProvider and create PipeConnection with your launcher");

    public Task StartAsync() => Task.CompletedTask;

    public Task LaunchBrowserAsync() => Task.CompletedTask;

    public Task QuitBrowserAsync() => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;
}

/// <summary>
/// Example custom connection implementation for experimental transports.
/// </summary>
#region CustomConnectionImplementation
public class CustomConnection : Connection
{
    public override bool IsActive => /* your logic */ true;

    public override ConnectionType ConnectionType => ConnectionType.WebSocket;

    public override async Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        await LogAsync("Custom connection starting");
        // Your startup logic
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await LogAsync("Custom connection stopping");
        // Your shutdown logic
    }

    public override async Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        // Your send logic
    }

    protected override async Task ReceiveDataAsync()
    {
        // Your receive logic
        // Call OnDataReceived.NotifyObserversAsync() with received data
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        // Your cleanup logic
    }
}
#endregion

/// <summary>
/// Custom connection usage.
/// </summary>
public static class CustomConnectionUsage
{
    public static async Task UseCustomConnection(string customConnectionString)
    {
        #region CustomConnectionUsage
        // Usage
        CustomConnection customConnection = new CustomConnection();
        Transport transport = new Transport(customConnection);
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync(customConnectionString);
        #endregion
    }
}
