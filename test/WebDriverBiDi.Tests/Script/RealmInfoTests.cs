namespace WebDriverBiDi.Script;

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
        RealmInfo realmInfo = info;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Worker));
        }
    }

    [Test]
    public void TestRealmInfoCopySemantics()
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
        RealmInfo copy = info with { };
        Assert.That(copy, Is.EqualTo(info));
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
        RealmInfo realmInfo = info;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Worklet));
        }
    }

    [Test]
    public void TestWorkletRealmInfoCopySemantics()
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
        Assert.That(info, Is.InstanceOf<WorkletRealmInfo>());
        WorkletRealmInfo realmInfo = (WorkletRealmInfo)info;
        WorkletRealmInfo copy = realmInfo with { };
        Assert.That(copy, Is.EqualTo(info));
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
        WindowRealmInfo realmInfo = (WindowRealmInfo)info;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Window));
            Assert.That(realmInfo.BrowsingContext, Is.EqualTo("myContext"));
            Assert.That(realmInfo.Sandbox, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWindowRealmInfoWithUserContext()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext",
                        "userContext": "myUserContext"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<WindowRealmInfo>());
        WindowRealmInfo realmInfo = (WindowRealmInfo)info;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Window));
            Assert.That(realmInfo.BrowsingContext, Is.EqualTo("myContext"));
            Assert.That(realmInfo.Sandbox, Is.Null);
            Assert.That(realmInfo.UserContext, Is.EqualTo("myUserContext"));
        }
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
        WindowRealmInfo realmInfo = (WindowRealmInfo)info;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Window));
            Assert.That(realmInfo.BrowsingContext, Is.EqualTo("myContext"));
            Assert.That(realmInfo.Sandbox, Is.EqualTo("mySandbox"));
        }
    }

    [Test]
    public void TestWindowRealmInfoCopySemantics()
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
        WindowRealmInfo realmInfo = (WindowRealmInfo)info;
        WindowRealmInfo copy = realmInfo with { };
        Assert.That(copy, Is.EqualTo(info));
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Worker));
        }
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
        DedicatedWorkerRealmInfo realmInfo = (DedicatedWorkerRealmInfo)info;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.DedicatedWorker));
            Assert.That(realmInfo.Owners, Has.Count.EqualTo(1));
            Assert.That(realmInfo.Owners[0], Is.EqualTo("ownerRealm"));
        }
    }

    [Test]
    public void TestDedicatedWorkerRealmInfoCopySemantics()
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
        DedicatedWorkerRealmInfo realmInfo = (DedicatedWorkerRealmInfo)info;
        DedicatedWorkerRealmInfo copy = realmInfo with { };
        Assert.That(copy, Is.EqualTo(info));
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
        SharedWorkerRealmInfo realmInfo = (SharedWorkerRealmInfo)info;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.SharedWorker));
        }
    }

    [Test]
    public void TestSharedWorkerRealmInfoCopySemantics()
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
        SharedWorkerRealmInfo realmInfo = (SharedWorkerRealmInfo)info;
        SharedWorkerRealmInfo copy = realmInfo with { };
        Assert.That(copy, Is.EqualTo(info));
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.ServiceWorker));
        }
    }

    [Test]
    public void TestServiceWorkerRealmInfoCopySemantics()
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
        ServiceWorkerRealmInfo realmInfo = (ServiceWorkerRealmInfo)info;
        ServiceWorkerRealmInfo copy = realmInfo with { };
        Assert.That(copy, Is.EqualTo(info));
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.Worklet));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.PaintWorklet));
        }
    }

    [Test]
    public void TestPaintWorkletRealmInfoCopySemantics()
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
        PaintWorkletRealmInfo copy = realmInfo with { };
        Assert.That(copy, Is.EqualTo(realmInfo));
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(realmInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(realmInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(realmInfo.Type, Is.EqualTo(RealmType.AudioWorklet));
        }
    }

    [Test]
    public void TestAudioWorkletRealmInfoCopySemantics()
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
        AudioWorkletRealmInfo copy = realmInfo with { };
        Assert.That(copy, Is.EqualTo(realmInfo));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'realm'"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidRealmTypeThrows()
    {
        string json = """
                      {
                        "realm": 123,
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("could not be converted"));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'origin'"));
    }

    [Test]
    public void TestDeserializingRealmInfoWithInvalidOriginTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": 123,
                        "type": "worker"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("could not be converted"));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must contain a 'type' property"));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("JSON for 'RealmInfo' type property contains unknown value 'invalid'"));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'context'"));
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": 123
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("value could not be converted"));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("value could not be converted"));
    }

    [Test]
    public void TestDeserializingWindowRealmInfoWithInvalidUserContextTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext",
                        "userContext": 2
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("value could not be converted"));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties including: 'owners'"));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("value could not be converted"));
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
        Assert.That(() => JsonSerializer.Deserialize<RealmInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("value could not be converted"));
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
}
