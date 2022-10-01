namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[TestFixture]
public class ScriptEvaluateResultJsonConverterTests
{
    [Test]
    public void TestCanDeserializeScriptEvaluateResultSuccess()
    {
        string json = @"{ ""realm"": ""myRealm"", ""result"": { ""type"": ""string"", ""value"": ""myResult"" } }";
        ScriptEvaluateResult? result = JsonConvert.DeserializeObject<ScriptEvaluateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ScriptEvaluateResultSuccess>());
        ScriptEvaluateResultSuccess successResult = (ScriptEvaluateResultSuccess)result!;
        Assert.That(successResult.RealmId, Is.EqualTo("myRealm"));
        Assert.That(successResult.Result.Type, Is.EqualTo("string"));
        Assert.That(successResult.Result.HasValue);
        Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myResult"));
    }

    [Test]
    public void TestCanDeserializeScriptEvaluateResultException()
    {
        string json = @"{ ""realm"": ""myRealm"", ""exceptionDetails"": { ""text"": ""exception thrown"", ""lineNumber"": 1, ""columnNumber"": 5, ""stacktrace"": { ""callFrames"": [] }, ""exception"": { ""type"": ""string"", ""value"": ""exception value"" } } }";
        ScriptEvaluateResult? result = JsonConvert.DeserializeObject<ScriptEvaluateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ScriptEvaluateResultException>());
        ScriptEvaluateResultException successResult = (ScriptEvaluateResultException)result!;
        Assert.That(successResult.RealmId, Is.EqualTo("myRealm"));
        Assert.That(successResult.ExceptionDetails.Text, Is.EqualTo("exception thrown"));
        Assert.That(successResult.ExceptionDetails.LineNumber, Is.EqualTo(1));
        Assert.That(successResult.ExceptionDetails.ColumnNumber, Is.EqualTo(5));
        Assert.That(successResult.ExceptionDetails.StackTrace.CallFrames, Has.Count.EqualTo(0));
        Assert.That(successResult.ExceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("exception value"));
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithInvalidPropertyThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""noWoman"": ""noCry"" }";
        Assert.That(() => JsonConvert.DeserializeObject<ScriptEvaluateResult>(json), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("ScriptTarget must contain either a 'result' or a 'exceptionDetails' property"));
    }
}
