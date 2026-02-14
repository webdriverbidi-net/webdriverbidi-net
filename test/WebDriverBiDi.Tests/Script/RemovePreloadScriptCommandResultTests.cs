namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class RemovePreloadScriptCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        RemovePreloadScriptCommandResult? result = JsonSerializer.Deserialize<RemovePreloadScriptCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        RemovePreloadScriptCommandResult? result = JsonSerializer.Deserialize<RemovePreloadScriptCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        RemovePreloadScriptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}