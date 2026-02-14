namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class GattConnectionAttemptedEventArgsTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress"
                      }
                      """;
        GattConnectionAttemptedEventArgs? eventArgs = JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress"
                      }
                      """;
        GattConnectionAttemptedEventArgs? eventArgs = JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        GattConnectionAttemptedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
   }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "address": "myAddress"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingAddressThrows()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json), Throws.InstanceOf<JsonException>());
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
        Assert.That(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
