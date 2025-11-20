namespace WebDriverBiDi.Session;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class UnsubscribeCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        UnsubscribeCommandResult? result = JsonSerializer.Deserialize<UnsubscribeCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        UnsubscribeCommandResult? result = JsonSerializer.Deserialize<UnsubscribeCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        UnsubscribeCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}