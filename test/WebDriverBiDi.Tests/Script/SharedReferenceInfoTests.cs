namespace WebDriverBiDi.Script;

using System.Text.Json;

public class SharedReferenceInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "sharedId": "mySharedId"
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);
        Assert.Equal("mySharedId", info.SharedId);
        Assert.Null(info.Handle);
    }

    [Fact]
    public void TestCanDeserializeWithHandle()
    {
        string json = """
                      {
                        "sharedId": "mySharedId",
                        "handle": "myHandle"
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);
        Assert.Equal("mySharedId", info.SharedId);
        Assert.Equal("myHandle", info.Handle);
    }

    [Fact]
    public void TestCanDeserializeWithAdditionalData()
    {
        string json = """
                      {
                        "sharedId": "mySharedId",
                        "extraData": "myExtraData",
                        "nested": { "key": 1 }
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);
        Assert.Equal("mySharedId", info.SharedId);
        Assert.Equal(2, info.AdditionalData.Count);
        Assert.Equal("myExtraData", info.AdditionalData["extraData"]);
        ReceivedDataDictionary? nested = info.AdditionalData["nested"] as ReceivedDataDictionary;
        Assert.NotNull(nested);
        Assert.Equal(1L, nested["key"]);

        // The conversion is performed once; a second read returns the same dictionary.
        Assert.Same(info.AdditionalData, info.AdditionalData);
    }

    [Fact]
    public void TestAdditionalDataIsEmptyWhenNoneReceived()
    {
        string json = """
                      {
                        "sharedId": "mySharedId"
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);
        Assert.Empty(info.AdditionalData);
        Assert.Same(ReceivedDataDictionary.EmptyDictionary, info.AdditionalData);
    }

    [Fact]
    public void TestToSharedReference()
    {
        string json = """
                      {
                        "sharedId": "mySharedId",
                        "handle": "myHandle"
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);

        SharedReference reference = info.ToSharedReference();
        Assert.Equal("mySharedId", reference.SharedId);
        Assert.Equal("myHandle", reference.Handle);
        Assert.Empty(reference.AdditionalData);
    }

    [Fact]
    public void TestToSharedReferenceCopiesAdditionalData()
    {
        string json = """
                      {
                        "sharedId": "mySharedId",
                        "extraData": "myExtraData",
                        "nested": { "key": 1 }
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);

        SharedReference reference = info.ToSharedReference();
        Assert.Equal(2, reference.AdditionalData.Count);
        Assert.Equal("myExtraData", reference.AdditionalData["extraData"]);

        // The copy is writable and detached from the received data.
        Dictionary<string, object?>? nested = reference.AdditionalData["nested"] as Dictionary<string, object?>;
        Assert.NotNull(nested);
        Assert.Equal(1L, nested["key"]);
        reference.AdditionalData["extraData"] = "changed";
        Assert.Equal("myExtraData", info.AdditionalData["extraData"]);

        string serialized = JsonSerializer.Serialize(reference);
        using JsonDocument document = JsonDocument.Parse(serialized);
        Assert.Equal("myExtraData", info.AdditionalData["extraData"]);
        Assert.Equal("changed", document.RootElement.GetProperty("extraData").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("nested").GetProperty("key").GetInt32());
    }

    [Fact]
    public void TestToSharedReferenceWithNullHandle()
    {
        string json = """
                      {
                        "sharedId": "mySharedId"
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);

        SharedReference reference = info.ToSharedReference();
        Assert.Equal("mySharedId", reference.SharedId);
        Assert.Null(reference.Handle);
    }

    [Fact]
    public void TestDeserializingWithMissingSharedIdThrows()
    {
        string json = """
                      {
                        "handle": "myHandle"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<SharedReferenceInfo>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "sharedId": "mySharedId"
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);
        SharedReferenceInfo copy = info with { };
        Assert.Equal(info, copy);
        Assert.NotSame(info, copy);
    }
}
