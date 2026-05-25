namespace WebDriverBiDi.WebExtension;

using System.Text.Json;

public class UninstallCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        UninstallCommandResult? result = JsonSerializer.Deserialize<UninstallCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        UninstallCommandResult? result = JsonSerializer.Deserialize<UninstallCommandResult>("{}");
        Assert.NotNull(result);
        UninstallCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
