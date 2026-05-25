namespace WebDriverBiDi.Network;

using System.Text.Json;

public class RemoveDataCollectorCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        RemoveDataCollectorCommandResult? result = JsonSerializer.Deserialize<RemoveDataCollectorCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        RemoveDataCollectorCommandResult? result = JsonSerializer.Deserialize<RemoveDataCollectorCommandResult>("{}");
        Assert.NotNull(result);
        RemoveDataCollectorCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
