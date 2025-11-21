namespace WebDriverBiDi.Session;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class UserPromptHandlerResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeUserPromptHandlerResult()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "unhandledPromptBehavior": {
                          "default": "accept",
                          "alert": "accept",
                          "confirm": "dismiss",
                          "prompt": "dismiss",
                          "beforeunload": "ignore",
                          "file": "ignore"
                        }
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        UserPromptHandlerResult? handlerResult = result.UnhandledPromptBehavior;
        Assert.That(handlerResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(handlerResult.Default, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handlerResult.Alert, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(handlerResult.Confirm, Is.EqualTo(UserPromptHandlerType.Dismiss));
            Assert.That(handlerResult.Prompt, Is.EqualTo(UserPromptHandlerType.Dismiss));
            Assert.That(handlerResult.BeforeUnload, Is.EqualTo(UserPromptHandlerType.Ignore));
            Assert.That(handlerResult.File, Is.EqualTo(UserPromptHandlerType.Ignore));
        }
    }

    [Test]
    public void TestUserPromptHandlerResultCopySemantics()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "unhandledPromptBehavior": {
                          "default": "accept",
                          "alert": "accept",
                          "confirm": "dismiss",
                          "prompt": "dismiss",
                          "beforeunload": "ignore",
                          "file": "ignore"
                        }
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        UserPromptHandlerResult? handlerResult = result.UnhandledPromptBehavior;
        Assert.That(handlerResult, Is.Not.Null);
        UserPromptHandlerResult copy = handlerResult with { };
        Assert.That(copy, Is.EqualTo(handlerResult));
    }
}
