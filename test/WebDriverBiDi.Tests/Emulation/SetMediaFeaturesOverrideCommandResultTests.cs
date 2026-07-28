namespace WebDriverBiDi.Emulation;

using System.Text.Json;

public class SetMediaFeaturesOverrideCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetMediaFeaturesOverrideCommandResult? result = JsonSerializer.Deserialize<SetMediaFeaturesOverrideCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetMediaFeaturesOverrideCommandResult? result = JsonSerializer.Deserialize<SetMediaFeaturesOverrideCommandResult>("{}");
        Assert.NotNull(result);
        SetMediaFeaturesOverrideCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
