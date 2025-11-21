namespace WebDriverBiDi.Browser;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class GetClientWindowsCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "clientWindows": [
                          {
                            "clientWindow": "myClientWindow",
                            "active": true,
                            "state": "normal",
                            "x": 100,
                            "y": 200,
                            "width": 300,
                            "height": 400
                          }
                        ]
                      }
                      """;
        GetClientWindowsCommandResult? result = JsonSerializer.Deserialize<GetClientWindowsCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.ClientWindows, Has.Count.EqualTo(1));
            Assert.That(result.ClientWindows[0].ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.ClientWindows[0].State, Is.EqualTo(ClientWindowState.Normal));
            Assert.That(result.ClientWindows[0].IsActive, Is.True);
            Assert.That(result.ClientWindows[0].X, Is.EqualTo(100));
            Assert.That(result.ClientWindows[0].Y, Is.EqualTo(200));
            Assert.That(result.ClientWindows[0].Width, Is.EqualTo(300));
            Assert.That(result.ClientWindows[0].Height, Is.EqualTo(400));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "clientWindows": [
                          {
                            "clientWindow": "myClientWindow",
                            "active": true,
                            "state": "normal",
                            "x": 100,
                            "y": 200,
                            "width": 300,
                            "height": 400
                          }
                        ]
                      }
                      """;
        GetClientWindowsCommandResult? result = JsonSerializer.Deserialize<GetClientWindowsCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        GetClientWindowsCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestCanDeserializeWithEmptyList()
    {
        string json = """
                      {
                        "clientWindows": []
                      }
                      """;
        GetClientWindowsCommandResult? result = JsonSerializer.Deserialize<GetClientWindowsCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ClientWindows, Is.Empty);
    }
}
