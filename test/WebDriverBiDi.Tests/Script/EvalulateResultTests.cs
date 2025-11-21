namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class EvaluateResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeScriptEvaluateResultSuccess()
    {
        string json = """
                      {
                        "type": "success",
                        "realm": "myRealm",
                        "result": {
                          "type": "string",
                          "value": "myResult"
                        }
                      }
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EvaluateResultSuccess>());
        EvaluateResultSuccess successResult = (EvaluateResultSuccess)result;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(successResult.RealmId, Is.EqualTo("myRealm"));
            Assert.That(successResult.Result.Type, Is.EqualTo("string"));
            Assert.That(successResult.Result.HasValue);
            Assert.That(successResult.Result.ValueAs<string>(), Is.EqualTo("myResult"));
        }
    }

    [Test]
    public void TestCanDeserializeScriptEvaluateResultException()
    {
        string json = """
                      {
                        "type": "exception",
                        "realm": "myRealm",
                        "exceptionDetails": {
                          "text": "exception thrown",
                          "lineNumber": 1,
                          "columnNumber": 5,
                          "stackTrace": {
                            "callFrames": [] 
                          },
                          "exception": {
                            "type": "string",
                            "value": "exception value"
                          }
                        }
                      }
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EvaluateResultException>());
        EvaluateResultException exceptionResult = (EvaluateResultException)result;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(exceptionResult.RealmId, Is.EqualTo("myRealm"));
            Assert.That(exceptionResult.ExceptionDetails.Text, Is.EqualTo("exception thrown"));
            Assert.That(exceptionResult.ExceptionDetails.LineNumber, Is.EqualTo(1));
            Assert.That(exceptionResult.ExceptionDetails.ColumnNumber, Is.EqualTo(5));
            Assert.That(exceptionResult.ExceptionDetails.StackTrace.CallFrames, Has.Count.EqualTo(0));
            Assert.That(exceptionResult.ExceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("exception value"));
        }
    }

    [Test]
    public void TestEvaluateResultSuccessCopySemantics()
    {
        string json = """
                      {
                        "type": "success",
                        "realm": "myRealm",
                        "result": {
                          "type": "string",
                          "value": "myResult"
                        }
                      }
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EvaluateResultSuccess>());
        EvaluateResultSuccess successResult = (EvaluateResultSuccess)result;
        EvaluateResultSuccess copy = successResult with { };
        Assert.That(copy, Is.EqualTo(successResult));
    }

    [Test]
    public void TestEvaluateResultExceptionCopySemantics()
    {
        string json = """
                      {
                        "type": "exception",
                        "realm": "myRealm",
                        "exceptionDetails": {
                          "text": "exception thrown",
                          "lineNumber": 1,
                          "columnNumber": 5,
                          "stackTrace": {
                            "callFrames": [] 
                          },
                          "exception": {
                            "type": "string",
                            "value": "exception value"
                          }
                        }
                      }
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<EvaluateResultException>());
        EvaluateResultException exceptionResult = (EvaluateResultException)result;
        EvaluateResultException copy = exceptionResult with { };
        Assert.That(copy, Is.EqualTo(exceptionResult));
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithInvalidTypePropertyValueThrows()
    {
        string json = """
                     {
                       "type": "invalid",
                       "realm": "myRealm",
                       "noWoman": "noCry"
                     }
                     """;
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("unknown type 'invalid' for script result"));
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithMissingTypePropertyThrows()
    {
        string json = """
                     {
                       "realm": "myRealm",
                       "noWoman": "noCry"
                     }
                     """;
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("Script response must contain a 'type' property"));
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithInvalidTypePropertyObjectThrows()
    {
        string json = """
                     {
                       "type": {
                         "noWoman": "noCry"
                       },
                       "realm": "myRealm",
                       "noWoman": "noCry"
                     }
                     """;
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("Script response 'type' property must be a string"));
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithMissingRealmValueThrows()
    {
        string json = """
                      {
                        "type": "success",
                        "result": {
                          "type": "string",
                          "value": "myResult"
                        }
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeScriptEvaluateResultWithInvalidRealmValueTypeThrows()
    {
        string json = """
                      {
                        "type": "success",
                        "realm": {
                          "noWoman": "noCry"
                        },
                        "result": {
                          "type": "string",
                          "value": "myResult"
                        }
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeScriptResultWithNonObjectThrows()
    {
        string json = @"[ ""invalid script result"" ]";
        Assert.That(() => JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCannotSerialize()
    {
        // NOTE: ScriptEvaluateResult and subclasses do not provide a way to instantiate
        // one directly using a constructor, so we will deserialize one from JSON.
        string json = """
                      {
                        "type": "success",
                        "realm": "myRealm",
                        "result": {
                          "type": "string",
                          "value": "myResult"
                        }
                      }
                      """;
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json, deserializationOptions);
        Assert.That(() => JsonSerializer.Serialize(result), Throws.InstanceOf<NotImplementedException>());
    }
}
