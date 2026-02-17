namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

[TestFixture]
public class SimulateDescriptorResponseCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SimulateDescriptorResponseCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorResponseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SimulateDescriptorResponseCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorResponseCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SimulateDescriptorResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
