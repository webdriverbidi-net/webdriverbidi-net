namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateGattDisconnectionCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateGattDisconnectionCommandResult? result = JsonSerializer.Deserialize<SimulateGattDisconnectionCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateGattDisconnectionCommandResult? result = JsonSerializer.Deserialize<SimulateGattDisconnectionCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateGattDisconnectionCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}