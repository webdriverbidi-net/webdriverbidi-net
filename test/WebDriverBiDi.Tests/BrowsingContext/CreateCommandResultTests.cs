namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;

[TestFixture]
public class CreateCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""context"": ""myContextId"" }";
        CreateCommandResult? result = JsonConvert.DeserializeObject<CreateCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.BrowsingContextId, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<CreateCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = @"{ ""context"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<CreateCommandResult>(json), Throws.InstanceOf<JsonReaderException>());
    }
}
