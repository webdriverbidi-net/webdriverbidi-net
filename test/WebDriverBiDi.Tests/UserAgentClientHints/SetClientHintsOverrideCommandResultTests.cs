namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json;

public class SetClientHintsOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetClientHintsOverrideCommandResult? result = JsonSerializer.Deserialize<SetClientHintsOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetClientHintsOverrideCommandResult? result = JsonSerializer.Deserialize<SetClientHintsOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetClientHintsOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
