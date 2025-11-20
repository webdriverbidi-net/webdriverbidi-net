namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SimulateGattConnectionResponseCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SimulateGattConnectionResponseCommandResult? result = JsonSerializer.Deserialize<SimulateGattConnectionResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateGattConnectionResponseCommandResult? result = JsonSerializer.Deserialize<SimulateGattConnectionResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SimulateGattConnectionResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}