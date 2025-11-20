namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class DisownCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        DisownCommandResult? result = JsonSerializer.Deserialize<DisownCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        DisownCommandResult? result = JsonSerializer.Deserialize<DisownCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        DisownCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}