namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class PrintCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""data"": ""some print data"" }";
        PrintCommandResult? result = JsonConvert.DeserializeObject<PrintCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data, Is.EqualTo("some print data"));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<PrintCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = @"{ ""data"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<PrintCommandResult>(json), Throws.InstanceOf<JsonReaderException>());
    }
}