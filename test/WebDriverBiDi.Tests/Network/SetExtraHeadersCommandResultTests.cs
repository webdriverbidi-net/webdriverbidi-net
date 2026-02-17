namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class SetExtraHeadersCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        SetExtraHeadersCommandResult? result = JsonSerializer.Deserialize<SetExtraHeadersCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        SetExtraHeadersCommandResult? result = JsonSerializer.Deserialize<SetExtraHeadersCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        SetExtraHeadersCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
