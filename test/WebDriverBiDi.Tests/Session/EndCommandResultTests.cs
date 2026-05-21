namespace WebDriverBiDi.Session;

using System.Text.Json;

public class EndCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        EndCommandResult? result = JsonSerializer.Deserialize<EndCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        EndCommandResult? result = JsonSerializer.Deserialize<EndCommandResult>("{}");
        Assert.NotNull(result);
        EndCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
