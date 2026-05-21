namespace WebDriverBiDi.Network;

using System.Text.Json;

public class ProvideResponseCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        ProvideResponseCommandResult? result = JsonSerializer.Deserialize<ProvideResponseCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ProvideResponseCommandResult? result = JsonSerializer.Deserialize<ProvideResponseCommandResult>("{}");
        Assert.NotNull(result);
        ProvideResponseCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
