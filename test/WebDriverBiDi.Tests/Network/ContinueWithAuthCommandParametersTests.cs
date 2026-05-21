namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ContinueWithAuthCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        ContinueWithAuthCommandParameters properties = new("requestId");
        Assert.Equal("network.continueWithAuth", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        ContinueWithAuthCommandParameters properties = new("requestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("requestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("default", action.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithCancelAction()
    {
        ContinueWithAuthCommandParameters properties = new("requestId")
        {
            Action = ContinueWithAuthActionType.Cancel
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("requestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("cancel", action.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithProvideCredentialsAction()
    {
        ContinueWithAuthCommandParameters properties = new("requestId")
        {
            Action = ContinueWithAuthActionType.ProvideCredentials
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("requestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("provideCredentials", action.Value<string>());

        Assert.True(serialized.ContainsKey("credentials"));
        JToken? credentialsToken = serialized["credentials"];
        Assert.NotNull(credentialsToken);
        Assert.Equal(JTokenType.Object, credentialsToken.Type);
        JObject? credentialsObject = credentialsToken as JObject;
        Assert.NotNull(credentialsObject);
        Assert.Equal(3, credentialsObject.Count);
        Assert.True(credentialsObject.ContainsKey("type"));
        JToken? credType = credentialsObject["type"];
        Assert.NotNull(credType);
        Assert.Equal(JTokenType.String, credType.Type);
        Assert.Equal("password", credType.Value<string>());
        Assert.True(credentialsObject.ContainsKey("username"));
        JToken? username = credentialsObject["username"];
        Assert.NotNull(username);
        Assert.Equal(JTokenType.String, username.Type);
        Assert.Equal(string.Empty, username.Value<string>());
        Assert.True(credentialsObject.ContainsKey("password"));
        JToken? password = credentialsObject["password"];
        Assert.NotNull(password);
        Assert.Equal(JTokenType.String, password.Type);
        Assert.Equal(string.Empty, password.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithProvideCredentialsActionAndCredentials()
    {
        ContinueWithAuthCommandParameters properties = new("requestId")
        {
            Action = ContinueWithAuthActionType.ProvideCredentials,
            Credentials = new AuthCredentials("myUserName", "myPassword")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("requestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("provideCredentials", action.Value<string>());

        Assert.True(serialized.ContainsKey("credentials"));
        JToken? credentialsToken = serialized["credentials"];
        Assert.NotNull(credentialsToken);
        Assert.Equal(JTokenType.Object, credentialsToken.Type);
        JObject? credentialsObject = credentialsToken as JObject;
        Assert.NotNull(credentialsObject);
        Assert.Equal(3, credentialsObject.Count);
        Assert.True(credentialsObject.ContainsKey("type"));
        JToken? credType = credentialsObject["type"];
        Assert.NotNull(credType);
        Assert.Equal(JTokenType.String, credType.Type);
        Assert.Equal("password", credType.Value<string>());
        Assert.True(credentialsObject.ContainsKey("username"));
        JToken? username = credentialsObject["username"];
        Assert.NotNull(username);
        Assert.Equal(JTokenType.String, username.Type);
        Assert.Equal("myUserName", username.Value<string>());
        Assert.True(credentialsObject.ContainsKey("password"));
        JToken? password = credentialsObject["password"];
        Assert.NotNull(password);
        Assert.Equal(JTokenType.String, password.Type);
        Assert.Equal("myPassword", password.Value<string>());
    }
}
