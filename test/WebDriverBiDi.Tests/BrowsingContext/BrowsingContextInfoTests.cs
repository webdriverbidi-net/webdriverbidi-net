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
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""userContext"": ""myUserContextId"", ""children"": [] }";
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
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
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""userContext"": ""default"", ""children"": [{ ""context"": ""childContextId"", ""url"": ""http://example.com/subdirectory"", ""originalOpener"": null, ""userContext"": ""default"", ""children"": [] }] }";
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.OriginalOpener, Is.EqualTo("openerContext"));
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Has.Count.EqualTo(1));
            Assert.That(info.Parent, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithOptionalParent()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""userContext"": ""myUserContextId"", ""originalOpener"": ""openerContext"", ""children"": [], ""parent"": ""parentContextId"" }";
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
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
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""originalOpener"": null, ""userContext"": ""myUserContextId"", ""children"": [] }";
        BrowsingContextInfo? info = JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.That(info, Is.InstanceOf<BrowsingContextInfo>());
        Assert.Multiple(() =>
        {
            Assert.That(info!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(info.Url, Is.EqualTo("http://example.com"));
            Assert.That(info.UserContextId, Is.EqualTo("myUserContextId"));
            Assert.That(info.OriginalOpener, Is.Null);
            Assert.That(info.Children, Is.Not.Null);
            Assert.That(info.Children, Is.Empty);
            Assert.That(info.Parent, Is.Null);
        });
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingContextThrows()
    {
        string json = @"{ ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""userContext"": ""myUserContextId"", ""children"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingUrlThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""originalOpener"": ""openerContext"", ""userContext"": ""myUserContextId"", ""children"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingOriginalOpenerThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""userContext"": ""myUserContextId"", ""children"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingChildrenThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""userContext"": ""myUserContextId"" }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithMissingUserContextThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""children"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidContextTypeThrows()
    {
        string json = @"{ ""context"": {}, ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""userContext"": ""myUserContextId"", ""children"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidUrlTypeThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": {}, ""originalOpener"": ""openerContext"", ""userContext"": ""myUserContextId"", ""children"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidOriginalOpenertypeThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""originalOpener"": {}, ""userContext"": ""myUserContextId"", ""children"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidUserContextTypeThrows()
    {
        string json = @"{ ""context"": ""myContextId, ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""userContext"": {}, ""children"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidChildrenTypeThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""userContext"": ""myUserContextId"", ""children"": ""invalid"" }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBrowsingContextInfoWithInvalidParentTypeThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""originalOpener"": ""openerContext"", ""userContext"": ""myUserContextId"", ""children"": [], ""parent"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<BrowsingContextInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
