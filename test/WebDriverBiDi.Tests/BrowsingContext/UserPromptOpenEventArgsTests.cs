namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class UserPromptOpenedEventArgsTests
{
    [Test]
    public void TestCanDeserializeWithTypeAlert()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""alert"", ""message"": ""some prompt message"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.Alert));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithTypeConfirm()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""confirm"", ""message"": ""some prompt message"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.Confirm));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithTypePrompt()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""prompt"", ""message"": ""some prompt message"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.Prompt));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithTypeBeforeUnload()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""beforeunload"", ""message"": ""some prompt message"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.BeforeUnload));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithOptionalDefaultValue()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""prompt"", ""message"": ""some prompt message"", ""defaultValue"": ""prompt default"" }";
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.PromptType, Is.EqualTo(UserPromptType.Prompt));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.EqualTo("prompt default"));
        });
    }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = @"{ ""type"": ""beforeunload"", ""message"": ""some prompt message"" }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = @"{ ""context"": {}, ""type"": ""beforeunload"", ""message"": ""some prompt message"" }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingTypeValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""message"": ""some prompt message"" }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidTypeValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""invalid"", ""message"": ""some prompt message"" }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestDeserializeWithMissingMessageValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""beforeunload"" }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidMessageValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""type"": ""beforeunload"", ""message"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
