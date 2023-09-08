namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class CaptureScreenshotCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""data"": ""some screenshot data"" }";
        CaptureScreenshotCommandResult? result = JsonConvert.DeserializeObject<CaptureScreenshotCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data, Is.EqualTo("some screenshot data"));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<CaptureScreenshotCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = @"{ ""data"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<CaptureScreenshotCommandResult>(json), Throws.InstanceOf<JsonReaderException>());
    }
}