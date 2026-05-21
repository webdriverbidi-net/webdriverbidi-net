namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateGattDisconnectionCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateGattDisconnectionCommandResult? result = JsonSerializer.Deserialize<SimulateGattDisconnectionCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateGattDisconnectionCommandResult? result = JsonSerializer.Deserialize<SimulateGattDisconnectionCommandResult>("{}");
        Assert.NotNull(result);
        SimulateGattDisconnectionCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
