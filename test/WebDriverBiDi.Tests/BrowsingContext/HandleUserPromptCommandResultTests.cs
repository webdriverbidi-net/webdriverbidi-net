namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class HandleUserPromptCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        HandleUserPromptCommandResult? result = JsonSerializer.Deserialize<HandleUserPromptCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        HandleUserPromptCommandResult? result = JsonSerializer.Deserialize<HandleUserPromptCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        HandleUserPromptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
