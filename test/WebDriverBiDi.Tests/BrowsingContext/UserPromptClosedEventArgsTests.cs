namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class UserPromptClosedEventArgsTests
{
    [Test]
    public void TestCanDeserializeWithAcceptedTrue()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": true }";
        UserPromptClosedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.IsAccepted, Is.EqualTo(true));
            Assert.That(eventArgs.UserText, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithAcceptedFalse()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": false }";
        UserPromptClosedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.IsAccepted, Is.EqualTo(false));
            Assert.That(eventArgs.UserText, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithUserText()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": true, ""userText"": ""some text"" }";
        UserPromptClosedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.IsAccepted, Is.EqualTo(true));
            Assert.That(eventArgs.UserText, Is.EqualTo("some text"));
        });
    }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = @"{ ""accepted"": true }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = @"{ ""context"": {}, ""accepted"": true }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingAcceptedValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"" }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidAcceptedValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": ""some value"" }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidUserTextValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""accepted"": true, ""userText"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptClosedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
