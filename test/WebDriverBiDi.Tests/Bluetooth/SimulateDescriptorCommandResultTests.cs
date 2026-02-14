namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateDescriptorCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateDescriptorCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateDescriptorCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateDescriptorCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}