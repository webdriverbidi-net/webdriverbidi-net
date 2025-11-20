namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class DisownDataCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        DisownDataCommandResult? result = JsonSerializer.Deserialize<DisownDataCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        DisownDataCommandResult? result = JsonSerializer.Deserialize<DisownDataCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        DisownDataCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}