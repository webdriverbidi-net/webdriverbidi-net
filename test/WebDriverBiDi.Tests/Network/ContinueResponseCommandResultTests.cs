namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ContinueResponseCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        ContinueResponseCommandResult? result = JsonSerializer.Deserialize<ContinueResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ContinueResponseCommandResult? result = JsonSerializer.Deserialize<ContinueResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        ContinueResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}