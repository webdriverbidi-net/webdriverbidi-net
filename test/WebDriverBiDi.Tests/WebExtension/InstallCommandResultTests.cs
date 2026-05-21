namespace WebDriverBiDi.WebExtension;

using System.Text.Json;

public class InstallCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "extension": "myExtensionId"
                      }
                      """;
        InstallCommandResult? result = JsonSerializer.Deserialize<InstallCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("myExtensionId", result.ExtensionId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "extension": "myExtensionId"
                      }
                      """;
        InstallCommandResult? result = JsonSerializer.Deserialize<InstallCommandResult>(json);
        Assert.NotNull(result);
        InstallCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingExtensionThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<InstallCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidExtensionTypeThrows()
    {
        string json = """
                      {
                        "extension": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<InstallCommandResult>(json));
    }
}
