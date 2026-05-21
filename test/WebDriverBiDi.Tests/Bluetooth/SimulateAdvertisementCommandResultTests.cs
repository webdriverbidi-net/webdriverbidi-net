namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class SimulateAdvertisementCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SimulateAdvertisementCommandResult? result = JsonSerializer.Deserialize<SimulateAdvertisementCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SimulateAdvertisementCommandResult? result = JsonSerializer.Deserialize<SimulateAdvertisementCommandResult>("{}");
        Assert.NotNull(result);
        SimulateAdvertisementCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
