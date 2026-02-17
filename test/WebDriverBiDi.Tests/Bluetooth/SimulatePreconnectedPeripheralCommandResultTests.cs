namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulatePreconnectedPeripheralCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulatePreconnectedPeripheralCommandResult? result = JsonSerializer.Deserialize<SimulatePreconnectedPeripheralCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulatePreconnectedPeripheralCommandResult? result = JsonSerializer.Deserialize<SimulatePreconnectedPeripheralCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulatePreconnectedPeripheralCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
