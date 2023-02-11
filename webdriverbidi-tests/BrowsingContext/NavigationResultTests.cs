namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class NavigationResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""url"": ""http://example.com"" }";
        NavigationResult? result = JsonConvert.DeserializeObject<NavigationResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Url, Is.EqualTo("http://example.com"));
            Assert.That(result.NavigationId, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithNavigationId()
    {
        string json = @"{ ""url"": ""http://example.com"", ""navigation"": ""myNavigationId"" }";
        NavigationResult? result = JsonConvert.DeserializeObject<NavigationResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Url, Is.EqualTo("http://example.com"));
            Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
        });
    }

    [Test]
    public void TestDeserializingWithMissingUrlThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<NavigationResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        string json = @"{ ""url"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigationResult>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithInvalidNavigationIdTypeThrows()
    {
        string json = @"{ ""url"": ""http://example.com"", ""navigation"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigationResult>(json), Throws.InstanceOf<JsonReaderException>());
    }
}