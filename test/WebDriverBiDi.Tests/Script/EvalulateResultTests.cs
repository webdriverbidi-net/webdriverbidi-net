namespace WebDriverBiDi.Script;

using System.Text.Json;

public class EvaluateResultTests
{
    [Fact]
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
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(result);
        Assert.IsType<EvaluateResultSuccess>(result);
        EvaluateResultSuccess successResult = (EvaluateResultSuccess)result;

        Assert.Equal("myRealm", successResult.RealmId);
        Assert.Equal(RemoteValueType.String, successResult.Result.Type);
        Assert.Equal("myResult", successResult.Result.ConvertTo<StringRemoteValue>().Value);
    }

    [Fact]
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
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(result);
        Assert.IsType<EvaluateResultException>(result);
        EvaluateResultException exceptionResult = (EvaluateResultException)result;

        Assert.Equal("myRealm", exceptionResult.RealmId);
        Assert.Equal("exception thrown", exceptionResult.ExceptionDetails.Text);
        Assert.Equal(1, exceptionResult.ExceptionDetails.LineNumber);
        Assert.Equal(5, exceptionResult.ExceptionDetails.ColumnNumber);
        Assert.Empty(exceptionResult.ExceptionDetails.StackTrace.CallFrames);
        Assert.Equal("exception value", exceptionResult.ExceptionDetails.Exception.ConvertTo<StringRemoteValue>().Value);
    }

    [Fact]
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
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(result);
        Assert.IsType<EvaluateResultSuccess>(result);
        EvaluateResultSuccess successResult = (EvaluateResultSuccess)result;
        EvaluateResultSuccess copy = successResult with { };
        Assert.Equal(successResult, copy);
    }

    [Fact]
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
        EvaluateResult? result = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(result);
        Assert.IsType<EvaluateResultException>(result);
        EvaluateResultException exceptionResult = (EvaluateResultException)result;
        EvaluateResultException copy = exceptionResult with { };
        Assert.Equal(exceptionResult, copy);
    }

    [Fact]
    public void TestDeserializeScriptEvaluateResultWithInvalidTypePropertyValueThrows()
    {
        string json = """
                     {
                       "type": "invalid",
                       "realm": "myRealm",
                       "noWoman": "noCry"
                     }
                     """;
        Assert.Contains("JSON for 'EvaluateResult' type property contains unknown value 'invalid'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<EvaluateResult>(json)).Message);
    }

    [Fact]
    public void TestDeserializeScriptEvaluateResultWithMissingTypePropertyThrows()
    {
        string json = """
                     {
                       "realm": "myRealm",
                       "noWoman": "noCry"
                     }
                     """;
        Assert.Contains("JSON for 'EvaluateResult' must contain a 'type' property", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<EvaluateResult>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("JSON 'type' property must be a string", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<EvaluateResult>(json)).Message);
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<EvaluateResult>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<EvaluateResult>(json));
    }

    [Fact]
    public void TestDeserializeScriptResultWithNonObjectThrows()
    {
        string json = @"[ ""invalid script result"" ]";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<EvaluateResult>(json));
    }
}
