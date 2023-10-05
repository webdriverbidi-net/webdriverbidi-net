namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class GetTreeCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""contexts"": [{ ""context"": ""myContextId"", ""url"": ""http://example.com"", ""children"": [] }] }";
        GetTreeCommandResult? result = JsonConvert.DeserializeObject<GetTreeCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ContextTree, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanDeserializeWithNoContexts()
    {
        string json = @"{ ""contexts"": [] }";
        GetTreeCommandResult? result = JsonConvert.DeserializeObject<GetTreeCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ContextTree, Is.Empty);
    }

    [Test]
    public void TestDeserializingWithMissingContextsThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<GetTreeCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextsTypeThrows()
    {
        string json = @"{ ""contexts"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<GetTreeCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextValueTypeThrows()
    {
        string json = @"{ ""contexts"": [""invalid""] }";
        Assert.That(() => JsonConvert.DeserializeObject<GetTreeCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}
