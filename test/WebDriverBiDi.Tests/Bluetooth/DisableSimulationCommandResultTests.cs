namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class DisableSimulationCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        DisableSimulationCommandResult? result = JsonSerializer.Deserialize<DisableSimulationCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        DisableSimulationCommandResult? result = JsonSerializer.Deserialize<DisableSimulationCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        DisableSimulationCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}