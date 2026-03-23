namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using WebDriverBiDi.Script;

[TestFixture]
public class ScriptEvaluateResultJsonConverterTests
{
    [Test]
    public void TestDeserializeSuccessTypeReturnsEvaluateResultSuccessWithCorrectProperties()
    {
        string json = """
                      {"type": "success", "realm": "testRealm", "result": {"type": "string", "value": "hello"}}
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EvaluateResultSuccess>());
        EvaluateResultSuccess successResult = (EvaluateResultSuccess)result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(successResult.RealmId, Is.EqualTo("testRealm"));
            Assert.That(successResult.Result.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(successResult.Result.ConvertTo<StringRemoteValue>().Value, Is.EqualTo("hello"));
        }
    }

    [Test]
    public void TestDeserializeExceptionTypeReturnsEvaluateResultException()
    {
        string json = """
                      {
                        "type": "exception",
                        "realm": "testRealm",
                        "exceptionDetails": {
                          "text": "error",
                          "columnNumber": 0,
                          "lineNumber": 1,
                          "stackTrace": {"callFrames": []},
                          "exception": {"type": "string", "value": "error"}
                        }
                      }
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EvaluateResultException>());
        EvaluateResultException exceptionResult = (EvaluateResultException)result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exceptionResult.RealmId, Is.EqualTo("testRealm"));
            Assert.That(exceptionResult.ExceptionDetails.Text, Is.EqualTo("error"));
            Assert.That(exceptionResult.ExceptionDetails.ColumnNumber, Is.EqualTo(0));
            Assert.That(exceptionResult.ExceptionDetails.LineNumber, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestDeserializeWithMissingTypeThrowsJsonException()
    {
        string json = """{"realm": "testRealm", "result": {"type": "string", "value": "hello"}}""";
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("Script response must contain a 'type' property"));
    }

    [Test]
    public void TestDeserializeWithNonStringTypeThrowsJsonException()
    {
        string json = """{"type": {"invalid": true}, "realm": "testRealm", "result": {"type": "string", "value": "hello"}}""";
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("Script response 'type' property must be a string"));
    }

    [Test]
    public void TestDeserializeWithUnknownTypeValueThrowsJsonException()
    {
        string json = """{"type": "unknown", "realm": "testRealm", "result": {"type": "string", "value": "hello"}}""";
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("unknown type 'unknown' for script result"));
    }

    [Test]
    public void TestDeserializeNonObjectThrowsJsonException()
    {
        string json = """["invalid script result"]""";
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestWriteThrowsNotImplementedException()
    {
        string json = """{"type": "success", "realm": "testRealm", "result": {"type": "string", "value": "hello"}}""";
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(() => JsonSerializer.Serialize(result!), Throws.InstanceOf<NotImplementedException>());
    }
}
