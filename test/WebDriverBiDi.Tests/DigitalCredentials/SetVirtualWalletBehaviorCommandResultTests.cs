namespace WebDriverBiDi.DigitalCredentials;

using System.Text.Json;

public class SetVirtualWalletBehaviorCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetVirtualWalletBehaviorCommandResult? result = JsonSerializer.Deserialize<SetVirtualWalletBehaviorCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetVirtualWalletBehaviorCommandResult? result = JsonSerializer.Deserialize<SetVirtualWalletBehaviorCommandResult>("{}");
        Assert.NotNull(result);
        SetVirtualWalletBehaviorCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
