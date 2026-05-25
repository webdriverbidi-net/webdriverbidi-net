namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Session;

public class CreateUserContextCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        CreateUserContextCommandParameters properties = new();
        Assert.Equal("browser.createUserContext", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        CreateUserContextCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptInsecureCertsTrue()
    {
        CreateUserContextCommandParameters properties = new()
        {
            AcceptInsecureCerts = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("acceptInsecureCerts"));
        JToken? acceptInsecureCerts = serialized["acceptInsecureCerts"];
        Assert.NotNull(acceptInsecureCerts);
        Assert.Equal(JTokenType.Boolean, acceptInsecureCerts.Type);
        Assert.True(acceptInsecureCerts.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAcceptInsecureCertsFalse()
    {
        CreateUserContextCommandParameters properties = new()
        {
            AcceptInsecureCerts = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("acceptInsecureCerts"));
        JToken? acceptInsecureCerts = serialized["acceptInsecureCerts"];
        Assert.NotNull(acceptInsecureCerts);
        Assert.Equal(JTokenType.Boolean, acceptInsecureCerts.Type);
        Assert.False(acceptInsecureCerts.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeParametersWithProxy()
    {
        CreateUserContextCommandParameters properties = new()
        {
            Proxy = new ManualProxyConfiguration()
            {
                HttpProxy = "example-proxy.com"
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("proxy"));
        JToken? proxyToken = serialized["proxy"];
        Assert.NotNull(proxyToken);
        Assert.Equal(JTokenType.Object, proxyToken.Type);
        JObject? proxyObject = proxyToken as JObject;
        Assert.NotNull(proxyObject);
        Assert.Equal(2, proxyObject.Count);

        Assert.True(proxyObject.ContainsKey("proxyType"));
        JToken? proxyType = proxyObject["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal("manual", proxyType.Value<string>());

        Assert.True(proxyObject.ContainsKey("httpProxy"));
        JToken? httpProxy = proxyObject["httpProxy"];
        Assert.NotNull(httpProxy);
        Assert.Equal("example-proxy.com", httpProxy.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithUnhandledPromptBehavior()
    {
        CreateUserContextCommandParameters properties = new()
        {
            UnhandledPromptBehavior = new UserPromptHandler()
            {
                Default = UserPromptHandlerType.Accept
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("unhandledPromptBehavior"));
        JToken? promptHandlerToken = serialized["unhandledPromptBehavior"];
        Assert.NotNull(promptHandlerToken);
        Assert.Equal(JTokenType.Object, promptHandlerToken.Type);
        JObject? promptHandlerObject = promptHandlerToken as JObject;
        Assert.NotNull(promptHandlerObject);
        Assert.Single(promptHandlerObject);

        Assert.True(promptHandlerObject.ContainsKey("default"));
        JToken? defaultHandler = promptHandlerObject["default"];
        Assert.NotNull(defaultHandler);
        Assert.Equal("accept", defaultHandler.Value<string>());
    }
}
