namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class SetBypassCSPCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetBypassCSPCommandResult? result = JsonSerializer.Deserialize<SetBypassCSPCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetBypassCSPCommandResult? result = JsonSerializer.Deserialize<SetBypassCSPCommandResult>("{}");
        Assert.NotNull(result);
        SetBypassCSPCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
