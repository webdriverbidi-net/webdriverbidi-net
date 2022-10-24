namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[TestFixture]
public class AddLoadScriptCommandResultTests
{
    [Test]
    public void TestCanDeserializeAddLoadScriptCommandResult()
    {
        string json = @"{ ""script"": ""myLoadScript"" }";
        AddLoadScriptCommandResult? result = JsonConvert.DeserializeObject<AddLoadScriptCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.LoadScriptId, Is.EqualTo("myLoadScript"));
    }
}