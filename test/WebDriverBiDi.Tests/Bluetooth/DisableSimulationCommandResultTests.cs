namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class DisableSimulationCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        DisableSimulationCommandResult? result = JsonSerializer.Deserialize<DisableSimulationCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        DisableSimulationCommandResult? result = JsonSerializer.Deserialize<DisableSimulationCommandResult>("{}");
        Assert.NotNull(result);
        DisableSimulationCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
