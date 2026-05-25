namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetForcedColorsModeThemeOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetForcedColorsModeThemeOverrideCommandResult? result = JsonSerializer.Deserialize<SetForcedColorsModeThemeOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetForcedColorsModeThemeOverrideCommandResult? result = JsonSerializer.Deserialize<SetForcedColorsModeThemeOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetForcedColorsModeThemeOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
