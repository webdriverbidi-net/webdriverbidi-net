namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class UserPromptHandlerTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanSerialize()
    {
        UserPromptHandler handler = new();
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }

    [Test]
    public void TestCanSerializeWithAllOptions()
    {
        UserPromptHandler handler = new()
        {
            Default = UserPromptHandlerType.Accept,
            Alert = UserPromptHandlerType.Accept,
            Confirm = UserPromptHandlerType.Dismiss,
            Prompt = UserPromptHandlerType.Dismiss,
            BeforeUnload = UserPromptHandlerType.Ignore,
            File = UserPromptHandlerType.Ignore,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(6));
            Assert.That(serialized, Contains.Key("default"));
            Assert.That(serialized["default"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["default"]!.Value<string>(), Is.EqualTo("accept"));
            Assert.That(serialized, Contains.Key("alert"));
            Assert.That(serialized["alert"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["alert"]!.Value<string>(), Is.EqualTo("accept"));
            Assert.That(serialized, Contains.Key("confirm"));
            Assert.That(serialized["confirm"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["confirm"]!.Value<string>(), Is.EqualTo("dismiss"));
            Assert.That(serialized, Contains.Key("prompt"));
            Assert.That(serialized["prompt"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["prompt"]!.Value<string>(), Is.EqualTo("dismiss"));
            Assert.That(serialized, Contains.Key("beforeunload"));
            Assert.That(serialized["beforeunload"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["beforeunload"]!.Value<string>(), Is.EqualTo("ignore"));
            Assert.That(serialized, Contains.Key("file"));
            Assert.That(serialized["file"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["file"]!.Value<string>(), Is.EqualTo("ignore"));
        });
    }

    [Test]
    public void TestCanSerializeWithOnlyDefault()
    {
        UserPromptHandler handler = new()
        {
            Default = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("default"));
            Assert.That(serialized["default"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["default"]!.Value<string>(), Is.EqualTo("accept"));
        });
    }

    [Test]
    public void TestCanSerializeWithOnlyAlert()
    {
        UserPromptHandler handler = new()
        {
            Alert = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("alert"));
            Assert.That(serialized["alert"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["alert"]!.Value<string>(), Is.EqualTo("accept"));
        });
    }

    [Test]
    public void TestCanSerializeWithOnlyConfirm()
    {
        UserPromptHandler handler = new()
        {
            Confirm = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("confirm"));
            Assert.That(serialized["confirm"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["confirm"]!.Value<string>(), Is.EqualTo("accept"));
        });
    }

    [Test]
    public void TestCanSerializeWithOnlyPrompt()
    {
        UserPromptHandler handler = new()
        {
            Prompt = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("prompt"));
            Assert.That(serialized["prompt"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["prompt"]!.Value<string>(), Is.EqualTo("accept"));
        });
    }

    [Test]
    public void TestCanSerializeWithOnlyBeforeUnload()
    {
        UserPromptHandler handler = new()
        {
            BeforeUnload = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("beforeunload"));
            Assert.That(serialized["beforeunload"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["beforeunload"]!.Value<string>(), Is.EqualTo("accept"));
        });
    }

    [Test]
    public void TestCanSerializeWithOnlyFile()
    {
        UserPromptHandler handler = new()
        {
            File = UserPromptHandlerType.Accept,
        };
        string json = JsonSerializer.Serialize(handler);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("file"));
            Assert.That(serialized["file"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["file"]!.Value<string>(), Is.EqualTo("accept"));
        });
    }

    [Test]
    public void TestCanDeserialize()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions);
        Assert.That(handler, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Default, Is.Null);
            Assert.That(handler.Alert, Is.Null);
            Assert.That(handler.Confirm, Is.Null);
            Assert.That(handler.Prompt, Is.Null);
            Assert.That(handler.BeforeUnload, Is.Null);
            Assert.That(handler.File, Is.Null);
         });
    }

    [Test]
    public void TestCanDeserializeWithAllOptions()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "default": "accept",
                        "alert": "accept",
                        "confirm": "dismiss",
                        "prompt": "dismiss",
                        "beforeunload": "ignore",
                        "file": "ignore"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions);
        Assert.That(handler, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Default, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handler.Alert, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handler.Confirm, Is.EqualTo(UserPromptHandlerType.Dismiss));
            Assert.That(handler.Prompt, Is.EqualTo(UserPromptHandlerType.Dismiss));
            Assert.That(handler.BeforeUnload, Is.EqualTo(UserPromptHandlerType.Ignore));
            Assert.That(handler.File, Is.EqualTo(UserPromptHandlerType.Ignore));
        });
    }

    [Test]
    public void TestCanDeserializeUserPromptHandlerWithOnlyDefault()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "default": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions);
        Assert.That(handler, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Default, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handler.Alert, Is.Null);
            Assert.That(handler.Confirm, Is.Null);
            Assert.That(handler.Prompt, Is.Null);
            Assert.That(handler.BeforeUnload, Is.Null);
            Assert.That(handler.File, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeUserPromptHandlerWithOnlyAlert()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "alert": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions);
        Assert.That(handler, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Default, Is.Null);
            Assert.That(handler.Alert, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handler.Confirm, Is.Null);
            Assert.That(handler.Prompt, Is.Null);
            Assert.That(handler.BeforeUnload, Is.Null);
            Assert.That(handler.File, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeUserPromptHandlerWithOnlyConfirm()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "confirm": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions);
        Assert.That(handler, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Default, Is.Null);
            Assert.That(handler.Alert, Is.Null);
            Assert.That(handler.Confirm, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handler.Prompt, Is.Null);
            Assert.That(handler.BeforeUnload, Is.Null);
            Assert.That(handler.File, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeUserPromptHandlerWithOnlyPrompt()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "prompt": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions);
        Assert.That(handler, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Default, Is.Null);
            Assert.That(handler.Alert, Is.Null);
            Assert.That(handler.Confirm, Is.Null);
            Assert.That(handler.Prompt, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handler.BeforeUnload, Is.Null);
            Assert.That(handler.File, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeUserPromptHandlerWithOnlyBeforeUnload()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "beforeunload": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions);
        Assert.That(handler, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Default, Is.Null);
            Assert.That(handler.Alert, Is.Null);
            Assert.That(handler.Confirm, Is.Null);
            Assert.That(handler.Prompt, Is.Null);
            Assert.That(handler.BeforeUnload, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handler.File, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeUserPromptHandlerWithOnlyFile()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "file": "accept"
                      }
                      """;
        UserPromptHandler? handler = JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions);
        Assert.That(handler, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(handler.Default, Is.Null);
            Assert.That(handler.Alert, Is.Null);
            Assert.That(handler.Confirm, Is.Null);
            Assert.That(handler.Prompt, Is.Null);
            Assert.That(handler.BeforeUnload, Is.Null);
            Assert.That(handler.File, Is.EqualTo(UserPromptHandlerType.Accept));
        });
    }

    [Test]
    public void TestCanDeserializingWithInvalidTypeValueThrows()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "default": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<UserPromptHandler>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }
}
