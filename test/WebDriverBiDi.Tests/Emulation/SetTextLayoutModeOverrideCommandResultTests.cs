namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetTextLayoutModeOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetTextLayoutModeOverrideCommandResult? result = JsonSerializer.Deserialize<SetTextLayoutModeOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetTextLayoutModeOverrideCommandResult? result = JsonSerializer.Deserialize<SetTextLayoutModeOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetTextLayoutModeOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
