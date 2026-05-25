namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class ActivateCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        ActivateCommandResult? result = JsonSerializer.Deserialize<ActivateCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        ActivateCommandResult? result = JsonSerializer.Deserialize<ActivateCommandResult>("{}");
        Assert.NotNull(result);
        ActivateCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
