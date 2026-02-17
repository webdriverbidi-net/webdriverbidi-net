namespace WebDriverBiDi.Emulation;

using System.Text.Json;

[TestFixture]
public class SetTouchOverrideCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetTouchOverrideCommandResult? result = JsonSerializer.Deserialize<SetTouchOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetTouchOverrideCommandResult? result = JsonSerializer.Deserialize<SetTouchOverrideCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetTouchOverrideCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
