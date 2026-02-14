namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class RemoveInterceptCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        RemoveInterceptCommandResult? result = JsonSerializer.Deserialize<RemoveInterceptCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        RemoveInterceptCommandResult? result = JsonSerializer.Deserialize<RemoveInterceptCommandResult>("{}");
        Assert.That(result, Is.Not.Null);
        RemoveInterceptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}