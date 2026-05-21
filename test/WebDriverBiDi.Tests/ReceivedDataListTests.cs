namespace WebDriverBiDi;

public class ReceivedDataListTests
{
    [Fact]
    public void TestCanWrapList()
    {
        List<object?> list =
        [
            "myString",
            23,
            true,
            null,
            new List<object?>()
            {
                "list string",
                45,
                true,
                null,
                new List<object?>() { "list list string", null, 67 },
                new Dictionary<string, object?>() { { "list dictionary string", "string" } }
            },
            new Dictionary<string, object?>()
            {
                { "dictionary dictionary string", "string" }
            }
        ];

        ReceivedDataList received = new(list);
        IList<object?>? wrappedList = received[4] as IList<object?>;
        IDictionary<string, object?>? wrappedDict = received[5] as IDictionary<string, object?>;

        Assert.Equal("myString", received[0]);
        Assert.Equal(23, received[1]);
        Assert.Equal(true, received[2]);
        Assert.Null(received[3]);
        Assert.NotNull(wrappedList);
        Assert.IsType<ReceivedDataList>(wrappedList);
        Assert.ThrowsAny<NotSupportedException>(() => wrappedList.Add("foo"));
        Assert.NotNull(wrappedDict);
        Assert.IsType<ReceivedDataDictionary>(wrappedDict);
        Assert.ThrowsAny<NotSupportedException>(() => wrappedDict["newKey"] = "newValue");
        Assert.ThrowsAny<NotSupportedException>(() => wrappedDict["string"] = "newValue");
    }

    [Fact]
    public void TestCanUnwrapListToCopy()
    {
        List<object?> list =
        [
            "myString",
            23,
            true,
            null,
            new List<object?>()
            {
                "list string",
                45,
                true,
                null,
                new List<object?>() { "list list string", null, 67 },
                new Dictionary<string, object?>() { { "list dictionary string", "string" } }
            },
            new Dictionary<string, object?>()
            {
                { "dictionary dictionary string", "string" }
            }
        ];

        ReceivedDataList received = new(list);
        List<object?> unwrapped = received.ToWritableCopy();
        IList<object?>? unwrappedList = unwrapped[4] as IList<object?>;
        IDictionary<string, object?>? unwrappedDict = unwrapped[5] as IDictionary<string, object?>;

        Assert.Equal("myString", unwrapped[0]);
        Assert.Equal(23, unwrapped[1]);
        Assert.Equal(true, unwrapped[2]);
        Assert.Null(unwrapped[3]);
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
        Assert.Empty(ReceivedDataList.EmptyList);
    }
}
