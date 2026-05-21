namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateCharacteristicResponseCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateCharacteristicResponseCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicResponseCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateCharacteristicResponseCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicResponseCommandResult>("{}");
        Assert.NotNull(result);
        SimulateCharacteristicResponseCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
