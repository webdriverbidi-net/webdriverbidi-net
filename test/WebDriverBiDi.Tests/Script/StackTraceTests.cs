namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class StackTraceTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""callFrames"": [ { ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" } ] }";
        StackTrace? stacktrace = JsonSerializer.Deserialize<StackTrace>(json);
        Assert.That(stacktrace, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(stacktrace!.CallFrames, Is.Not.Null);
            Assert.That(stacktrace.CallFrames, Has.Count.EqualTo(1));
            Assert.That(stacktrace.CallFrames[0], Is.TypeOf<StackFrame>());
        });
    }

    [Test]
    public void TestDeserializeWithMissingCallFramesThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonSerializer.Deserialize<StackTrace>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidCallFramesTypeThrows()
    {
        string json = @"{ ""callFrames"": { ""frame"": { ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" } } }";
        Assert.That(() => JsonSerializer.Deserialize<StackTrace>(json), Throws.InstanceOf<JsonException>());
    }
}