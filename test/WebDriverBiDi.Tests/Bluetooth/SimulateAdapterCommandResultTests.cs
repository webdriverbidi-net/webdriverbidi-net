namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateAdapterCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateAdapterCommandResult? result = JsonSerializer.Deserialize<SimulateAdapterCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateAdapterCommandResult? result = JsonSerializer.Deserialize<SimulateAdapterCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateAdapterCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
