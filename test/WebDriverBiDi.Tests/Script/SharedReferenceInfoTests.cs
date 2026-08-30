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
    public void TestCanDeserializeIgnoringUnknownData()
    {
        string json = """
                      {
                        "sharedId": "mySharedId",
                        "extraData": "myExtraData"
                      }
                      """;
        SharedReferenceInfo? info = JsonSerializer.Deserialize<SharedReferenceInfo>(json);
        Assert.NotNull(info);
        Assert.Equal("mySharedId", info.SharedId);
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
