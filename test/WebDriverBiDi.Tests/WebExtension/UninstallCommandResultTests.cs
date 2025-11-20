namespace WebDriverBiDi.WebExtension;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class UninstallCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        UninstallCommandResult? result = JsonSerializer.Deserialize<UninstallCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        UninstallCommandResult? result = JsonSerializer.Deserialize<UninstallCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        UninstallCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}