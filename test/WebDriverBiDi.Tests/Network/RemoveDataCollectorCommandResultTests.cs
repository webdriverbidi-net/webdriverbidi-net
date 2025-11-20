namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class RemoveDataCollectorCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        RemoveDataCollectorCommandResult? result = JsonSerializer.Deserialize<RemoveDataCollectorCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        RemoveDataCollectorCommandResult? result = JsonSerializer.Deserialize<RemoveDataCollectorCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        RemoveDataCollectorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}