namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class HandleUserPromptCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        HandleUserPromptCommandResult? result = JsonSerializer.Deserialize<HandleUserPromptCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        HandleUserPromptCommandResult? result = JsonSerializer.Deserialize<HandleUserPromptCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        HandleUserPromptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}