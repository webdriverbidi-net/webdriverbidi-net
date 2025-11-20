namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class RemoveInterceptCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        RemoveInterceptCommandResult? result = JsonSerializer.Deserialize<RemoveInterceptCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        RemoveInterceptCommandResult? result = JsonSerializer.Deserialize<RemoveInterceptCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        RemoveInterceptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}