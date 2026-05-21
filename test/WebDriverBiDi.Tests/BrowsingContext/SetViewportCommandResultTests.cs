namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class SetViewportCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetViewportCommandResult? result = JsonSerializer.Deserialize<SetViewportCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetViewportCommandResult? result = JsonSerializer.Deserialize<SetViewportCommandResult>("{}");
        Assert.NotNull(result);
        SetViewportCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
