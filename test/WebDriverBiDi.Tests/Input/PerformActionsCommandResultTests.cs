namespace WebDriverBiDi.Input;

using System.Text.Json;

public class PerformActionsCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        PerformActionsCommandResult? result = JsonSerializer.Deserialize<PerformActionsCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        PerformActionsCommandResult? result = JsonSerializer.Deserialize<PerformActionsCommandResult>("{}");
        Assert.NotNull(result);
        PerformActionsCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
