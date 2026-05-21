namespace WebDriverBiDi.Input;

using System.Text.Json;

public class ReleaseActionsCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        ReleaseActionsCommandResult? result = JsonSerializer.Deserialize<ReleaseActionsCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ReleaseActionsCommandResult? result = JsonSerializer.Deserialize<ReleaseActionsCommandResult>("{}");
        Assert.NotNull(result);
        ReleaseActionsCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
