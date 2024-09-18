namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class LocatorTests
{
    [Test]
    public void TestCanSerializeCssLocator()
    {
        CssLocator value = new(".selector");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("css"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo(".selector"));
        });
    }

    [Test]
    public void TestCanSerializeXPathLocator()
    {
        XPathLocator value = new("//selector");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("xpath"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("//selector"));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocator()
    {
        InnerTextLocator value = new("text to locate");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithMaxDepthZero()
    {
        InnerTextLocator value = new("text to locate")
        {
            MaxDepth = 0
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("maxDepth"));
            Assert.That(parsed["maxDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(parsed["maxDepth"]!.Value<ulong>(), Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithMaxDepthNonZero()
    {
        InnerTextLocator value = new("text to locate")
        {
            MaxDepth = 10
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("maxDepth"));
            Assert.That(parsed["maxDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(parsed["maxDepth"]!.Value<ulong>(), Is.EqualTo(10));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithIgnoreCaseTrue()
    {
        InnerTextLocator value = new("text to locate")
        {
            IgnoreCase = true
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("ignoreCase"));
            Assert.That(parsed["ignoreCase"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(parsed["ignoreCase"]!.Value<bool>(), Is.True);
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithIgnoreCaseFalse()
    {
        InnerTextLocator value = new("text to locate")
        {
            IgnoreCase = false
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("ignoreCase"));
            Assert.That(parsed["ignoreCase"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(parsed["ignoreCase"]!.Value<bool>(), Is.False);
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithMatchTypeFull()
    {
        InnerTextLocator value = new("text to locate")
        {
            MatchType = InnerTextMatchType.Full
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("matchType"));
            Assert.That(parsed["matchType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["matchType"]!.Value<string>(), Is.EqualTo("full"));
        });
    }

    [Test]
    public void TestCanSerializeInnerTextLocatorWithMatchTypePartial()
    {
        InnerTextLocator value = new("text to locate")
        {
            MatchType = InnerTextMatchType.Partial
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(3));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("innerText"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("text to locate"));
            Assert.That(parsed, Contains.Key("matchType"));
            Assert.That(parsed["matchType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["matchType"]!.Value<string>(), Is.EqualTo("partial"));
        });
    }

    [Test]
    public void TestCanSerializeAccessibilityLocator()
    {
        AccessibilityLocator value = new();
        Assert.Multiple(() =>
        {
            Assert.That(value.Name, Is.Null);
            Assert.That(value.Role, Is.Null);
        });

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("accessibility"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? accessibilityValue = parsed["value"] as JObject;
            Assert.That(accessibilityValue!, Is.Empty);
        });
    }

    [Test]
    public void TestCanSerializeAccessibilityWithNameAndRole()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName",
            Role = "accessibleRole"
        };
        Assert.Multiple(() =>
        {
            Assert.That(value.Name, Is.EqualTo("accessibleName"));
            Assert.That(value.Role, Is.EqualTo("accessibleRole"));
        });

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("accessibility"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? accessibilityValue = parsed["value"] as JObject;
            Assert.That(accessibilityValue, Is.Not.Null);
            Assert.That(accessibilityValue!, Has.Count.EqualTo(2));
            Assert.That(accessibilityValue, Contains.Key("name"));
            Assert.That(accessibilityValue!["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(accessibilityValue!["name"]!.Value<string>(), Is.EqualTo("accessibleName"));
            Assert.That(accessibilityValue, Contains.Key("role"));
            Assert.That(accessibilityValue!["role"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(accessibilityValue!["role"]!.Value<string>(), Is.EqualTo("accessibleRole"));
        });
    }

    [Test]
    public void TestCanSerializeAccessibilityWithName()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName"
        };
        Assert.Multiple(() =>
        {
            Assert.That(value.Name, Is.EqualTo("accessibleName"));
            Assert.That(value.Role, Is.Null);
        });

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("accessibility"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? accessibilityValue = parsed["value"] as JObject;
            Assert.That(accessibilityValue, Is.Not.Null);
            Assert.That(accessibilityValue!, Has.Count.EqualTo(1));
            Assert.That(accessibilityValue, Contains.Key("name"));
            Assert.That(accessibilityValue!["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(accessibilityValue!["name"]!.Value<string>(), Is.EqualTo("accessibleName"));
        });
    }

    [Test]
    public void TestCanSerializeAccessibilityWithRole()
    {
        AccessibilityLocator value = new()
        {
            Role = "accessibleRole"
        };
        Assert.Multiple(() =>
        {
            Assert.That(value.Name, Is.Null);
            Assert.That(value.Role, Is.EqualTo("accessibleRole"));
        });

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("accessibility"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? accessibilityValue = parsed["value"] as JObject;
            Assert.That(accessibilityValue, Is.Not.Null);
            Assert.That(accessibilityValue!, Has.Count.EqualTo(1));
            Assert.That(accessibilityValue, Contains.Key("role"));
            Assert.That(accessibilityValue!["role"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(accessibilityValue!["role"]!.Value<string>(), Is.EqualTo("accessibleRole"));
        });
    }

    [Test]
    public void TestCanSerializeAccessibilityWithRemovingName()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName",
            Role = "accessibleRole"
        };
        value.Name = null;
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("accessibility"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? accessibilityValue = parsed["value"] as JObject;
            Assert.That(accessibilityValue, Is.Not.Null);
            Assert.That(accessibilityValue!, Has.Count.EqualTo(1));
            Assert.That(accessibilityValue, Contains.Key("role"));
            Assert.That(accessibilityValue!["role"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(accessibilityValue!["role"]!.Value<string>(), Is.EqualTo("accessibleRole"));
        });
    }

    [Test]
    public void TestCanSerializeAccessibilityWithRemovingRole()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName",
            Role = "accessibleRole"
        };
        value.Role = null;
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("accessibility"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? accessibilityValue = parsed["value"] as JObject;
            Assert.That(accessibilityValue, Is.Not.Null);
            Assert.That(accessibilityValue!, Has.Count.EqualTo(1));
            Assert.That(accessibilityValue, Contains.Key("name"));
            Assert.That(accessibilityValue!["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(accessibilityValue!["name"]!.Value<string>(), Is.EqualTo("accessibleName"));
        });
    }

    [Test]
    public void TestCanSerializeAccessibilityWithRemovingNameAndRole()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName",
            Role = "accessibleRole"
        };
        value.Name = null;
        value.Role = null;
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("accessibility"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? accessibilityValue = parsed["value"] as JObject;
            Assert.That(accessibilityValue, Is.Empty);
        });
    }
}
