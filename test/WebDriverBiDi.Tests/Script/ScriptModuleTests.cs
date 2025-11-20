namespace WebDriverBiDi.Script;

using TestUtilities;
using WebDriverBiDi.Protocol;

[TestFixture]
public class ScriptModuleTests
{
    [Test]
    public async Task TestExecuteCallFunctionCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<EvaluateResult> task = module.CallFunctionAsync(new CallFunctionCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true));
        task.Wait(TimeSpan.FromSeconds(1));
        EvaluateResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EvaluateResultSuccess>());
        EvaluateResultSuccess? successResult = result as EvaluateResultSuccess;
        Assert.That(successResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(successResult.RealmId, Is.EqualTo("myRealmId"));
            Assert.That(successResult.ResultType, Is.EqualTo(EvaluateResultType.Success));
            Assert.That(successResult.Result, Is.Not.Null);
            Assert.That(successResult.Result.Type, Is.EqualTo("string"));
            Assert.That(successResult.Result.HasValue);
            Assert.That(successResult.Result.Value, Is.TypeOf<string>());
            Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myStringValue"));
        });
    }

    [Test]
    public async Task TestExecuteCallFunctionCommandReturningError()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<EvaluateResult> task = module.CallFunctionAsync(new CallFunctionCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true));
        task.Wait(TimeSpan.FromSeconds(1));
        EvaluateResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EvaluateResultException>());
        EvaluateResultException? exceptionResult = result as EvaluateResultException;
        Assert.That(exceptionResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(exceptionResult.RealmId, Is.EqualTo("myRealmId"));
            Assert.That(exceptionResult.ResultType, Is.EqualTo(EvaluateResultType.Exception));
            Assert.That(exceptionResult.ExceptionDetails.Text, Is.EqualTo("error received from script"));
            Assert.That(exceptionResult.ExceptionDetails.LineNumber, Is.EqualTo(2));
            Assert.That(exceptionResult.ExceptionDetails.ColumnNumber, Is.EqualTo(5));
            Assert.That(exceptionResult.ExceptionDetails.StackTrace, Is.Not.Null);
            Assert.That(exceptionResult.ExceptionDetails.StackTrace.CallFrames, Is.Empty);
            Assert.That(exceptionResult.ExceptionDetails.Exception.HasValue);
            Assert.That(exceptionResult.ExceptionDetails.Exception.Value, Is.TypeOf<string>());
            Assert.That(exceptionResult.ExceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("myStringValue"));
        });
    }

    [Test]
    public async Task TestExecuteEvaluateCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");
        
        Task<EvaluateResult> task = module.EvaluateAsync(new EvaluateCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true));
        task.Wait(TimeSpan.FromSeconds(1));
        EvaluateResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EvaluateResultSuccess>());
        EvaluateResultSuccess? successResult = result as EvaluateResultSuccess;
        Assert.That(successResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(successResult.RealmId, Is.EqualTo("myRealmId"));
            Assert.That(successResult.ResultType, Is.EqualTo(EvaluateResultType.Success));
            Assert.That(successResult.Result, Is.Not.Null);
            Assert.That(successResult.Result.Type, Is.EqualTo("string"));
            Assert.That(successResult.Result.HasValue);
            Assert.That(successResult.Result.Value, Is.TypeOf<string>());
            Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myStringValue"));
        });
    }

    [Test]
    public async Task TestExecuteEvaluateCommandReturningError()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<EvaluateResult> task = module.EvaluateAsync(new EvaluateCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true));
        task.Wait(TimeSpan.FromSeconds(1));
        EvaluateResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EvaluateResultException>());
        EvaluateResultException? exceptionResult = result as EvaluateResultException;
        Assert.That(exceptionResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(exceptionResult.RealmId, Is.EqualTo("myRealmId"));
            Assert.That(exceptionResult.ResultType, Is.EqualTo(EvaluateResultType.Exception));
            Assert.That(exceptionResult.ExceptionDetails.Text, Is.EqualTo("error received from script"));
            Assert.That(exceptionResult.ExceptionDetails.LineNumber, Is.EqualTo(2));
            Assert.That(exceptionResult.ExceptionDetails.ColumnNumber, Is.EqualTo(5));
            Assert.That(exceptionResult.ExceptionDetails.StackTrace, Is.Not.Null);
            Assert.That(exceptionResult.ExceptionDetails.StackTrace.CallFrames, Is.Empty);
            Assert.That(exceptionResult.ExceptionDetails.Exception.HasValue);
            Assert.That(exceptionResult.ExceptionDetails.Exception.Value, Is.TypeOf<string>());
            Assert.That(exceptionResult.ExceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("myStringValue"));
        });
    }

    [Test]
    public async Task TestExecuteGetRealmsCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<GetRealmsCommandResult> task = module.GetRealmsAsync(new GetRealmsCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        GetRealmsCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Realms, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result.Realms[0].Type, Is.EqualTo(RealmType.Window));
            Assert.That(result.Realms[0], Is.TypeOf<WindowRealmInfo>());
        });
        WindowRealmInfo info = result.Realms[0].As<WindowRealmInfo>();
        Assert.That(info, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(info.RealmId, Is.EqualTo("myRealmId"));
            Assert.That(info.Origin, Is.EqualTo("myOrigin"));
            Assert.That(info.BrowsingContext, Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public async Task TestExecuteDisownCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<DisownCommandResult> task = module.DisownAsync(new DisownCommandParameters(new ContextTarget("myContextId"), new string[] { "myValue" }));
        task.Wait(TimeSpan.FromSeconds(1));
        DisownCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<DisownCommandResult>());
   }

    [Test]
    public async Task TestCanReceiveRealmCreatedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        ManualResetEvent syncEvent = new(false);
        module.OnRealmCreated.AddObserver((RealmCreatedEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.RealmId, Is.EqualTo("myRealm"));
                Assert.That(e.Origin, Is.EqualTo("myOrigin"));
                Assert.That(e.Type, Is.EqualTo(RealmType.Window));
                Assert.That(e.As<WindowRealmInfo>().BrowsingContext, Is.EqualTo("myContext"));
            });
            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveRealmCreatedEventForNonWindowRealm()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        ManualResetEvent syncEvent = new(false);
        module.OnRealmCreated.AddObserver((RealmCreatedEventArgs e) =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(e.RealmId, Is.EqualTo("myRealm"));
                Assert.That(e.Origin, Is.EqualTo("myOrigin"));
                Assert.That(e.Type, Is.EqualTo(RealmType.Worker));
            });
            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveRealmDestroyedEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        ManualResetEvent syncEvent = new(false);
        module.OnRealmDestroyed.AddObserver((RealmDestroyedEventArgs e) =>
        {
            Assert.That(e.RealmId, Is.EqualTo("myRealm"));
            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanReceiveMessageEvent()
    {
        TestConnection connection = new();
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        ManualResetEvent syncEvent = new(false);
        module.OnMessage.AddObserver((MessageEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.ChannelId, Is.EqualTo("myChannel"));
                Assert.That(e.Data, Is.Not.Null);
                Assert.That(e.Data.Type, Is.EqualTo("string"));
                Assert.That(e.Data.ValueAs<string>(), Is.EqualTo("myChannelValue"));
                Assert.That(e.Source, Is.Not.Null);
                Assert.That(e.Source.RealmId, Is.EqualTo("myRealm"));
            });
            syncEvent.Set();
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
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestCanAddPreloadScript()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (sender, e) =>
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
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        Task<AddPreloadScriptCommandResult> task = module.AddPreloadScriptAsync(new AddPreloadScriptCommandParameters("window.foo = false;"));
        task.Wait(TimeSpan.FromSeconds(1));
        AddPreloadScriptCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<AddPreloadScriptCommandResult>());
        Assert.That(result.PreloadScriptId, Is.EqualTo("loadScriptId"));
    }

    [Test]
    public async Task TestCanRemovePreloadScript()
    {
        TestConnection connection = new();
        connection.DataSendComplete +=  async (sender, e) =>
        {
            string responseJson = $$"""
                                  {
                                    "type": "success",
                                    "id": {{e.SentCommandId}},
                                    "result": {}
                                  }
                                  """;
            await connection.RaiseDataReceivedEventAsync(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        ScriptModule module = new(driver);
        await driver.StartAsync("ws:localhost");
        
        Task<RemovePreloadScriptCommandResult> task = module.RemovePreloadScriptAsync(new RemovePreloadScriptCommandParameters("loadScriptId"));
        task.Wait(TimeSpan.FromSeconds(1));
        RemovePreloadScriptCommandResult result = task.Result;

        Assert.That(result, Is.Not.Null);
    }
}
