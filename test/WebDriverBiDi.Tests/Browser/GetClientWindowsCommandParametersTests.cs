namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class GetClientWindowsCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        GetClientWindowsCommandParameters properties = new();
        Assert.Equal("browser.getClientWindows", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        GetClientWindowsCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }
}
