namespace WebDriverBiDi.Network;

using System.Text.Json;

public class HeaderTests
{
    [Fact]
    public void TestCanConstructHeader()
    {
        Header header = new("name", "value");
        Assert.Equal("name", header.Name);
        Assert.Equal(BytesValue.FromString("value"), header.Value);
    }

    [Fact]
    public void TestCanDeserializeHeader()
    {
        string json = """
                      {
                        "name": "headerName",
                        "value": {
                          "type": "string",
                          "value": "headerValue" 
                        }
                      }
                      """;
        Header? header = JsonSerializer.Deserialize<Header>(json);
        Assert.NotNull(header);

        Assert.Equal("headerName", header.Name);
        Assert.Equal(BytesValueType.String, header.Value.Type);
        Assert.Equal("headerValue", header.Value.Value);
    }

    [Fact]
    public void TestCanDeserializeHeaderWithBinaryValue()
    {
        byte[] byteArray = new byte[] { 0x41, 0x42, 0x43 };
        string base64Value = Convert.ToBase64String(byteArray);
        string json = $$"""
                      {
                        "name": "headerName",
                        "value": {
                          "type": "base64",
                          "value": "{{base64Value}}"
                        }
                      }
                      """;
        Header? header = JsonSerializer.Deserialize<Header>(json);
        Assert.NotNull(header);

        Assert.Equal("headerName", header.Name);
        Assert.Equal(BytesValueType.Base64, header.Value.Type);
        Assert.Equal(base64Value, header.Value.Value);
    }

    [Fact]
    public void TestDeserializingWithMissingNameThrows()
    {
        string json = """
                      {
                        "value": {
                          "type": "string",
                          "value": "headerValue" 
                        }
                      }
                      """;
        Assert.Contains("missing required properties including: 'name", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Header>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWithMissingValueThrows()
    {
        string json = """
                      {
                        "name": "headerName"
                      }
                      """;
        Assert.Contains("missing required properties including: 'value", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Header>(json)).Message);
    }
}
