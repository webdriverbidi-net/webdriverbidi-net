namespace WebDriverBiDi.Script;

using System.Text.Json;

public class DisownCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        DisownCommandResult? result = JsonSerializer.Deserialize<DisownCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        DisownCommandResult? result = JsonSerializer.Deserialize<DisownCommandResult>("{}");
        Assert.NotNull(result);
        DisownCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
