namespace WebDriverBiDi.Permissions;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetPermissionCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        SetPermissionCommandResult? result = JsonSerializer.Deserialize<SetPermissionCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetPermissionCommandResult? result = JsonSerializer.Deserialize<SetPermissionCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        SetPermissionCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}