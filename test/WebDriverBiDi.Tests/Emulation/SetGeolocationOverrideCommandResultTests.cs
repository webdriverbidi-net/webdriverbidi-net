namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetGeolocationOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetGeolocationOverrideCommandResult? result = JsonSerializer.Deserialize<SetGeolocationOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetGeolocationOverrideCommandResult? result = JsonSerializer.Deserialize<SetGeolocationOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetGeolocationOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}