namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.Session;

[TestFixture]
public class UserPromptOpenedEventArgsTests
{
    [Test]
    public void TestCanDeserializeWithTypeAlert()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.PromptType, Is.EqualTo(UserPromptType.Alert));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithTypeConfirm()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "confirm",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.PromptType, Is.EqualTo(UserPromptType.Confirm));
            Assert.That(eventArgs.Handler, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithTypePrompt()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "prompt",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.PromptType, Is.EqualTo(UserPromptType.Prompt));
            Assert.That(eventArgs.Handler, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithTypeBeforeUnload()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "beforeunload",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.PromptType, Is.EqualTo(UserPromptType.BeforeUnload));
            Assert.That(eventArgs.Handler, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithHandlerAccept()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.PromptType, Is.EqualTo(UserPromptType.Alert));
            Assert.That(eventArgs.Handler, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithHandlerDismiss()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "dismiss",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.PromptType, Is.EqualTo(UserPromptType.Alert));
            Assert.That(eventArgs.Handler, Is.EqualTo(UserPromptHandlerType.Dismiss));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithHandlerIgnore()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "ignore",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.PromptType, Is.EqualTo(UserPromptType.Alert));
            Assert.That(eventArgs.Handler, Is.EqualTo(UserPromptHandlerType.Ignore));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithOptionalDefaultValue()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "prompt",
                        "handler": "accept",
                        "message": "some prompt message",
                        "defaultValue": "prompt default"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.PromptType, Is.EqualTo(UserPromptType.Prompt));
            Assert.That(eventArgs.Handler, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(eventArgs.Message, Is.EqualTo("some prompt message"));
            Assert.That(eventArgs.DefaultValue, Is.EqualTo("prompt default"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        UserPromptOpenedEventArgs? eventArgs = JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        UserPromptOpenedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
   }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = """
                      {
                        "type": "beforeunload",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = """
                      {
                        "context": {},
                        "type": "beforeunload",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "invalid",
                        "handler": "accept",
                        "message": "some prompt message"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestDeserializeWithMissingHandlerTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "message": "some prompt message"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidHandlerTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "alert",
                        "handler": "invalid",
                        "message": "some prompt message"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestDeserializeWithMissingMessageValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "beforeunload",
                        "handler": "accept"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidMessageValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "type": "beforeunload",
                        "handler": "accept",
                        "message": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptOpenedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
