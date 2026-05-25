namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetDownloadBehaviorCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetDownloadBehaviorCommandParameters properties = new();
        Assert.Equal("browser.setDownloadBehavior", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetDownloadBehaviorCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("downloadBehavior"));
        JToken? downloadBehavior = serialized["downloadBehavior"];
        Assert.NotNull(downloadBehavior);
        Assert.Equal(JTokenType.Null, downloadBehavior.Type);
    }

    [Fact]
    public void TestCanSerializeParametersWithContexts()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            UserContexts = ["myUserContext"],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("downloadBehavior"));
        JToken? downloadBehavior = serialized["downloadBehavior"];
        Assert.NotNull(downloadBehavior);
        Assert.Equal(JTokenType.Null, downloadBehavior.Type);

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContexts = serialized["userContexts"];
        Assert.NotNull(userContexts);
        Assert.Equal(JTokenType.Array, userContexts.Type);
        JArray? contextsArray = serialized["userContexts"] as JArray;

        Assert.NotNull(contextsArray);
        Assert.Single(contextsArray);
        Assert.Equal(JTokenType.String, contextsArray[0].Type);
        Assert.Equal("myUserContext", contextsArray[0].Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAllowingDownloads()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorAllowed("my/destination/folder"),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("downloadBehavior"));
        JToken? downloadBehavior = serialized["downloadBehavior"];
        Assert.NotNull(downloadBehavior);
        Assert.Equal(JTokenType.Object, downloadBehavior.Type);

        JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
        Assert.NotNull(behaviorObject);
        Assert.Equal(2, behaviorObject.Count);

        Assert.True(behaviorObject.ContainsKey("type"));
        JToken? type = behaviorObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("allowed", type.Value<string>());

        Assert.True(behaviorObject.ContainsKey("destinationFolder"));
        JToken? destinationFolder = behaviorObject["destinationFolder"];
        Assert.NotNull(destinationFolder);
        Assert.Equal(JTokenType.String, destinationFolder.Type);
        Assert.Equal("my/destination/folder", destinationFolder.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAllowingDownloadsWithUserContexts()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorAllowed("my/destination/folder"),
            UserContexts = ["myUserContext"],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("downloadBehavior"));
        JToken? downloadBehavior = serialized["downloadBehavior"];
        Assert.NotNull(downloadBehavior);
        Assert.Equal(JTokenType.Object, downloadBehavior.Type);

        JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
        Assert.NotNull(behaviorObject);
        Assert.Equal(2, behaviorObject.Count);

        Assert.True(behaviorObject.ContainsKey("type"));
        JToken? type = behaviorObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("allowed", type.Value<string>());

        Assert.True(behaviorObject.ContainsKey("destinationFolder"));
        JToken? destinationFolder = behaviorObject["destinationFolder"];
        Assert.NotNull(destinationFolder);
        Assert.Equal(JTokenType.String, destinationFolder.Type);
        Assert.Equal("my/destination/folder", destinationFolder.Value<string>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContexts = serialized["userContexts"];
        Assert.NotNull(userContexts);
        Assert.Equal(JTokenType.Array, userContexts.Type);

        JArray? contextsArray = userContexts as JArray;
        Assert.NotNull(contextsArray);
        Assert.Single(contextsArray);
        Assert.Equal(JTokenType.String, contextsArray[0].Type);
        Assert.Equal("myUserContext", contextsArray[0].Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDenyingDownloads()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorDenied()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("downloadBehavior"));
        JToken? downloadBehavior = serialized["downloadBehavior"];
        Assert.NotNull(downloadBehavior);
        Assert.Equal(JTokenType.Object, downloadBehavior.Type);

        JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
        Assert.NotNull(behaviorObject);
        Assert.Single(behaviorObject);

        Assert.True(behaviorObject.ContainsKey("type"));
        JToken? type = behaviorObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("denied", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDenyingDownloadsWithUserContexts()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorDenied(),
            UserContexts = ["myUserContext"],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("downloadBehavior"));
        JToken? downloadBehavior = serialized["downloadBehavior"];
        Assert.NotNull(downloadBehavior);
        Assert.Equal(JTokenType.Object, downloadBehavior.Type);

        JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
        Assert.NotNull(behaviorObject);
        Assert.Single(behaviorObject);

        Assert.True(behaviorObject.ContainsKey("type"));
        JToken? type = behaviorObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("denied", type.Value<string>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContexts = serialized["userContexts"];
        Assert.NotNull(userContexts);
        Assert.Equal(JTokenType.Array, userContexts.Type);

        JArray? contextsArray = userContexts as JArray;
        Assert.NotNull(contextsArray);
        Assert.Single(contextsArray);
        Assert.Equal(JTokenType.String, contextsArray[0].Type);
        Assert.Equal("myUserContext", contextsArray[0].Value<string>());
    }

    [Fact]
    public void TestCanGetResetParameters()
    {
        SetDownloadBehaviorCommandParameters properties = SetDownloadBehaviorCommandParameters.ResetDownloadBehavior;
        Assert.NotNull(properties);

        Assert.Null(properties.DownloadBehavior);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetDownloadBehaviorCommandParameters firstInstance = SetDownloadBehaviorCommandParameters.ResetDownloadBehavior;
        SetDownloadBehaviorCommandParameters secondInstance = SetDownloadBehaviorCommandParameters.ResetDownloadBehavior;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
