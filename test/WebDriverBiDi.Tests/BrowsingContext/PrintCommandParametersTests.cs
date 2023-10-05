namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class PrintCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        PrintCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.print"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        PrintCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithMargins()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Margins = new()
            {
                Right = 2.54,
                Left = 2.54,
                Top = 2.54,
                Bottom = 2.54
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("margin"));
            Assert.That(serialized["margin"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? margins = serialized["margin"] as JObject;
        Assert.That(margins, Is.Not.Null);
        Assert.That(margins, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(margins!, Contains.Key("right"));
            Assert.That(margins!["right"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(margins["right"]!.Value<double>(), Is.EqualTo(2.54));
            Assert.That(margins, Contains.Key("left"));
            Assert.That(margins["left"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(margins["left"]!.Value<double>(), Is.EqualTo(2.54));
            Assert.That(margins, Contains.Key("top"));
            Assert.That(margins["top"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(margins["top"]!.Value<double>(), Is.EqualTo(2.54));
            Assert.That(margins, Contains.Key("bottom"));
            Assert.That(margins["bottom"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(margins["bottom"]!.Value<double>(), Is.EqualTo(2.54));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNullMarginValues()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Margins = new()
            {
                Right = null,
                Left = null,
                Top = null,
                Bottom = null
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("margin"));
            Assert.That(serialized["margin"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? margins = serialized["margin"] as JObject;
        Assert.That(margins, Is.Not.Null);
        Assert.That(margins, Has.Count.EqualTo(0));
    }

    [Test]
    public void TestSettingMarginsToInvalidValuesThrows()
    {
        PrintMarginParameters properties = new();
        Assert.Multiple(() =>
        {
            Assert.That(() => properties.Top = -1, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be greater than or equal to zero"));
            Assert.That(() => properties.Bottom = -1, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be greater than or equal to zero"));
            Assert.That(() => properties.Left = -1, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be greater than or equal to zero"));
            Assert.That(() => properties.Right = -1, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be greater than or equal to zero"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNullPageSizeValues()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Page = new()
            {
                Width = null,
                Height = null
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("page"));
            Assert.That(serialized["page"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? margins = serialized["page"] as JObject;
        Assert.That(margins, Is.Not.Null);
        Assert.That(margins, Has.Count.EqualTo(0));
    }

    [Test]
    public void TestSettingPageSizeToInvalidValuesThrows()
    {
        PrintPageParameters properties = new();
        Assert.Multiple(() =>
        {
            Assert.That(() => properties.Width = -1, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be greater than or equal to zero"));
            Assert.That(() => properties.Height = -1, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be greater than or equal to zero"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNullMargins()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Margins = null
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithPageSize()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Page = new()
            {
                Width = 24,
                Height = 29.7
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("page"));
            Assert.That(serialized["page"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? pageSize = serialized["page"] as JObject;
        Assert.That(pageSize, Is.Not.Null);
        Assert.That(pageSize, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(pageSize!, Contains.Key("width"));
            Assert.That(pageSize!["width"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(pageSize["width"]!.Value<double>(), Is.EqualTo(24));
            Assert.That(pageSize, Contains.Key("height"));
            Assert.That(pageSize["height"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(pageSize["height"]!.Value<double>(), Is.EqualTo(29.7));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNullPageSize()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Page = null
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOrientation()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Orientation = PrintOrientation.Landscape
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("orientation"));
            Assert.That(serialized["orientation"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["orientation"]!.Value<string>(), Is.EqualTo("landscape"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNullOrientation()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Orientation = null
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithBackground()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Background = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("background"));
            Assert.That(serialized["background"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["background"]!.Value<bool>(), Is.True);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNullBackground()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Background = null
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithScale()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Scale = 1.5
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("scale"));
            Assert.That(serialized["scale"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["scale"]!.Value<double>(), Is.EqualTo(1.5));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNullScale()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            Scale = null
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestSettingScaleToInvalidValueThrows()
    {
        PrintCommandParameters properties = new("myContextId");
        Assert.Multiple(() =>
        {
            Assert.That(() => properties.Scale = -1, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be between 0.1 and 2.0"));
            Assert.That(() => properties.Scale = 0.01, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be between 0.1 and 2.0"));
            Assert.That(() => properties.Scale = 2.01, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be between 0.1 and 2.0"));
            Assert.That(() => properties.Scale = 0, Throws.InstanceOf<ArgumentOutOfRangeException>().With.Message.Contains("Value must be between 0.1 and 2.0"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithShrinkToFit()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            ShrinkToFit = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("shrinkToFit"));
            Assert.That(serialized["shrinkToFit"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["shrinkToFit"]!.Value<bool>(), Is.False);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithNullShrinkToFit()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            ShrinkToFit = null
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithPageRanges()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            PageRanges = new()
            {
                1,
                "3-5"
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("pageRanges"));
            Assert.That(serialized["pageRanges"]!.Type, Is.EqualTo(JTokenType.Array));
        });

        JArray? pageRanges = serialized["pageRanges"] as JArray;
        Assert.That(pageRanges, Is.Not.Null);
        Assert.That(pageRanges, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(pageRanges![0].Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(pageRanges[0].Value<long>, Is.EqualTo(1));
            Assert.That(pageRanges[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(pageRanges[1].Value<string>, Is.EqualTo("3-5"));
         });
    }

    [Test]
    public void TestCanSerializeParametersWithNoPageRanges()
    {
        PrintCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        });
    }

    [Test]
    public void TestSerializeParametersWithInvalidPageRangesThrows()
    {
        PrintCommandParameters properties = new("myContextId")
        {
            PageRanges = new()
            {
                1,
                "3-5",
                true
            }
        };
        Assert.That(() => JsonSerializer.Serialize(properties), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Page range must be a string or an integer value"));
    }
}
