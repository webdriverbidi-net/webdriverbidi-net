namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class GattConnectionAttemptedEventArgsTests
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
                        "context": "myContextId",
                        "address": "myAddress"
                      }
                      """;
        GattConnectionAttemptedEventArgs? eventArgs = JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
        });
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "address": "myAddress"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "address": "myAddress"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingAddressThrows()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidAddressTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
