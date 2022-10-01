namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class NavigateResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""url"": ""http://example.com"" }";
        NavigateResult? result = JsonConvert.DeserializeObject<NavigateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url, Is.EqualTo("http://example.com"));
        Assert.That(result.NavigationId, Is.Null);
    }

    public void TestCanDeserializeWithNavigationId()
    {
        string json = @"{ ""url"": ""http://example.com"", ""navigation"": ""myNavigationId"" }";
        NavigateResult? result = JsonConvert.DeserializeObject<NavigateResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url, Is.EqualTo("http://example.com"));
        Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
    }

    [Test]
    public void TestDeserializingWithMissingUrlThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<NavigateResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        string json = @"{ ""url"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigateResult>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithInvalidNavigationIdTypeThrows()
    {
        string json = @"{ ""url"": ""http://example.com"", ""navigation"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<NavigateResult>(json), Throws.InstanceOf<JsonReaderException>());
    }
}