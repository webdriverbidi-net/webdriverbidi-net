namespace WebDriverBiDi.Session;

using System.Text.Json;

public class UserPromptHandlerResultTests
{
    [Fact]
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
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        UserPromptHandlerResult? handlerResult = result.UnhandledPromptBehavior;
        Assert.NotNull(handlerResult);

        Assert.Equal(UserPromptHandlerType.Accept, handlerResult.Default);
        Assert.Equal(UserPromptHandlerType.Accept, handlerResult.Alert);
        Assert.Equal(UserPromptHandlerType.Dismiss, handlerResult.Confirm);
        Assert.Equal(UserPromptHandlerType.Dismiss, handlerResult.Prompt);
        Assert.Equal(UserPromptHandlerType.Ignore, handlerResult.BeforeUnload);
        Assert.Equal(UserPromptHandlerType.Ignore, handlerResult.File);
    }

    [Fact]
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
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        UserPromptHandlerResult? handlerResult = result.UnhandledPromptBehavior;
        Assert.NotNull(handlerResult);
        UserPromptHandlerResult copy = handlerResult with { };
        Assert.Equal(handlerResult, copy);
    }
}
