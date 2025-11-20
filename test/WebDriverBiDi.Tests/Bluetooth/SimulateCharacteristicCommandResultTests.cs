namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateCharacteristicCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateCharacteristicCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateCharacteristicCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateCharacteristicCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}