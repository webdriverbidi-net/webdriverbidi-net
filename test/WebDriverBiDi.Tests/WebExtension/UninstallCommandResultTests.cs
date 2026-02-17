namespace WebDriverBiDi.WebExtension;

using System.Text.Json;

[TestFixture]
public class UninstallCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        UninstallCommandResult? result = JsonSerializer.Deserialize<UninstallCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        UninstallCommandResult? result = JsonSerializer.Deserialize<UninstallCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        UninstallCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
