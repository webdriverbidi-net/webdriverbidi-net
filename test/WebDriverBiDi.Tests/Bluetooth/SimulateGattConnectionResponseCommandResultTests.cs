namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateGattConnectionResponseCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateGattConnectionResponseCommandResult? result = JsonSerializer.Deserialize<SimulateGattConnectionResponseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateGattConnectionResponseCommandResult? result = JsonSerializer.Deserialize<SimulateGattConnectionResponseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateGattConnectionResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
