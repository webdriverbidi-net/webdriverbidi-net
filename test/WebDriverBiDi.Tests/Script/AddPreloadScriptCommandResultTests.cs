namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class AddPreloadScriptCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeAddLoadScriptCommandResult()
    {
        string json = """
                      {
                        "script": "myLoadScript"
                      }
                      """;
        AddPreloadScriptCommandResult? result = JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.PreloadScriptId, Is.EqualTo("myLoadScript"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "script": "myLoadScript"
                      }
                      """;
        AddPreloadScriptCommandResult? result = JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        AddPreloadScriptCommandResult copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
    }
}
