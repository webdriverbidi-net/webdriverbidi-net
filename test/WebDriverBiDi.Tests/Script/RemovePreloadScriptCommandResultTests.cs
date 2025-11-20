namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class RemovePreloadScriptCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        RemovePreloadScriptCommandResult? result = JsonSerializer.Deserialize<RemovePreloadScriptCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        RemovePreloadScriptCommandResult? result = JsonSerializer.Deserialize<RemovePreloadScriptCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        RemovePreloadScriptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}