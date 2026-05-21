namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SharedReferenceTests
{
    [Fact]
    public void TestCanSerializeSharedReference()
    {
        SharedReference reference = new("mySharedId");
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.Single(referenceObject);

        Assert.True(referenceObject.ContainsKey("sharedId"));
        JToken? sharedId = referenceObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal("mySharedId", sharedId.Value<string>());
    }

    [Fact]
    public void TestCanEditSharedReferenceSharedId()
    {
        SharedReference reference = new("mySharedId")
        {
            SharedId = "myNewSharedId"
        };
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.Single(referenceObject);

        Assert.True(referenceObject.ContainsKey("sharedId"));
        JToken? sharedId = referenceObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal("myNewSharedId", sharedId.Value<string>());
    }

    [Fact]
    public void TestCanSerializeSharedReferenceWithHandle()
    {
        SharedReference reference = new("mySharedId")
        {
            Handle = "myHandle"
        };
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.Equal(2, referenceObject.Count);

        Assert.True(referenceObject.ContainsKey("sharedId"));
        JToken? sharedId = referenceObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal("mySharedId", sharedId.Value<string>());

        Assert.True(referenceObject.ContainsKey("handle"));
        JToken? handle = referenceObject["handle"];
        Assert.NotNull(handle);
        Assert.Equal("myHandle", handle.Value<string>());
    }

    [Fact]
    public void TestSharedReferenceCopySemantics()
    {
        SharedReference reference = new("mySharedId");
        SharedReference copy = reference with { };
        Assert.Equal(reference, copy);
    }

    [Fact]
    public void TestSharedReferenceSharedIdGetterReturnsValue()
    {
        SharedReference reference = new("mySharedId");
        Assert.Equal("mySharedId", reference.SharedId);
    }

    [Fact]
    public void TestSharedReferenceSharedIdSetterSetsValue()
    {
        SharedReference reference = new("mySharedId");
        reference.SharedId = "myNewSharedId";
        Assert.Equal("myNewSharedId", reference.SharedId);
    }

    [Fact]
    public void TestSettingSharedReferenceHandleToNullThrows()
    {
        SharedReference reference = new("mySharedId");
        Assert.ThrowsAny<ArgumentNullException>(() => reference.SharedId = null!);
    }

    [Fact]
    public void TestDeserializingSharedReferenceThrowsWhenSharedIdIsMissing()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<SharedReference>(json));
    }

    [Fact]
    public void TestDeserializingSharedReferenceThrowsWhenSharedIdIsInvalidType()
    {
        string json = """
                      {
                        "sharedId": 123
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<SharedReference>(json));
    }

    [Fact]
    public void TestCanSerializeSharedReferenceWithAdditionalProperties()
    {
        SharedReference reference = new("mySharedId");
        reference.AdditionalData["myPropertyName"] = "myValue";
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.Equal(2, referenceObject.Count);

        Assert.True(referenceObject.ContainsKey("sharedId"));
        JToken? sharedId = referenceObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal("mySharedId", sharedId.Value<string>());

        Assert.True(referenceObject.ContainsKey("myPropertyName"));
        JToken? myPropertyName = referenceObject["myPropertyName"];
        Assert.NotNull(myPropertyName);
        Assert.Equal(JTokenType.String, myPropertyName.Type);
        Assert.Equal("myValue", myPropertyName.Value<string>());
    }
}
