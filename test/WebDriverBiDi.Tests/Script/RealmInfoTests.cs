namespace WebDriverBiDi.Script;

using System.Text.Json;

public class RealmInfoTests
{
    [Fact]
    public void TestDeserializingWithMissingRealmThrows()
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
    public void TestDeserializingWithInvalidRealmTypeThrows()
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
    public void TestDeserializingWithMissingOriginThrows()
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
    public void TestDeserializingWithInvalidOriginTypeThrows()
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
    public void TestDeserializingWithMissingTypeThrows()
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
    public void TestDeserializingWithInvalidTypeThrows()
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
    public void TestDeserializingWithNonStringTypeThrows()
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
    public void TestDeserializingWithNonObjectThrows()
    {
        string json = @"[ ""invalid realm info"" ]";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json));
    }

    [Fact]
    public void TestCanCastToProperSubclassType()
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
    public void TestCannotCastToImproperSubclassType()
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
