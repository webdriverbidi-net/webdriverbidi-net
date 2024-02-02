namespace WebDriverBiDi.Browser;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class CreateUserContextCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""userContext"": ""myUserContext"" }";
        CreateUserContextCommandResult? result = JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.UserContextId, Is.EqualTo("myUserContext"));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = @"{ ""userContext"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<CreateUserContextCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
