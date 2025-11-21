namespace WebDriverBiDi;

[TestFixture]
public class ReceivedDataListTests
{
    [Test]
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(received[0], Is.EqualTo("myString"));
            Assert.That(received[1], Is.EqualTo(23));
            Assert.That(received[2], Is.EqualTo(true));
            Assert.That(received[3], Is.Null);
            Assert.That(wrappedList, Is.Not.Null);
            Assert.That(wrappedList!, Is.InstanceOf<ReceivedDataList>());
            Assert.That(() => wrappedList!.Add("foo"), Throws.InstanceOf<NotSupportedException>());
            Assert.That(wrappedDict, Is.Not.Null);
            Assert.That(wrappedDict, Is.InstanceOf<ReceivedDataDictionary>());
            Assert.That(() => wrappedDict!["newKey"] = "newValue", Throws.InstanceOf<NotSupportedException>());
            Assert.That(() => wrappedDict!["string"] = "newValue", Throws.InstanceOf<NotSupportedException>());
        }
    }

    [Test]
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unwrapped[0], Is.EqualTo("myString"));
            Assert.That(unwrapped[1], Is.EqualTo(23));
            Assert.That(unwrapped[2], Is.EqualTo(true));
            Assert.That(unwrapped[3], Is.Null);
            Assert.That(unwrappedList, Is.Not.Null);
            Assert.That(unwrappedList!, Is.InstanceOf<List<object?>>());
            Assert.That(unwrappedDict, Is.Not.Null);
            Assert.That(unwrappedDict, Is.InstanceOf<Dictionary<string, object?>>());
        }

        unwrappedList!.Add("foo");
        unwrappedDict!["bar"] = "baz";
    }

    [Test]
    public void TestEmptyDictionary()
    {
        Assert.That(ReceivedDataList.EmptyList, Is.Empty);
    }
}
