namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class EndCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        EndCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("session.end"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        EndCommandParameters properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }
}
