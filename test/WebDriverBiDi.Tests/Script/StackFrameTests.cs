namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

[TestFixture]
public class StackFrameTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
        StackFrame? stackFrame = JsonConvert.DeserializeObject<StackFrame>(json);
        Assert.That(stackFrame, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(stackFrame!.FunctionName, Is.EqualTo("myFunction"));
            Assert.That(stackFrame.LineNumber, Is.EqualTo(1));
            Assert.That(stackFrame.ColumnNumber, Is.EqualTo(5));
            Assert.That(stackFrame.Url, Is.EqualTo("http://some.url/file.js"));
        });
    }

    [Test]
    public void TestDeserializeWithMissingFunctionNameThrows()
    {
        string json = @"{ ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
        Assert.That(() => JsonConvert.DeserializeObject<StackFrame>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidFunctionNameTypeThrows()
    {
        string json = @"{ ""functionName"": {}, ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
       Assert.That(() => JsonConvert.DeserializeObject<StackFrame>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingLineNumberThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
        Assert.That(() => JsonConvert.DeserializeObject<StackFrame>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidLineNumberTypeThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": {}, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
       Assert.That(() => JsonConvert.DeserializeObject<StackFrame>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingColumnNumberThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""url"": ""http://some.url/file.js"" }";
        Assert.That(() => JsonConvert.DeserializeObject<StackFrame>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidColumnNumberTypeThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": {}, ""url"": ""http://some.url/file.js"" }";
       Assert.That(() => JsonConvert.DeserializeObject<StackFrame>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingUrlThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, }";
        Assert.That(() => JsonConvert.DeserializeObject<StackFrame>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidUrlTypeThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, ""url"": {} }";
       Assert.That(() => JsonConvert.DeserializeObject<StackFrame>(json), Throws.InstanceOf<JsonReaderException>());
    }
}
