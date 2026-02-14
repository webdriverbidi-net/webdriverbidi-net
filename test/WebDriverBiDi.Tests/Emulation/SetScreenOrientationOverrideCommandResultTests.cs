namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetScreenOrientationOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetScreenOrientationOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenOrientationOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetScreenOrientationOverrideCommandResult? result = JsonSerializer.Deserialize<SetScreenOrientationOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetScreenOrientationOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}