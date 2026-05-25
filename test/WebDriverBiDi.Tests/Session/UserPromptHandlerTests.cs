namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class UserPromptHandlerTests
{
    [Fact]
    public void TestCanSerialize()
    {
        UserPromptHandler handler = new();
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeWithAllOptions()
    {
        UserPromptHandler handler = new()
        {
            Default = UserPromptHandlerType.Accept,
            Alert = UserPromptHandlerType.Accept,
            Confirm = UserPromptHandlerType.Dismiss,
            Prompt = UserPromptHandlerType.Dismiss,
            BeforeUnload = UserPromptHandlerType.Ignore,
            File = UserPromptHandlerType.Ignore,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(6, serialized.Count);

        Assert.True(serialized.ContainsKey("default"));
        JToken? defaultToken = serialized["default"];
        Assert.NotNull(defaultToken);
        Assert.Equal(JTokenType.String, defaultToken.Type);
        Assert.Equal("accept", defaultToken.Value<string>());

        Assert.True(serialized.ContainsKey("alert"));
        JToken? alert = serialized["alert"];
        Assert.NotNull(alert);
        Assert.Equal(JTokenType.String, alert.Type);
        Assert.Equal("accept", alert.Value<string>());

        Assert.True(serialized.ContainsKey("confirm"));
        JToken? confirm = serialized["confirm"];
        Assert.NotNull(confirm);
        Assert.Equal(JTokenType.String, confirm.Type);
        Assert.Equal("dismiss", confirm.Value<string>());

        Assert.True(serialized.ContainsKey("prompt"));
        JToken? prompt = serialized["prompt"];
        Assert.NotNull(prompt);
        Assert.Equal(JTokenType.String, prompt.Type);
        Assert.Equal("dismiss", prompt.Value<string>());

        Assert.True(serialized.ContainsKey("beforeunload"));
        JToken? beforeunload = serialized["beforeunload"];
        Assert.NotNull(beforeunload);
        Assert.Equal(JTokenType.String, beforeunload.Type);
        Assert.Equal("ignore", beforeunload.Value<string>());

        Assert.True(serialized.ContainsKey("file"));
        JToken? file = serialized["file"];
        Assert.NotNull(file);
        Assert.Equal(JTokenType.String, file.Type);
        Assert.Equal("ignore", file.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithOnlyDefault()
    {
        UserPromptHandler handler = new()
        {
            Default = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("default"));
        JToken? defaultToken = serialized["default"];
        Assert.NotNull(defaultToken);
        Assert.Equal(JTokenType.String, defaultToken.Type);
        Assert.Equal("accept", defaultToken.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithOnlyAlert()
    {
        UserPromptHandler handler = new()
        {
            Alert = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("alert"));
        JToken? alert = serialized["alert"];
        Assert.NotNull(alert);
        Assert.Equal(JTokenType.String, alert.Type);
        Assert.Equal("accept", alert.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithOnlyConfirm()
    {
        UserPromptHandler handler = new()
        {
            Confirm = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("confirm"));
        JToken? confirm = serialized["confirm"];
        Assert.NotNull(confirm);
        Assert.Equal(JTokenType.String, confirm.Type);
        Assert.Equal("accept", confirm.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithOnlyPrompt()
    {
        UserPromptHandler handler = new()
        {
            Prompt = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("prompt"));
        JToken? prompt = serialized["prompt"];
        Assert.NotNull(prompt);
        Assert.Equal(JTokenType.String, prompt.Type);
        Assert.Equal("accept", prompt.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithOnlyBeforeUnload()
    {
        UserPromptHandler handler = new()
        {
            BeforeUnload = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("beforeunload"));
        JToken? beforeunload = serialized["beforeunload"];
        Assert.NotNull(beforeunload);
        Assert.Equal(JTokenType.String, beforeunload.Type);
        Assert.Equal("accept", beforeunload.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithOnlyFile()
    {
        UserPromptHandler handler = new()
        {
            File = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("file"));
        JToken? file = serialized["file"];
        Assert.NotNull(file);
        Assert.Equal(JTokenType.String, file.Type);
        Assert.Equal("accept", file.Value<string>());
    }

    [Fact]
    public void TestCanDeserialize()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json);
        Assert.NotNull(handler);

        Assert.Null(handler.Default);
        Assert.Null(handler.Alert);
        Assert.Null(handler.Confirm);
        Assert.Null(handler.Prompt);
        Assert.Null(handler.BeforeUnload);
        Assert.Null(handler.File);
    }

    [Fact]
    public void TestCanDeserializeWithAllOptions()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "default": "accept",
                        "alert": "accept",
                        "confirm": "dismiss",
                        "prompt": "dismiss",
                        "beforeunload": "ignore",
                        "file": "ignore"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json);
        Assert.NotNull(handler);

        Assert.Equal(UserPromptHandlerType.Accept, handler.Default);
        Assert.Equal(UserPromptHandlerType.Accept, handler.Alert);
        Assert.Equal(UserPromptHandlerType.Dismiss, handler.Confirm);
        Assert.Equal(UserPromptHandlerType.Dismiss, handler.Prompt);
        Assert.Equal(UserPromptHandlerType.Ignore, handler.BeforeUnload);
        Assert.Equal(UserPromptHandlerType.Ignore, handler.File);
    }

    [Fact]
    public void TestCanDeserializeUserPromptHandlerWithOnlyDefault()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "default": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json);
        Assert.NotNull(handler);

        Assert.Equal(UserPromptHandlerType.Accept, handler.Default);
        Assert.Null(handler.Alert);
        Assert.Null(handler.Confirm);
        Assert.Null(handler.Prompt);
        Assert.Null(handler.BeforeUnload);
        Assert.Null(handler.File);
    }

    [Fact]
    public void TestCanDeserializeUserPromptHandlerWithOnlyAlert()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "alert": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json);
        Assert.NotNull(handler);

        Assert.Null(handler.Default);
        Assert.Equal(UserPromptHandlerType.Accept, handler.Alert);
        Assert.Null(handler.Confirm);
        Assert.Null(handler.Prompt);
        Assert.Null(handler.BeforeUnload);
        Assert.Null(handler.File);
    }

    [Fact]
    public void TestCanDeserializeUserPromptHandlerWithOnlyConfirm()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "confirm": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json);
        Assert.NotNull(handler);

        Assert.Null(handler.Default);
        Assert.Null(handler.Alert);
        Assert.Equal(UserPromptHandlerType.Accept, handler.Confirm);
        Assert.Null(handler.Prompt);
        Assert.Null(handler.BeforeUnload);
        Assert.Null(handler.File);
    }

    [Fact]
    public void TestCanDeserializeUserPromptHandlerWithOnlyPrompt()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "prompt": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json);
        Assert.NotNull(handler);

        Assert.Null(handler.Default);
        Assert.Null(handler.Alert);
        Assert.Null(handler.Confirm);
        Assert.Equal(UserPromptHandlerType.Accept, handler.Prompt);
        Assert.Null(handler.BeforeUnload);
        Assert.Null(handler.File);
    }

    [Fact]
    public void TestCanDeserializeUserPromptHandlerWithOnlyBeforeUnload()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "beforeunload": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json);
        Assert.NotNull(handler);

        Assert.Null(handler.Default);
        Assert.Null(handler.Alert);
        Assert.Null(handler.Confirm);
        Assert.Null(handler.Prompt);
        Assert.Equal(UserPromptHandlerType.Accept, handler.BeforeUnload);
        Assert.Null(handler.File);
    }

    [Fact]
    public void TestCanDeserializeUserPromptHandlerWithOnlyFile()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "file": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json);
        Assert.NotNull(handler);

        Assert.Null(handler.Default);
        Assert.Null(handler.Alert);
        Assert.Null(handler.Confirm);
        Assert.Null(handler.Prompt);
        Assert.Null(handler.BeforeUnload);
        Assert.Equal(UserPromptHandlerType.Accept, handler.File);
    }

    [Fact]
    public void TestCanDeserializingWithInvalidTypeValueThrows()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "default": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptHandler>(json));
    }
}
