namespace WebDriverBiDi.Browser;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class CloseCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        CloseCommandResult? result = JsonSerializer.Deserialize<CloseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        CloseCommandResult? result = JsonSerializer.Deserialize<CloseCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        CloseCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}