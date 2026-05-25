namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetScriptingEnabledCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetScriptingEnabledCommandResult? result = JsonSerializer.Deserialize<SetScriptingEnabledCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetScriptingEnabledCommandResult? result = JsonSerializer.Deserialize<SetScriptingEnabledCommandResult>("{}");
        Assert.NotNull(result);
        SetScriptingEnabledCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
