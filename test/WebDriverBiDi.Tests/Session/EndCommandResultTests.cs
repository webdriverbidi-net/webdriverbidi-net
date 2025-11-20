namespace WebDriverBiDi.Session;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class EndCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        EndCommandResult? result = JsonSerializer.Deserialize<EndCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.AdditionalData, Is.Empty);
    }

    [Test]
    public void TestCopySemantics()
    {
        EndCommandResult? result = JsonSerializer.Deserialize<EndCommandResult>("{}", deserializationOptions);
        Assert.That(result, Is.Not.Null);
        EndCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}