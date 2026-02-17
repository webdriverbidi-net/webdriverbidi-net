namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetForcedColorsModeThemeOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetForcedColorsModeThemeOverrideCommandResult? result = JsonSerializer.Deserialize<SetForcedColorsModeThemeOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetForcedColorsModeThemeOverrideCommandResult? result = JsonSerializer.Deserialize<SetForcedColorsModeThemeOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetForcedColorsModeThemeOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
