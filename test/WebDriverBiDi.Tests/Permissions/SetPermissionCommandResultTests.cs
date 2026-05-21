namespace WebDriverBiDi.Permissions;

using System.Text.Json;

public class SetPermissionCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        SetPermissionCommandResult? result = JsonSerializer.Deserialize<SetPermissionCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        SetPermissionCommandResult? result = JsonSerializer.Deserialize<SetPermissionCommandResult>("{}");
        Assert.NotNull(result);
        SetPermissionCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
