namespace WebDriverBiDi.Network;

using System.Text.Json;

public class ReadOnlyHeaderTests
{
    [Fact]
    public void CanDeserializeHeader()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [
                          {
                            "name": "headerName",
                            "value": {
                              "type": "string",
                              "value": "headerValue" 
                            }
                          }
                        ],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        RequestData? request = JsonSerializer.Deserialize<RequestData>(json);
        Assert.NotNull(request);
        ReadOnlyHeader header = request.Headers[0];

        Assert.Equal("headerName", header.Name);
        Assert.Equal(BytesValueType.String, header.Value.Type);
        Assert.Equal("headerValue", header.Value.Value);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "request": "myRequestId",
                        "url": "requestUrl",
                        "method": "get",
                        "headers": [
                          {
                            "name": "headerName",
                            "value": {
                              "type": "string",
                              "value": "headerValue" 
                            }
                          }
                        ],
                        "cookies": [],
                        "destination": "document",
                        "initiatorType": "other",
                        "headersSize": 0,
                        "bodySize": 0,
                        "timings": {
                          "timeOrigin": 1,
                          "requestTime": 2,
                          "redirectStart": 3,
                          "redirectEnd": 4,
                          "fetchStart": 5,
                          "dnsStart": 6,
                          "dnsEnd": 7,
                          "connectStart": 8,
                          "connectEnd": 9,
                          "tlsStart": 10,
                          "requestStart": 11,
                          "responseStart": 12,
                          "responseEnd": 13
                        }
                      }
                      """;
        RequestData? request = JsonSerializer.Deserialize<RequestData>(json);
        Assert.NotNull(request);
        ReadOnlyHeader header = request.Headers[0];
        ReadOnlyHeader copy = header with { };
        Assert.Equal(header, copy);
    }
}
