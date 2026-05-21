namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class GetClientWindowsCommandResultTests
{
    [Fact]
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
        GetClientWindowsCommandResult? result = JsonSerializer.Deserialize<GetClientWindowsCommandResult>(json);
        Assert.NotNull(result);

        Assert.Single(result.ClientWindows);
        Assert.Equal("myClientWindow", result.ClientWindows[0].ClientWindowId);
        Assert.Equal(ClientWindowState.Normal, result.ClientWindows[0].State);
        Assert.True(result.ClientWindows[0].IsActive);
        Assert.Equal(100u, result.ClientWindows[0].X);
        Assert.Equal(200u, result.ClientWindows[0].Y);
        Assert.Equal(300u, result.ClientWindows[0].Width);
        Assert.Equal(400u, result.ClientWindows[0].Height);
    }

    [Fact]
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
        GetClientWindowsCommandResult? result = JsonSerializer.Deserialize<GetClientWindowsCommandResult>(json);
        Assert.NotNull(result);
        GetClientWindowsCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestCanDeserializeWithEmptyList()
    {
        string json = """
                      {
                        "clientWindows": []
                      }
                      """;
        GetClientWindowsCommandResult? result = JsonSerializer.Deserialize<GetClientWindowsCommandResult>(json);
        Assert.NotNull(result);
        Assert.Empty(result.ClientWindows);
    }
}
