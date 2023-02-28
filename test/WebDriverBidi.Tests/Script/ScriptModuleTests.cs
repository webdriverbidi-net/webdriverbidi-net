namespace WebDriverBidi.Script;

using TestUtilities;
using WebDriverBidi.Protocol;

[TestFixture]
public class ScriptModuleTests
{
    [Test]
    public void TestExecuteCallFunctionCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""type"": ""success"", ""realm"": ""myRealmId"", ""result"": { ""type"": ""string"", ""value"": ""myStringValue"" } } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);

        var task = module.CallFunction(new CallFunctionCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EvaluateResultSuccess>());
        var successResult = result as EvaluateResultSuccess;
        Assert.That(successResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(successResult!.RealmId, Is.EqualTo("myRealmId"));
            Assert.That(successResult.ResultType, Is.EqualTo(EvaluateResultType.Success));
            Assert.That(successResult.Result, Is.Not.Null);
            Assert.That(successResult.Result.Type, Is.EqualTo("string"));
            Assert.That(successResult.Result.HasValue);
            Assert.That(successResult.Result.Value, Is.TypeOf<string>());
            Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myStringValue"));
        });
    }

    [Test]
    public void TestExecuteCallFunctionCommandReturningError()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""type"": ""exception"", ""realm"": ""myRealmId"", ""exceptionDetails"": { ""text"": ""error received from script"", ""lineNumber"": 2, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myStringValue"" }, ""stacktrace"": { ""callFrames"": [] } } } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);

        var task = module.CallFunction(new CallFunctionCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EvaluateResultException>());
        var exceptionResult = result as EvaluateResultException;
        Assert.That(exceptionResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(exceptionResult!.RealmId, Is.EqualTo("myRealmId"));
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
    public void TestExecuteEvaluateCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""type"": ""success"", ""realm"": ""myRealmId"", ""result"": { ""type"": ""string"", ""value"": ""myStringValue"" } } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);
        
        var task = module.Evaluate(new EvaluateCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EvaluateResultSuccess>());
        var successResult = result as EvaluateResultSuccess;
        Assert.That(successResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(successResult!.RealmId, Is.EqualTo("myRealmId"));
            Assert.That(successResult.ResultType, Is.EqualTo(EvaluateResultType.Success));
            Assert.That(successResult.Result, Is.Not.Null);
            Assert.That(successResult.Result.Type, Is.EqualTo("string"));
            Assert.That(successResult.Result.HasValue);
            Assert.That(successResult.Result.Value, Is.TypeOf<string>());
            Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myStringValue"));
        });
    }

    [Test]
    public void TestExecuteEvaluateCommandReturningError()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""type"": ""exception"", ""realm"": ""myRealmId"", ""exceptionDetails"": { ""text"": ""error received from script"", ""lineNumber"": 2, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myStringValue"" }, ""stacktrace"": { ""callFrames"": [] } } } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);

        var task = module.Evaluate(new EvaluateCommandParameters("myFunction() {}", new ContextTarget("myContextId"), true));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EvaluateResultException>());
        var exceptionResult = result as EvaluateResultException;
        Assert.That(exceptionResult, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(exceptionResult!.RealmId, Is.EqualTo("myRealmId"));
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
    public void TestExecuteGetRealmsCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""realms"": [ { ""realm"": ""myRealmId"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": ""myContextId"" } ] } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);

        var task = module.GetRealms(new GetRealmsCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Realms, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(result.Realms[0].Type, Is.EqualTo(RealmType.Window));
            Assert.That(result.Realms[0], Is.TypeOf<WindowRealmInfo>());
        });
        WindowRealmInfo? info = result.Realms[0] as WindowRealmInfo;
        Assert.That(info, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(info!.RealmId, Is.EqualTo("myRealmId"));
            Assert.That(info.Origin, Is.EqualTo("myOrigin"));
            Assert.That(info.BrowsingContext, Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestExecuteDisownCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": {} }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);

        var task = module.Disown(new DisownCommandParameters(new ContextTarget("myContextId"), new string[] { "myValue" }));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EmptyResult>());
   }

    [Test]
    public void TestCanReceiveRealmCreatedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.RealmCreated += (object? obj, RealmCreatedEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.RealmId, Is.EqualTo("myRealm"));
                Assert.That(e.Origin, Is.EqualTo("myOrigin"));
                Assert.That(e.Type, Is.EqualTo(RealmType.Window));
                Assert.That(e.BrowsingContext, Is.EqualTo("myContext"));
            });
            syncEvent.Set();
        };

        string eventJson = @"{ ""method"": ""script.realmCreated"", ""params"": { ""realm"": ""myRealm"", ""type"": ""window"", ""context"": ""myContext"", ""origin"": ""myOrigin"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveRealmCreatedEventForNonWindowRealm()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.RealmCreated += (object? obj, RealmCreatedEventArgs e) => {
            Assert.Multiple(() =>
            {
                Assert.That(e.RealmId, Is.EqualTo("myRealm"));
                Assert.That(e.Origin, Is.EqualTo("myOrigin"));
                Assert.That(e.Type, Is.EqualTo(RealmType.Worker));
                Assert.That(e.BrowsingContext, Is.Null);
            });
            syncEvent.Set();
        };

        string eventJson = @"{ ""method"": ""script.realmCreated"", ""params"": { ""realm"": ""myRealm"", ""type"": ""worker"", ""origin"": ""myOrigin"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveRealmDestroyedEvent()
    {
        TestConnection connection = new();
        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);

        ManualResetEvent syncEvent = new(false);
        module.RealmDestroyed += (object? obj, RealmDestroyedEventArgs e) =>
        {
            Assert.That(e.RealmId, Is.EqualTo("myRealm"));
            syncEvent.Set();
        };

        string eventJson = @"{ ""method"": ""script.realmDestroyed"", ""params"": { ""realm"": ""myRealm"" } }";
        connection.RaiseDataReceivedEvent(eventJson);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanAddPreloadScript()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { ""script"": ""loadScriptId"" } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);
        var task = module.AddPreloadScript(new AddPreloadScriptCommandParameters("window.foo = false;"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<AddPreloadScriptCommandResult>());
        Assert.That(result.PreloadScriptId, Is.EqualTo("loadScriptId"));
    }

    [Test]
    public void TestCanRemovePreloadScript()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""id"": " + e.SentCommandId + @", ""result"": { } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        Driver driver = new(new Transport(TimeSpan.FromMilliseconds(500), connection));
        ScriptModule module = new(driver);
        
        var task = module.RemovePreloadScript(new RemovePreloadScriptCommandParameters("loadScriptId"));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EmptyResult>());
    }
}