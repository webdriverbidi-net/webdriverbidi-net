namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetViewportCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetViewportCommandResult? result = JsonSerializer.Deserialize<SetViewportCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetViewportCommandResult? result = JsonSerializer.Deserialize<SetViewportCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetViewportCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}