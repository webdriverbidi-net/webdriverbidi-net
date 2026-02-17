namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class SetViewportCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetViewportCommandResult? result = JsonSerializer.Deserialize<SetViewportCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetViewportCommandResult? result = JsonSerializer.Deserialize<SetViewportCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetViewportCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
