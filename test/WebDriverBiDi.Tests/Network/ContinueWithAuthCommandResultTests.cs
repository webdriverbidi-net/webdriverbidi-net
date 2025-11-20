namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class ContinueWithAuthCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        ContinueWithAuthCommandResult? result = JsonSerializer.Deserialize<ContinueWithAuthCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ContinueWithAuthCommandResult? result = JsonSerializer.Deserialize<ContinueWithAuthCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        ContinueWithAuthCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}