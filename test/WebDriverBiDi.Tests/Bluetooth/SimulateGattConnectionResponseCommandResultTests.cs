namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateGattConnectionResponseCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateGattConnectionResponseCommandResult? result = JsonSerializer.Deserialize<SimulateGattConnectionResponseCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateGattConnectionResponseCommandResult? result = JsonSerializer.Deserialize<SimulateGattConnectionResponseCommandResult>("{}");
        Assert.NotNull(result);
        SimulateGattConnectionResponseCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
