namespace WebDriverBidi.Script;

using TestUtilities;

[TestFixture]
public class ScriptModuleTests
{
    [Test]
    public void TestExecuteCallFunctionCommand()
    {
        string responseJson = @"{ ""result"": { ""type"": ""success"", ""realm"": ""myRealmId"", ""result"": { ""type"": ""string"", ""value"": ""myStringValue"" } } }";
        TestDriver driver = new TestDriver();
        ScriptModule module = new ScriptModule(driver);
        var task = module.CallFunction(new CallFunctionCommandSettings("myFunction() {}", new ContextTarget("myContextId"), true));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<ScriptEvaluateResultSuccess>());
        var successResult = result as ScriptEvaluateResultSuccess;
        Assert.That(successResult, Is.Not.Null);
        Assert.That(successResult!.RealmId, Is.EqualTo("myRealmId"));
        Assert.That(successResult.Result, Is.Not.Null);
        Assert.That(successResult.Result.Type, Is.EqualTo("string"));
        Assert.That(successResult.Result.HasValue);
        Assert.That(successResult.Result.Value, Is.TypeOf<string>());
        Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myStringValue"));
    }

    [Test]
    public void TestExecuteCallFunctionCommandReturningError()
    {
        string responseJson = @"{ ""result"": { ""type"": ""exception"", ""realm"": ""myRealmId"", ""exceptionDetails"": { ""text"": ""error received from script"", ""lineNumber"": 2, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myStringValue"" }, ""stacktrace"": { ""callFrames"": [] } } } }";
        TestDriver driver = new TestDriver();
        ScriptModule module = new ScriptModule(driver);
        var task = module.CallFunction(new CallFunctionCommandSettings("myFunction() {}", new ContextTarget("myContextId"), true));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<ScriptEvaluateResultException>());
        var exceptionResult = result as ScriptEvaluateResultException;
        Assert.That(exceptionResult, Is.Not.Null);
        Assert.That(exceptionResult!.RealmId, Is.EqualTo("myRealmId"));
        Assert.That(exceptionResult.ExceptionDetails.Text, Is.EqualTo("error received from script"));
        Assert.That(exceptionResult.ExceptionDetails.LineNumber, Is.EqualTo(2));
        Assert.That(exceptionResult.ExceptionDetails.ColumnNumber, Is.EqualTo(5));
        Assert.That(exceptionResult.ExceptionDetails.StackTrace, Is.Not.Null);
        Assert.That(exceptionResult.ExceptionDetails.StackTrace.CallFrames.Count, Is.EqualTo(0));
        Assert.That(exceptionResult.ExceptionDetails.Exception.HasValue);
        Assert.That(exceptionResult.ExceptionDetails.Exception.Value, Is.TypeOf<string>());
        Assert.That(exceptionResult.ExceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("myStringValue"));
    }

    [Test]
    public void TestExecuteEvaluateCommand()
    {
        string responseJson = @"{ ""result"": { ""type"": ""success"", ""realm"": ""myRealmId"", ""result"": { ""type"": ""string"", ""value"": ""myStringValue"" } } }";
        TestDriver driver = new TestDriver();
        ScriptModule module = new ScriptModule(driver);
        var task = module.Evaluate(new EvaluateCommandSettings("myFunction() {}", new ContextTarget("myContextId"), true));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<ScriptEvaluateResultSuccess>());
        var successResult = result as ScriptEvaluateResultSuccess;
        Assert.That(successResult, Is.Not.Null);
        Assert.That(successResult!.RealmId, Is.EqualTo("myRealmId"));
        Assert.That(successResult.Result, Is.Not.Null);
        Assert.That(successResult.Result.Type, Is.EqualTo("string"));
        Assert.That(successResult.Result.HasValue);
        Assert.That(successResult.Result.Value, Is.TypeOf<string>());
        Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myStringValue"));
    }

    [Test]
    public void TestExecuteEvaluateCommandReturningError()
    {
        string responseJson = @"{ ""result"": { ""type"": ""exception"", ""realm"": ""myRealmId"", ""exceptionDetails"": { ""text"": ""error received from script"", ""lineNumber"": 2, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myStringValue"" }, ""stacktrace"": { ""callFrames"": [] } } } }";
        TestDriver driver = new TestDriver();
        ScriptModule module = new ScriptModule(driver);
        var task = module.Evaluate(new EvaluateCommandSettings("myFunction() {}", new ContextTarget("myContextId"), true));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<ScriptEvaluateResultException>());
        var exceptionResult = result as ScriptEvaluateResultException;
        Assert.That(exceptionResult, Is.Not.Null);
        Assert.That(exceptionResult!.RealmId, Is.EqualTo("myRealmId"));
        Assert.That(exceptionResult.ExceptionDetails.Text, Is.EqualTo("error received from script"));
        Assert.That(exceptionResult.ExceptionDetails.LineNumber, Is.EqualTo(2));
        Assert.That(exceptionResult.ExceptionDetails.ColumnNumber, Is.EqualTo(5));
        Assert.That(exceptionResult.ExceptionDetails.StackTrace, Is.Not.Null);
        Assert.That(exceptionResult.ExceptionDetails.StackTrace.CallFrames.Count, Is.EqualTo(0));
        Assert.That(exceptionResult.ExceptionDetails.Exception.HasValue);
        Assert.That(exceptionResult.ExceptionDetails.Exception.Value, Is.TypeOf<string>());
        Assert.That(exceptionResult.ExceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("myStringValue"));
    }

    [Test]
    public void TestExecuteGetRealmsCommandReturningError()
    {
        string responseJson = @"{ ""result"": { ""realms"": [ { ""realm"": ""myRealmId"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": ""myContextId"" } ] } }";
        TestDriver driver = new TestDriver();
        ScriptModule module = new ScriptModule(driver);
        var task = module.GetRealms(new GetRealmsCommandSettings());
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Realms.Count, Is.EqualTo(1));
        Assert.That(result.Realms[0].Type, Is.EqualTo(RealmType.Window));
        Assert.That(result.Realms[0], Is.TypeOf<WindowRealmInfo>());
        WindowRealmInfo? info = result.Realms[0] as WindowRealmInfo;
        Assert.That(info!.RealmId, Is.EqualTo("myRealmId"));
        Assert.That(info.Origin, Is.EqualTo("myOrigin"));
        Assert.That(info.BrowsingContext, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestExecuteDisownCommandReturningError()
    {
        string responseJson = @"{ ""result"": {} }";
        TestDriver driver = new TestDriver();
        ScriptModule module = new ScriptModule(driver);
        var task = module.Disown(new DisownCommandSettings(new ContextTarget("myContextId"), new string[] { "myValue" }));
        driver.WaitForCommandSet(TimeSpan.FromSeconds(1));

        driver.EmitResponse(responseJson);
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<EmptyResult>());
   }

    [Test]
    public void TestCanReceiveRealmCreatedEvent()
    {
        string eventJson = @"{ ""method"": ""script.realmCreated"", ""params"": { ""realm"": ""myRealm"", ""type"": ""window"", ""context"": ""myContext"", ""origin"": ""myOrigin"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        ScriptModule module = new ScriptModule(driver);
        module.RealmCreated += (object? obj, RealmCreatedEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.RealmId, Is.EqualTo("myRealm"));
            Assert.That(e.Origin, Is.EqualTo("myOrigin"));
            Assert.That(e.Type, Is.EqualTo(RealmType.Window));
            Assert.That(e.BrowsingContext, Is.EqualTo("myContext"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void TestCanReceiveRealmDestroyedEvent()
    {
        string eventJson = @"{ ""method"": ""script.realmDestroyed"", ""params"": { ""realm"": ""myRealm"" } }";
        bool eventRaised = false;
        TestDriver driver = new TestDriver();
        ScriptModule module = new ScriptModule(driver);
        module.RealmDestroyed += (object? obj, RealmDestroyedEventArgs e) =>
        {
            eventRaised = true;
            Assert.That(e.RealmId, Is.EqualTo("myRealm"));
        };

        driver.EmitResponse(eventJson);
        Assert.That(eventRaised, Is.True);
    }
}