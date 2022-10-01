namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class UserPromptOpenedEventArgsTests
{
    [Test]
    public void TestCanDeserializeWithTypeAlert()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""alert"", ""message"": ""some prompt message"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.Alert));
        Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
    }

    [Test]
    public void TestCanDeserializeWithTypeConfirm()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""confirm"", ""message"": ""some prompt message"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.Confirm));
        Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
    }

    [Test]
    public void TestCanDeserializeWithTypePrompt()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""prompt"", ""message"": ""some prompt message"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.Prompt));
        Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
    }

    [Test]
    public void TestCanDeserializeWithTypeBeforeUnload()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""beforeunload"", ""message"": ""some prompt message"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.BeforeUnload));
        Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
    }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = @"{ ""type"": ""beforeunload"", ""message"": ""some prompt message"" }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = @"{ ""context"": {}, ""type"": ""beforeunload"", ""message"": ""some prompt message"" }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingTypeValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""message"": ""some prompt message"" }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidTypeValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""invalid"", ""message"": ""some prompt message"" }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithMissingMessageValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""beforeunload"" }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidMessageValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""beforeunload"", ""message"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }
}
