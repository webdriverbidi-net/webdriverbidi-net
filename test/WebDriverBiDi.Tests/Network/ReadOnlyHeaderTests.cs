namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class ReadOnlyHeaderTests
{
    [Test]
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
        Assert.That(request, Is.Not.Null);
        ReadOnlyHeader header = request.Headers[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(header.Name, Is.EqualTo("headerName"));
            Assert.That(header.Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(header.Value.Value, Is.EqualTo("headerValue"));
        }       
    }

    [Test]
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
        Assert.That(request, Is.Not.Null);
        ReadOnlyHeader header = request.Headers[0];
        ReadOnlyHeader copy = header with { };
        Assert.That(copy, Is.EqualTo(header));
    }
}
