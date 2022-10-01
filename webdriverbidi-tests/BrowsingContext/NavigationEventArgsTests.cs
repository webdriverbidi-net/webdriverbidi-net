namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class NavigationEventArgsTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"" }";
        NavigationEventArgs? eventArgs = JsonConvert.DeserializeObject<NavigationEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
        Assert.That(eventArgs.NavigationId, Is.Null);
    }

    [Test]
    public void TestCanDeserializeWithNavigationId()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""navigation"": ""myNavigationId"" }";
        NavigationEventArgs? eventArgs = JsonConvert.DeserializeObject<NavigationEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
        Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
        Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
    }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        string json = @"{ ""url"": ""http://example.com"" }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigationEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        string json = @"{ ""context"": {}, ""url"": ""http://example.com"" }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigationEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingUrlValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"" }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigationEventArgs>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidUrlValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigationEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithInvalidNavigationValueThrows()
    {
        string json = @"{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""navigation"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigationEventArgs>(json), Throws.InstanceOf<JsonReaderException>());
    }
}
