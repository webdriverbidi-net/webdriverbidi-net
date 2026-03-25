namespace WebDriverBiDi.Input;

using System.Text.Json;

[TestFixture]
public class FileDialogInfoTests
{
    [Test]
    public void TestCanDeserializeWithMultipleTrue()
    {
        string json = """
                        {
                            "context": "myContextId",
                            "multiple": true
                        }
                        """;
        FileDialogOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs, Is.InstanceOf<FileDialogOpenedEventArgs>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.IsMultiple, Is.True);
            Assert.That(eventArgs.Element, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithMultipleFalse()
    {
        string json = """
                        {
                            "context": "myContextId",
                            "multiple": false
                        }
                        """;
        FileDialogOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs, Is.InstanceOf<FileDialogOpenedEventArgs>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.IsMultiple, Is.False);
            Assert.That(eventArgs.Element, Is.Null);
        }
    }

    [Test]
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
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs, Is.InstanceOf<FileDialogOpenedEventArgs>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.IsMultiple, Is.False);
            Assert.That(eventArgs.Element, Is.Null);
            Assert.That(eventArgs.UserContextId, Is.EqualTo("myUserContextId"));
        }
    }

    [Test]
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
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs, Is.InstanceOf<FileDialogOpenedEventArgs>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.IsMultiple, Is.True);
            Assert.That(eventArgs.Element, Is.Not.Null);
            Assert.That(eventArgs.Element!.SharedId, Is.EqualTo("mySharedId"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                        {
                          "context": "myContextId",
                          "multiple": true
                        }
                        """;
        FileDialogOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs, Is.InstanceOf<FileDialogOpenedEventArgs>());
        FileDialogOpenedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "multiple": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "multiple": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingMMultipleThrows()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidMMultipleTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": "true"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidUserContextTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": true,
                        "userContext": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidElementTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": true,
                        "element": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<FileDialogOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
