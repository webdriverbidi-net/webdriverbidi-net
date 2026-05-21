namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class RemoteObjectReferenceTests
{
    [Fact]
    public void TestCanSerializeRemoteObjectReference()
    {
        RemoteObjectReference reference = new("myHandle");
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.Single(referenceObject);

        Assert.True(referenceObject.ContainsKey("handle"));
        JToken? handle = referenceObject["handle"];
        Assert.NotNull(handle);
        Assert.Equal("myHandle", handle.Value<string>());
    }

    [Fact]
    public void TestCanEditRemoteObjectReferenceHandle()
    {
        RemoteObjectReference reference = new("myHandle")
        {
            Handle = "myNewHandle"
        };
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.Single(referenceObject);

        Assert.True(referenceObject.ContainsKey("handle"));
        JToken? handle = referenceObject["handle"];
        Assert.NotNull(handle);
        Assert.Equal("myNewHandle", handle.Value<string>());
    }

    [Fact]
    public void TestCanSerializeRemoteObjectReferenceWithSharedId()
    {
        RemoteObjectReference reference = new("myHandle")
        {
            SharedId = "mySharedId"
        };
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.Equal(2, referenceObject.Count);

        Assert.True(referenceObject.ContainsKey("handle"));
        JToken? handle = referenceObject["handle"];
        Assert.NotNull(handle);
        Assert.Equal("myHandle", handle.Value<string>());

        Assert.True(referenceObject.ContainsKey("sharedId"));
        JToken? sharedId = referenceObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal("mySharedId", sharedId.Value<string>());
    }

    [Fact]
    public void TestRemoteObjectReferenceCopySemantics()
    {
        RemoteObjectReference reference = new("myHandle");
        RemoteObjectReference copy = reference with { };
        Assert.Equal(reference, copy);
    }

    [Fact]
    public void TestRemoteObjectReferenceHandleGetterReturnsValue()
    {
        RemoteObjectReference reference = new("myHandle");
        Assert.Equal("myHandle", reference.Handle);
    }

    [Fact]
    public void TestRemoteObjectReferenceHandleSetterSetsValue()
    {
        RemoteObjectReference reference = new("myHandle");
        reference.Handle = "myNewHandle";
        Assert.Equal("myNewHandle", reference.Handle);
    }

    [Fact]
    public void TestSettingRemoteObjectReferenceSharedIdToNullThrows()
    {
        RemoteObjectReference reference = new("myHandle");
        Assert.ThrowsAny<ArgumentNullException>(() => reference.Handle = null!);
    }

    [Fact]
    public void TestDeserializingRemoteObjectReferenceThrowsWhenHandleIsMissing()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteObjectReference>(json));
    }

    [Fact]
    public void TestDeserializingRemoteObjectReferenceThrowsWhenHandleIsInvalidType()
    {
        string json = """
                      {
                        "handle": 123
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteObjectReference>(json));
    }

    [Fact]
    public void TestCanSerializeRemoteObjectReferenceWithAdditionalProperties()
    {
        RemoteObjectReference reference = new("myHandle");
        reference.AdditionalData["myPropertyName"] = "myValue";
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.Equal(2, referenceObject.Count);

        Assert.True(referenceObject.ContainsKey("handle"));
        JToken? handle = referenceObject["handle"];
        Assert.NotNull(handle);
        Assert.Equal("myHandle", handle.Value<string>());

        Assert.True(referenceObject.ContainsKey("myPropertyName"));
        JToken? myPropertyName = referenceObject["myPropertyName"];
        Assert.NotNull(myPropertyName);
        Assert.Equal(JTokenType.String, myPropertyName.Type);
        Assert.Equal("myValue", myPropertyName.Value<string>());
    }
}
