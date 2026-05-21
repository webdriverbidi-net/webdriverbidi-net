namespace WebDriverBiDi.Network;

using System.Text.Json;

public class ContinueResponseCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        ContinueResponseCommandResult? result = JsonSerializer.Deserialize<ContinueResponseCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ContinueResponseCommandResult? result = JsonSerializer.Deserialize<ContinueResponseCommandResult>("{}");
        Assert.NotNull(result);
        ContinueResponseCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
