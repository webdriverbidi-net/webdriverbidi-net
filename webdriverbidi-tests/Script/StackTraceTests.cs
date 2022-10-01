namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[TestFixture]
public class StackTraceTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""callFrames"": [ { ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" } ] }";
        StackTrace? stacktrace = JsonConvert.DeserializeObject<StackTrace>(json);
        Assert.That(stacktrace, Is.Not.Null);
        Assert.That(stacktrace!.CallFrames, Is.Not.Null);
        Assert.That(stacktrace.CallFrames.Count, Is.EqualTo(1));
        Assert.That(stacktrace.CallFrames[0], Is.TypeOf<StackFrame>());
    }

    [Test]
    public void TestDeserializeWithMissingCallFramesThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<StackTrace>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidCallFramesTypeThrows()
    {
        string json = @"{ ""callFrames"": { ""frame"": { ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" } } }";
        Assert.That(() => JsonConvert.DeserializeObject<StackTrace>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}