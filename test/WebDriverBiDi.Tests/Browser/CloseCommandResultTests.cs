namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class CloseCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        CloseCommandResult? result = JsonSerializer.Deserialize<CloseCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        CloseCommandResult? result = JsonSerializer.Deserialize<CloseCommandResult>("{}");
        Assert.NotNull(result);
        CloseCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
