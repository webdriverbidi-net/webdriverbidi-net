namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class NavigationResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""url"": ""http://example.com"" }";
        NavigationResult? result = JsonSerializer.Deserialize<NavigationResult>(json);
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
        NavigationResult? result = JsonSerializer.Deserialize<NavigationResult>(json);
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
        Assert.That(() => JsonSerializer.Deserialize<NavigationResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        string json = @"{ ""url"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<NavigationResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidNavigationIdTypeThrows()
    {
        string json = @"{ ""url"": ""http://example.com"", ""navigation"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<NavigationResult>(json), Throws.InstanceOf<JsonException>());
    }
}