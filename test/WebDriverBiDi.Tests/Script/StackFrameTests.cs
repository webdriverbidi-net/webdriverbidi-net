namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class StackFrameTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
        StackFrame? stackFrame = JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions);
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
        Assert.That(() => JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidFunctionNameTypeThrows()
    {
        string json = @"{ ""functionName"": {}, ""lineNumber"": 1, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
       Assert.That(() => JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingLineNumberThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
        Assert.That(() => JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidLineNumberTypeThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": {}, ""columnNumber"": 5, ""url"": ""http://some.url/file.js"" }";
       Assert.That(() => JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingColumnNumberThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""url"": ""http://some.url/file.js"" }";
        Assert.That(() => JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidColumnNumberTypeThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": {}, ""url"": ""http://some.url/file.js"" }";
       Assert.That(() => JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingUrlThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, }";
        Assert.That(() => JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidUrlTypeThrows()
    {
        string json = @"{ ""functionName"": ""myFunction"", ""lineNumber"": 1, ""columnNumber"": 5, ""url"": {} }";
       Assert.That(() => JsonSerializer.Deserialize<StackFrame>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
