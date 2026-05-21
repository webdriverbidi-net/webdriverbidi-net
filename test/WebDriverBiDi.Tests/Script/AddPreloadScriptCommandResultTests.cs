namespace WebDriverBiDi.Script;

using System.Text.Json;

public class AddPreloadScriptCommandResultTests
{
    [Fact]
    public void TestCanDeserializeAddLoadScriptCommandResult()
    {
        string json = """
                      {
                        "script": "myLoadScript"
                      }
                      """;
        AddPreloadScriptCommandResult? result = JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal("myLoadScript", result.PreloadScriptId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "script": "myLoadScript"
                      }
                      """;
        AddPreloadScriptCommandResult? result = JsonSerializer.Deserialize<AddPreloadScriptCommandResult>(json);
        Assert.NotNull(result);
        AddPreloadScriptCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }
}
