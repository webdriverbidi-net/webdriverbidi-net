namespace WebDriverBiDi.Input;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class FileDialogOpenedEventArgsTests
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
        FileDialogOpenedEventArgs eventArgs = new(info);
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
        FileDialogInfo? info = JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<FileDialogInfo>());
        FileDialogOpenedEventArgs eventArgs = new(info);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.IsMultiple, Is.False);
            Assert.That(eventArgs.Element, Is.Null);
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
        FileDialogOpenedEventArgs eventArgs = new(info);
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
        FileDialogInfo? info = JsonSerializer.Deserialize<FileDialogInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<FileDialogInfo>());
        FileDialogOpenedEventArgs eventArgs = new(info);
        FileDialogOpenedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
     }
}
