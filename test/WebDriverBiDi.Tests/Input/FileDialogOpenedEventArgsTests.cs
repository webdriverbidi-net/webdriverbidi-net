namespace WebDriverBiDi.Input;

using System.Text.Json;

public class FileDialogInfoTests
{
    [Fact]
    public void TestCanDeserializeWithMultipleTrue()
    {
        string json = """
                        {
                            "context": "myContextId",
                            "multiple": true
                        }
                        """;
        FileDialogOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);
        Assert.IsType<FileDialogOpenedEventArgs>(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.True(eventArgs.IsMultiple);
        Assert.Null(eventArgs.Element);
    }

    [Fact]
    public void TestCanDeserializeWithMultipleFalse()
    {
        string json = """
                        {
                            "context": "myContextId",
                            "multiple": false
                        }
                        """;
        FileDialogOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);
        Assert.IsType<FileDialogOpenedEventArgs>(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.False(eventArgs.IsMultiple);
        Assert.Null(eventArgs.Element);
    }

    [Fact]
    public void TestCanDeserializeWithUserContext()
    {
        string json = """
                        {
                            "context": "myContextId",
                            "multiple": false,
                            "userContext": "myUserContextId"
                        }
                        """;
        FileDialogOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);
        Assert.IsType<FileDialogOpenedEventArgs>(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.False(eventArgs.IsMultiple);
        Assert.Null(eventArgs.Element);
        Assert.Equal("myUserContextId", eventArgs.UserContextId);
    }

    [Fact]
    public void TestCanDeserializeWithElementReference()
    {
        string json = """
                    {
                      "context": "myContextId",
                      "multiple": true,
                      "element": {
                        "sharedId": "mySharedId"
                      }
                    }
                    """;
        FileDialogOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);
        Assert.IsType<FileDialogOpenedEventArgs>(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.True(eventArgs.IsMultiple);
        Assert.NotNull(eventArgs.Element);
        Assert.Equal("mySharedId", eventArgs.Element.SharedId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                        {
                          "context": "myContextId",
                          "multiple": true
                        }
                        """;
        FileDialogOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json);
        Assert.NotNull(eventArgs);
        Assert.IsType<FileDialogOpenedEventArgs>(eventArgs);
        FileDialogOpenedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "multiple": true
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "multiple": true
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingMMultipleThrows()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidMMultipleTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": "true"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidUserContextTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": true,
                        "userContext": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidElementTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": true,
                        "element": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithNonSharedReferenceThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": true,
                        "element": {
                          "type": "node",
                          "value": {
                            "nodeType": 1,
                            "nodeValue": "",
                            "childNodeCount": 0
                          }
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json));
    }
}
