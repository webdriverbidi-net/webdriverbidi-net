namespace WebDriverBiDi.Script;

using System.Text.Json;

public class MessageEventArgsTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "channel": "myChannel", 
                        "data": {
                          "type": "string",
                          "value": "myChannelValue"
                        },
                        "source": {
                          "realm": "myRealm" 
                        }
                      }
                      """;
        MessageEventArgs? eventArgs = JsonSerializer.Deserialize<MessageEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myChannel", eventArgs.ChannelId);
        Assert.Equal(RemoteValueType.String, eventArgs.Data.Type);
        Assert.Equal("myChannelValue", eventArgs.Data.ConvertTo<StringRemoteValue>().Value);
        Assert.Equal("myRealm", eventArgs.Source.RealmId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "channel": "myChannel", 
                        "data": {
                          "type": "string",
                          "value": "myChannelValue"
                        },
                        "source": {
                          "realm": "myRealm" 
                        }
                      }
                      """;
        MessageEventArgs? eventArgs = JsonSerializer.Deserialize<MessageEventArgs>(json);
        Assert.NotNull(eventArgs);
        MessageEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingChannelValueThrows()
    {
        string json = """
                      {
                        "data": {
                          "type": "string",
                          "value": "myChannelValue"
                        },
                        "source": {
                          "realm": "myRealm" 
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<MessageEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidChannelValueThrows()
    {
        string json = """
                      {
                        "channel": {},
                        "data": {
                          "type": "string",
                          "value": "myChannelValue"
                        },
                        "source": {
                          "realm": "myRealm" 
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<MessageEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingDataValueThrows()
    {
        string json = """
                      {
                        "channel": "myChannel",
                        "source": {
                          "realm": "myRealm" 
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<MessageEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidDataValueThrows()
    {
        string json = """
                      {
                        "channel": "myChannel",
                        "data": "invalidData",
                        "source": {
                          "realm": "myRealm" 
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<MessageEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingSourceValueThrows()
    {
        string json = """
                      {
                        "channel": "myChannel",
                        "data": {
                          "type": "string",
                          "value": "myChannelValue"
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<MessageEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidSourceValueThrows()
    {
        string json = """
                      {
                        "channel": "myChannel", 
                        "data": {
                          "type": "string",
                          "value": "myChannelValue"
                        },
                        "source": ""
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<MessageEventArgs>(json));
    }
}
