namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetCacheBehaviorCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetCacheBehaviorCommandResult? result = JsonSerializer.Deserialize<SetCacheBehaviorCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetCacheBehaviorCommandResult? result = JsonSerializer.Deserialize<SetCacheBehaviorCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetCacheBehaviorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}