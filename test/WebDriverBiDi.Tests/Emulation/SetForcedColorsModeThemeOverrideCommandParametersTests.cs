namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetForcedColorsModeThemeOverrideCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setForcedColorsModeThemeOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("theme"));
            Assert.That(serialized["theme"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["theme"]!.Value<JObject?>, Is.Null);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithNoneMode()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new()
        {
            Theme = ForcedColorsModeTheme.None
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("theme"));
            Assert.That(serialized["theme"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["theme"]!.Value<string>(), Is.EqualTo("none"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithLightMode()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new()
        {
            Theme = ForcedColorsModeTheme.Light
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("theme"));
            Assert.That(serialized["theme"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["theme"]!.Value<string>(), Is.EqualTo("light"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithDarkMode()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new()
        {
            Theme = ForcedColorsModeTheme.Dark
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("theme"));
            Assert.That(serialized["theme"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["theme"]!.Value<string>(), Is.EqualTo("dark"));
        }
    }

    [Test]
    public void TestCanGetResetParameters()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        Assert.That(properties, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(properties.Theme, Is.Null);
            Assert.That(properties.Contexts, Is.Null);
            Assert.That(properties.UserContexts, Is.Null);
        }
    }

    [Test]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetForcedColorsModeThemeOverrideCommandParameters firstInstance = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        SetForcedColorsModeThemeOverrideCommandParameters secondInstance = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        Assert.That(firstInstance, Is.Not.SameAs(secondInstance));
    }
}
