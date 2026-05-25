namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetGeolocationOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetGeolocationOverrideCommandResult? result = JsonSerializer.Deserialize<SetGeolocationOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetGeolocationOverrideCommandResult? result = JsonSerializer.Deserialize<SetGeolocationOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetGeolocationOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
