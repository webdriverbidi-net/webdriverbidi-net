namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetScrollbarTypeOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetScrollbarTypeOverrideCommandResult? result = JsonSerializer.Deserialize<SetScrollbarTypeOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetScrollbarTypeOverrideCommandResult? result = JsonSerializer.Deserialize<SetScrollbarTypeOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetScrollbarTypeOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
