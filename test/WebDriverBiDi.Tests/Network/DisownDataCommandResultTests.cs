namespace WebDriverBiDi.Network;

using System.Text.Json;

public class DisownDataCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        DisownDataCommandResult? result = JsonSerializer.Deserialize<DisownDataCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        DisownDataCommandResult? result = JsonSerializer.Deserialize<DisownDataCommandResult>("{}");
        Assert.NotNull(result);
        DisownDataCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
