namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

public class SetFilesCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SharedReference element = new("mySharedId");
        SetFilesCommandParameters properties = new("myContextId", element);
        Assert.Equal("input.setFiles", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SharedReference element = new("mySharedId");
        SetFilesCommandParameters properties = new("myContextId", element);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("element"));
        JToken? elementToken = serialized["element"];
        Assert.NotNull(elementToken);
        Assert.Equal(JTokenType.Object, elementToken.Type);
        JObject? elementObject = elementToken.Value<JObject>();
        Assert.NotNull(elementObject);
        Assert.Single(elementObject);
        Assert.True(elementObject.ContainsKey("sharedId"));
        JToken? sharedId = elementObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal(JTokenType.String, sharedId.Type);
        Assert.Equal("mySharedId", sharedId.Value<string>());

        Assert.True(serialized.ContainsKey("files"));
        JToken? filesToken = serialized["files"];
        Assert.NotNull(filesToken);
        Assert.Equal(JTokenType.Array, filesToken.Type);
        Assert.Empty(filesToken);
    }

    [Fact]
    public void TestCanSerializeParametersWithFileList()
    {
        SharedReference element = new("mySharedId");
        SetFilesCommandParameters properties = new("myContextId", element);
        properties.Files.Add("path/to/file1.txt");
        properties.Files.Add("path/to/file2.txt");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("element"));
        JToken? elementToken = serialized["element"];
        Assert.NotNull(elementToken);
        Assert.Equal(JTokenType.Object, elementToken.Type);
        JObject? elementObject = elementToken.Value<JObject>();
        Assert.NotNull(elementObject);
        Assert.Single(elementObject);
        Assert.True(elementObject.ContainsKey("sharedId"));
        JToken? sharedId = elementObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal(JTokenType.String, sharedId.Type);
        Assert.Equal("mySharedId", sharedId.Value<string>());

        Assert.True(serialized.ContainsKey("files"));
        JToken? filesToken = serialized["files"];
        Assert.NotNull(filesToken);
        Assert.Equal(JTokenType.Array, filesToken.Type);
        JArray? filesArray = filesToken.Value<JArray>();
        Assert.NotNull(filesArray);
        Assert.Equal(2, filesArray.Count);
        Assert.Equal(JTokenType.String, filesArray[0].Type);
        Assert.Equal("path/to/file1.txt", filesArray[0].Value<string>());
        Assert.Equal(JTokenType.String, filesArray[1].Type);
        Assert.Equal("path/to/file2.txt", filesArray[1].Value<string>());
    }
}
