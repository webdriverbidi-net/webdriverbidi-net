namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetExtraHeadersCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetExtraHeadersCommandResult? result = JsonSerializer.Deserialize<SetExtraHeadersCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetExtraHeadersCommandResult? result = JsonSerializer.Deserialize<SetExtraHeadersCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetExtraHeadersCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}