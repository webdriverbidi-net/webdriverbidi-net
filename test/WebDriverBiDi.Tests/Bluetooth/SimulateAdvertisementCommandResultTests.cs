namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateAdvertisementCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateAdvertisementCommandResult? result = JsonSerializer.Deserialize<SimulateAdvertisementCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateAdvertisementCommandResult? result = JsonSerializer.Deserialize<SimulateAdvertisementCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateAdvertisementCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
