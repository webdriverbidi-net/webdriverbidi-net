namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class PrintCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""data"": ""some print data"" }";
        PrintCommandResult? result = JsonSerializer.Deserialize<PrintCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Data, Is.EqualTo("some print data"));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonSerializer.Deserialize<PrintCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = @"{ ""data"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<PrintCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}