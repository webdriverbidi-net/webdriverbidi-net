namespace WebDriverBiDi.Input;

using System.Text.Json;

[TestFixture]
public class PerformActionsCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        PerformActionsCommandResult? result = JsonSerializer.Deserialize<PerformActionsCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        PerformActionsCommandResult? result = JsonSerializer.Deserialize<PerformActionsCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        PerformActionsCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}