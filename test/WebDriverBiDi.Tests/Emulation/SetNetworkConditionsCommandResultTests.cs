namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetNetworkConditionsCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetNetworkConditionsCommandResult? result = JsonSerializer.Deserialize<SetNetworkConditionsCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetNetworkConditionsCommandResult? result = JsonSerializer.Deserialize<SetNetworkConditionsCommandResult>("{}");
        Assert.NotNull(result);
        SetNetworkConditionsCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
