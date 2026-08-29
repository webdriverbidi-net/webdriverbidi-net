namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class UserPromptClosedEventArgsTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserializeWithAcceptedTrue()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "accepted": true
                      }
                      """;
        UserPromptClosedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal(UserPromptType.Confirm, eventArgs.PromptType);
        Assert.True(eventArgs.IsAccepted);
        Assert.Null(eventArgs.UserText);
    }

    [Fact]
    public void TestCanDeserializeWithAcceptedFalse()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "accepted": false
                      }
                      """;
        UserPromptClosedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.False(eventArgs.IsAccepted);
        Assert.Null(eventArgs.UserText);
    }

    [Fact]
    public void TestCanDeserializeWithUserText()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "accepted": true,
                        "userText": "some text"
                      }
                      """;
        UserPromptClosedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.True(eventArgs.IsAccepted);
        Assert.Equal("some text", eventArgs.UserText);
    }

    [Fact]
    public void TestCanDeserializeWithUserContext()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "accepted": true,
                        "userContext": "myUserContextId"
                      }
                      """;
        UserPromptClosedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.True(eventArgs.IsAccepted);
        Assert.Equal("myUserContextId", eventArgs.UserContextId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "accepted": true
                      }
                      """;
        UserPromptClosedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);
        UserPromptClosedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = """
                      {
                        "accepted": true
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = """
                      {
                        "context": {},
                        "accepted": true
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithNullContextValueThrows()
    {
        string json = """
                      {
                        "context": null,
                        "accepted": true
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithMissingAcceptedValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithInvalidAcceptedValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "accepted": "some value"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithnullAcceptedValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "accepted": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithInvalidUserTextValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "accepted": true,
                        "userText": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithMissingTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "accepted": true
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json, this.options));
    }
}
