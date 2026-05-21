namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateDescriptorResponseCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateDescriptorResponseCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorResponseCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateDescriptorResponseCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorResponseCommandResult>("{}");
        Assert.NotNull(result);
        SimulateDescriptorResponseCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
