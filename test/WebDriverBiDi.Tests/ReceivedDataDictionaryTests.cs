namespace WebDriverBiDi;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

[TestFixture]
public class ReceivedDataDictionaryTests
{
    [Test]
    public void TestCanWrapDictionary()
    {
        Dictionary<string, object?> dictionary = new()
        {
            { "string", "myString" },
            { "long", 23 },
            { "bool", true },
            { "null", null },
            { "list", new List<object?>()
                {
                    "list string",
                    45,
                    true,
                    null,
                    new List<object?>() { "list list string", null, 67 },
                    new Dictionary<string, object?>() { { "list dictionary string", "string" } }
                }
            },
            { "dictionary", new Dictionary<string, object?>()
                {
                    { "dictionary dictionary string", "string" }
                }
            }
        };

        ReceivedDataDictionary received = new(dictionary);
        IList<object?>? wrappedList = received["list"] as IList<object?>;
        IDictionary<string, object?>? wrappedDict = received["dictionary"] as IDictionary<string, object?>;
        Assert.Multiple(() =>
        {
            Assert.That(received["string"], Is.EqualTo("myString"));
            Assert.That(received["long"], Is.EqualTo(23));
            Assert.That(received["bool"], Is.EqualTo(true));
            Assert.That(received["null"], Is.Null);
            Assert.That(wrappedList, Is.Not.Null);
            Assert.That(wrappedList!, Is.InstanceOf<ReceivedDataList>());
            Assert.That(() => wrappedList!.Add("foo"), Throws.InstanceOf<NotSupportedException>());
            Assert.That(wrappedDict, Is.Not.Null);
            Assert.That(wrappedDict, Is.InstanceOf<ReceivedDataDictionary>());
            Assert.That(() => wrappedDict!["newKey"] = "newValue", Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => wrappedDict!["string"] = "newValue", Throws.InstanceOf<NotSupportedException>());
        });
    }

    [Test]
    public void TestCanUnwrapDictionaryToCopy()
    {
        Dictionary<string, object?> dictionary = new()
        {
            { "string", "myString" },
            { "long", 23 },
            { "bool", true },
            { "null", null },
            { "list", new List<object?>()
                {
                    "list string",
                    45,
                    true,
                    null,
                    new List<object?>() { "list list string", null, 67 },
                    new Dictionary<string, object?>() { { "list dictionary string", "string" } }
                }
            },
            { "dictionary", new Dictionary<string, object?>()
                {
                    { "dictionary dictionary string", "string" }
                }
            }
        };

        ReceivedDataDictionary received = new(dictionary);
        IDictionary<string, object?> unwrapped = received.ToWritableCopy();
        IList<object?>? unwrappedList = unwrapped["list"] as IList<object?>;
        IDictionary<string, object?>? unwrappedDict = unwrapped["dictionary"] as IDictionary<string, object?>;
        Assert.Multiple(() =>
        {
            Assert.That(unwrapped["string"], Is.EqualTo("myString"));
            Assert.That(unwrapped["long"], Is.EqualTo(23));
            Assert.That(unwrapped["bool"], Is.EqualTo(true));
            Assert.That(unwrapped["null"], Is.Null);
            Assert.That(unwrappedList, Is.Not.Null);
            Assert.That(unwrappedList!, Is.InstanceOf<List<object?>>());
            Assert.That(unwrappedDict, Is.Not.Null);
            Assert.That(unwrappedDict, Is.InstanceOf<Dictionary<string, object?>>());
        });

        unwrappedList!.Add("foo");
        unwrappedDict!["bar"] = "baz";
    }

    [Test]
    public void TestEmptyDictionary()
    {
        Assert.That(ReceivedDataDictionary.EmptyDictionary, Is.Empty);
    }

    [Test]
    public void TestCanCreateFromDeserializedData()
    {
        string json = @"{ ""stringProperty"": ""stringValue"", ""intValue"": 123, ""floatValue"": 456.78, ""trueBoolValue"": true, ""falseBoolValue"": false, ""nullValue"": null, ""listValue"": [ ""listString"", 901, true, null ], ""objectValue"": { ""objectProperty"": ""objectValue"" } }";
        OnlyOverflowData? deserializedValue = JsonSerializer.Deserialize<OnlyOverflowData>(json);
        Assert.That(deserializedValue!.ReceivedData, Has.Count.EqualTo(8));
    }

    private class OnlyOverflowData
    {
        private Dictionary<string, JsonElement> overflowData = [];
        private ReceivedDataDictionary receivedData = ReceivedDataDictionary.EmptyDictionary;

        [JsonIgnore]
        public ReceivedDataDictionary ReceivedData
        {
            get
            {
                if (this.overflowData.Count > 0 && this.receivedData.Count == 0)
                {
                    this.receivedData = JsonConverterUtilities.ConvertIncomingExtensionData(this.overflowData);
                }

                return this.receivedData;
            }
        }

        [JsonExtensionData]
        [JsonInclude]
        private Dictionary<string, JsonElement> OverflowData { get => this.overflowData; set => this.overflowData = value; }
    }
}
