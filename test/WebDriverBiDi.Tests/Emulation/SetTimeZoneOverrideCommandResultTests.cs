namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetTimeZoneOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetTimeZoneOverrideCommandResult? result = JsonSerializer.Deserialize<SetTimeZoneOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetTimeZoneOverrideCommandResult? result = JsonSerializer.Deserialize<SetTimeZoneOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetTimeZoneOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
