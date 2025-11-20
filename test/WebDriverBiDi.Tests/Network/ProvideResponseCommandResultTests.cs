namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ProvideResponseCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        ProvideResponseCommandResult? result = JsonSerializer.Deserialize<ProvideResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ProvideResponseCommandResult? result = JsonSerializer.Deserialize<ProvideResponseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        ProvideResponseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}