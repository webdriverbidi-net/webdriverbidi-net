namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetScreenOrientationOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetScreenOrientationOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenOrientationOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetScreenOrientationOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenOrientationOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetScreenOrientationOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
