namespace WebDriverBiDi.WebExtension;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class InstallCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "extension": "myExtensionId"
                      }
                      """;
        InstallCommandResult? result = JsonSerializer.Deserialize<InstallCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ExtensionId, Is.EqualTo("myExtensionId"));
    }

    [Test]
    public void TestDeserializingWithMissingExtensionThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<InstallCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidExtensionTypeThrows()
    {
        string json = """
                      {
                        "extension": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<InstallCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
