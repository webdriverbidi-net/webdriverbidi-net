namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateCharacteristicResponseCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateCharacteristicResponseCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicResponseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateCharacteristicResponseCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicResponseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateCharacteristicResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
