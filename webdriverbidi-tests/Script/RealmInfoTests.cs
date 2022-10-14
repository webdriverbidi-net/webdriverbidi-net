namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[TestFixture]
public class RealmInfoJsonConverterTests
{
    [Test]
    public void TestCanDeserializeWorkerRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""worker"" }";
        RealmInfo? info = JsonConvert.DeserializeObject<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<RealmInfo>());
        RealmInfo realmInfo = (RealmInfo)info!;
        Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
        Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
        Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Worker));
    }

    [Test]
    public void TestCanDeserializeWorkletRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""worklet"" }";
        RealmInfo? info = JsonConvert.DeserializeObject<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<RealmInfo>());
        RealmInfo realmInfo = (RealmInfo)info!;
        Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
        Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
        Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Worklet));
    }

    [Test]
    public void TestCanDeserializeWindowRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": ""myContext"" }";
        RealmInfo? info = JsonConvert.DeserializeObject<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<WindowRealmInfo>());
        WindowRealmInfo realmInfo = (WindowRealmInfo)info!;
        Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
        Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
        Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Window));
        Assert.That(realmInfo.BrowsingContext, Is.EqualTo("myContext"));
        Assert.That(realmInfo.Sandbox, Is.Null);
    }

    [Test]
    public void TestCanDeserializeWindowRealmInfoWithSandbox()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": ""myContext"" , ""sandbox"": ""mySandbox"" }";
        RealmInfo? info = JsonConvert.DeserializeObject<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<WindowRealmInfo>());
        WindowRealmInfo realmInfo = (WindowRealmInfo)info!;
        Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
        Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
        Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Window));
        Assert.That(realmInfo.BrowsingContext, Is.EqualTo("myContext"));
        Assert.That(realmInfo.Sandbox, Is.EqualTo("mySandbox"));
    }

    [Test]
    public void TestCanDeserializeSpecializedWorkerRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""service-worker"" }";
        RealmInfo? info = JsonConvert.DeserializeObject<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<RealmInfo>());
        RealmInfo realmInfo = (RealmInfo)info!;
        Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
        Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
        Assert.That(realmInfo.Type, Is.EqualTo(RealmType.ServiceWorker));
    }

    [Test]
    public void TestCanDeserializeSpecializedWorkletRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""paint-worklet"" }";
        RealmInfo? info = JsonConvert.DeserializeObject<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<RealmInfo>());
        RealmInfo realmInfo = (RealmInfo)info!;
        Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
        Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
        Assert.That(realmInfo.Type, Is.EqualTo(RealmType.PaintWorklet));
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingRealmThrows()
    {
        string json = @"{ ""origin"": ""myOrigin"", ""type"": ""worker"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RealmInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'realm'"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingOriginThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""type"": ""worker"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RealmInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'origin'"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingTypeThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RealmInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'type'"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidTypeThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RealmInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Error setting value"));
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithMissingContextThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""window"" }";
        Assert.That(() => JsonConvert.DeserializeObject<RealmInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'context'"));
    }

}