namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class RemoveUserContextCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        RemoveUserContextCommandResult? result = JsonSerializer.Deserialize<RemoveUserContextCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        RemoveUserContextCommandResult? result = JsonSerializer.Deserialize<RemoveUserContextCommandResult>("{}");
        Assert.NotNull(result);
        RemoveUserContextCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
