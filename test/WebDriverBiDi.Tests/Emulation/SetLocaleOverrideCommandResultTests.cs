namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetLocaleOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetLocaleOverrideCommandResult? result = JsonSerializer.Deserialize<SetLocaleOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetLocaleOverrideCommandResult? result = JsonSerializer.Deserialize<SetLocaleOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetLocaleOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
