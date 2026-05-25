namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class SetDownloadBehaviorCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetDownloadBehaviorCommandResult? result = JsonSerializer.Deserialize<SetDownloadBehaviorCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetDownloadBehaviorCommandResult? result = JsonSerializer.Deserialize<SetDownloadBehaviorCommandResult>("{}");
        Assert.NotNull(result);
        SetDownloadBehaviorCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
