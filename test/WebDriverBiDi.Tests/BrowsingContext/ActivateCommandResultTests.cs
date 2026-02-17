namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class ActivateCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        ActivateCommandResult? result = JsonSerializer.Deserialize<ActivateCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ActivateCommandResult? result = JsonSerializer.Deserialize<ActivateCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        ActivateCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
