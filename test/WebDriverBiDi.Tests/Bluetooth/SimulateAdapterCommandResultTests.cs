namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateAdapterCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateAdapterCommandResult? result = JsonSerializer.Deserialize<SimulateAdapterCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateAdapterCommandResult? result = JsonSerializer.Deserialize<SimulateAdapterCommandResult>("{}");
        Assert.NotNull(result);
        SimulateAdapterCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
