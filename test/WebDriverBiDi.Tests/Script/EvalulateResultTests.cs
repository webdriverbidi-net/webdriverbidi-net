namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

[TestFixture]
public class EvaluateResultTests
{
    [Test]
    public void TestCanDeserializeScriptEvaluateResultSuccess()
    {
        string json = @"{ ""type"": ""success"", ""realm"": ""myRealm"", ""result"": { ""type"": ""string"", ""value"": ""myResult"" } }";
        EvaluateResult? result = JsonConvert.DeserializeObject<EvaluateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EvaluateResultSuccess>());
        EvaluateResultSuccess successResult = (EvaluateResultSuccess)result!;
        Assert.Multiple(() =>
        {
            Assert.That(successResult.RealmId, Is.EqualTo("myRealm"));
            Assert.That(successResult.Result.Type, Is.EqualTo("string"));
            Assert.That(successResult.Result.HasValue);
            Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myResult"));
        });
    }

    [Test]
    public void TestCanDeserializeScriptEvaluateResultException()
    {
        string json = @"{ ""type"": ""exception"", ""realm"": ""myRealm"", ""exceptionDetails"": { ""text"": ""exception thrown"", ""lineNumber"": 1, ""columnNumber"": 5, ""stacktrace"": { ""callFrames"": [] }, ""exception"": { ""type"": ""string"", ""value"": ""exception value"" } } }";
        EvaluateResult? result = JsonConvert.DeserializeObject<EvaluateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EvaluateResultException>());
        EvaluateResultException successResult = (EvaluateResultException)result!;
        Assert.Multiple(() =>
        {
            Assert.That(successResult.RealmId, Is.EqualTo("myRealm"));
            Assert.That(successResult.ExceptionDetails.Text, Is.EqualTo("exception thrown"));
            Assert.That(successResult.ExceptionDetails.LineNumber, Is.EqualTo(1));
            Assert.That(successResult.ExceptionDetails.ColumnNumber, Is.EqualTo(5));
            Assert.That(successResult.ExceptionDetails.StackTrace.CallFrames, Has.Count.EqualTo(0));
            Assert.That(successResult.ExceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("exception value"));
        });
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithInvalidTypePropertyValueThrows()
    {
        string json = @"{ ""type"": ""invalid"", ""realm"": ""myRealm"", ""noWoman"": ""noCry"" }";
        Assert.That(() => JsonConvert.DeserializeObject<EvaluateResult>(json), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("unknown type 'invalid' for script result"));
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithMissingTypePropertyThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""noWoman"": ""noCry"" }";
        Assert.That(() => JsonConvert.DeserializeObject<EvaluateResult>(json), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Script response must contain a 'type' property that contains a non-null string value"));
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithInvalidTypePropertyObjectThrows()
    {
        string json = @"{ ""type"": { ""noWoman"": ""noCry"" }, ""realm"": ""myRealm"", ""noWoman"": ""noCry"" }";
        Assert.That(() => JsonConvert.DeserializeObject<EvaluateResult>(json), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Script response must contain a 'type' property that contains a non-null string value"));
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithMissingRealmValueThrows()
    {
        string json = @"{ ""type"": ""success"", ""result"": { ""type"": ""string"", ""value"": ""myResult"" } }";
        Assert.That(() => JsonConvert.DeserializeObject<EvaluateResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithInvalidRealmValueTypeThrows()
    {
        string json = @"{ ""type"": ""success"", ""realm"": { ""noWoman"": ""noCry"" }, ""result"": { ""type"": ""string"", ""value"": ""myResult"" } }";
        Assert.That(() => JsonConvert.DeserializeObject<EvaluateResult>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCannotSerialize()
    {
        // NOTE: ScriptEvaluateResult and subclasses do not provide a way to instantiate
        // one directly using a constructor, so we will deserialize one from JSON.
        string json = @"{ ""type"": ""success"", ""realm"": ""myRealm"", ""result"": { ""type"": ""string"", ""value"": ""myResult"" } }";
        EvaluateResult? result = JsonConvert.DeserializeObject<EvaluateResult>(json);
        Assert.That(() => JsonConvert.SerializeObject(result), Throws.InstanceOf<NotImplementedException>());
    }
}
