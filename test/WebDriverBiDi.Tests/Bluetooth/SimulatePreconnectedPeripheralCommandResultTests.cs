namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulatePreconnectedPeripheralCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulatePreconnectedPeripheralCommandResult? result = JsonSerializer.Deserialize<SimulatePreconnectedPeripheralCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulatePreconnectedPeripheralCommandResult? result = JsonSerializer.Deserialize<SimulatePreconnectedPeripheralCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulatePreconnectedPeripheralCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}