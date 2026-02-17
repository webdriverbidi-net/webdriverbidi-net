namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class DisableSimulationCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        DisableSimulationCommandResult? result = JsonSerializer.Deserialize<DisableSimulationCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        DisableSimulationCommandResult? result = JsonSerializer.Deserialize<DisableSimulationCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        DisableSimulationCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
