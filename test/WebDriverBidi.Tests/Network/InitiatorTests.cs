namespace WebDriverBidi.Network;

using Newtonsoft.Json;

[TestFixture]
public class InitiatorTests
{
    [Test]
    public void TestCanDeserializeInitiator()
    {
        string json = @"{ ""type"": ""script"" }";
        Initiator? initiator = JsonConvert.DeserializeObject<Initiator>(json);
        Assert.That(initiator, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(initiator!.Type, Is.EqualTo(InitiatorType.Script));
            Assert.That(initiator.ColumnNumber, Is.Null);
            Assert.That(initiator.LineNumber, Is.Null);
            Assert.That(initiator.StackTrace, Is.Null);
            Assert.That(initiator.RequestId, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeInitiatorWithOptionalValues()
    {
        string json = @"{ ""type"": ""script"", ""lineNumber"": 2, ""columnNumber"": 1, ""stackTrace"": { ""callFrames"": [ { ""functionName"": ""myFunction"", ""lineNumber"": 2, ""columnNumber"": 1, ""url"": ""http://some.url/file.js"" } ] }, ""request"": ""myRequestId"" }";
        Initiator? initiator = JsonConvert.DeserializeObject<Initiator>(json);
        Assert.That(initiator, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(initiator!.Type, Is.EqualTo(InitiatorType.Script));
            Assert.That(initiator.ColumnNumber, Is.EqualTo(1));
            Assert.That(initiator.LineNumber, Is.EqualTo(2));
            Assert.That(initiator.StackTrace, Is.Not.Null);
            Assert.That(initiator.StackTrace!.CallFrames, Has.Count.EqualTo(1));
            Assert.That(initiator.RequestId, Is.EqualTo("myRequestId"));
        });
    }

    [Test]
    public void TestDeserializingInitiatorWithMissingTypeThrows()
    {
        string json = @"{ ""lineNumber"": 2, ""culumnNumber"": 1, ""stackTrace"": { ""callFrames"": [ { ""functionName"": ""myFunction"", ""lineNumber"": 2, ""columnNumber"": 1, ""url"": ""http://some.url/file.js"" } ] }, ""request"": ""myRequestId"" }";
        Assert.That(() => JsonConvert.DeserializeObject<Initiator>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'type' not found in JSON"));
    }

    [Test]
    public void TestDeserializingInitiatorWithInvalidTypeValueThrows()
    {
        string json = @"{ ""type"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<Initiator>(json), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("value 'invalid' is not valid for enum type"));
    }
}