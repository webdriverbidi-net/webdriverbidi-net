namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ActivateCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        ActivateCommandResult? result = JsonSerializer.Deserialize<ActivateCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ActivateCommandResult? result = JsonSerializer.Deserialize<ActivateCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        ActivateCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}