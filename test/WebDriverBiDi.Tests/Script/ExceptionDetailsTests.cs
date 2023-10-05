namespace WebDriverBiDi.Script;

using Newtonsoft.Json;

[TestFixture]
public class ExceptionDetailsTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""text"": ""exception message"", ""lineNumber"": 1, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myException"" }, ""stacktrace"": { ""callFrames"": [] } }";
        ExceptionDetails? exceptionDetails = JsonConvert.DeserializeObject<ExceptionDetails>(json);
        Assert.That(exceptionDetails, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(exceptionDetails!.Text, Is.EqualTo("exception message"));
            Assert.That(exceptionDetails.LineNumber, Is.EqualTo(1));
            Assert.That(exceptionDetails.ColumnNumber, Is.EqualTo(5));
            Assert.That(exceptionDetails.Exception.ValueAs<string>(), Is.EqualTo("myException"));
            Assert.That(exceptionDetails.StackTrace.CallFrames, Is.Empty);
        });
    }

    [Test]
    public void TestDeserializeWithMissingTextThrows()
    {
        string json = @"{ ""lineNumber"": 1, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myException"" }, ""stacktrace"": { ""callFrames"": [] } }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidTextTypeThrows()
    {
        string json = @"{ ""text"": bool, ""lineNumber"": 1, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myException"" }, ""stacktrace"": { ""callFrames"": [] } }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingLineNumberThrows()
    {
        string json = @"{ ""text"": ""exception message"", ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myException"" }, ""stacktrace"": { ""callFrames"": [] } }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidLineNumberTypeThrows()
    {
        string json = @"{ ""text"": ""exception message"", ""lineNumber"": true, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myException"" }, ""stacktrace"": { ""callFrames"": [] } }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingColumnNumberThrows()
    {
        string json = @"{ ""text"": ""exception message"", ""lineNumber"": 1, ""exception"": { ""type"": ""string"", ""value"": ""myException"" }, ""stacktrace"": { ""callFrames"": [] } }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidColumnNumberTypeThrows()
    {
        string json = @"{ ""text"": ""exception message"", ""lineNumber"": 1, ""columnNumber"": true, ""exception"": { ""type"": ""string"", ""value"": ""myException"" }, ""stacktrace"": { ""callFrames"": [] } }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingExceptionThrows()
    {
        string json = @"{ ""text"": ""exception message"", ""lineNumber"": 1, ""columnNumber"": 5, ""stacktrace"": { ""callFrames"": []} }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidExceptionTypeThrows()
    {
        string json = @"{ ""text"": ""exception message"", ""lineNumber"": 1, ""columnNumber"": 5, ""exception"": ""myException"", ""stacktrace"": { ""callFrames"": [] } }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingStackTraceThrows()
    {
        string json = @"{ ""text"": ""exception message"", ""lineNumber"": 1, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myException"" } }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidStackTraceTypeThrows()
    {
        string json = @"{ ""text"": ""exception message"", ""lineNumber"": 1, ""columnNumber"": 5, ""exception"": { ""type"": ""string"", ""value"": ""myException"" }, ""stacktrace"": ""stacktrace"" }";
        Assert.That(() => JsonConvert.DeserializeObject<ExceptionDetails>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}
