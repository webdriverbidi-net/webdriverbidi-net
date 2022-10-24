namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[TestFixture]
public class AddPreloadScriptCommandResultTests
{
    [Test]
    public void TestCanDeserializeAddLoadScriptCommandResult()
    {
        string json = @"{ ""script"": ""myLoadScript"" }";
        AddPreloadScriptCommandResult? result = JsonConvert.DeserializeObject<AddPreloadScriptCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.LoadScriptId, Is.EqualTo("myLoadScript"));
    }
}