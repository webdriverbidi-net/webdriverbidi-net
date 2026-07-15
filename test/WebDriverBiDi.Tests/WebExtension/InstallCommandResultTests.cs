namespace WebDriverBiDi.WebExtension;

using System.Text.Json;

public class InstallCommandResultTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "extension": "myExtensionId"
                      }
                      """;
        InstallCommandResult? result = JsonSerializer.Deserialize<InstallCommandResult>(json, this.options);
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
        InstallCommandResult? result = JsonSerializer.Deserialize<InstallCommandResult>(json, this.options);
        Assert.NotNull(result);
        InstallCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingExtensionThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<InstallCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidExtensionTypeThrows()
    {
        string json = """
                      {
                        "extension": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<InstallCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullExtensionThrows()
    {
        string json = """
                      {
                        "extension": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<InstallCommandResult>(json, this.options));
    }
}
