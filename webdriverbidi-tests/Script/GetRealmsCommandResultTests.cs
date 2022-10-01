namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[TestFixture]
public class GetRealmsCommandResultTests
{
    [Test]
    public void TestCanDeserializeGetRealmsCommandResult()
    {
        string json = @"{ ""realms"": [] }";
        GetRealmsCommandResult? result = JsonConvert.DeserializeObject<GetRealmsCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Realms.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestCanDeserializeGetRealmsCommandResultWithWindowRealmInfo()
    {
        string json = @"{ ""realms"": [ { ""realm"": ""realmId"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": ""contextId"" } ] }";
        GetRealmsCommandResult? result = JsonConvert.DeserializeObject<GetRealmsCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Realms.Count, Is.EqualTo(1));
        Assert.That(result!.Realms[0], Is.TypeOf<WindowRealmInfo>());
        Assert.That(result!.Realms[0].RealmId, Is.EqualTo("realmId"));
        Assert.That(result!.Realms[0].Origin, Is.EqualTo("myOrigin"));
        Assert.That(result!.Realms[0].Type, Is.EqualTo(RealmType.Window));
        Assert.That(((WindowRealmInfo)result!.Realms[0]).BrowsingContext, Is.EqualTo("contextId"));
    }

    [Test]
    public void TestCanDeserializeGetRealmsCommandResultWithNonWindowRealmInfo()
    {
        string json = @"{ ""realms"": [ { ""realm"": ""realmId"", ""origin"": ""myOrigin"", ""type"": ""worker"" } ] }";
        GetRealmsCommandResult? result = JsonConvert.DeserializeObject<GetRealmsCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Realms.Count, Is.EqualTo(1));
        Assert.That(result!.Realms[0], Is.Not.TypeOf<WindowRealmInfo>());
        Assert.That(result!.Realms[0].RealmId, Is.EqualTo("realmId"));
        Assert.That(result!.Realms[0].Origin, Is.EqualTo("myOrigin"));
        Assert.That(result!.Realms[0].Type, Is.EqualTo(RealmType.Worker));
    }
}