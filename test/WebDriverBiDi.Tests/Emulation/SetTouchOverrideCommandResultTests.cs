namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetTouchOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetTouchOverrideCommandResult? result = JsonSerializer.Deserialize<SetTouchOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetTouchOverrideCommandResult? result = JsonSerializer.Deserialize<SetTouchOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetTouchOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
