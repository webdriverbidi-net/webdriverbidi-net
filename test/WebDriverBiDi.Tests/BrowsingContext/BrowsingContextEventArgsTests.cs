namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class BrowsingContextEventArgsTests
{
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
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        BrowsingContextEventArgs eventArgs = new(info);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs.ClientWindowId, Is.EqualTo("myClientWindowId"));
            Assert.That(eventArgs.UserContextId, Is.EqualTo("myUserContextId"));
            Assert.That(eventArgs.OriginalOpener, Is.EqualTo("openerContext"));
            Assert.That(eventArgs.Children, Is.Not.Null);
            Assert.That(eventArgs.Children, Is.Empty);
            Assert.That(eventArgs.Parent, Is.Null);
        }
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
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        BrowsingContextEventArgs eventArgs = new(info);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs.ClientWindowId, Is.EqualTo("myClientWindowId"));
            Assert.That(eventArgs.OriginalOpener, Is.EqualTo("openerContext"));
            Assert.That(eventArgs.Children, Is.Not.Null);
            Assert.That(eventArgs.Children, Has.Count.EqualTo(1));
            Assert.That(eventArgs.Parent, Is.Null);
        }
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
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        BrowsingContextEventArgs eventArgs = new(info);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs.ClientWindowId, Is.EqualTo("myClientWindowId"));
            Assert.That(eventArgs.OriginalOpener, Is.EqualTo("openerContext"));
            Assert.That(eventArgs.Children, Is.Not.Null);
            Assert.That(eventArgs.Children, Has.Count.EqualTo(0));
            Assert.That(eventArgs.Parent, Is.Not.Null);
            Assert.That(eventArgs.Parent, Is.EqualTo("parentContextId"));
        }
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
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        BrowsingContextEventArgs eventArgs = new(info);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs.ClientWindowId, Is.EqualTo("myClientWindowId"));
            Assert.That(eventArgs.UserContextId, Is.EqualTo("myUserContextId"));
            Assert.That(eventArgs.OriginalOpener, Is.Null);
            Assert.That(eventArgs.Children, Is.Not.Null);
            Assert.That(eventArgs.Children, Is.Empty);
            Assert.That(eventArgs.Parent, Is.Null);
        }
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
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json);
        Assert.That(info, Is.Not.Null);
        BrowsingContextEventArgs eventArgs = new(info);
        BrowsingContextEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }
}
