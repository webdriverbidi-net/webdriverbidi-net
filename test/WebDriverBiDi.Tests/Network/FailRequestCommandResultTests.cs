namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class FailRequestCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        FailRequestCommandResult? result = JsonSerializer.Deserialize<FailRequestCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        FailRequestCommandResult? result = JsonSerializer.Deserialize<FailRequestCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        FailRequestCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}