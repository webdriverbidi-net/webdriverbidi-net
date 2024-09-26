namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class GetRealmsCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeGetRealmsCommandResult()
    {
        string json = """
                      {
                        "realms": []
                      }
                      """;
        GetRealmsCommandResult? result = JsonSerializer.Deserialize<GetRealmsCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Realms, Is.Empty);
    }

    [Test]
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
        GetRealmsCommandResult? result = JsonSerializer.Deserialize<GetRealmsCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Realms, Has.Count.EqualTo(1));
        Assert.That(result!.Realms[0], Is.TypeOf<WindowRealmInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(result!.Realms[0].RealmId, Is.EqualTo("realmId"));
            Assert.That(result!.Realms[0].Origin, Is.EqualTo("myOrigin"));
            Assert.That(result!.Realms[0].Type, Is.EqualTo(RealmType.Window));
            Assert.That(((WindowRealmInfo)result!.Realms[0]).BrowsingContext, Is.EqualTo("contextId"));
        });
    }

    [Test]
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
        GetRealmsCommandResult? result = JsonSerializer.Deserialize<GetRealmsCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Realms, Has.Count.EqualTo(1));
        Assert.That(result!.Realms[0], Is.Not.TypeOf<WindowRealmInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(result!.Realms[0].RealmId, Is.EqualTo("realmId"));
            Assert.That(result!.Realms[0].Origin, Is.EqualTo("myOrigin"));
            Assert.That(result!.Realms[0].Type, Is.EqualTo(RealmType.Worker));
        });
    }
}
