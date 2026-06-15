namespace WebDriverBiDi.Script;

using System.Text.Json;

public class EvaluateResultSuccessTests
{
    [Fact]
    public void TestCanDeserialize()
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
    public void TestCopySemantics()
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
    public void TestDeserializingWithMissingRealmValueThrows()
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
    public void TestDeserializingWithInvalidRealmValueTypeThrows()
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
}
