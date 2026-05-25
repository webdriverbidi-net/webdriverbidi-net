namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateDescriptorCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateDescriptorCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateDescriptorCommandResult? result = JsonSerializer.Deserialize<SimulateDescriptorCommandResult>("{}");
        Assert.NotNull(result);
        SimulateDescriptorCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
