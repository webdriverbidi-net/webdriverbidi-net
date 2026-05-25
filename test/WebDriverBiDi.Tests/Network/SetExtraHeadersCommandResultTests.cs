namespace WebDriverBiDi.Network;

using System.Text.Json;

public class SetExtraHeadersCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetExtraHeadersCommandResult? result = JsonSerializer.Deserialize<SetExtraHeadersCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetExtraHeadersCommandResult? result = JsonSerializer.Deserialize<SetExtraHeadersCommandResult>("{}");
        Assert.NotNull(result);
        SetExtraHeadersCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
