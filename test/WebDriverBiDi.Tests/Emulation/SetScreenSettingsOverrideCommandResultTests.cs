namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetScreenSettingsOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetScreenSettingsOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenSettingsOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetScreenSettingsOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenSettingsOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetScreenSettingsOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
