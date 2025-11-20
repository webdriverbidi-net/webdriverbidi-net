namespace WebDriverBiDi.Input;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ReleaseActionsCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        ReleaseActionsCommandResult? result = JsonSerializer.Deserialize<ReleaseActionsCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ReleaseActionsCommandResult? result = JsonSerializer.Deserialize<ReleaseActionsCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        ReleaseActionsCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}