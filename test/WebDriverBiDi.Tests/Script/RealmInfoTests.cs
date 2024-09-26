namespace WebDriverBiDi.Script;

using System.Runtime;
using System.Text.Json;

[TestFixture]
public class RealmInfoTests
{
    [Test]
    public void TestCanDeserializeWorkerRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
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
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "worklet"
                      }
                      """;
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
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext"
                      }
                      """;
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
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext",
                        "sandbox": "mySandbox"
                      }
                      """;
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
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
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
    public void TestCanDeserializeDedicatedWorkerRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker",
                        "owners": [ "ownerRealm" ]
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<DedicatedWorkerRealmInfo>());
        DedicatedWorkerRealmInfo realmInfo = (DedicatedWorkerRealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.DedicatedWorker));
            Assert.That(realmInfo.Owners, Has.Count.EqualTo(1));
            Assert.That(realmInfo.Owners[0], Is.EqualTo("ownerRealm"));
        });
    }

    [Test]
    public void TestCanDeserializeSharedWorkerRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "shared-worker"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<SharedWorkerRealmInfo>());
        SharedWorkerRealmInfo realmInfo = (SharedWorkerRealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.SharedWorker));
        });
    }

    [Test]
    public void TestCanDeserializeServiceWorkerRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "service-worker"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<ServiceWorkerRealmInfo>());
        ServiceWorkerRealmInfo realmInfo = (ServiceWorkerRealmInfo)info!;
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
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "worklet"
                      }
                      """;
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
    public void TestCanDeserializePaintWorkletRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "paint-worklet"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<PaintWorkletRealmInfo>());
        PaintWorkletRealmInfo realmInfo = (PaintWorkletRealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.PaintWorklet));
        });
    }

    [Test]
    public void TestCanDeserializeAudioWorkletRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "audio-worklet"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<AudioWorkletRealmInfo>());
        AudioWorkletRealmInfo realmInfo = (AudioWorkletRealmInfo)info!;
        Assert.Multiple(() =>
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.AudioWorklet));
        });
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingRealmThrows()
    {
        string json = """
                      {
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'realm' property is required"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidRealmTypeThrows()
    {
        string json = """
                      {
                        "realm": null,
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'realm' property must be a string"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingOriginThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "type": "worker"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'origin' property is required"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidOriginTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": null,
                        "type": "worker"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'origin' property must be a string"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithMissingTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'type' property is required"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("'invalid' is not valid for enum type"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithNonStringTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": null
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithMissingContextThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'context' property is required"));
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": null
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'context' property must be a string"));
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithInvalidSandboxTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext",
                        "sandbox": 2
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'sandbox' property must be a string"));
    }

    [Test]
    public void TestDeserializingDedicatedWorkerRealmInfoWithMissingOwnersThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("DedicatedWorkerRealmInfo 'owners' property is required"));
    }

    [Test]
    public void TestDeserializingDedicatedWorkerRealmInfoWithInvalidOwnersTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker",
                        "owners": ""
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("DedicatedWorkerRealmInfo 'owners' property must be an array"));
    }

    [Test]
    public void TestDeserializingDedicatedWorkerRealmInfoWithInvalidOwnersEntryTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker",
                        "owners": [ 123 ]
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("All elements of DedicatedWorkerRealmInfo 'owners' property array must be strings"));
    }

    [Test]
    public void TestDeserializingDedicatedWorkerRealmInfoWithInvalidOwnersEntryValueThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker",
                        "owners": [ "" ]
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("All elements of DedicatedWorkerRealmInfo 'owners' property array must be non-null and non-empty strings"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithNonObjectThrows()
    {
        string json = @"[ ""invalid realm info"" ]";
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanCastToProperSubclassTypeOfRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "service-worker",
                        "owners": [ "ownerRealm" ]
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<ServiceWorkerRealmInfo>());
        Assert.That(info.As<ServiceWorkerRealmInfo>(), Is.Not.Null);
    }

    [Test]
    public void TestCannotCastToImproperSubclassTypeOfRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "service-worker",
                        "owners": [ "ownerRealm" ]
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<ServiceWorkerRealmInfo>());
        Assert.That(() => info.As<SharedWorkerRealmInfo>(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("cannot be cast"));
    }

    [Test]
    public void TestCannotSerialize()
    {
        // NOTE: RealmInfo does not provide a way to instantiate one directly
        // using a constructor, so we will deserialize one from JSON.
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(() => JsonSerializer.Serialize(info), Throws.InstanceOf<NotImplementedException>());
    }
}
