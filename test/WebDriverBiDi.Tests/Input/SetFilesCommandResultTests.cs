namespace WebDriverBiDi.Input;

using System.Text.Json;

public class SetFilesCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetFilesCommandResult? result = JsonSerializer.Deserialize<SetFilesCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetFilesCommandResult? result = JsonSerializer.Deserialize<SetFilesCommandResult>("{}");
        Assert.NotNull(result);
        SetFilesCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
