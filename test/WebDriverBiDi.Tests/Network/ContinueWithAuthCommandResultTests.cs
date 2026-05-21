namespace WebDriverBiDi.Network;

using System.Text.Json;

public class ContinueWithAuthCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        ContinueWithAuthCommandResult? result = JsonSerializer.Deserialize<ContinueWithAuthCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ContinueWithAuthCommandResult? result = JsonSerializer.Deserialize<ContinueWithAuthCommandResult>("{}");
        Assert.NotNull(result);
        ContinueWithAuthCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
