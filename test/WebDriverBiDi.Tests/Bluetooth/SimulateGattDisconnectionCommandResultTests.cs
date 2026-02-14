namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateGattDisconnectionCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateGattDisconnectionCommandResult? result = JsonSerializer.Deserialize<SimulateGattDisconnectionCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateGattDisconnectionCommandResult? result = JsonSerializer.Deserialize<SimulateGattDisconnectionCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateGattDisconnectionCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}