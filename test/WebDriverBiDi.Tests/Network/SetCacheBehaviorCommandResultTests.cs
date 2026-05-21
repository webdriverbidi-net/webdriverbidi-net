namespace WebDriverBiDi.Network;

using System.Text.Json;

public class SetCacheBehaviorCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetCacheBehaviorCommandResult? result = JsonSerializer.Deserialize<SetCacheBehaviorCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetCacheBehaviorCommandResult? result = JsonSerializer.Deserialize<SetCacheBehaviorCommandResult>("{}");
        Assert.NotNull(result);
        SetCacheBehaviorCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
