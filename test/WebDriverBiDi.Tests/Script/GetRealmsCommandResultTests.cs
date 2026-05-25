namespace WebDriverBiDi.Script;

using System.Text.Json;

public class GetRealmsCommandResultTests
{
    [Fact]
    public void TestCanDeserializeGetRealmsCommandResult()
    {
        string json = """
                      {
                        "realms": []
                      }
                      """;
        GetRealmsCommandResult? result = JsonSerializer.Deserialize<GetRealmsCommandResult>(json);
        Assert.NotNull(result);
        Assert.Empty(result.Realms);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realms": []
                      }
                      """;
        GetRealmsCommandResult? result = JsonSerializer.Deserialize<GetRealmsCommandResult>(json);
        Assert.NotNull(result);
        GetRealmsCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestCanDeserializeGetRealmsCommandResultWithWindowRealmInfo()
    {
        string json = """
                      {
                        "realms": [
                          {
                            "realm": "realmId",
                            "origin": "myOrigin",
                            "type": "window",
                            "context": "contextId"
                          }
                        ]
                      }
                      """;
        GetRealmsCommandResult? result = JsonSerializer.Deserialize<GetRealmsCommandResult>(json);
        Assert.NotNull(result);
        Assert.Single(result.Realms);
        Assert.IsType<WindowRealmInfo>(result.Realms[0]);

        Assert.Equal("realmId", result.Realms[0].RealmId);
        Assert.Equal("myOrigin", result.Realms[0].Origin);
        Assert.Equal(RealmType.Window, result.Realms[0].Type);
        Assert.Equal("contextId", ((WindowRealmInfo)result.Realms[0]).BrowsingContext);
    }

    [Fact]
    public void TestCanDeserializeGetRealmsCommandResultWithNonWindowRealmInfo()
    {
        string json = """
                      {
                        "realms": [
                          {
                            "realm": "realmId",
                            "origin": "myOrigin",
                            "type": "worker"
                          }
                        ]
                      }
                      """;
        GetRealmsCommandResult? result = JsonSerializer.Deserialize<GetRealmsCommandResult>(json);
        Assert.NotNull(result);
        Assert.Single(result.Realms);
        Assert.IsNotType<WindowRealmInfo>(result.Realms[0]);

        Assert.Equal("realmId", result.Realms[0].RealmId);
        Assert.Equal("myOrigin", result.Realms[0].Origin);
        Assert.Equal(RealmType.Worker, result.Realms[0].Type);
    }
}
