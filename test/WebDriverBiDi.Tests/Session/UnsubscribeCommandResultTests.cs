namespace WebDriverBiDi.Session;

using System.Text.Json;

public class UnsubscribeCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        UnsubscribeCommandResult? result = JsonSerializer.Deserialize<UnsubscribeCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        UnsubscribeCommandResult? result = JsonSerializer.Deserialize<UnsubscribeCommandResult>("{}");
        Assert.NotNull(result);
        UnsubscribeCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
