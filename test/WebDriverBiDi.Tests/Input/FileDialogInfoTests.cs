namespace WebDriverBiDi.Input;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class FileDialogInfoTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeWithMultipleTrue()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": true
                      }
                      """;
        FileDialogInfo? info = JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<FileDialogInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Multiple, Is.True);
            Assert.That(info.Element, Is.Null);
        });
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
        FileDialogInfo? info = JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<FileDialogInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Multiple, Is.False);
            Assert.That(info.Element, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithElementReference()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "multiple": true,
                        "element": {
                          "type": "node",
                          "sharedId": "mySharedId",
                          "value": {
                            "nodeType": 1,
                            "nodeValue": "",
                            "childNodeCount": 0
                          }
                        }
                      }
                      """;
        FileDialogInfo? info = JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<FileDialogInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Multiple, Is.True);
            Assert.That(info.Element, Is.Not.Null);
            Assert.That(info.Element!.SharedId, Is.EqualTo("mySharedId"));
        });
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "multiple": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingMMultipleThrows()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }
}