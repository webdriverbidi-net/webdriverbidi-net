namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class RemoveUserContextCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        RemoveUserContextCommandParameters properties = new("myUserContext");
        Assert.Equal("browser.removeUserContext", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        RemoveUserContextCommandParameters properties = new("myUserContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("userContext"));
        JToken? userContext = serialized["userContext"];
        Assert.NotNull(userContext);
        Assert.Equal(JTokenType.String, userContext.Type);
        Assert.Equal("myUserContext", userContext.Value<string>());
    }
}
