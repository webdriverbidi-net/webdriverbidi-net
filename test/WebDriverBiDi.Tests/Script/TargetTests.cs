namespace WebDriverBiDi.Script;

using System.Text.Json;

public class TargetTests
{
    [Fact]
    public void TestDeserializingWithInvalidJsonThrows()
    {
        string json = @"{ ""invalid"": ""invalidValue"" }";
        Assert.Contains("JSON for 'Target' must contain one of the following properties: context, realm", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Target>(json)).Message);
        Assert.Contains("JSON for 'Target' must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Target>(@"[ ""invalid target"" ]")).Message);
    }
}
