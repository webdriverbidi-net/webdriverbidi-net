namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class AddPreloadScriptCommandResultTests
{
    [Test]
    public void TestCanDeserializeAddLoadScriptCommandResult()
    {
        string json = @"{ ""script"": ""myLoadScript"" }";
        AddPreloadScriptCommandResult? result = JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.PreloadScriptId, Is.EqualTo("myLoadScript"));
    }
}