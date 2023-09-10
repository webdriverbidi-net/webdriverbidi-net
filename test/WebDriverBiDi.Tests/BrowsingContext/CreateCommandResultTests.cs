namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class CreateCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""context"": ""myContextId"" }";
        CreateCommandResult? result = JsonSerializer.Deserialize<CreateCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.BrowsingContextId, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonSerializer.Deserialize<CreateCommandResult>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = @"{ ""context"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<CreateCommandResult>(json), Throws.InstanceOf<JsonException>());
    }
}