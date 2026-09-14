namespace WebDriverBiDi.Script;

using System.Text.Json;

public class EvaluateResultTests
{
    [Fact]
    public void TestDeserializeWithInvalidTypePropertyValueThrows()
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
    public void TestDeserializeWithMissingTypePropertyThrows()
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
    public void TestDeserializeWithInvalidTypePropertyObjectThrows()
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
    public void TestDeserializeWithNonObjectThrows()
    {
        string json = @"[ ""invalid script result"" ]";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<EvaluateResult>(json));
    }

    [Fact]
    public void TestCanCastToProperSubclassType()
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
        EvaluateResult? info = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(info);
        Assert.IsType<EvaluateResultSuccess>(info);
        Assert.NotNull(info.As<EvaluateResultSuccess>());
    }

    [Fact]
    public void TestCannotCastToImproperSubclassType()
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
        EvaluateResult? info = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(info);
        Assert.IsType<EvaluateResultSuccess>(info);
        Assert.Contains("cannot be cast", Assert.ThrowsAny<WebDriverBiDiException>(() => info.As<EvaluateResultException>()).Message);
    }

    [Fact]
    public void TestTryCastToSubclassTypeReturnsTrue()
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
        EvaluateResult? info = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(info);
        Assert.IsType<EvaluateResultSuccess>(info);
        bool result = info.TryAs(out EvaluateResultSuccess? outEvaluateResult);
        Assert.True(result);
        Assert.NotNull(outEvaluateResult);
    }

    [Fact]
    public void TestTryCastToImproperSubclassTypeReturnsFalse()
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
        EvaluateResult? info = JsonSerializer.Deserialize<EvaluateResult>(json);
        Assert.NotNull(info);
        Assert.IsType<EvaluateResultSuccess>(info);
        bool result = info.TryAs(out EvaluateResultException? outEvaluateResult);
        Assert.False(result);
        Assert.Null(outEvaluateResult);
    }
}
