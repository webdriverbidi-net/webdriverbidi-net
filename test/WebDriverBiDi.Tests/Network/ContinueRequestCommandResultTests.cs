namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ContinueRequestCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        ContinueRequestCommandResult? result = JsonSerializer.Deserialize<ContinueRequestCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ContinueRequestCommandResult? result = JsonSerializer.Deserialize<ContinueRequestCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        ContinueRequestCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}