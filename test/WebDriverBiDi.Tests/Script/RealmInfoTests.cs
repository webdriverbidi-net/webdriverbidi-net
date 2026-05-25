namespace WebDriverBiDi.Script;

using System.Text.Json;

public class RealmInfoTests
{
    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<WorkerRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.Worker, info.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<RealmInfo>(info, exactMatch: false);
        RealmInfo copy = info with { };
        Assert.Equal(info, copy);
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<WorkletRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.Worklet, info.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        WorkletRealmInfo realmInfo = Assert.IsType<WorkletRealmInfo>(info);
        WorkletRealmInfo copy = realmInfo with { };
        Assert.Equal(info, copy);
    }

    [Fact]
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
        Assert.NotNull(info);
        WindowRealmInfo realmInfo = Assert.IsType<WindowRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.Window, realmInfo.Type);
        Assert.Equal("myContext", realmInfo.BrowsingContext);
        Assert.Null(realmInfo.Sandbox);
    }

    [Fact]
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
        Assert.NotNull(info);
        WindowRealmInfo realmInfo = Assert.IsType<WindowRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.Window, realmInfo.Type);
        Assert.Equal("myContext", realmInfo.BrowsingContext);
        Assert.Null(realmInfo.Sandbox);
        Assert.Equal("myUserContext", realmInfo.UserContext);
    }

    [Fact]
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
        Assert.NotNull(info);
        WindowRealmInfo realmInfo = Assert.IsType<WindowRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.Window, realmInfo.Type);
        Assert.Equal("myContext", realmInfo.BrowsingContext);
        Assert.Equal("mySandbox", realmInfo.Sandbox);
    }

    [Fact]
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
        Assert.NotNull(info);
        WindowRealmInfo realmInfo = Assert.IsType<WindowRealmInfo>(info);
        WindowRealmInfo copy = realmInfo with { };
        Assert.Equal(info, copy);
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<WorkerRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.Worker, info.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        DedicatedWorkerRealmInfo realmInfo = Assert.IsType<DedicatedWorkerRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.DedicatedWorker, realmInfo.Type);
        Assert.Single(realmInfo.Owners);
        Assert.Equal("ownerRealm", realmInfo.Owners[0]);
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<DedicatedWorkerRealmInfo>(info);
        DedicatedWorkerRealmInfo realmInfo = (DedicatedWorkerRealmInfo)info;
        DedicatedWorkerRealmInfo copy = realmInfo with { };
        Assert.Equal(info, copy);
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<SharedWorkerRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.SharedWorker, info.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        SharedWorkerRealmInfo realmInfo = Assert.IsType<SharedWorkerRealmInfo>(info);
        SharedWorkerRealmInfo copy = realmInfo with { };
        Assert.Equal(info, copy);
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<ServiceWorkerRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.ServiceWorker, info.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        ServiceWorkerRealmInfo realmInfo = Assert.IsType<ServiceWorkerRealmInfo>(info);
        ServiceWorkerRealmInfo copy = realmInfo with { };
        Assert.Equal(info, copy);
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<WorkletRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.Worklet, info.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        PaintWorkletRealmInfo realmInfo = Assert.IsType<PaintWorkletRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.PaintWorklet, realmInfo.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        PaintWorkletRealmInfo realmInfo = Assert.IsType<PaintWorkletRealmInfo>(info);
        PaintWorkletRealmInfo copy = realmInfo with { };
        Assert.Equal(realmInfo, copy);
    }

    [Fact]
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
        Assert.NotNull(info);
        AudioWorkletRealmInfo realmInfo = Assert.IsType<AudioWorkletRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.AudioWorklet, realmInfo.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        AudioWorkletRealmInfo realmInfo = Assert.IsType<AudioWorkletRealmInfo>(info);
        AudioWorkletRealmInfo copy = realmInfo with { };
        Assert.Equal(realmInfo, copy);
    }

    [Fact]
    public void TestDeserializingRealmInfoWithMissingRealmThrows()
    {
        string json = """
                      {
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
        Assert.Contains("missing required properties including: 'realm'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRealmInfoWithInvalidRealmTypeThrows()
    {
        string json = """
                      {
                        "realm": 123,
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
        Assert.Contains("could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRealmInfoWithMissingOriginThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "type": "worker"
                      }
                      """;
        Assert.Contains("missing required properties including: 'origin'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRealmInfoWithInvalidOriginTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": 123,
                        "type": "worker"
                      }
                      """;
        Assert.Contains("could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRealmInfoWithMissingTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin"
                      }
                      """;
        Assert.Contains("must contain a 'type' property", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRealmInfoWithInvalidTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "invalid"
                      }
                      """;
        Assert.Contains("JSON for 'RealmInfo' type property contains unknown value 'invalid'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRealmInfoWithNonStringTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json));
    }

    [Fact]
    public void TestDeserializingWindowRealmInfoWithMissingContextThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window"
                      }
                      """;
        Assert.Contains("missing required properties including: 'context'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingDedicatedWorkerRealmInfoWithMissingOwnersThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker"
                      }
                      """;
        Assert.Contains("missing required properties including: 'owners'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRealmInfoWithNonObjectThrows()
    {
        string json = @"[ ""invalid realm info"" ]";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json));
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<ServiceWorkerRealmInfo>(info);
        Assert.NotNull(info.As<ServiceWorkerRealmInfo>());
    }

    [Fact]
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
        Assert.NotNull(info);
        Assert.IsType<ServiceWorkerRealmInfo>(info);
        Assert.Contains("cannot be cast", Assert.ThrowsAny<WebDriverBiDiException>(() => info.As<SharedWorkerRealmInfo>()).Message);
    }
}
