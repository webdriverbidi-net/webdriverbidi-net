namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class CaptureScreenshotCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "data": "some screenshot data"
                      }
                      """;
        CaptureScreenshotCommandResult? result = JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data, Is.EqualTo("some screenshot data"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "data": "some screenshot data"
                      }
                      """;
        CaptureScreenshotCommandResult? result = JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        CaptureScreenshotCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = """
                      {
                        "data": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<CaptureScreenshotCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}
