namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using WebDriverBiDi.Script;

[TestFixture]
public class RealmInfoJsonConverterTests
{
    [Test]
    public void TestDeserializeWindowRealmWithContextReturnsWindowRealmInfo()
    {
        string json = """{"type": "window", "realm": "realm1", "origin": "https://example.com", "context": "ctx1"}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<WindowRealmInfo>());
        WindowRealmInfo windowRealm = (WindowRealmInfo)result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(windowRealm.RealmId, Is.EqualTo("realm1"));
            Assert.That(windowRealm.Origin, Is.EqualTo("https://example.com"));
            Assert.That(windowRealm.Type, Is.EqualTo(RealmType.Window));
            Assert.That(windowRealm.BrowsingContext, Is.EqualTo("ctx1"));
        }
    }

    [Test]
    public void TestDeserializeWindowRealmWithSandboxAndUserContext()
    {
        string json = """{"type": "window", "realm": "realm1", "origin": "https://example.com", "context": "ctx1", "sandbox": "sandbox1", "userContext": "userCtx1"}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<WindowRealmInfo>());
        WindowRealmInfo windowRealm = (WindowRealmInfo)result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(windowRealm.Sandbox, Is.EqualTo("sandbox1"));
            Assert.That(windowRealm.UserContext, Is.EqualTo("userCtx1"));
        }
    }

    [Test]
    public void TestDeserializeDedicatedWorkerWithOwners()
    {
        string json = """{"type": "dedicated-worker", "realm": "realm1", "origin": "https://example.com", "owners": ["owner1", "owner2"]}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<DedicatedWorkerRealmInfo>());
        DedicatedWorkerRealmInfo dedicatedWorkerRealm = (DedicatedWorkerRealmInfo)result!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dedicatedWorkerRealm.RealmId, Is.EqualTo("realm1"));
            Assert.That(dedicatedWorkerRealm.Owners, Has.Count.EqualTo(2));
            Assert.That(dedicatedWorkerRealm.Owners[0], Is.EqualTo("owner1"));
            Assert.That(dedicatedWorkerRealm.Owners[1], Is.EqualTo("owner2"));
        }
    }

    [Test]
    public void TestDeserializeSharedWorkerReturnsSharedWorkerRealmInfo()
    {
        string json = """{"type": "shared-worker", "realm": "realm1", "origin": "https://example.com"}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<SharedWorkerRealmInfo>());
        Assert.That(result!.RealmId, Is.EqualTo("realm1"));
    }

    [Test]
    public void TestDeserializeServiceWorkerReturnsServiceWorkerRealmInfo()
    {
        string json = """{"type": "service-worker", "realm": "realm1", "origin": "https://example.com"}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ServiceWorkerRealmInfo>());
        Assert.That(result!.RealmId, Is.EqualTo("realm1"));
    }

    [Test]
    public void TestDeserializeAudioWorkletReturnsAudioWorkletRealmInfo()
    {
        string json = """{"type": "audio-worklet", "realm": "realm1", "origin": "https://example.com"}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<AudioWorkletRealmInfo>());
        Assert.That(result!.RealmId, Is.EqualTo("realm1"));
    }

    [Test]
    public void TestDeserializePaintWorkletReturnsPaintWorkletRealmInfo()
    {
        string json = """{"type": "paint-worklet", "realm": "realm1", "origin": "https://example.com"}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<PaintWorkletRealmInfo>());
        Assert.That(result!.RealmId, Is.EqualTo("realm1"));
    }

    [Test]
    public void TestDeserializeWorkerTypeReturnsBaseRealmInfo()
    {
        string json = """{"type": "worker", "realm": "realm1", "origin": "https://example.com"}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<RealmInfo>());
        Assert.That(result, Is.Not.InstanceOf<WindowRealmInfo>());
        Assert.That(result!.RealmId, Is.EqualTo("realm1"));
    }

    [Test]
    public void TestDeserializeWithMissingTypeThrowsJsonException()
    {
        string json = """{"realm": "realm1", "origin": "https://example.com"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'type' property is required"));
    }

    [Test]
    public void TestDeserializeWithNonStringTypeThrowsJsonException()
    {
        string json = """{"type": 123, "realm": "realm1", "origin": "https://example.com"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'type' property must be a string"));
    }

    [Test]
    public void TestDeserializeWithMissingRealmThrowsJsonException()
    {
        string json = """{"type": "window", "origin": "https://example.com", "context": "ctx1"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'realm' property is required"));
    }

    [Test]
    public void TestDeserializeWithNonStringRealmThrowsJsonException()
    {
        string json = """{"type": "window", "realm": 123, "origin": "https://example.com", "context": "ctx1"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'realm' property must be a string"));
    }

    [Test]
    public void TestDeserializeWithMissingOriginThrowsJsonException()
    {
        string json = """{"type": "window", "realm": "realm1", "context": "ctx1"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'origin' property is required"));
    }

    [Test]
    public void TestDeserializeWithNonStringOriginThrowsJsonException()
    {
        string json = """{"type": "window", "realm": "realm1", "origin": 123, "context": "ctx1"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'origin' property must be a string"));
    }

    [Test]
    public void TestDeserializeNonObjectThrowsJsonException()
    {
        string json = """["invalid realm info"]""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must be an object"));
    }

    [Test]
    public void TestDeserializeWindowRealmWithMissingContextThrowsJsonException()
    {
        string json = """{"type": "window", "realm": "realm1", "origin": "https://example.com"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'context' property is required"));
    }

    [Test]
    public void TestDeserializeWindowRealmWithNonStringContextThrowsJsonException()
    {
        string json = """{"type": "window", "realm": "realm1", "origin": "https://example.com", "context": 123}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'context' property must be a string"));
    }

    [Test]
    public void TestDeserializeWindowRealmWithNonStringSandboxThrowsJsonException()
    {
        string json = """{"type": "window", "realm": "realm1", "origin": "https://example.com", "context": "ctx1", "sandbox": 123}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'sandbox' property must be a string"));
    }

    [Test]
    public void TestDeserializeWindowRealmWithNonStringUserContextThrowsJsonException()
    {
        string json = """{"type": "window", "realm": "realm1", "origin": "https://example.com", "context": "ctx1", "userContext": 123}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'userContext' property must be a string"));
    }

    [Test]
    public void TestDeserializeDedicatedWorkerWithMissingOwnersThrowsJsonException()
    {
        string json = """{"type": "dedicated-worker", "realm": "realm1", "origin": "https://example.com"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'owners' property is required"));
    }

    [Test]
    public void TestDeserializeDedicatedWorkerWithNonArrayOwnersThrowsJsonException()
    {
        string json = """{"type": "dedicated-worker", "realm": "realm1", "origin": "https://example.com", "owners": "not-an-array"}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'owners' property must be an array"));
    }

    [Test]
    public void TestDeserializeDedicatedWorkerWithOwnersContainingNonStringElementThrowsJsonException()
    {
        string json = """{"type": "dedicated-worker", "realm": "realm1", "origin": "https://example.com", "owners": ["owner1", 123]}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must be strings"));
    }

    [Test]
    public void TestDeserializeDedicatedWorkerWithOwnersContainingEmptyStringElementThrowsJsonException()
    {
        string json = """{"type": "dedicated-worker", "realm": "realm1", "origin": "https://example.com", "owners": ["owner1", ""]}""";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("non-null and non-empty"));
    }

    [Test]
    public void TestWriteThrowsNotImplementedException()
    {
        string json = """{"type": "window", "realm": "realm1", "origin": "https://example.com", "context": "ctx1"}""";
        RealmInfo? result = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(() => JsonSerializer.Serialize(result!), Throws.InstanceOf<NotImplementedException>());
    }
}
