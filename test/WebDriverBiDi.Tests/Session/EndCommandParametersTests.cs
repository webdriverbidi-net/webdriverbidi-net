namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class EndCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        EndCommandParameters properties = new();
        Assert.Equal("session.end", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        EndCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }
}
