namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class StatusCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        StatusCommandParameters properties = new();
        Assert.Equal("session.status", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        StatusCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }
}
