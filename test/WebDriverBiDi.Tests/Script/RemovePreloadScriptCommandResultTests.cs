namespace WebDriverBiDi.Script;

using System.Text.Json;

public class RemovePreloadScriptCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        RemovePreloadScriptCommandResult? result = JsonSerializer.Deserialize<RemovePreloadScriptCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        RemovePreloadScriptCommandResult? result = JsonSerializer.Deserialize<RemovePreloadScriptCommandResult>("{}");
        Assert.NotNull(result);
        RemovePreloadScriptCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
