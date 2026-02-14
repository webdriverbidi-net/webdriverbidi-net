namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateServiceCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateServiceCommandResult? result = JsonSerializer.Deserialize<SimulateServiceCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateServiceCommandResult? result = JsonSerializer.Deserialize<SimulateServiceCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateServiceCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}