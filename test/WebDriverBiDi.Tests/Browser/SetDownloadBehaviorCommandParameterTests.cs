namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetDownloadBehaviorCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetDownloadBehaviorCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("browser.setDownloadBehavior"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetDownloadBehaviorCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("downloadBehavior"));
            Assert.That(serialized["downloadBehavior"]!.Type, Is.EqualTo(JTokenType.Null));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithContexts()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            UserContexts = ["myUserContext"],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("downloadBehavior"));
            Assert.That(serialized["downloadBehavior"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? contextsArray = serialized["userContexts"] as JArray;
            Assert.That(contextsArray, Is.Not.Null);
            Assert.That(contextsArray, Has.Count.EqualTo(1));
            Assert.That(contextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[0].Value<string>(), Is.EqualTo("myUserContext"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAllowingDownloads()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorAllowed(),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("downloadBehavior"));
            Assert.That(serialized["downloadBehavior"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
            Assert.That(behaviorObject, Has.Count.EqualTo(1));
            Assert.That(behaviorObject, Contains.Key("type"));
            Assert.That(behaviorObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(behaviorObject!["type"]!.Value<string>, Is.EqualTo("allowed"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAllowingDownloadsWithUserContexts()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorAllowed(),
            UserContexts = ["myUserContext"],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("downloadBehavior"));
            Assert.That(serialized["downloadBehavior"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
            Assert.That(behaviorObject, Has.Count.EqualTo(1));
            Assert.That(behaviorObject, Contains.Key("type"));
            Assert.That(behaviorObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(behaviorObject!["type"]!.Value<string>, Is.EqualTo("allowed"));
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? contextsArray = serialized["userContexts"] as JArray;
            Assert.That(contextsArray, Is.Not.Null);
            Assert.That(contextsArray, Has.Count.EqualTo(1));
            Assert.That(contextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[0].Value<string>(), Is.EqualTo("myUserContext"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAllowingDownloadsSettingDownloadPath()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorAllowed()
            {
                DestinationFolder = "my/destination/folder",
            },
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("downloadBehavior"));
            Assert.That(serialized["downloadBehavior"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
            Assert.That(behaviorObject, Has.Count.EqualTo(2));
            Assert.That(behaviorObject, Contains.Key("type"));
            Assert.That(behaviorObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(behaviorObject!["type"]!.Value<string>, Is.EqualTo("allowed"));
            Assert.That(behaviorObject, Contains.Key("destinationFolder"));
            Assert.That(behaviorObject!["destinationFolder"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(behaviorObject!["destinationFolder"]!.Value<string>, Is.EqualTo("my/destination/folder"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithDenyingDownloads()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorDenied()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("downloadBehavior"));
            Assert.That(serialized["downloadBehavior"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
            Assert.That(behaviorObject, Has.Count.EqualTo(1));
            Assert.That(behaviorObject, Contains.Key("type"));
            Assert.That(behaviorObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(behaviorObject!["type"]!.Value<string>, Is.EqualTo("denied"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithDenyingDownloadsWithUserContexts()
    {
        SetDownloadBehaviorCommandParameters properties = new()
        {
            DownloadBehavior = new DownloadBehaviorDenied(),
            UserContexts = ["myUserContext"],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("downloadBehavior"));
            Assert.That(serialized["downloadBehavior"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? behaviorObject = serialized["downloadBehavior"] as JObject;
            Assert.That(behaviorObject, Has.Count.EqualTo(1));
            Assert.That(behaviorObject, Contains.Key("type"));
            Assert.That(behaviorObject!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(behaviorObject!["type"]!.Value<string>, Is.EqualTo("denied"));
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? contextsArray = serialized["userContexts"] as JArray;
            Assert.That(contextsArray, Is.Not.Null);
            Assert.That(contextsArray, Has.Count.EqualTo(1));
            Assert.That(contextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[0].Value<string>(), Is.EqualTo("myUserContext"));
        });
    }
}