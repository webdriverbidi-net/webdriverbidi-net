namespace WebDriverBiDi.Permissions;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetPermissionsCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Granted, "https://example.com");
        Assert.That(properties.MethodName, Is.EqualTo("permissions.setPermission"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Granted, "https://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("descriptor"));
            Assert.That(serialized["descriptor"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject descriptor = (JObject)serialized["descriptor"]!;
            Assert.That(descriptor, Has.Count.EqualTo(1));
            Assert.That(descriptor, Contains.Key("name"));
            Assert.That(descriptor["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(descriptor["name"]!.Value<string>(), Is.EqualTo("myPermission"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("granted"));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("https://example.com"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithDescriptorConstructor()
    {
        SetPermissionCommandParameters properties = new(new PermissionDescriptor("myPermission"), PermissionState.Granted, "https://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("descriptor"));
            Assert.That(serialized["descriptor"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject descriptor = (JObject)serialized["descriptor"]!;
            Assert.That(descriptor, Has.Count.EqualTo(1));
            Assert.That(descriptor, Contains.Key("name"));
            Assert.That(descriptor["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(descriptor["name"]!.Value<string>(), Is.EqualTo("myPermission"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("granted"));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("https://example.com"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithEmbeddedOrigin()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Granted, "https://example.com")
        {
            EmbeddedOrigin = "myEmbeddedOrigin"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("descriptor"));
            Assert.That(serialized["descriptor"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject descriptor = (JObject)serialized["descriptor"]!;
            Assert.That(descriptor, Has.Count.EqualTo(1));
            Assert.That(descriptor, Contains.Key("name"));
            Assert.That(descriptor["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(descriptor["name"]!.Value<string>(), Is.EqualTo("myPermission"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("granted"));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("https://example.com"));
            Assert.That(serialized, Contains.Key("embeddedOrigin"));
            Assert.That(serialized["embeddedOrigin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["embeddedOrigin"]!.Value<string>(), Is.EqualTo("myEmbeddedOrigin"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithUserContext()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Granted, "https://example.com")
        {
            UserContextId = "myContext"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("descriptor"));
            Assert.That(serialized["descriptor"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject descriptor = (JObject)serialized["descriptor"]!;
            Assert.That(descriptor, Has.Count.EqualTo(1));
            Assert.That(descriptor, Contains.Key("name"));
            Assert.That(descriptor["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(descriptor["name"]!.Value<string>(), Is.EqualTo("myPermission"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("granted"));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("https://example.com"));
            Assert.That(serialized, Contains.Key("userContext"));
            Assert.That(serialized["userContext"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["userContext"]!.Value<string>(), Is.EqualTo("myContext"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithPermissionDenied()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Denied, "https://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("descriptor"));
            Assert.That(serialized["descriptor"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject descriptor = (JObject)serialized["descriptor"]!;
            Assert.That(descriptor, Has.Count.EqualTo(1));
            Assert.That(descriptor, Contains.Key("name"));
            Assert.That(descriptor["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(descriptor["name"]!.Value<string>(), Is.EqualTo("myPermission"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("denied"));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("https://example.com"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithPermissionPrompt()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Prompt, "https://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("descriptor"));
            Assert.That(serialized["descriptor"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject descriptor = (JObject)serialized["descriptor"]!;
            Assert.That(descriptor, Has.Count.EqualTo(1));
            Assert.That(descriptor, Contains.Key("name"));
            Assert.That(descriptor["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(descriptor["name"]!.Value<string>(), Is.EqualTo("myPermission"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("prompt"));
            Assert.That(serialized, Contains.Key("origin"));
            Assert.That(serialized["origin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["origin"]!.Value<string>(), Is.EqualTo("https://example.com"));
        }
    }
}
