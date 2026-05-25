namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetUserAgentOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetUserAgentOverrideCommandResult? result = JsonSerializer.Deserialize<SetUserAgentOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetUserAgentOverrideCommandResult? result = JsonSerializer.Deserialize<SetUserAgentOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetUserAgentOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
