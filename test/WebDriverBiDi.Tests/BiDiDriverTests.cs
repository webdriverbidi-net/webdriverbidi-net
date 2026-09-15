namespace WebDriverBiDi;

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using PinchHitter;
using TestUtilities;
using WebDriverBiDi.Bluetooth;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.DigitalCredentials;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Speculation;
using WebDriverBiDi.Storage;
using WebDriverBiDi.UserAgentClientHints;
using WebDriverBiDi.WebExtension;

[Collection("EventSourceTests")]
public class BiDiDriverTests
{
    [Fact]
    public async Task TestCanDetermineIsStarted()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true
        };

        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        Assert.False(driver.IsStarted);
        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.True(driver.IsStarted);
        await driver.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(driver.IsStarted);
    }

    [Fact]
    public async Task TestCanExecuteCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        TestCommandResult result = await driver.ExecuteCommandAsync(command, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("command result value", result.Value);
    }

    [Fact]
    public async Task TestCanExecuteCommandWithError()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string errorJson = """
                               {
                                 "type": "error",
                                 "id": 1,
                                 "error": "unknown command", 
                                 "message": "This is a test error message",
                                 "stacktrace": "remote stack trace"
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(errorJson);
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        WebDriverBiDiCommandException? caughtException = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(async () => await driver.ExecuteCommandAsync(command, cancellationToken: TestContext.Current.CancellationToken));
        Assert.NotNull(caughtException);

        Assert.Contains("'unknown command' error executing command module.command: This is a test error message", caughtException.Message);
        Assert.Equal(ErrorCode.UnknownCommand, caughtException.ErrorCode);
        Assert.Equal("unknown command", caughtException.ProtocolErrorType);
        Assert.Equal("This is a test error message", caughtException.ProtocolErrorMessage);
        Assert.Equal("remote stack trace", caughtException.RemoteStackTrace);
        Assert.NotNull(caughtException.ErrorDetails);
        Assert.Equal("unknown command", caughtException.ErrorDetails.ErrorType);
        Assert.Equal(ErrorCode.UnknownCommand, caughtException.ErrorDetails.ErrorCode);
        Assert.Equal("This is a test error message", caughtException.ErrorDetails.ErrorMessage);
        Assert.Equal("remote stack trace", caughtException.ErrorDetails.StackTrace);
    }

    [Fact]
    public async Task TestCanExecuteCommandThatReturnsThrownExceptionThrows()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string exceptionJson = """
                                   {
                                     "type": "success",
                                     "id": 1, 
                                     "noResult": {
                                       "invalid": "unknown command",
                                       "message": "This is a test error message"
                                     }
                                   }
                                   """;
            await connection.RaiseDataReceivedEventAsync(exceptionJson);
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        Assert.Contains("Response did not contain properly formed JSON for response type", (await Assert.ThrowsAnyAsync<WebDriverBiDiSerializationException>(async () => await driver.ExecuteCommandAsync(command, cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestCanExecuteReceiveErrorWithoutCommand()
    {
        ErrorResult? response = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnexpectedErrorReceived.AddObserver(e =>
        {
            response = e.ErrorData;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string errorJson = """
                           {
                             "type": "error",
                             "id": null,
                             "error": "unknown command",
                             "message": "This is a test error message"
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(errorJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(response);

        Assert.Equal(ErrorCode.UnknownCommand, response.ErrorCode);
        Assert.Equal("unknown command", response.ErrorType);
        Assert.Equal("This is a test error message", response.ErrorMessage);
    }

    [Fact]
    public async Task TestCanReceiveKnownEvent()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask);
        driver.OnEventReceived.AddObserver(e =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string eventJson = """
                           {
                             "type": "event",
                             "method": "module.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(eventName, receivedEvent);
        Assert.NotNull(receivedData);
        Assert.IsType<TestEventArgs>(receivedData);

        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.NotNull(convertedData);
        Assert.Equal("paramValue", convertedData.ParamName);
    }

    [Fact]
    public async Task TestRegisteringDuplicateEventThrows()
    {
        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask);
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() => driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask));
        Assert.StartsWith("An event named 'module.event' has already been registered.", exception.Message);
    }

    [Fact]
    public async Task TestRegisteringEventAfterStartingDriverThrows()
    {
        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask));
    }

    [Fact]
    public async Task TestDriverWillProcessPendingMessagesOnStop()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        string eventName = "module.event";
        TaskCompletionSource processingReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            MessageProcessingStarted = () => processingReached.TrySetResult(),
            MessageProcessingGate = () => gate.Task,
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask);
        driver.OnEventReceived.AddObserver(e =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string eventJson = """
                           {
                             "type": "event",
                             "method": "module.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);

        // The event is held inside the processing loop when the stop begins, so the stop must process
        // it before completing; releasing the gate afterwards lets that happen without a timed delay.
        await processingReached.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task stopTask = driver.StopAsync(TestContext.Current.CancellationToken);
        gate.TrySetResult();
        await stopTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(eventName, receivedEvent);
        Assert.NotNull(receivedData);
        Assert.IsType<TestEventArgs>(receivedData);

        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.NotNull(convertedData);
        Assert.Equal("paramValue", convertedData.ParamName);
    }

    [Fact]
    public async Task TestUnregisteredEventRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnknownMessageReceived.AddObserver(e =>
        {
            receivedMessage = e.Message;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string serialized = """
                            {
                              "type": "event",
                              "method": "module.event",
                              "params": {
                                "paramName": "paramValue"
                              }
                            }
                            """;
        await connection.RaiseDataReceivedEventAsync(serialized);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(serialized, receivedMessage);
    }

    [Fact]
    public async Task TestUnconformingDataRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnknownMessageReceived.AddObserver(e =>
        {
            receivedMessage = e.Message;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string serialized = """
                            {
                              "someProperty": "someValue",
                              "params": {
                                "thisMessage": "matches no protocol message"
                              }
                            }
                            """;
        await connection.RaiseDataReceivedEventAsync(serialized);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(serialized, receivedMessage);
    }

    [Fact]
    public async Task TestNotificationOfEventHandlerError()
    {
        EventObserverErrorInfo? errorInfo = null;
        TaskCompletionSource handlerFaultedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource errorReportedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        EventObserver<BrowsingContextEventArgs> browsingContextObserver = driver.BrowsingContext.OnContextCreated.AddObserver(async e =>
        {
            try
            {
                throw new InvalidOperationException("This is a test exception from an event handler");
            }
            finally
            {
                await Task.Yield();
                handlerFaultedTaskCompletionSource.TrySetResult();
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously, "Test observer");

        driver.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            errorInfo = e.ErrorInfo with { };
            errorReportedTaskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string eventJson = """
                           {
                             "type": "event",
                             "method": "browsingContext.contextCreated",
                             "params": {
                               "context": "myContextId",
                               "clientWindow": "myClientWindowId",
                               "url": "http://example.com",
                               "originalOpener": "openerContext",
                               "userContext": "myUserContextId",
                               "children": []
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await handlerFaultedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await errorReportedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(errorInfo);
        Assert.Equal(browsingContextObserver.Id, errorInfo.ObserverId);
        Assert.IsType<InvalidOperationException>(errorInfo.Exception);
        Assert.Equal("browsingContext.contextCreated", errorInfo.ObservableEventName);
        Assert.Equal("Test observer", errorInfo.ObserverDescription);
    }

    [Fact]
    public async Task TestModuleAvailability()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        try
        {

            Assert.IsType<BluetoothModule>(driver.Bluetooth);
            Assert.IsType<BrowserModule>(driver.Browser);
            Assert.IsType<BrowsingContextModule>(driver.BrowsingContext);
            Assert.IsType<DigitalCredentialsModule>(driver.DigitalCredentials);
            Assert.IsType<EmulationModule>(driver.Emulation);
            Assert.IsType<InputModule>(driver.Input);
            Assert.IsType<LogModule>(driver.Log);
            Assert.IsType<NetworkModule>(driver.Network);
            Assert.IsType<PermissionsModule>(driver.Permissions);
            Assert.IsType<ScriptModule>(driver.Script);
            Assert.IsType<SessionModule>(driver.Session);
            Assert.IsType<SpeculationModule>(driver.Speculation);
            Assert.IsType<StorageModule>(driver.Storage);
            Assert.IsType<UserAgentClientHintsModule>(driver.UserAgentClientHints);
            Assert.IsType<WebExtensionModule>(driver.WebExtension);
        }
        finally
        {
            await driver.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task TestCanRegisterModule()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.IsType<TestProtocolModule>(driver.GetModule<TestProtocolModule>("protocol"));
    }

    [Fact]
    public async Task TestRegisteringModuleWithDuplicateNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver, 0, false));
        Assert.StartsWith("A module with the name 'protocol' has already been registered", Assert.ThrowsAny<ArgumentException>(() => driver.RegisterModule(new TestProtocolModule(driver))).Message);
    }

    [Fact]
    public async Task TestRegisteringModuleAfterStartingDriverThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterModule(new TestProtocolModule(driver, 0, false)));
    }

    [Fact]
    public async Task TestGettingInvalidModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Contains("Module 'protocol' is not registered with this driver", Assert.ThrowsAny<ArgumentException>(() => driver.GetModule<TestProtocolModule>("protocol")).Message);
    }

    [Fact]
    public async Task TestGettingInvalidModuleTypeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.Equal("Module 'protocol' is registered with this driver, but the module object is not of type WebDriverBiDi.Session.SessionModule", Assert.ThrowsAny<InvalidCastException>(() => driver.GetModule<SessionModule>("protocol")).Message);
    }

    [Fact]
    public async Task TestReceivingNullValueFromSendingCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("is unexpectedly null", exception.Message);
    }

    [Fact]
    public async Task TestCanceledCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ShouldCancelCommand = true
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Contains("was canceled before a result was received", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestUncompletedCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnUncompletedCommand = true
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Contains("is unexpectedly null", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestExecutingCommandWillThrowWhenTimeout()
    {
        await using BiDiDriver driver = new(TimeSpan.Zero, new Transport(new TestWebSocketConnection()));
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Contains("Timed out executing command test.command", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestTimedOutCommandIgnoresLateResponse()
    {
        // A response arriving after the command timed out must be discarded quietly: it must
        // not be reported as an unknown message, and with Terminate behavior configured it must
        // not terminate the transport, so the next command runs (and here simply times out
        // again, because the test connection never answers it).
        TaskCompletionSource discardedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool unknownMessageReceived = false;
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider);
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;
        TimeSpan commandTimeout = TimeSpan.FromSeconds(10);
        await using BiDiDriver driver = new(commandTimeout, transport);
        driver.TransportConfiguration.UnknownMessageBehavior = TransportErrorBehavior.Terminate;
        driver.TransportConfiguration.UnexpectedErrorBehavior = TransportErrorBehavior.Terminate;
        driver.OnUnknownMessageReceived.AddObserver(e => unknownMessageReceived = true);
        driver.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.Contains("Discarding late response"))
            {
                discardedTaskCompletionSource.TrySetResult();
            }
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        // The command timeout is elapsed on the virtual clock as soon as the command arms it.
        Task firstCommandTask = driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(firstCommandTask, commandTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await firstCommandTask);

        string lateResponse = """{"type":"success","id":1,"result":{"parameterName":"parameterValue"}}""";
        await connection.RaiseDataReceivedEventAsync(lateResponse);
        await discardedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.False(unknownMessageReceived);
        Task secondCommandTask = driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(secondCommandTask, commandTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(async () => await secondCommandTask);
    }

    [Fact]
    public async Task TestResponseArrivingBetweenTimeoutAndCancellationIsReturned()
    {
        // The command's wait reports a timeout, but the response lands before the driver
        // cancels the command. Cancellation is then a no-op, and the result must be returned
        // rather than a spurious timeout being thrown.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            ReturnUncompletedCommand = true,
            UncompletedCommandBehavior = command =>
            {
                command.SetResult(new TestCommandResult { Value = "raced in" });
                return false;
            },
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal("raced in", result.Value);
    }

    [Fact]
    public async Task TestReceivingInvalidErrorValueFromSendingCommandThrows()
    {
        TestCommandResult result = new();
        result.SetIsErrorValue(true);
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true,
            CustomReturnValue = result
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal("Could not convert error response from transport for SendCommandAndWait to ErrorResult", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestReceivingInvalidResultTypeFromSendingCommandThrows()
    {
        TestCommandResultInvalid result = new()
        {
            Value = "invalid",
        };

        Assert.Equal("invalid", result.Value);
        Assert.Equal(result, result with { });

        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true,
            CustomReturnValue = result
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal("Could not convert response from transport for SendCommandAndWait to WebDriverBiDi.TestUtilities.TestCommandResult", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestDriverCanEmitLogMessagesFromProtocol()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(100), transport);
        await driver.StartAsync("ws://localhost", TestContext.Current.CancellationToken);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        driver.OnLogMessage.AddObserver(logs.Add);
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);
        Assert.Single(logs);

        Assert.Equal("test log message", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Warn, logs[0].Level);
        // LogMessageEventArgs.Timestamp is DateTime.UtcNow with no TimeProvider seam, so comparing it
        // against another DateTime.UtcNow read taken earlier in the test compares two samples of a
        // clock that can step backwards. What is actually knowable without a seam is that the property
        // was populated and carries the UTC kind its documentation promises.
        Assert.Equal(DateTimeKind.Utc, logs[0].Timestamp.Kind);
        Assert.NotEqual(default, logs[0].Timestamp);
        Assert.Equal(Connection.LoggerComponentName, logs[0].ComponentName);
    }

    [Fact]
    public async Task TestDriverCanUseDefaultTransport()
    {
        TaskCompletionSource connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void ConnectionHandler(ClientConnectionEventArgs e) { connectionTaskCompletionSource.TrySetResult(); }
        static void Handler(ServerDataReceivedEventArgs e) { }
        Server server = new();
        ServerEventObserver<ServerDataReceivedEventArgs> dataReceivedObserver = server.OnDataReceived.AddObserver(Handler);
        server.OnClientConnected.AddObserver(ConnectionHandler);
        await server.StartAsync();

        await using BiDiDriver driver = new();
        Assert.False(driver.IsStarted);
        await driver.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        await connectionTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.True(driver.IsStarted);
        await driver.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(driver.IsStarted);

        await server.StopAsync();
        dataReceivedObserver.Unobserve();
    }

    [Fact]
    public async Task TestDriverRestartAfterReceiveLoopFaultOpensNewSession()
    {
        // A synchronous observer of the connection's own log event that throws propagates into the
        // connection's receive loop and ends it while the socket is still open. The transport tears the
        // session down when the connection reports the error. The documented recovery, StopAsync followed
        // by StartAsync, must then open a new session whose responses are read. Were the connection still
        // reporting itself active, StartAsync would adopt the old socket, which nothing reads any more,
        // and every command sent on the new session would time out.
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        TaskCompletionSource<string> firstClientConnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<string> secondClientConnectedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int clientConnectionCount = 0;
        await using Server server = new();
        server.OnClientConnected.AddObserver(e =>
        {
            TaskCompletionSource<string> connected = Interlocked.Increment(ref clientConnectionCount) == 1
                ? firstClientConnectedTaskCompletionSource
                : secondClientConnectedTaskCompletionSource;
            connected.TrySetResult(e.ConnectionId);
        });
        server.OnDataReceived.AddObserver(async e =>
        {
            // The WebSocket handshake request arrives through this event as well; only command payloads,
            // which are JSON objects, are answered.
            if (string.IsNullOrEmpty(e.Data) || !e.Data.StartsWith("{", StringComparison.Ordinal))
            {
                return;
            }

            using JsonDocument command = JsonDocument.Parse(e.Data);
            long commandId = command.RootElement.GetProperty("id").GetInt64();
            await server.SendWebSocketDataAsync(e.ConnectionId, $$$"""{"type":"success","id":{{{commandId}}},"result":{"ready":true,"message":"ok"}}""");
        });
        await server.StartAsync();

        WebSocketConnection connection = new()
        {
            // The traffic message for a received payload is what the failing observer below reacts to.
            LogLevel = WebDriverBiDiLogLevel.Trace,
        };
        int remainingFailures = 1;
        connection.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.StartsWith("RECV", StringComparison.Ordinal) && Interlocked.Exchange(ref remainingFailures, 0) == 1)
            {
                throw new InvalidOperationException("log observer failure");
            }
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), transport);

        // Added after the transport is constructed, so the transport's own error observer, which tears the
        // session down, has run to completion before this one is notified.
        TaskCompletionSource connectionErrorTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnConnectionError.AddObserver(e =>
        {
            connectionErrorTaskCompletionSource.TrySetResult();
        });

        string connectionString = $"ws://127.0.0.1:{server.Port}";
        await driver.StartAsync(connectionString, testCancellationToken);
        string firstConnectionId = await firstClientConnectedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);

        await server.SendWebSocketDataAsync(firstConnectionId, """{"type":"event","method":"unregistered.event","params":{}}""");
        await connectionErrorTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);
        Assert.False(driver.IsStarted);
        Assert.False(connection.IsActive);

        await driver.StopAsync(testCancellationToken);
        await driver.StartAsync(connectionString, testCancellationToken);
        string secondConnectionId = await secondClientConnectedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);
        Assert.NotEqual(firstConnectionId, secondConnectionId);

        StatusCommandResult status = await driver.Session.StatusAsync(cancellationToken: testCancellationToken);
        Assert.True(status.IsReady);
    }

    [Fact]
    public async Task TestMalformedEventResponseLogsError()
    {
        string connectionId = string.Empty;
        TaskCompletionSource connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void ConnectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionTaskCompletionSource.TrySetResult();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(ConnectionHandler);
        await server.StartAsync();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30));
        driver.TransportConfiguration.ProtocolErrorBehavior = TransportErrorBehavior.Collect;
        driver.TransportConfiguration.UnknownMessageBehavior = TransportErrorBehavior.Collect;

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
            await connectionTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver(e =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logTaskCompletionSource.TrySetResult();
                }
            });

            bool unknownMessageEventRaised = false;
            driver.OnUnknownMessageReceived.AddObserver(e =>
            {
                unknownMessageEventRaised = true;
            });

            // This payload omits the required "timestamp" field, which should cause an exception
            // in parsing.
            string eventJson = """
                               {
                                 "type": "event",
                                 "method": "browsingContext.load",
                                 "params": {
                                   "context": "myContext",
                                   "url": "https://example.com",
                                   "navigation": "myNavigationId"
                                 }
                               }
                               """;
            await server.SendWebSocketDataAsync(connectionId, eventJson);
            await logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Single(driverLog);
            Assert.Contains("Unexpected error parsing event JSON", driverLog[0]);

            // With both categories set to Collect, exactly one collected exception proves
            // the malformed payload of a recognized event was captured once, as a protocol
            // error, and was not additionally reported as an unknown message.
            AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await driver.StopAsync(TestContext.Current.CancellationToken));
            Assert.Single(exception.InnerExceptions);
            Assert.False(unknownMessageEventRaised);
        }
        finally
        {
            await driver.StopAsync(TestContext.Current.CancellationToken);
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task TestMalformedNonCommandErrorResponseLogsError()
    {
        string connectionId = string.Empty;
        TaskCompletionSource connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void ConnectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionTaskCompletionSource.TrySetResult();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(ConnectionHandler);
        await server.StartAsync();
        await using BiDiDriver driver = new();
        driver.TransportConfiguration.ProtocolErrorBehavior = TransportErrorBehavior.Collect;
        driver.TransportConfiguration.UnknownMessageBehavior = TransportErrorBehavior.Collect;

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
            await connectionTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            driver.BrowsingContext.OnLoad.AddObserver(e =>
            {
            });

            TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver(e =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logTaskCompletionSource.TrySetResult();
                }
            });

            bool unknownMessageEventRaised = false;
            driver.OnUnknownMessageReceived.AddObserver(e =>
            {
                unknownMessageEventRaised = true;
            });

            // This payload uses an object for the error field, which should cause an exception
            // in parsing.
            string json = """
                          {
                            "type": "error",
                            "id": null,
                            "error": {
                              "code": "unknown error"
                            },
                            "message": "This is a test error message"
                          }
                          """;
            await server.SendWebSocketDataAsync(connectionId, json);
            await logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            Assert.Single(driverLog);
            Assert.Contains("Unexpected error parsing error JSON", driverLog[0]);

            // With both categories set to Collect, exactly one collected exception proves
            // the malformed-but-recognized error message was captured once, as a protocol
            // error, and was not additionally reported as an unknown message.
            AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await driver.StopAsync(TestContext.Current.CancellationToken));
            Assert.Single(exception.InnerExceptions);
            Assert.False(unknownMessageEventRaised);
        }
        finally
        {
            await driver.StopAsync(TestContext.Current.CancellationToken);
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task TestMalformedIncomingMessageLogsError()
    {
        string connectionId = string.Empty;
        TaskCompletionSource connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void ConnectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionTaskCompletionSource.TrySetResult();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(ConnectionHandler);
        await server.StartAsync();
        await using BiDiDriver driver = new();

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
            await connectionTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver(e =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logTaskCompletionSource.TrySetResult();
                }
            });

            TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            string unknownMessage = string.Empty;
            driver.OnUnknownMessageReceived.AddObserver(e =>
            {
                unknownMessage = e.Message;
                unknownMessageTaskCompletionSource.TrySetResult();
            });

            // This payload uses unparsable JSON, which should cause an exception
            // in parsing.
            string unparsableJson = """
                               {
                                 "type": "error",
                                 "id": null,
                                 { "errorMessage" },
                                 "message": "This is a test error message"
                               }
                               """;
            await server.SendWebSocketDataAsync(connectionId, unparsableJson);
            await Task.WhenAll(
                logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
                unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

            Assert.Single(driverLog);
            Assert.Contains("Unexpected error parsing JSON message", driverLog[0]);
            Assert.NotEmpty(unknownMessage);
        }
        finally
        {
            await driver.StopAsync(TestContext.Current.CancellationToken);
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task TestCanModifyEventHandlerExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Equal(TransportErrorBehavior.Ignore, driver.TransportConfiguration.EventHandlerExceptionBehavior);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        Assert.Equal(TransportErrorBehavior.Collect, transport.EventHandlerExceptionBehavior);
    }

    [Fact]
    public async Task TestCanModifyUnexpectedErrorExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Equal(TransportErrorBehavior.Ignore, driver.TransportConfiguration.UnexpectedErrorBehavior);
        driver.TransportConfiguration.UnexpectedErrorBehavior = TransportErrorBehavior.Collect;
        Assert.Equal(TransportErrorBehavior.Collect, transport.UnexpectedErrorBehavior);
    }

    [Fact]
    public async Task TestCanModifyProtocolErrorExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Equal(TransportErrorBehavior.Ignore, driver.TransportConfiguration.ProtocolErrorBehavior);
        driver.TransportConfiguration.ProtocolErrorBehavior = TransportErrorBehavior.Collect;
        Assert.Equal(TransportErrorBehavior.Collect, transport.ProtocolErrorBehavior);
    }

    [Fact]
    public async Task TestCanModifyUnknownMessageExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Equal(TransportErrorBehavior.Ignore, driver.TransportConfiguration.UnknownMessageBehavior);
        driver.TransportConfiguration.UnknownMessageBehavior = TransportErrorBehavior.Collect;
        Assert.Equal(TransportErrorBehavior.Collect, transport.UnknownMessageBehavior);
    }

    [Fact]
    public async Task TestTransportConfigurationIsTheTransportItself()
    {
        // The driver keeps no copy of these settings: the property hands back the transport, so a value
        // set through either reference is seen through the other.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Same(transport, driver.TransportConfiguration);

        driver.TransportConfiguration.ShutdownTimeout = TimeSpan.FromSeconds(3);
        Assert.Equal(TimeSpan.FromSeconds(3), transport.ShutdownTimeout);

        transport.ConnectionLockTimeout = TimeSpan.FromSeconds(7);
        Assert.Equal(TimeSpan.FromSeconds(7), driver.TransportConfiguration.ConnectionLockTimeout);
    }

    [Fact]
    public async Task TestTransportConfigurationValidatesTimeouts()
    {
        // The validation belongs to the transport, and reaching it through the driver does not bypass it.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Throws<ArgumentOutOfRangeException>(() => driver.TransportConfiguration.ShutdownTimeout = TimeSpan.FromSeconds(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => driver.TransportConfiguration.ConnectionLockTimeout = TimeSpan.FromSeconds(-1));
    }

    [Fact]
    public async Task TestTransportDiagnosticsAreReadableThroughoutTheLifecycle()
    {
        // None of these throws at any point of the lifecycle, which is what makes them safe to poll.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Same(transport, driver.TransportDiagnostics);

        Assert.Equal(TransportState.Disconnected, driver.TransportDiagnostics.State);
        Assert.Equal(0, driver.TransportDiagnostics.IncomingQueueDepth);
        Assert.Equal(0, driver.TransportDiagnostics.PendingCommandCount);

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connected, driver.TransportDiagnostics.State);
        Assert.True(driver.IsStarted);

        await driver.StopAsync(TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Disconnected, driver.TransportDiagnostics.State);
        Assert.False(driver.IsStarted);
        Assert.Equal(0, driver.TransportDiagnostics.PendingCommandCount);
    }

    [Fact]
    public async Task TestCanExecuteParallelCommands()
    {
        // delayCommandInFlight: set by the delay command's handler as soon as it starts,
        // before waiting on the gate. The fast command's handler waits on this first,
        // proving both commands were in-flight simultaneously before either responded.
        // delayResponseGate: held until after Task.WaitAny confirms the fast command
        // finished first, then released so the delay command can respond.
        TaskCompletionSource delayCommandInFlightTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource delayResponseGateTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // Each responder is kept so the test can await it at the end: an exception thrown while
        // building or raising a response would otherwise surface only as the consumer's timeout.
        ConcurrentBag<Task> responderTasks = [];

        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(e =>
        {
            responderTasks.Add(Task.Run(async () =>
            {
                long start = Stopwatch.GetTimestamp();
                if (e.SentCommandName is not null && e.SentCommandName.Contains("delay"))
                {
                    delayCommandInFlightTaskCompletionSource.TrySetResult();
                    await delayResponseGateTaskCompletionSource.Task;
                }
                else
                {
                    await delayCommandInFlightTaskCompletionSource.Task;
                }

                TimeSpan elapsed = Stopwatch.GetElapsedTime(start);
                string eventJson = $$"""
                                   {
                                     "type": "success",
                                     "id": {{e.SentCommandId}},
                                     "result": {
                                       "value": "command result value for {{e.SentCommandName}}",
                                       "elapsed": {{elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)}}
                                     }
                                   }
                                   """;
                await connection.RaiseDataReceivedEventAsync(eventJson);
            }));
            return Task.CompletedTask;
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string delayCommandName = "module.delayCommand";
        TestCommandParameters delayCommand = new(delayCommandName);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);

        Task<TestCommandResult>[] parallelTasks =
        [
            driver.ExecuteCommandAsync(delayCommand, cancellationToken: TestContext.Current.CancellationToken),
            driver.ExecuteCommandAsync(command, cancellationToken: TestContext.Current.CancellationToken),
        ];

        Task<TestCommandResult> firstFinished = await Task.WhenAny(parallelTasks).WaitAsync(TestContext.Current.CancellationToken);
        int indexOfFirstFinishedTask = Array.IndexOf(parallelTasks, firstFinished);
        delayResponseGateTaskCompletionSource.TrySetResult();
        TestCommandResult[] results = await Task.WhenAll(parallelTasks).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(1, indexOfFirstFinishedTask);
        Assert.Equal($"command result value for {delayCommandName}", results[0].Value);
        Assert.Equal($"command result value for {commandName}", results[1].Value);

        await Task.WhenAll(responderTasks);
    }

    [Fact]
    public async Task TestCanDisposeStartedDriver()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanDisposeDriverWithoutStarting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        await driver.DisposeAsync();
    }

    [Fact]
    public async Task TestStopThenDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.StopAsync(TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
    }

    [Fact]
    public async Task TestExecuteCommandAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandWithTimeoutAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanUseAwaitUsingPattern()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        string commandValue = string.Empty;
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        });

        await using (BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport))
        {
            await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
            TestCommandResult result = await driver.ExecuteCommandAsync(new TestCommandParameters("module.command"), cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(result.Value);
            commandValue = result.Value;
        }

        Assert.Equal("command result value", commandValue);
    }

    [Fact]
    public async Task TestRegisteringModuleAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        Assert.ThrowsAny<ObjectDisposedException>(() => driver.RegisterModule(new TestProtocolModule(driver)));
    }

    [Fact]
    public async Task TestGettingModuleAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        driver.RegisterModule(module);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        Assert.ThrowsAny<ObjectDisposedException>(() => driver.GetModule<TestProtocolModule>(module.ModuleName));
    }

    [Fact]
    public async Task TestRegisteringEventAfterDisposeThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        Assert.ThrowsAny<ObjectDisposedException>(() => driver.RegisterEvent("protocol.event", eventInvoker));
    }

    [Fact]
    public async Task TestRegistrationOverridesAreNotCalledWhileBuiltInModulesAreRegistered()
    {
        // The overrides record into fields the derived constructor assigns. The driver's constructor
        // registers the built-in modules and their events. Had it done so through the virtual
        // RegisterModule and RegisterEvent, each override would run from the base constructor, before
        // those fields were assigned, and construction would fail with a NullReferenceException.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using RegistrationRecordingDriver driver = new(transport);

        Assert.Empty(driver.RegisteredModuleNames);
        Assert.Empty(driver.RegisteredEventNames);

        // Bypassing the overrides must not bypass the registrations themselves.
        Assert.Same(driver.Session, driver.GetModule<SessionModule>(SessionModule.SessionModuleName));
        Assert.Same(driver.Log, driver.GetModule<LogModule>(LogModule.LogModuleName));
    }

    [Fact]
    public async Task TestRegistrationOverridesAreCalledForRegistrationsAfterConstruction()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using RegistrationRecordingDriver driver = new(transport);

        // The custom module registers its event from its own constructor, through the driver it was
        // given, and is then registered with the driver. Both go through the public virtual members.
        driver.RegisterModule(new TestProtocolModule(driver));

        Assert.Equal("protocol", Assert.Single(driver.RegisteredModuleNames));
        Assert.Equal("protocol.event", Assert.Single(driver.RegisteredEventNames));
    }

    [Fact]
    public async Task TestBuiltInModuleExecutorForwardsToDriver()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        // Module.Driver is protected and the built-in modules are sealed, so reflection is the only way
        // a test can reach the executor the driver constructed them with.
        PropertyInfo? moduleDriverProperty = typeof(Module).GetProperty("Driver", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(moduleDriverProperty);
        IBiDiModuleHost executor = (IBiDiModuleHost)moduleDriverProperty.GetValue(driver.Session)!;

        Assert.NotSame(driver, executor);

        // Observer faults in the built-in modules' events must reach the driver's pipeline. A delegate
        // bound to the same method on the same target compares equal.
        IEventObserverErrorReporter reporter = Assert.IsAssignableFrom<IEventObserverErrorReporter>(executor);
        Assert.Equal(((IEventObserverErrorReporter)driver).EventObserverErrorReporter, reporter.EventObserverErrorReporter);

        // Before start, the driver rejects a command, which is enough to show that each overload reached it.
        await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await executor.ExecuteCommandAsync(new TestCommandParameters("module.command"), cancellationToken: TestContext.Current.CancellationToken));
        await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await executor.ExecuteCommandAsync<TestCommandResult>((CommandParameters)new TestCommandParameters("module.command"), cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestDisposeDisposesTransport()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.False(transport.IsDisposed);
        await driver.DisposeAsync();
        Assert.True(transport.IsDisposed);
    }

    [Fact]
    public async Task TestDisposeDisposesTransportWithoutStarting()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.False(transport.IsDisposed);
        await driver.DisposeAsync();
        Assert.True(transport.IsDisposed);
    }

    [Fact]
    public async Task TestDisposeLogsExceptionFromStopAsync()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        driver.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
        });
        transport.ThrowOnDisconnect = true;
        await driver.DisposeAsync();
        Assert.Contains(logs,
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated disconnect failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == BiDiDriver.LoggerComponentName);
    }

    [Fact]
    public async Task TestDisposeSurvivesThrowingLogObserver()
    {
        // A synchronously throwing observer of OnLogMessage must not turn the
        // suppress-and-log disposal into a throwing DisposeAsync that skips the
        // disposal of the transport. The observer's failure is instead routed
        // through the observer-error pipeline and surfaced via
        // OnEventHandlerErrorOccurred.
        List<EventHandlerErrorOccurredEventArgs> observerErrors = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        driver.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            observerErrors.Add(e);
        });
        driver.OnLogMessage.AddObserver(e => throw new WebDriverBiDiException("Simulated log observer failure"));
        transport.ThrowOnDisconnect = true;
        await driver.DisposeAsync();

        Assert.True(transport.IsDisposed);
        Assert.Contains(observerErrors,
            e => e.ErrorInfo.ObservableEventName == driver.OnLogMessage.EventName
                 && e.ErrorInfo.Exception.Message.Contains("Simulated log observer failure"));
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverBeforeStarting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverAfterStartingThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        InvalidOperationException exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => driver.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken));
        Assert.Contains("Cannot register a type info resolver after the transport is connected", exception.Message);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(() => driver.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanExecuteCommandWithUntypedCommandParameters()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("command result value", result.Value);
    }

    [Fact]
    public async Task TestCanExecuteCommandWithUntypedCommandParametersAndTimeout()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromMilliseconds(1500), TestContext.Current.CancellationToken);
        Assert.Equal("command result value", result.Value);
    }

    [Fact]
    public async Task TestExecuteCommandWithUntypedParametersAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        CommandParameters command = new TestCommandParameters("test.command");
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandWithUntypedParametersAndTimeoutAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        CommandParameters command = new TestCommandParameters("test.command");
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestStartAsyncThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await driver.StartAsync("ws://localhost:5555", cts.Token));
    }

    [Fact]
    public async Task TestExecuteCommandThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TestCommandParameters commandParams = new("module.command");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await driver.ExecuteCommandAsync(commandParams, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task TestExecuteCommandWithTimeoutThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TestCommandParameters commandParams = new("module.command");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await driver.ExecuteCommandAsync(commandParams, TimeSpan.FromSeconds(5), cts.Token));
    }

    [Fact]
    public async Task TestExecuteCommandCancelsCommandOnCancellation()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(_ =>
        {
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        using CancellationTokenSource cts = new();
        CommandParameters command = new TestCommandParameters("module.command");

        Task<TestCommandResult> executeTask = driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(30), cts.Token);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await executeTask);
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithNullCommandParametersThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(null!, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithNegativeTimeoutThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        CommandParameters command = new TestCommandParameters("module.command");
        await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(-5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithTimeoutExceedingMaximumThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        CommandParameters command = new TestCommandParameters("module.command");
        ArgumentOutOfRangeException exception = await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.MaxValue, TestContext.Current.CancellationToken));
        Assert.Equal("commandTimeout", exception.ParamName);
        Assert.Contains("no greater than", exception.Message);
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithZeroTimeoutDoesNotThrowArgumentOutOfRangeException()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        CommandParameters command = new TestCommandParameters("module.command");
        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.Zero, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithExplicitPositiveTimeoutSucceeds()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        });
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("command result value", result.Value);
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithInfiniteTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        });
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        CommandParameters command = new TestCommandParameters("module.command");
        await driver.ExecuteCommandAsync<TestCommandResult>(command, Timeout.InfiniteTimeSpan, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithNullTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            // force a delay in responding to ensure that the timeout is actually being applied
            await Task.Yield();
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        });
        TestTransport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        CommandParameters command = new TestCommandParameters("module.command");
        await driver.ExecuteCommandAsync<TestCommandResult>(command, null, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestRegisterEventWithNullEventNameThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentException>(() => driver.RegisterEvent(null!, eventInvoker));
    }

    [Fact]
    public async Task TestRegisterEventWithEmptyEventNameThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentException>(() => driver.RegisterEvent(string.Empty, eventInvoker));
    }

    [Fact]
    public async Task TestRegisterEventWithNullEventInvokerThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = null!;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentNullException>(() => driver.RegisterEvent("protocol.event", eventInvoker!));
    }

    [Fact]
    public async Task TestRegisterModuleWithNullModuleThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentNullException>(() => driver.RegisterModule(null!));
    }

    [Fact]
    public async Task TestGetModuleWithNullModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentException>(() => driver.GetModule<TestProtocolModule>(null!));
    }

    [Fact]
    public async Task TestGetModuleWithEmptyModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentException>(() => driver.GetModule<TestProtocolModule>(string.Empty));
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverWithNullResolverThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => driver.RegisterTypeInfoResolverAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCreatingWithNegativeDefaultCommandTimeoutThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new BiDiDriver(TimeSpan.FromSeconds(-1), transport));
    }

    [Fact]
    public void TestCreatingWithDefaultCommandTimeoutExceedingMaximumThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        ArgumentOutOfRangeException exception = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new BiDiDriver(TimeSpan.MaxValue, transport));
        Assert.Equal("defaultCommandWaitTimeout", exception.ParamName);
        Assert.Contains("no greater than", exception.Message);
    }

    [Fact]
    public async Task TestCreatingWithMaximumDefaultCommandTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        TimeSpan maximumTimeout = TimeSpan.FromMilliseconds(uint.MaxValue - 1);
        await using BiDiDriver driver = new(maximumTimeout, transport);
        Assert.Equal(maximumTimeout, driver.DefaultCommandTimeout);
    }

    [Fact]
    public async Task TestCreatingWithZeroDefaultCommandTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.Zero, transport);
        Assert.Equal(TimeSpan.Zero, driver.DefaultCommandTimeout);
    }

    [Fact]
    public async Task TestCreatingWithInfiniteDefaultCommandTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(Timeout.InfiniteTimeSpan, transport);
        Assert.Equal(Timeout.InfiniteTimeSpan, driver.DefaultCommandTimeout);
    }

    [Fact]
    public async Task TestCreatingWithNullTransportThrows()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => _ = new BiDiDriver(TimeSpan.FromSeconds(1), null!));
    }

    [Fact]
    public async Task TestCreatingWithConnectedTransportThrows()
    {
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() => _ = new BiDiDriver(TimeSpan.FromSeconds(1), transport));
        Assert.Contains("must be disconnected", exception.Message);
    }

    [Fact]
    public async Task TestExecuteCommandAsyncRoutesOutOfOrderResponsesByCommandId()
    {
        const int callerCount = 3;

        Dictionary<long, string> commandIdToSenderValue = [];
        TaskCompletionSource sendCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(e =>
        {
            JsonDocument document = JsonDocument.Parse(connection.DataSent ??= string.Empty);
            string senderValue = document.RootElement.GetProperty("params").GetProperty("parameterName").GetString() ?? string.Empty;
            commandIdToSenderValue[e.SentCommandId] = senderValue;
            sendCompleted.TrySetResult();
            return Task.CompletedTask;
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(Timeout.InfiniteTimeSpan, transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        Task<TestCommandResult>[] senderTasks = new Task<TestCommandResult>[callerCount];
        string[] expectedValues = new string[callerCount];
        for (int i = 0; i < callerCount; i++)
        {
            sendCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            string senderValue = $"sender-{i}";
            expectedValues[i] = senderValue;
            senderTasks[i] = Task.Run(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("module.command", senderValue), cancellationToken: TestContext.Current.CancellationToken));
            await sendCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }

        Assert.Equal(callerCount, transport.PendingCommandCount);

        List<long> commandIdsInSendOrder = [.. commandIdToSenderValue.Keys];
        commandIdsInSendOrder.Sort();

        for (int i = commandIdsInSendOrder.Count - 1; i >= 0; i--)
        {
            long commandId = commandIdsInSendOrder[i];
            string senderValue = commandIdToSenderValue[commandId];
            string responseJson = $$$"""{"type":"success","id":{{{commandId}}},"result":{"value":"{{{senderValue}}}"}}""";
            await connection.RaiseDataReceivedEventAsync(responseJson);
        }

        TestCommandResult[] results = await Task.WhenAll(senderTasks);
        Assert.Equal(0, transport.PendingCommandCount);

        for (int i = 0; i < callerCount; i++)
        {
            Assert.Equal(expectedValues[i], results[i].Value);
        }

        await driver.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestMidCommandRemoteDisconnectFaultsExecuteCommandWithConnectionException()
    {
        // Verify that a remote disconnect while a command is pending causes
        // BiDiDriver.ExecuteCommandAsync to fault with WebDriverBiDiConnectionException
        // promptly, rather than hanging until the command timeout expires. This is the
        // driver-level passthrough of the transport behavior covered by
        // TransportTests.TestRemoteDisconnectFailsPendingCommands.
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        Transport transport = new(connection);
        // Large default timeout: if the disconnect path were broken and the
        // command sat in the pending collection, the test would hang until this
        // elapsed. The assertion timeout below is much shorter, so a broken
        // path fails fast rather than timing out the whole suite.
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        TestCommandParameters command = new("module.command");
        Task<TestCommandResult> executeTask = Task.Run(() => driver.ExecuteCommandAsync(command, cancellationToken: TestContext.Current.CancellationToken));

        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        WebDriverBiDiConnectionException? caught = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(async () =>
        {
            Task completed = await Task.WhenAny(executeTask, Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken));
            Assert.Same(executeTask, completed);
            await executeTask;
        });
        Assert.Contains("Remote end closed the connection", caught.Message);
    }

    [Fact]
    public async Task TestRacedCapturedTaskFaultIsReportedViaEventHandlerError()
    {
        // A handler task that races into the capture buffer after WaitForAsync collected
        // its Nth task (but before TryComplete closes the writer) has
        // ShouldReportAsyncFault = false on its original continuation. The drain in
        // WaitForAsync must attach a new reporting continuation so the fault surfaces
        // via OnEventHandlerErrorOccurred instead of being silently swallowed.
        EventObserverErrorInfo? errorInfo = null;
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource allowFaultTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource errorReportedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        driver.BrowsingContext.OnContextCreated.AddObserver(
            async (BrowsingContextEventArgs e) =>
            {
                if (e.BrowsingContextId == "racedContextId")
                {
                    handlerStartedTaskCompletionSource.TrySetResult();
                    await allowFaultTaskCompletionSource.Task;
                    throw new InvalidOperationException("raced task fault");
                }
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        driver.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            errorInfo = e.ErrorInfo with { };
            errorReportedTaskCompletionSource.TrySetResult();
        });

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string collectedEventJson = """
            {
              "type": "event",
              "method": "browsingContext.contextCreated",
              "params": {
                "context": "collectedContextId",
                "clientWindow": "myClientWindowId",
                "url": "http://example.com",
                "originalOpener": null,
                "userContext": "default",
                "children": []
              }
            }
            """;

        string racedEventJson = """
            {
              "type": "event",
              "method": "browsingContext.contextCreated",
              "params": {
                "context": "racedContextId",
                "clientWindow": "myClientWindowId",
                "url": "http://example.com",
                "originalOpener": null,
                "userContext": "default",
                "children": []
              }
            }
            """;

        EventObserver<BrowsingContextEventArgs> observer = driver.BrowsingContext.OnContextCreated.AddObserver(
            _ => { },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();

        // Deliver the event that WaitForAsync(1) will collect.
        await connection.RaiseDataReceivedEventAsync(collectedEventJson);
        // Deliver the raced event — its handler starts and blocks.
        await connection.RaiseDataReceivedEventAsync(racedEventJson);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // WaitForAsync collects 1 task, auto-closes, drains the raced task.
        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        _ = Assert.Single(tasks);

        // Let the raced handler fault.
        allowFaultTaskCompletionSource.TrySetResult();
        await errorReportedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.NotNull(errorInfo);
        InvalidOperationException ex = Assert.IsType<InvalidOperationException>(errorInfo.Exception);
        Assert.Equal("raced task fault", ex.Message);
        Assert.Equal("browsingContext.contextCreated", errorInfo.ObservableEventName);
    }

    [Fact]
    public async Task TestRegisteringModuleWhileStartIsInProgressThrows()
    {
        // ConnectAsync publishes the transport's Connecting state before it opens the connection.
        // The connection's StartBarrier holds that open so that the registration attempt happens
        // while the driver is still starting (IsStarted is false, because the transport has not
        // yet finished connecting).
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
        };
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        Task startTask = driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.False(driver.IsStarted);
        InvalidOperationException exception = Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterModule(new TestProtocolModule(driver, 0, false)));
        Assert.Equal("Cannot register a module after the driver has started", exception.Message);

        startBarrier.SetResult();
        await startTask;
        Assert.True(driver.IsStarted);
    }

    [Fact]
    public async Task TestRegisteringEventWhileStartIsInProgressThrows()
    {
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
        };
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        Task startTask = driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.False(driver.IsStarted);
        InvalidOperationException exception = Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterEvent<TestEventArgs>("module.event", (e) => Task.CompletedTask));
        Assert.Equal("Cannot register an event after the driver has started", exception.Message);

        startBarrier.SetResult();
        await startTask;
        Assert.True(driver.IsStarted);
    }

    [Fact]
    public async Task TestRegistrationCannotInterleaveWithConnectStateTransition()
    {
        // A registration and the publication of the transport's Connecting state are performed under
        // one and the same lock, so a registration that is under way cannot be overtaken by a connect
        // beginning on another thread. The module's name is read while that lock is held, so the hook
        // below runs at exactly the point a test needs to observe: the connect has been let all the
        // way up to the instant before it would publish, and the state sampled from inside the
        // registration must still be Disconnected. If the two could interleave, the sample could
        // observe Connecting instead, and the registration would land against a transport that was
        // no longer idle.
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        TaskCompletionSource registrationHoldsLock = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource connectReadyToPublish = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseRegistration = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            // Fires once the connect owns the connection semaphore, which is the step immediately
            // before it publishes the Connecting state. Waiting for it removes any dependence on
            // how quickly the connect thread is scheduled.
            AfterAcquireLockCallback = () => connectReadyToPublish.TrySetResult(),
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        // Seeded with a value the assertion would reject, so a hook that never runs fails the test
        // rather than passing by default.
        TransportState stateObservedDuringRegistration = TransportState.Connected;
        RegistrationHookModule module = new(driver, "hookedModule", () =>
        {
            registrationHoldsLock.TrySetResult();
            releaseRegistration.Task.GetAwaiter().GetResult();
            stateObservedDuringRegistration = transport.State;
        });

        Task registrationTask = Task.Run(() => driver.RegisterModule(module), cancellationToken);
        await registrationHoldsLock.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        Task startTask = Task.Run(() => driver.StartAsync("ws://localhost:5555", cancellationToken), cancellationToken);
        await connectReadyToPublish.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        releaseRegistration.SetResult();
        await registrationTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        await startTask.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        Assert.Equal(TransportState.Disconnected, stateObservedDuringRegistration);

        // The registration completed in full despite the concurrent connect, and the connect went on
        // to succeed once the registration released the lock.
        Assert.IsType<RegistrationHookModule>(driver.GetModule<RegistrationHookModule>("hookedModule"));
        Assert.True(driver.IsStarted);
    }

    [Fact]
    public async Task TestRegistrationIsAllowedAgainAfterFailedStart()
    {
        // With BypassStart disabled, the real WebSocketConnection.StartAsync rejects a
        // non-WebSocket scheme, so ConnectAsync throws and the driver must re-open
        // registration rather than leaving the instance permanently locked.
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
        };
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        await Assert.ThrowsAnyAsync<ArgumentException>(() => driver.StartAsync("http://localhost:5555", TestContext.Current.CancellationToken));
        Assert.False(driver.IsStarted);

        driver.RegisterModule(new TestProtocolModule(driver, 0, false));
        driver.RegisterEvent<TestEventArgs>("module.event", (e) => Task.CompletedTask);
        Assert.IsType<TestProtocolModule>(driver.GetModule<TestProtocolModule>("protocol"));
    }

    [Fact]
    public async Task TestRegistrationIsAllowedAgainAfterStop()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterModule(new TestProtocolModule(driver, 0, false)));

        await driver.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(driver.IsStarted);

        driver.RegisterModule(new TestProtocolModule(driver, 0, false));
        driver.RegisterEvent<TestEventArgs>("module.event", (e) => Task.CompletedTask);

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.True(driver.IsStarted);
        Assert.IsType<TestProtocolModule>(driver.GetModule<TestProtocolModule>("protocol"));
    }

    [Fact]
    public async Task TestRegistrationIsAllowedAgainAfterStopThrowsCollectedErrors()
    {
        // With a Collect-mode error behavior, StopAsync surfaces the collected errors by
        // throwing an AggregateException after the transport teardown has fully completed.
        // The driver must still end up stopped on that path: IsStarted false and
        // registration legal again.
        TaskCompletionSource eventReceivedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        driver.RegisterEvent<TestEventArgs>("module.event", (e) => Task.CompletedTask);
        driver.OnEventReceived.AddObserver(e =>
        {
            eventReceivedTaskCompletionSource.TrySetResult();
            throw new WebDriverBiDiException("This is an unexpected exception");
        });

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        string json = """
                      {
                        "type": "event",
                        "method": "module.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);
        await eventReceivedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(() => driver.StopAsync(TestContext.Current.CancellationToken));
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.False(driver.IsStarted);

        driver.RegisterModule(new TestProtocolModule(driver, 0, false));
        driver.RegisterEvent<TestEventArgs>("module.otherEvent", (e) => Task.CompletedTask);
        Assert.IsType<TestProtocolModule>(driver.GetModule<TestProtocolModule>("protocol"));
    }

    [Fact]
    public async Task TestAsyncFaultingObserverOfDriverEventHandlerErrorOccurredDoesNotCauseFeedbackLoop()
    {
        // The driver forwards the transport's error-occurred event into its own
        // OnEventHandlerErrorOccurred, so a failure in an observer of the driver's error
        // event must be captured without re-raising; re-raising would re-invoke the same
        // failing observer again in an unbounded feedback loop of error events.
        int errorObserverInvocationCount = 0;
        int capturedErrorCount = 0;
        TaskCompletionSource secondCaptureTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            AfterUnhandledErrorCaptured = () =>
            {
                if (Interlocked.Increment(ref capturedErrorCount) == 2)
                {
                    secondCaptureTaskCompletionSource.TrySetResult();
                }
            },
        };
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        driver.RegisterEvent<TestEventArgs>("module.event", (e) => Task.CompletedTask);
        driver.OnEventReceived.AddObserver(
            async e =>
            {
                await Task.Yield();
                throw new WebDriverBiDiException("original handler failure");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        driver.OnEventHandlerErrorOccurred.AddObserver(
            async e =>
            {
                Interlocked.Increment(ref errorObserverInvocationCount);
                await Task.Yield();
                throw new WebDriverBiDiException("error observer failure");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        string json = """
                      {
                        "type": "event",
                        "method": "module.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);
        await secondCaptureTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(1, Volatile.Read(ref errorObserverInvocationCount));
        Assert.Equal(2, Volatile.Read(ref capturedErrorCount));

        // A feedback loop would keep re-invoking the error observer and capturing further errors,
        // every one of which would surface as an extra inner exception on stop; the exact count
        // below is the deterministic negative check.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(() => driver.StopAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("original handler failure"));
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("error observer failure"));
    }

    [Fact]
    public async Task TestFaultingModuleEventDispatchStillNotifiesEventReceivedObservers()
    {
        // The module-level event dispatch and the driver-level OnEventReceived dispatch are
        // independent: a synchronous fault in the module dispatch must not prevent
        // OnEventReceived observers from being notified, and the fault is still surfaced
        // through EventHandlerExceptionBehavior.
        TaskCompletionSource eventReceivedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        driver.RegisterEvent<TestEventArgs>("module.event", (e) => throw new WebDriverBiDiException("module dispatch failure"));
        driver.OnEventReceived.AddObserver(e => eventReceivedTaskCompletionSource.TrySetResult());

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        string json = """
                      {
                        "type": "event",
                        "method": "module.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);

        // The OnEventReceived observer completing proves the second dispatch stage ran even
        // though the module dispatch faulted.
        await eventReceivedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(() => driver.StopAsync(TestContext.Current.CancellationToken));
        Assert.Contains(exception.Flatten().InnerExceptions, e => e.Message.Contains("module dispatch failure"));
    }

    [Fact]
    public async Task TestFaultsInBothEventDispatchStagesAreBothSurfaced()
    {
        // When both the module-level dispatch and the driver-level OnEventReceived dispatch
        // fault for the same event, both faults are surfaced together rather than one masking
        // the other.
        TaskCompletionSource eventReceivedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        driver.RegisterEvent<TestEventArgs>("module.event", (e) => throw new WebDriverBiDiException("module dispatch failure"));
        driver.OnEventReceived.AddObserver(e =>
        {
            eventReceivedTaskCompletionSource.TrySetResult();
            throw new WebDriverBiDiException("event received observer failure");
        });

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        string json = """
                      {
                        "type": "event",
                        "method": "module.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);
        await eventReceivedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(() => driver.StopAsync(TestContext.Current.CancellationToken));
        AggregateException flattened = exception.Flatten();
        Assert.Contains(flattened.InnerExceptions, e => e.Message.Contains("module dispatch failure"));
        Assert.Contains(flattened.InnerExceptions, e => e.Message.Contains("event received observer failure"));
    }

    [Fact]
    public async Task TestRegistrationIsRejectedWhenTransportWasConnectedExternally()
    {
        // The driver never observed StartAsync, so its start-requested flag is false; the
        // IsStarted check (backed by Transport.IsConnected) must still refuse registration.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.True(driver.IsStarted);

        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterModule(new TestProtocolModule(driver, 0, false)));
        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterEvent<TestEventArgs>("module.event", (e) => Task.CompletedTask));
    }

    [Fact]
    public async Task TestConcurrentStopDuringInFlightStartDoesNotReopenRegistration()
    {
        // CC-2: while a StartAsync is in flight (its ConnectAsync has not yet marked the transport
        // connected), a racing StopAsync must not clear the start-requested flag. Clearing it would
        // wrongly re-open module and event registration for the remainder of that start, even though
        // the driver is starting. The start barrier holds the connection's StartAsync open inside
        // ConnectAsync, reproducing that in-flight window deterministically.
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
        };
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        // StartAsync sets the start-requested flag synchronously, then blocks inside ConnectAsync on
        // the start barrier before the transport is marked connected.
        Task startTask = driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        // A concurrent stop, running while the transport is not yet connected, must leave the
        // in-flight start's flag untouched rather than treating the not-connected transport as a
        // completed teardown.
        await driver.StopAsync(TestContext.Current.CancellationToken);

        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterModule(new TestProtocolModule(driver, 0, false)));
        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterEvent<TestEventArgs>("module.event", (e) => Task.CompletedTask));

        // Releasing the barrier lets the in-flight start complete normally.
        startBarrier.TrySetResult();
        await startTask;
        Assert.True(driver.IsStarted);
    }

    [Fact]
    public async Task TestUnserializableCommandParametersThrowSerializationException()
    {
        // A NaN pressure cannot be represented in JSON. The failure happens while serializing the
        // outbound command and must surface as the library's serialization exception rather than
        // a raw System.Text.Json exception, mirroring the handling of unreadable responses.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        Input.PerformActionsCommandParameters parameters = new("myContext");
        Input.PointerSourceActions pointer = new();
        pointer.Actions.Add(new Input.PointerDownAction(0) { Pressure = double.NaN });
        parameters.Actions.Add(pointer);

        WebDriverBiDiSerializationException exception = await Assert.ThrowsAsync<WebDriverBiDiSerializationException>(
            async () => await driver.Input.PerformActionsAsync(parameters, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Could not serialize command 'input.performActions'", exception.Message);
        Assert.IsType<JsonException>(exception.InnerException);
    }

    [Fact]
    public async Task TestResultExposesPayloadExtensionDataThroughAdditionalData()
    {
        // Properties inside the result object that the result type does not define are payload
        // extension data. EmptyResult defines nothing, so everything in the object qualifies.
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            await connection.RaiseDataReceivedEventAsync("""{"type":"success","id":1,"result":{"goog:extra":"payload value"}}""");
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        Input.ReleaseActionsCommandResult result = await driver.Input.ReleaseActionsAsync(new Input.ReleaseActionsCommandParameters("myContext"), cancellationToken: TestContext.Current.CancellationToken);
        Assert.Single(result.AdditionalData);
        Assert.Equal("payload value", result.AdditionalData["goog:extra"]);
        Assert.Empty(result.AdditionalResponseProperties);
    }

    [Fact]
    public async Task TestResultExposesEnvelopeExtensionDataThroughAdditionalResponseProperties()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            await connection.RaiseDataReceivedEventAsync("""{"type":"success","id":1,"goog:channel":"channel value","result":{}}""");
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        Input.ReleaseActionsCommandResult result = await driver.Input.ReleaseActionsAsync(new Input.ReleaseActionsCommandParameters("myContext"), cancellationToken: TestContext.Current.CancellationToken);
        Assert.Empty(result.AdditionalData);
        Assert.Single(result.AdditionalResponseProperties);
        Assert.Equal("channel value", result.AdditionalResponseProperties["goog:channel"]);
    }

    [Fact]
    public async Task TestResultDeclaringItsOwnExtensionDataKeepsItsLeftovers()
    {
        // A result type with its own [JsonExtensionData] member has already captured the leftovers,
        // so the transport does not report them a second time through AdditionalData.
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            await connection.RaiseDataReceivedEventAsync("""{"type":"success","id":1,"result":{"kept":"v","goog:extra":"x"}}""");
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        ResultWithOwnExtensionData result = await driver.ExecuteCommandAsync(new ResultShapeCommandParameters<ResultWithOwnExtensionData>(), cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("v", result.Kept);
        Assert.NotNull(result.Extra);
        Assert.Equal("x", result.Extra["goog:extra"].GetString());
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public async Task TestResultTreatsIgnoredAndGetterOnlyMembersAsNotConsumingWireProperties()
    {
        // "skipped" is [JsonIgnore]d and "Computed" is getter-only: the serializer cannot assign
        // either, so wire properties with those names are extension data, not consumed members.
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            await connection.RaiseDataReceivedEventAsync("""{"type":"success","id":1,"result":{"kept":"v","skipped":"s","Computed":"c"}}""");
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        ResultWithIgnoredMembers result = await driver.ExecuteCommandAsync(new ResultShapeCommandParameters<ResultWithIgnoredMembers>(), cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("v", result.Kept);
        Assert.Null(result.Skipped);
        Assert.Equal(2, result.AdditionalData.Count);
        Assert.Equal("s", result.AdditionalData["skipped"]);
        Assert.Equal("c", result.AdditionalData["Computed"]);
    }

    [Fact]
    public async Task TestResultExposesPayloadAndEnvelopeExtensionDataSeparately()
    {
        // The two positions are distinct surfaces; neither shadows or merges into the other.
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            await connection.RaiseDataReceivedEventAsync("""{"type":"success","id":1,"goog:channel":"channel value","result":{"value":"typed","goog:extra":42}}""");
        });

        Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        TestCommandResult result = await driver.ExecuteCommandAsync(new TestCommandParameters("module.command"), cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("typed", result.Value);
        Assert.Single(result.AdditionalData);
        Assert.Equal(42L, result.AdditionalData["goog:extra"]);
        Assert.Single(result.AdditionalResponseProperties);
        Assert.Equal("channel value", result.AdditionalResponseProperties["goog:channel"]);
    }

    private sealed class RegistrationRecordingDriver : BiDiDriver
    {
        private readonly List<string> registeredModuleNames;
        private readonly List<string> registeredEventNames;

        public RegistrationRecordingDriver(Transport transport)
            : base(TimeSpan.FromMilliseconds(500), transport)
        {
            // Assigned in the constructor body rather than by field initializers: initializers run before
            // the base constructor, and would hide a call into an override from that constructor.
            this.registeredModuleNames = [];
            this.registeredEventNames = [];
        }

        public IReadOnlyList<string> RegisteredModuleNames => this.registeredModuleNames;

        public IReadOnlyList<string> RegisteredEventNames => this.registeredEventNames;

        public override void RegisterModule(Module module)
        {
            this.registeredModuleNames.Add(module.ModuleName);
            base.RegisterModule(module);
        }

        public override void RegisterEvent<T>(string eventName, Func<EventInfo<T>, Task> eventInvoker)
        {
            this.registeredEventNames.Add(eventName);
            base.RegisterEvent(eventName, eventInvoker);
        }
    }

    private sealed class ResultShapeCommandParameters<T> : CommandParameters<T>
        where T : CommandResult
    {
        [JsonIgnore]
        public override string MethodName => "module.command";
    }

    private record ResultWithOwnExtensionData : CommandResult
    {
        [JsonPropertyName("kept")]
        public string? Kept { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? Extra { get; set; }
    }

    private record ResultWithIgnoredMembers : CommandResult
    {
        [JsonPropertyName("kept")]
        public string? Kept { get; set; }

        [JsonIgnore]
        [JsonPropertyName("skipped")]
        public string? Skipped { get; set; }

        public string Computed => "computed";
    }
}
