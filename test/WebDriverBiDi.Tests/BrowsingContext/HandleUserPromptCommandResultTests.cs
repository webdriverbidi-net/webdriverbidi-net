namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class HandleUserPromptCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        HandleUserPromptCommandResult? result = JsonSerializer.Deserialize<HandleUserPromptCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        HandleUserPromptCommandResult? result = JsonSerializer.Deserialize<HandleUserPromptCommandResult>("{}");
        Assert.NotNull(result);
        HandleUserPromptCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
