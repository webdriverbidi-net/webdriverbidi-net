namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetScreenSettingsOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetScreenSettingsOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenSettingsOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetScreenSettingsOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenSettingsOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetScreenSettingsOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}