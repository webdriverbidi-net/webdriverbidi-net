namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateCharacteristicResponseCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateCharacteristicResponseCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateCharacteristicResponseCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateCharacteristicResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}