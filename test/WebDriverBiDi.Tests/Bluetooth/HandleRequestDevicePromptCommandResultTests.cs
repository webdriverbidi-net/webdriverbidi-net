namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class HandleRequestDevicePromptCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        HandleRequestDevicePromptCommandResult? result = JsonSerializer.Deserialize<HandleRequestDevicePromptCommandResult>("{}");
        Assert.NotNull(result);
        Assert.Empty(result.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        HandleRequestDevicePromptCommandResult? result = JsonSerializer.Deserialize<HandleRequestDevicePromptCommandResult>("{}");
        Assert.NotNull(result);
        HandleRequestDevicePromptCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
