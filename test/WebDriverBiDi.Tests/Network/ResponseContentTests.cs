namespace WebDriverBiDi.Network;

using System.Text.Json;

public class ResponseContentTests
{
    [Fact]
    public void TestCanDeserializeResponseContent()
    {
        string json = """
                      {
                        "size": 300
                      }
                      """;
        ResponseContent? responseContent = JsonSerializer.Deserialize<ResponseContent>(json);
        Assert.NotNull(responseContent);

        Assert.Equal(300u, responseContent.Size);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "size": 300
                      }
                      """;
        ResponseContent? responseContent = JsonSerializer.Deserialize<ResponseContent>(json);
        Assert.NotNull(responseContent);
        ResponseContent copy = responseContent with { };
        Assert.Equal(responseContent, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingSizeThrows()
    {
        string json = "{}";
        Assert.Contains("missing required properties including: 'size", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseContent>(json)).Message);
    }
}
