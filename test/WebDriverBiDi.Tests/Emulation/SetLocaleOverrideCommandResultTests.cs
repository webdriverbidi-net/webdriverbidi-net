namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetLocaleOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetLocaleOverrideCommandResult? result = JsonSerializer.Deserialize<SetLocaleOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetLocaleOverrideCommandResult? result = JsonSerializer.Deserialize<SetLocaleOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetLocaleOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}