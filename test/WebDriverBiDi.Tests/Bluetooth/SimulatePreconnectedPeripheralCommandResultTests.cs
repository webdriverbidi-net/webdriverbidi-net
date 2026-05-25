namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulatePreconnectedPeripheralCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulatePreconnectedPeripheralCommandResult? result = JsonSerializer.Deserialize<SimulatePreconnectedPeripheralCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulatePreconnectedPeripheralCommandResult? result = JsonSerializer.Deserialize<SimulatePreconnectedPeripheralCommandResult>("{}");
        Assert.NotNull(result);
        SimulatePreconnectedPeripheralCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
