namespace WebDriverBiDi.Network;

using System.Text.Json;

public class FailRequestCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        FailRequestCommandResult? result = JsonSerializer.Deserialize<FailRequestCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        FailRequestCommandResult? result = JsonSerializer.Deserialize<FailRequestCommandResult>("{}");
        Assert.NotNull(result);
        FailRequestCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
