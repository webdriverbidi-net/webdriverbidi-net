namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class GattConnectionAttemptedEventArgsTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress"
                      }
                      """;
        GattConnectionAttemptedEventArgs? eventArgs = JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress"
                      }
                      """;
        GattConnectionAttemptedEventArgs? eventArgs = JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);
        GattConnectionAttemptedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "address": "myAddress"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "address": "myAddress"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullContextThrows()
    {
        string json = """
                      {
                        "context": null,
                        "address": "myAddress"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingAddressThrows()
    {
        string json = """
                      {
                        "context": "myContextId"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidAddressTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullAddressTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<GattConnectionAttemptedEventArgs>(json, this.options));
    }
}
