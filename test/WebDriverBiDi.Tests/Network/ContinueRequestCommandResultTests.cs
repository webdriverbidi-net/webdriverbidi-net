namespace WebDriverBiDi.Network;

using System.Text.Json;

public class ContinueRequestCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        ContinueRequestCommandResult? result = JsonSerializer.Deserialize<ContinueRequestCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ContinueRequestCommandResult? result = JsonSerializer.Deserialize<ContinueRequestCommandResult>("{}");
        Assert.NotNull(result);
        ContinueRequestCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
