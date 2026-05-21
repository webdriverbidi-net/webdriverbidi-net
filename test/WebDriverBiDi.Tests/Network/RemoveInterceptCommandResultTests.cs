namespace WebDriverBiDi.Network;

using System.Text.Json;

public class RemoveInterceptCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        RemoveInterceptCommandResult? result = JsonSerializer.Deserialize<RemoveInterceptCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        RemoveInterceptCommandResult? result = JsonSerializer.Deserialize<RemoveInterceptCommandResult>("{}");
        Assert.NotNull(result);
        RemoveInterceptCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
