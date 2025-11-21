namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class MessageEventArgsTests
{
    [Test]
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
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.ChannelId, Is.EqualTo("myChannel"));
            Assert.That(eventArgs.Data.Type, Is.EqualTo("string"));
            Assert.That(eventArgs.Data.ValueAs<string>(), Is.EqualTo("myChannelValue"));
            Assert.That(eventArgs.Source.RealmId, Is.EqualTo("myRealm"));
        }
    }

    [Test]
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
        Assert.That(eventArgs, Is.Not.Null);
        MessageEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<MessageEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<MessageEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<MessageEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<MessageEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<MessageEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<MessageEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
