namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class BrowsingContextInfoTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.ClientWindowId, Is.EqualTo("myClientWindowId"));
            Assert.That(info.UserContextId, Is.EqualTo("myUserContextId"));
            Assert.That(info.OriginalOpener, Is.EqualTo("openerContext"));
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Is.Empty);
            Assert.That(info.Parent, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithChildren()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "default",
                        "children": [
                          {
                            "context": "childContextId", 
                            "clientWindow": "myClientWindowId",
                            "url": "http://example.com/subdirectory",
                            "originalOpener": null,
                            "userContext": "default",
                            "children": []
                          }
                        ]
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.ClientWindowId, Is.EqualTo("myClientWindowId"));
            Assert.That(info.OriginalOpener, Is.EqualTo("openerContext"));
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Has.Count.EqualTo(1));
            Assert.That(info.Parent, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithOptionalParent()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "userContext": "myUserContextId",
                        "originalOpener": "openerContext",
                        "children": [],
                        "parent": "parentContextId"
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.ClientWindowId, Is.EqualTo("myClientWindowId"));
            Assert.That(info.OriginalOpener, Is.EqualTo("openerContext"));
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Has.Count.EqualTo(0));
            Assert.That(info.Parent, Is.Not.Null);
            Assert.That(info.Parent, Is.EqualTo("parentContextId"));
        });
    }

    [Test]
    public void TestCanDeserializeWithNullOriginalOpener()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": null,
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.ClientWindowId, Is.EqualTo("myClientWindowId"));
            Assert.That(info.UserContextId, Is.EqualTo("myUserContextId"));
            Assert.That(info.OriginalOpener, Is.Null);
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Is.Empty);
            Assert.That(info.Parent, Is.Null);
        });
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        BrowsingContextInfo copy = info with { };
        Assert.That(copy, Is.EqualTo(info));
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingContextThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingClientWindowIdThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingUrlThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingOriginalOpenerThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingChildrenThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingUserContextThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "children": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidClientWindowThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": {},
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidUrlTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": {},
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": [] 
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidOriginalOpenerTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": {},
                        "userContext": "myUserContextId",
                        "children": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidUserContextTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "clientWindow": "myClientWindowId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": {},
                        "children": [] 
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidChildrenTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidParentTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "originalOpener": "openerContext",
                        "userContext": "myUserContextId",
                        "children": [],
                        "parent": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
