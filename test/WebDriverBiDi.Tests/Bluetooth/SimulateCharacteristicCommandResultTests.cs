namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateCharacteristicCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateCharacteristicCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateCharacteristicCommandResult? result = JsonSerializer.Deserialize<SimulateCharacteristicCommandResult>("{}");
        Assert.NotNull(result);
        SimulateCharacteristicCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
