namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetScrollbarTypeOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetScrollbarTypeOverrideCommandResult? result = JsonSerializer.Deserialize<SetScrollbarTypeOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetScrollbarTypeOverrideCommandResult? result = JsonSerializer.Deserialize<SetScrollbarTypeOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetScrollbarTypeOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
