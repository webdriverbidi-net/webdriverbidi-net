namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.Session;

public class UserPromptOpenedEventArgsTests
{
    [Fact]
    public void TestCanDeserializeWithTypeAlert()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Alert, eventArgs.PromptType);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Null(eventArgs.DefaultValue);
    }

    [Fact]
    public void TestCanDeserializeWithTypeConfirm()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Confirm, eventArgs.PromptType);
        Assert.Equal(UserPromptHandlerType.Accept, eventArgs.Handler);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Null(eventArgs.DefaultValue);
    }

    [Fact]
    public void TestCanDeserializeWithTypePrompt()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "prompt",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Prompt, eventArgs.PromptType);
        Assert.Equal(UserPromptHandlerType.Accept, eventArgs.Handler);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Null(eventArgs.DefaultValue);
    }

    [Fact]
    public void TestCanDeserializeWithTypeBeforeUnload()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "beforeunload",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.BeforeUnload, eventArgs.PromptType);
        Assert.Equal(UserPromptHandlerType.Accept, eventArgs.Handler);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Null(eventArgs.DefaultValue);
    }

    [Fact]
    public void TestCanDeserializeWithHandlerAccept()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Alert, eventArgs.PromptType);
        Assert.Equal(UserPromptHandlerType.Accept, eventArgs.Handler);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Null(eventArgs.DefaultValue);
    }

    [Fact]
    public void TestCanDeserializeWithHandlerDismiss()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "dismiss",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Alert, eventArgs.PromptType);
        Assert.Equal(UserPromptHandlerType.Dismiss, eventArgs.Handler);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Null(eventArgs.DefaultValue);
    }

    [Fact]
    public void TestCanDeserializeWithHandlerIgnore()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "ignore",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Alert, eventArgs.PromptType);
        Assert.Equal(UserPromptHandlerType.Ignore, eventArgs.Handler);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Null(eventArgs.DefaultValue);
    }

    [Fact]
    public void TestCanDeserializeWithOptionalDefaultValue()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "prompt",
                        "handler": "accept",
                        "message": "some prompt message",
                        "defaultValue": "prompt default"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Prompt, eventArgs.PromptType);
        Assert.Equal(UserPromptHandlerType.Accept, eventArgs.Handler);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Equal("prompt default", eventArgs.DefaultValue);
    }

    [Fact]
    public void TestCanDeserializeWithUserContext()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "prompt",
                        "handler": "accept",
                        "message": "some prompt message",
                        "userContext": "myUserContextId"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Prompt, eventArgs.PromptType);
        Assert.Equal(UserPromptHandlerType.Accept, eventArgs.Handler);
        Assert.Equal("some prompt message", eventArgs.Message);
        Assert.Equal("myUserContextId", eventArgs.UserContextId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);
        UserPromptOpenedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = """
                      {
                        "type": "beforeunload",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = """
                      {
                        "context": {},
                        "type": "beforeunload",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "invalid",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingHandlerTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "message": "some prompt message"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidHandlerTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "invalid",
                        "message": "some prompt message"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingMessageValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "beforeunload",
                        "handler": "accept"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidMessageValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "beforeunload",
                        "handler": "accept",
                        "message": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json));
    }
}
