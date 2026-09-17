namespace WebDriverBiDi.JsonConverters;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.BrowsingContext;

public class SentinelNullJsonConverterTests
{
    [Fact]
    public void TestCanSerialize()
    {
        TestClass instance = new();
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeWithViewportProperty()
    {
        TestClass instance = new()
        {
            ViewportProperty = new Viewport()
        };
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("viewport"));
        Assert.NotNull(serialized["viewport"]);
    }

    [Fact]
    public void TestCanSerializeWithViewportNullProperty()
    {
        TestClass instance = new()
        {
            ViewportProperty = Viewport.ResetToDefaultViewport
        };
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("viewport"));
        JToken? viewport = serialized["viewport"];
        Assert.NotNull(viewport);
        Assert.Equal(JTokenType.Null, viewport.Type);
    }

    [Fact]
    public void TestCanSerializeWithDoubleProperty()
    {
        TestClass instance = new()
        {
            DoubleProperty = 2,
        };
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("double"));
        JToken? doubleToken = serialized["double"];
        Assert.NotNull(doubleToken);
        Assert.Equal(2, doubleToken.Value<double>());
    }

    [Fact]
    public void TestCanSerializeWithDoubleNullProperty()
    {
        TestClass instance = new()
        {
            DoubleProperty = -1,
        };
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("double"));
        JToken? doubleToken = serialized["double"];
        Assert.NotNull(doubleToken);
        Assert.Equal(JTokenType.Null, doubleToken.Type);
    }

    [Fact]
    public void TestSerializingWithoutValueConverterUsesTypeMetadataForValue()
    {
        // The two-argument converter writes a value that is not the sentinel with the serializer's own
        // metadata, so an integer-valued double is written as a JSON integer.
        TestClass instance = new()
        {
            DoubleProperty = 2,
        };
        string json = JsonSerializer.Serialize(instance);
        Assert.Equal("""{"double":2}""", json);
    }

    [Fact]
    public void TestCanSerializeWithValueConverterProperty()
    {
        TestClass instance = new()
        {
            FixedDoubleProperty = 2,
        };
        string json = JsonSerializer.Serialize(instance);
        Assert.Equal("""{"fixedDouble":2.0}""", json);
        JObject serialized = JObject.Parse(json);
        JToken? doubleToken = serialized["fixedDouble"];
        Assert.NotNull(doubleToken);
        Assert.Equal(JTokenType.Float, doubleToken.Type);
        Assert.Equal(2.0, doubleToken.Value<double>());
    }

    [Fact]
    public void TestCanSerializeWithValueConverterNullProperty()
    {
        TestClass instance = new()
        {
            FixedDoubleProperty = -1,
        };
        string json = JsonSerializer.Serialize(instance);
        Assert.Equal("""{"fixedDouble":null}""", json);
    }

    [Fact]
    public void TestSerializingWithValueConverterPropagatesValueConverterFailure()
    {
        // The value converter, not the serializer's default handling, writes the value, so its own
        // rejection of a value JSON cannot represent is what surfaces.
        TestClass instance = new()
        {
            FixedDoubleProperty = double.PositiveInfinity,
        };
        Assert.Throws<JsonException>(() => JsonSerializer.Serialize(instance));
    }

    [Fact]
    public void TestCannotDeserialize()
    {
        string json = """{ "double": 3 }""";
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Deserialize<TestClass>(json));
    }

    [Fact]
    public void TestCannotDeserializeWithValueConverter()
    {
        string json = """{ "fixedDouble": 3 }""";
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Deserialize<TestClass>(json));
    }

    [Fact]
    public void TestWriteWithNullValueWritesNothing()
    {
        SentinelNullJsonConverter<Viewport, ViewportSentinelChecker> converter = new();
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        writer.WriteStartObject();
        converter.Write(writer, null!, JsonSerializerOptions.Default);
        writer.WriteEndObject();
        writer.Flush();
        string json = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal("{}", json);
    }

    private class TestClass
    {
        [JsonConverter(typeof(SentinelNullJsonConverter<Viewport, ViewportSentinelChecker>))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("viewport")]
        public Viewport? ViewportProperty { get; set; }

        [JsonConverter(typeof(SentinelNullJsonConverter<double, NegativeDoubleSentinelChecker>))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("double")]
        public double? DoubleProperty { get; set; }

        [JsonConverter(typeof(SentinelNullJsonConverter<double, NegativeDoubleSentinelChecker, FixedDoubleJsonConverter>))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("fixedDouble")]
        public double? FixedDoubleProperty { get; set; }
    }
}
