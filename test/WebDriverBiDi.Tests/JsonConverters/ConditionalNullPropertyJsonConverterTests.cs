namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.BrowsingContext;

[TestFixture]
public class ConditionalNullPropertyJsonConverterTests
{
    [Test]
    public void TestCanSerialize()
    {
        TestClass instance = new();
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.Zero);
    }

    [Test]
    public void TestCanSerializeWithViewportProperty()
    {
        TestClass instance = new()
        {
            ViewportProperty = new Viewport()
        };
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.That(serialized, Contains.Key("viewport"));
        Assert.That(serialized["viewport"], Is.Not.Null);
    }

    [Test]
    public void TestCanSerializeWithViewportNullProperty()
    {
        TestClass instance = new()
        {
            ViewportProperty = Viewport.ResetToDefaultViewport
        };
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.That(serialized, Contains.Key("viewport"));
        Assert.That(serialized["viewport"]!.Type, Is.EqualTo(JTokenType.Null));
    }

    [Test]
    public void TestCanSerializeWithDoubleProperty()
    {
        TestClass instance = new()
        {
            DoubleProperty = 2,
        };
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.That(serialized, Contains.Key("double"));
        Assert.That(serialized["double"]!.Value<double>(), Is.EqualTo(2));
    }

    [Test]
    public void TestCanSerializeWithDoubleNullProperty()
    {
        TestClass instance = new()
        {
            DoubleProperty = -1,
        };
        string json = JsonSerializer.Serialize(instance);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.That(serialized, Contains.Key("double"));
        Assert.That(serialized["double"]!.Type, Is.EqualTo(JTokenType.Null));
    }

    [Test]
    public void TestCanSerializeWithUnaffectedProperty()
    {
        TestClass instance = new()
        {
            StringProperty = "hello",
        };
        Assert.That(() => JsonSerializer.Serialize(instance), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestCannotDeserialize()
    {
        string json = """{ "double": 3 }""";
        Assert.That(() => JsonSerializer.Deserialize<TestClass>(json), Throws.InstanceOf<NotImplementedException>());
    }

    private class TestClass
    {
        [JsonConverter(typeof(ConditionalNullPropertyJsonConverter<Viewport>))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("viewport")]
        public Viewport? ViewportProperty { get; set; }

        [JsonConverter(typeof(ConditionalNullPropertyJsonConverter<double>))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("double")]
        public double? DoubleProperty { get; set; }

        [JsonConverter(typeof(ConditionalNullPropertyJsonConverter<string>))]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("string")]
        public string? StringProperty { get; set; }
    }
}
