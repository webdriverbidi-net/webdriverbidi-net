namespace WebDriverBiDi;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.Internal;

public class ReceivedDataDictionaryTests
{
    [Fact]
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

        Assert.Equal("myString", received["string"]);
        Assert.Equal(23, received["long"]);
        Assert.Equal(true, received["bool"]);
        Assert.Null(received["null"]);
        Assert.NotNull(wrappedList);
        Assert.IsType<ReceivedDataList>(wrappedList);
        Assert.ThrowsAny<NotSupportedException>(() => wrappedList.Add("foo"));
        Assert.NotNull(wrappedDict);
        Assert.IsType<ReceivedDataDictionary>(wrappedDict);
        Assert.ThrowsAny<NotSupportedException>(() => wrappedDict["newKey"] = "newValue");
        Assert.ThrowsAny<NotSupportedException>(() => wrappedDict["string"] = "newValue");
    }

    [Fact]
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

        Assert.Equal("myString", unwrapped["string"]);
        Assert.Equal(23, unwrapped["long"]);
        Assert.Equal(true, unwrapped["bool"]);
        Assert.Null(unwrapped["null"]);
        Assert.NotNull(unwrappedList);
        Assert.IsType<List<object?>>(unwrappedList);
        Assert.NotNull(unwrappedDict);
        Assert.IsType<Dictionary<string, object?>>(unwrappedDict);

        unwrappedList.Add("foo");
        unwrappedDict["bar"] = "baz";
    }

    [Fact]
    public void TestEmptyDictionary()
    {
        Assert.Empty(ReceivedDataDictionary.EmptyDictionary);
    }
}
