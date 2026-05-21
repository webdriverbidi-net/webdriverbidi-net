namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class TraverseHistoryCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        TraverseHistoryCommandResult? result = JsonSerializer.Deserialize<TraverseHistoryCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        TraverseHistoryCommandResult? result = JsonSerializer.Deserialize<TraverseHistoryCommandResult>("{}");
        Assert.NotNull(result);
        TraverseHistoryCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
