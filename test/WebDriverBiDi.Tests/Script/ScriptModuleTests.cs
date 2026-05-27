namespace WebDriverBiDi.Script;

using TestUtilities;
using WebDriverBiDi.Protocol;

public class ScriptModuleTests
{
    [Fact]
    public async Task TestExecuteCallFunctionCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "type": "success", 
                                      "realm": "myRealmId", 
                                      "result": {
                                        "type": "string",
                                        "value": "myStringValue"
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        EvaluateResult result = await module.CallFunctionAsync(new CallFunctionCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.IsType<EvaluateResultSuccess>(result);
        EvaluateResultSuccess? successResult = result as EvaluateResultSuccess;
        Assert.NotNull(successResult);

        Assert.Equal("myRealmId", successResult.RealmId);
        Assert.Equal(EvaluateResultType.Success, successResult.ResultType);
        Assert.NotNull(successResult.Result);
        Assert.Equal(RemoteValueType.String, successResult.Result.Type);
        Assert.Equal("myStringValue", successResult.Result.ConvertTo<StringRemoteValue>().Value);
    }

    [Fact]
    public async Task TestExecuteCallFunctionCommandReturningError()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "type": "exception", 
                                      "realm": "myRealmId", 
                                      "exceptionDetails": {
                                        "text": "error received from script",
                                        "lineNumber": 2,
                                        "columnNumber": 5,
                                        "exception": {
                                          "type": "string",
                                          "value": "myStringValue"
                                        },
                                        "stackTrace": {
                                          "callFrames": []
                                        } 
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        EvaluateResult result = await module.CallFunctionAsync(new CallFunctionCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.IsType<EvaluateResultException>(result);
        EvaluateResultException? exceptionResult = result as EvaluateResultException;
        Assert.NotNull(exceptionResult);

        Assert.Equal("myRealmId", exceptionResult.RealmId);
        Assert.Equal(EvaluateResultType.Exception, exceptionResult.ResultType);
        Assert.Equal("error received from script", exceptionResult.ExceptionDetails.Text);
        Assert.Equal(2, exceptionResult.ExceptionDetails.LineNumber);
        Assert.Equal(5, exceptionResult.ExceptionDetails.ColumnNumber);
        Assert.NotNull(exceptionResult.ExceptionDetails.StackTrace);
        Assert.Empty(exceptionResult.ExceptionDetails.StackTrace.CallFrames);
        Assert.Equal("myStringValue", exceptionResult.ExceptionDetails.Exception.ConvertTo<StringRemoteValue>().Value);
    }

    [Fact]
    public async Task TestExecuteEvaluateCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "type": "success", 
                                      "realm": "myRealmId", 
                                      "result": {
                                        "type": "string",
                                        "value": "myStringValue"
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        EvaluateResult result = await module.EvaluateAsync(new EvaluateCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.IsType<EvaluateResultSuccess>(result);
        EvaluateResultSuccess? successResult = result as EvaluateResultSuccess;
        Assert.NotNull(successResult);

        Assert.Equal("myRealmId", successResult.RealmId);
        Assert.Equal(EvaluateResultType.Success, successResult.ResultType);
        Assert.NotNull(successResult.Result);
        Assert.Equal(RemoteValueType.String, successResult.Result.Type);
        Assert.Equal("myStringValue", successResult.Result.ConvertTo<StringRemoteValue>().Value);
    }

    [Fact]
    public async Task TestExecuteEvaluateCommandReturningError()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "type": "exception", 
                                      "realm": "myRealmId", 
                                      "exceptionDetails": {
                                        "text": "error received from script",
                                        "lineNumber": 2,
                                        "columnNumber": 5,
                                        "exception": {
                                          "type": "string",
                                          "value": "myStringValue"
                                        },
                                        "stackTrace": {
                                          "callFrames": []
                                        } 
                                      }
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        EvaluateResult result = await module.EvaluateAsync(new EvaluateCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.IsType<EvaluateResultException>(result);
        EvaluateResultException? exceptionResult = result as EvaluateResultException;
        Assert.NotNull(exceptionResult);

        Assert.Equal("myRealmId", exceptionResult.RealmId);
        Assert.Equal(EvaluateResultType.Exception, exceptionResult.ResultType);
        Assert.Equal("error received from script", exceptionResult.ExceptionDetails.Text);
        Assert.Equal(2, exceptionResult.ExceptionDetails.LineNumber);
        Assert.Equal(5, exceptionResult.ExceptionDetails.ColumnNumber);
        Assert.NotNull(exceptionResult.ExceptionDetails.StackTrace);
        Assert.Empty(exceptionResult.ExceptionDetails.StackTrace.CallFrames);
        Assert.Equal("myStringValue", exceptionResult.ExceptionDetails.Exception.ConvertTo<StringRemoteValue>().Value);
    }

    [Fact]
    public async Task TestExecuteGetRealmsCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "realms": [
                                        {
                                          "realm": "myRealmId",
                                          "origin": "myOrigin",
                                          "type": "window",
                                          "context": "myContextId"
                                        } 
                                      ]
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        GetRealmsCommandResult result = await module.GetRealmsAsync(new GetRealmsCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Single(result.Realms);

        Assert.Equal(RealmType.Window, result.Realms[0].Type);
        Assert.IsType<WindowRealmInfo>(result.Realms[0]);

        WindowRealmInfo info = result.Realms[0].As<WindowRealmInfo>();
        Assert.NotNull(info);

        Assert.Equal("myRealmId", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal("myContextId", info.BrowsingContext);
    }

    [Fact]
    public async Task TestExecuteGetRealmsCommandWithNoArgument()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "realms": []
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        GetRealmsCommandResult result = await module.GetRealmsAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result.Realms);
    }

    [Fact]
    public async Task TestExecuteDisownCommand()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        DisownCommandResult result = await module.DisownAsync(new DisownCommandParameters(new ContextTarget("myContextId"), new string[] { "myValue" }), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.IsType<DisownCommandResult>(result);
    }

    [Fact]
    public async Task TestCanReceiveRealmCreatedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnRealmCreated.AddObserver(e =>
        {
            Assert.Equal("myRealm", e.RealmId);
            Assert.Equal("myOrigin", e.Origin);
            Assert.Equal(RealmType.Window, e.Type);
            Assert.Equal("myContext", e.As<WindowRealmInfo>().BrowsingContext);

            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "script.realmCreated",
                             "params": {
                               "realm": "myRealm",
                               "type": "window",
                               "context": "myContext",
                               "origin": "myOrigin"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveRealmCreatedEventForNonWindowRealm()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnRealmCreated.AddObserver(e =>
        {
            Assert.Equal("myRealm", e.RealmId);
            Assert.Equal("myOrigin", e.Origin);
            Assert.Equal(RealmType.Worker, e.Type);

            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "script.realmCreated",
                             "params": {
                               "realm": "myRealm",
                               "type": "worker",
                               "origin": "myOrigin"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveRealmDestroyedEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnRealmDestroyed.AddObserver(e =>
        {
            Assert.Equal("myRealm", e.RealmId);
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "script.realmDestroyed",
                             "params": {
                               "realm": "myRealm"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanReceiveMessageEvent()
    {
        TestWebSocketConnection connection = new();
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        module.OnMessage.AddObserver(e =>
        {
            Assert.Equal("myChannel", e.ChannelId);
            Assert.NotNull(e.Data);
            Assert.Equal(RemoteValueType.String, e.Data.Type);
            Assert.Equal("myChannelValue", e.Data.ConvertTo<StringRemoteValue>().Value);
            Assert.NotNull(e.Source);
            Assert.Equal("myRealm", e.Source.RealmId);

            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        string eventJson = """
                           {
                             "type": "event",
                             "method": "script.message",
                             "params": {
                               "channel": "myChannel", 
                               "data": {
                                 "type": "string",
                                 "value": "myChannelValue"
                               },
                               "source": {
                                 "realm": "myRealm" 
                               }
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCanAddPreloadScript()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {
                                      "script": "loadScriptId"
                                    }
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        AddPreloadScriptCommandResult result = await module.AddPreloadScriptAsync(new AddPreloadScriptCommandParameters("window.foo = false;"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.IsType<AddPreloadScriptCommandResult>(result);
        Assert.Equal("loadScriptId", result.PreloadScriptId);
    }

    [Fact]
    public async Task TestCanRemovePreloadScript()
    {
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), new Transport(connection));
        ScriptModule module = driver.Script;
        await driver.StartAsync("ws:localhost", cancellationToken: TestContext.Current.CancellationToken);

        RemovePreloadScriptCommandResult result = await module.RemovePreloadScriptAsync(new RemovePreloadScriptCommandParameters("loadScriptId"), cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(result);
    }
}
