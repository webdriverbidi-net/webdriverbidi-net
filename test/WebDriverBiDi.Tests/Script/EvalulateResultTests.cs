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
}
