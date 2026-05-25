namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateServiceCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateServiceCommandResult? result = JsonSerializer.Deserialize<SimulateServiceCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateServiceCommandResult? result = JsonSerializer.Deserialize<SimulateServiceCommandResult>("{}");
        Assert.NotNull(result);
        SimulateServiceCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
