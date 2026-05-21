namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CloseCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        CloseCommandParameters properties = new();
        Assert.Equal("browser.close", properties.MethodName);
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
