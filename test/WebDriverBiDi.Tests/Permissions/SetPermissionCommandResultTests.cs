namespace WebDriverBiDi.Permissions;

using System.Text.Json;

[TestFixture]
public class SetPermissionCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetPermissionCommandResult? result = JsonSerializer.Deserialize<SetPermissionCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetPermissionCommandResult? result = JsonSerializer.Deserialize<SetPermissionCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetPermissionCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
