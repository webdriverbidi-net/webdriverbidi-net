namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetViewportMetaOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetViewportMetaOverrideCommandResult? result = JsonSerializer.Deserialize<SetViewportMetaOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetViewportMetaOverrideCommandResult? result = JsonSerializer.Deserialize<SetViewportMetaOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetViewportMetaOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
