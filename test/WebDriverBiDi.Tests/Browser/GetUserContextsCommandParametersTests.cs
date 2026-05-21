namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class GetUserContextsCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        GetUserContextsCommandParameters properties = new();
        Assert.Equal("browser.getUserContexts", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        CloseCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }
}
