namespace WebDriverBiDi.Input;

using System.Text.Json;

[TestFixture]
public class ReleaseActionsCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        ReleaseActionsCommandResult? result = JsonSerializer.Deserialize<ReleaseActionsCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        ReleaseActionsCommandResult? result = JsonSerializer.Deserialize<ReleaseActionsCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        ReleaseActionsCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
