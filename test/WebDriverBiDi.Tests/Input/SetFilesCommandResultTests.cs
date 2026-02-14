namespace WebDriverBiDi.Input;

using System.Text.Json;

[TestFixture]
public class SetFilesCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetFilesCommandResult? result = JsonSerializer.Deserialize<SetFilesCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetFilesCommandResult? result = JsonSerializer.Deserialize<SetFilesCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetFilesCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
