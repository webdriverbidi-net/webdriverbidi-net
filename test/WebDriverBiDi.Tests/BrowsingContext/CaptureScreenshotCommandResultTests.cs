namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class CaptureScreenshotCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""data"": ""some screenshot data"" }";
        CaptureScreenshotCommandResult? result = JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data, Is.EqualTo("some screenshot data"));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = @"{ ""data"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
