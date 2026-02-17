namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateCharacteristicCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateCharacteristicCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateCharacteristicCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateCharacteristicCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
