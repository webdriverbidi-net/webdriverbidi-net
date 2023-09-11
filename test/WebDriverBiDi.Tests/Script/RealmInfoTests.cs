namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class RealmInfoTests
{
    [Test]
    public void TestCanDeserializeWorkerRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""worker"" }";
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<RealmInfo>());
        RealmInfo realmInfo = (RealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Worker));
        });
    }

    [Test]
    public void TestCanDeserializeWorkletRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""worklet"" }";
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<RealmInfo>());
        RealmInfo realmInfo = (RealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Worklet));
        });
    }

    [Test]
    public void TestCanDeserializeWindowRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": ""myContext"" }";
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<WindowRealmInfo>());
        WindowRealmInfo realmInfo = (WindowRealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Window));
            Assert.That(realmInfo.BrowsingContext, Is.EqualTo("myContext"));
            Assert.That(realmInfo.Sandbox, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWindowRealmInfoWithSandbox()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": ""myContext"" , ""sandbox"": ""mySandbox"" }";
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<WindowRealmInfo>());
        WindowRealmInfo realmInfo = (WindowRealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Window));
            Assert.That(realmInfo.BrowsingContext, Is.EqualTo("myContext"));
            Assert.That(realmInfo.Sandbox, Is.EqualTo("mySandbox"));
        });
    }

    [Test]
    public void TestCanDeserializeSpecializedWorkerRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""service-worker"" }";
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<RealmInfo>());
        RealmInfo realmInfo = (RealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.ServiceWorker));
        });
    }

    [Test]
    public void TestCanDeserializeSpecializedWorkletRealmInfo()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""paint-worklet"" }";
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<RealmInfo>());
        RealmInfo realmInfo = (RealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.PaintWorklet));
        });
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingRealmThrows()
    {
        string json = @"{ ""origin"": ""myOrigin"", ""type"": ""worker"" }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'realm' property is required"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidRealmTypeThrows()
    {
        string json = @"{ ""origin"": ""myOrigin"", ""type"": ""worker"", ""realm"": null }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'realm' property must be a string"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingOriginThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""type"": ""worker"" }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'origin' property is required"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidOriginTypeThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""type"": ""worker"", ""origin"": null }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'origin' property must be a string"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingTypeThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"" }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'type' property is required"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidTypeThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""invalid"" }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("'invalid' is not valid for enum type"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithNonStringTypeThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": null }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithMissingContextThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""window"" }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'context' property is required"));
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithInvalidContextTypeThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": null }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'context' property must be a string"));
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithInvalidSandboxTypeThrows()
    {
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""window"", ""context"": ""myContext"", ""sandbox"": 2 }";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'sandbox' property must be a string"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithNonObjectThrows()
    {
        string json = @"[ ""invalid realm info"" ]";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCannotSerialize()
    {
        // NOTE: RealmInfo does not provide a way to instantiate one directly
        // using a constructor, so we will deserialize one from JSON.
        string json = @"{ ""realm"": ""myRealm"", ""origin"": ""myOrigin"", ""type"": ""worker"" }";
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(() => JsonSerializer.Serialize(info), Throws.InstanceOf<NotImplementedException>());
    }
}