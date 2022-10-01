namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class UserPromptClosedEventArgsTests
{
    [Test]
    public void TestCanDeserializeWithAcceptedTrue()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": true }";
        UserPromptClosedEventArgs? eventArgs = JsonConvert.DeserializeObject<UserPromptClosedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.IsAccepted, Is.EqualTo(true));
        Assert.That(eventArgs.UserText, Is.Null);
    }

    [Test]
    public void TestCanDeserializeWithAcceptedFalse()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": false }";
        UserPromptClosedEventArgs? eventArgs = JsonConvert.DeserializeObject<UserPromptClosedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.IsAccepted, Is.EqualTo(false));
        Assert.That(eventArgs.UserText, Is.Null);
    }

    [Test]
    public void TestCanDeserializeWithUserText()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": true, ""userText"": ""some text"" }";
        UserPromptClosedEventArgs? eventArgs = JsonConvert.DeserializeObject<UserPromptClosedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.IsAccepted, Is.EqualTo(true));
        Assert.That(eventArgs.UserText, Is.EqualTo("some text"));
    }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = @"{ ""accepted"": true }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = @"{ ""context"": {}, ""accepted"": true }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingAcceptedValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"" }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidAcceptedValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": ""some value"" }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithInvalidUserTextValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": true, ""userText"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }
}
