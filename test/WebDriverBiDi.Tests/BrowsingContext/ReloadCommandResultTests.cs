namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ReloadCommandResultTests
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
                        "url": "http://example.com"
                      }
                      """;
        ReloadCommandResult? result = JsonSerializer.Deserialize<ReloadCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Url, Is.EqualTo("http://example.com"));
            Assert.That(result.NavigationId, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithNavigationId()
    {
        string json = """
                      {
                        "url": "http://example.com",
                        "navigation": "myNavigationId"
                      }
                      """;
        ReloadCommandResult? result = JsonSerializer.Deserialize<ReloadCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Url, Is.EqualTo("http://example.com"));
            Assert.That(result.NavigationId, Is.EqualTo("myNavigationId"));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "url": "http://example.com"
                      }
                      """;
        ReloadCommandResult? result = JsonSerializer.Deserialize<ReloadCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        ReloadCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }

    [Test]
    public void TestDeserializingWithMissingUrlThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<ReloadCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        string json = """
                      {
                        "url": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ReloadCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidNavigationIdTypeThrows()
    {
        string json = """
                      {
                        "url": "http://example.co",
                        "navigation": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ReloadCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
