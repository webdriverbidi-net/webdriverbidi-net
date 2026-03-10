namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class SetBypassCSPCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetBypassCSPCommandResult? result = JsonSerializer.Deserialize<SetBypassCSPCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetBypassCSPCommandResult? result = JsonSerializer.Deserialize<SetBypassCSPCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetBypassCSPCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
