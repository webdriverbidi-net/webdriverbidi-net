namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

public class NonNullElementListJsonConverterTests
{
    private static JsonSerializerOptions Options<T>()
        where T : class
    {
        return new JsonSerializerOptions { Converters = { new NonNullElementListJsonConverter<T>() } };
    }

    [Fact]
    public void TestDeserializingValidArray()
    {
        string json = """
                      [
                        { "name": "first", "value": { "type": "string", "value": "firstValue" } },
                        { "name": "second", "value": { "type": "string", "value": "secondValue" } }
                      ]
                      """;
        List<Header>? result = JsonSerializer.Deserialize<List<Header>>(json, Options<Header>());
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("first", result[0].Name);
        Assert.Equal("second", result[1].Name);
    }

    [Fact]
    public void TestDeserializingValidEmptyArray()
    {
        List<Header>? result = JsonSerializer.Deserialize<List<Header>>("[]", Options<Header>());
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void TestDeserializingArrayWithNullElementThrows()
    {
        string json = """
                      [
                        { "name": "first", "value": { "type": "string", "value": "firstValue" } },
                        null
                      ]
                      """;
        JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<List<Header>>(json, Options<Header>()));
        Assert.Contains("may not be null", exception.Message);
        Assert.Contains("Header", exception.Message);
    }

    [Fact]
    public void TestDeserializingArrayOfOnlyNullThrows()
    {
        JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<List<Header>>("[null]", Options<Header>()));
        Assert.Contains("may not be null", exception.Message);
    }

    [Fact]
    public void TestDeserializingNonArrayThrows()
    {
        JsonException exception = Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<List<Header>>("\"not-an-array\"", Options<Header>()));
        Assert.Contains("must be an array", exception.Message);
        Assert.Contains("String", exception.Message);
    }

    [Fact]
    public void TestDeserializingInvalidElementThrows()
    {
        // An element that is neither null nor valid for the element type must still be rejected by the
        // element type's own deserialization, not silently skipped by this converter.
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<List<RemoteValue>>("[\"not-a-remote-value\"]", Options<RemoteValue>()));
    }

    [Fact]
    public void TestSerializingIsATransparentPassThrough()
    {
        // Writing must produce exactly what the elements would produce without the converter.
        List<Header> headers = [new Header("first", "firstValue")];
        string withConverter = JsonSerializer.Serialize(headers, Options<Header>());
        string withoutConverter = JsonSerializer.Serialize(headers);
        Assert.Equal(withoutConverter, withConverter);
    }

    [Fact]
    public void TestSerializingTheProxyCapabilityStillEmitsItsAddressList()
    {
        // ManualProxyConfiguration.SerializableNoProxyAddresses, the shim behind NoProxyAddresses, is
        // the single member carrying this converter that is also sent, as part of the capabilities of
        // session.new, and it is the reason this converter implements Write rather than throwing as the
        // library's inbound-only converters do. If Write is ever changed to throw, this test fails
        // rather than the failure reaching a user's session.new.
        ManualProxyConfiguration proxy = new() { NoProxyAddresses = { "localhost", "127.0.0.1" } };
        Assert.Contains(@"""noProxy"":[""localhost"",""127.0.0.1""]", JsonSerializer.Serialize(proxy));
    }

    [Fact]
    public void TestSerializingEmptyList()
    {
        Assert.Equal("[]", JsonSerializer.Serialize(new List<Header>(), Options<Header>()));
    }

    [Fact]
    public void TestRoundTripPreservesElements()
    {
        List<Header> headers =
        [
            new Header("first", "firstValue"),
            new Header("second", "secondValue"),
        ];
        string json = JsonSerializer.Serialize(headers, Options<Header>());
        List<Header>? result = JsonSerializer.Deserialize<List<Header>>(json, Options<Header>());
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("first", result[0].Name);
        Assert.Equal("second", result[1].Name);
    }

    [Fact]
    public void TestDeserializingNullListIsHandledByTheSerializer()
    {
        // A null for the list itself, rather than for an element, is the serializer's business: it
        // short-circuits before the converter is invoked, so a nullable member stays null.
        Assert.Null(JsonSerializer.Deserialize<List<Header>>("null", Options<Header>()));
    }
}
