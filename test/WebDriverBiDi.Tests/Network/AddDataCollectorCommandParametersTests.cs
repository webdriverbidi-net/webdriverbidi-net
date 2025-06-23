namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class AddDataCollectorCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        AddDataCollectorCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("network.addDataCollector"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        AddDataCollectorCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("dataTypes"));
            Assert.That(serialized["dataTypes"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? dataTypesArray = serialized["dataTypes"] as JArray;
            Assert.That(dataTypesArray, Is.Not.Null);
            Assert.That(dataTypesArray, Has.Count.EqualTo(1));
            Assert.That(dataTypesArray![0].Value<string>(), Is.EqualTo("response"));
            Assert.That(serialized, Contains.Key("maxEncodedDataSize"));
            Assert.That(serialized["maxEncodedDataSize"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxEncodedDataSize"]!.Value<ulong>(), Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithBrowsingContexts()
    {
        AddDataCollectorCommandParameters properties = new()
        {
            DataTypes = new List<DataType> { DataType.Response },
            MaxEncodedDataSize = 100,
        };
        properties.BrowsingContexts.Add("myContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("dataTypes"));
            Assert.That(serialized["dataTypes"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? dataTypesArray = serialized["dataTypes"] as JArray;
            Assert.That(dataTypesArray, Is.Not.Null);
            Assert.That(dataTypesArray, Has.Count.EqualTo(1));
            Assert.That(dataTypesArray![0].Value<string>(), Is.EqualTo("response"));
            Assert.That(serialized, Contains.Key("maxEncodedDataSize"));
            Assert.That(serialized["maxEncodedDataSize"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxEncodedDataSize"]!.Value<ulong>(), Is.EqualTo(100));
            Assert.That(serialized, Contains.Key("contexts"));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? contextsArray = serialized["contexts"] as JArray;
            Assert.That(contextsArray, Is.Not.Null);
            Assert.That(contextsArray, Has.Count.EqualTo(1));
            Assert.That(contextsArray![0].Value<string>(), Is.EqualTo("myContext"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithUserContexts()
    {
        AddDataCollectorCommandParameters properties = new()
        {
            DataTypes = new List<DataType> { DataType.Response },
            MaxEncodedDataSize = 100,
        };
        properties.UserContexts.Add("myUserContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("dataTypes"));
            Assert.That(serialized["dataTypes"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? dataTypesArray = serialized["dataTypes"] as JArray;
            Assert.That(dataTypesArray, Is.Not.Null);
            Assert.That(dataTypesArray, Has.Count.EqualTo(1));
            Assert.That(dataTypesArray![0].Value<string>(), Is.EqualTo("response"));
            Assert.That(serialized, Contains.Key("maxEncodedDataSize"));
            Assert.That(serialized["maxEncodedDataSize"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxEncodedDataSize"]!.Value<ulong>(), Is.EqualTo(100));
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? userContextsArray = serialized["userContexts"] as JArray;
            Assert.That(userContextsArray, Is.Not.Null);
            Assert.That(userContextsArray, Has.Count.EqualTo(1));
            Assert.That(userContextsArray![0].Value<string>(), Is.EqualTo("myUserContext"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithCollectorType()
    {
        AddDataCollectorCommandParameters properties = new()
        {
            DataTypes = new List<DataType> { DataType.Response },
            MaxEncodedDataSize = 100,
            CollectorType = CollectorType.Blob,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("dataTypes"));
            Assert.That(serialized["dataTypes"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? dataTypesArray = serialized["dataTypes"] as JArray;
            Assert.That(dataTypesArray, Is.Not.Null);
            Assert.That(dataTypesArray, Has.Count.EqualTo(1));
            Assert.That(dataTypesArray![0].Value<string>(), Is.EqualTo("response"));
            Assert.That(serialized, Contains.Key("maxEncodedDataSize"));
            Assert.That(serialized["maxEncodedDataSize"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxEncodedDataSize"]!.Value<ulong>(), Is.EqualTo(100));
            Assert.That(serialized, Contains.Key("collectorType"));
            Assert.That(serialized["collectorType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized!["collectorType"]!.Value<string>(), Is.EqualTo("blob"));
        });
    }
}
