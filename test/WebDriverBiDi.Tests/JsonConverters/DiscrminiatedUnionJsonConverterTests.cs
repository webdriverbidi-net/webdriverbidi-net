namespace WebDriverBiDi.JsonConverters;

using System.Reflection;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;

[TestFixture]
public class DiscriminatedUnionJsonConverterTests
{
    [Test]
    public void TestCanDeserializeDerivedType()
    {
        string json = """
                      {
                        "type": "typeA",
                        "propertyA": "Value for A"
                      }
                      """;
        ParentTypeWithChildTypes result = JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json)!;
        Assert.That(result, Is.TypeOf<DerivedTypeA>());
        DerivedTypeA derivedA = (DerivedTypeA)result;
        Assert.That(derivedA.Type, Is.EqualTo("typeA"));
        Assert.That(derivedA.PropertyA, Is.EqualTo("Value for A"));
    }

    [Test]
    public void TestDeserializingDerivedTypeWithInvalidDiscriminatorValueThrows()
    {
        string json = """
                      {
                        "type": "typeC",
                        "propertyC": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("JSON for 'ParentTypeWithChildTypes' type property contains unknown value 'typeC'"));
    }

    [Test]
    public void TestDeserializingDerivedTypeWithMissingDiscriminatorPropertyThrows()
    {
        string json = """
                      {
                        "propertyC": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("JSON for 'ParentTypeWithChildTypes' must contain a 'type' property"));
    }

    [Test]
    public void TestDeserializingBaseTypeWithInvalidJsonSchemaThrows()
    {
        string json = """
                      [
                        "type",
                        "propertyC"
                      ]
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("JSON for 'ParentTypeWithChildTypes' must be an object, but starting token was StartArray"));
    }

    [Test]
    public void TestDeserializingDerivedTypeWithInvalidDiscriminatorValueTypeThrows()
    {
        string json = """
                      {
                        "type": 123,
                        "propertyC": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("JSON 'type' property must be a string"));
    }

    [Test]
    public void TestDeserializingWithEmptyDiscriminatorValueThrows()
    {
        string json = """
                      {
                        "type": "asdf",
                        "propertyC": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<EmptyDiscriminator>(json), Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("must have a non-empty Discriminator"));
    }

    [Test]
    public void TestDeserializingWithMissingDiscriminatorValueReturnsNullForAttributeValue()
    {
        string json = """
                      {
                        "propertyC": true
                      }
                      """;
        NullForEmptyDiscriminator? result = JsonSerializer.Deserialize<NullForEmptyDiscriminator>(json);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TestCanDeserializeFallbackDerivedType()
    {
        string json = """
                      {
                        "type": "someUnknownType",
                        "propertyA": "Value for A"
                      }
                      """;
        ParentTypeWithFallback result = JsonSerializer.Deserialize<ParentTypeWithFallback>(json)!;
        Assert.That(result, Is.TypeOf<FallbackDerivedType>());
        FallbackDerivedType fallbackResult = (FallbackDerivedType)result;
        Assert.That(fallbackResult.Type, Is.EqualTo("someUnknownType"));
        Assert.That(fallbackResult.PropertyA, Is.EqualTo("Value for A"));
    }

    [Test]
    public void TestCanDeserializeBasedOnPropertyPresence()
    {
        string jsonA = """
                      {
                        "propertyA": "Value for A"
                      }
                      """;
        PropertyPresence resultA = JsonSerializer.Deserialize<PropertyPresence>(jsonA)!;
        Assert.That(resultA, Is.TypeOf<PropertyPresenceChildA>());
        PropertyPresenceChildA childAResult = (PropertyPresenceChildA)resultA;
        Assert.That(childAResult.PropertyA, Is.EqualTo("Value for A"));

        string jsonB = """
                      {
                        "propertyB": 123
                      }
                      """;
        PropertyPresence resultB = JsonSerializer.Deserialize<PropertyPresence>(jsonB)!;
        Assert.That(resultB, Is.TypeOf<PropertyPresenceChildB>());
        PropertyPresenceChildB childBResult = (PropertyPresenceChildB)resultB;
        Assert.That(childBResult.PropertyB, Is.EqualTo(123));
    }

    [Test]
    public void TestDeserializingBasedOnPropertyPresenceWithInvalidValueThrows()
    {
        string jsonA = """
                      {
                        "propertyC": "Value for C"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<PropertyPresence>(jsonA), Throws.InstanceOf<JsonException>().With.Message.Contains("one of the following properties"));
    }

    [Test]
    public void TestCanSerializeDerivedType()
    {
        ParentTypeWithChildTypes derivedB = new DerivedTypeB
        {
            Type = "typeB",
            PropertyB = 42
        };
        string json = JsonSerializer.Serialize(derivedB);
        Assert.That(json, Is.Not.Null.Or.Empty);
    }

    [Test]
    public void TestDeserializingTypeWithMissingDiscriminatedTypePropertyAttributeThrows()
    {
        string json = """
                      {
                        "type": "child"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<MissingDiscriminatorAttribute>(json), Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("must have a [DiscriminatedTypeProperty] or [DiscriminatedTypePresence] attribute"));
    }

    [JsonConverter(typeof(DiscriminatedUnionJsonConverter<ParentTypeWithChildTypes>))]
    [DiscriminatedTypeProperty("type")]
    [DiscriminatedDerivedType(typeof(DerivedTypeA), "typeA")]
    [DiscriminatedDerivedType(typeof(DerivedTypeB), "typeB")]
    private abstract record ParentTypeWithChildTypes
    {
        [JsonConstructor]
        public ParentTypeWithChildTypes()
        {
        }

        [JsonPropertyName("type")]
        [JsonInclude]
        public string Type { get; init; } = string.Empty;
    }

    private record DerivedTypeA : ParentTypeWithChildTypes
    {
        [JsonConstructor]
        public DerivedTypeA()
            : base()
        {
        }

        [JsonPropertyName("propertyA")]
        [JsonInclude]
        public string PropertyA { get; init; } = string.Empty;
    }

    private record DerivedTypeB : ParentTypeWithChildTypes
    {
        [JsonConstructor]
        public DerivedTypeB()
            : base()
        {
        }

        [JsonPropertyName("propertyB")]
        [JsonInclude]
        public int PropertyB { get; init; }
    }

    private record DerivedTypeC : ParentTypeWithChildTypes
    {
        [JsonConstructor]
        public DerivedTypeC()
            : base()
        {
        }

        [JsonPropertyName("propertyC")]
        [JsonInclude]
        public bool PropertyC { get; init; }
    }

    [JsonConverter(typeof(DiscriminatedUnionJsonConverter<ParentTypeWithFallback>))]
    [DiscriminatedTypeProperty("type", UnmatchedValueType = typeof(FallbackDerivedType))]
    [DiscriminatedDerivedType(typeof(ChildNotFallbackType), "childNotFallback")]
    private record ParentTypeWithFallback
    {
        [JsonConstructor]
        public ParentTypeWithFallback()
        {
        }

        [JsonPropertyName("type")]
        [JsonInclude]
        public string Type { get; init; } = string.Empty;
    }

    private record FallbackDerivedType : ParentTypeWithFallback
    {
        [JsonConstructor]
        public FallbackDerivedType()
            : base()
        {
        }

        [JsonPropertyName("propertyA")]
        [JsonInclude]
        public string PropertyA { get; init; } = "propertyA";
    }

    private record ChildNotFallbackType : ParentTypeWithFallback
    {
        [JsonConstructor]
        public ChildNotFallbackType()
            : base()
        {
        }

        [JsonPropertyName("propertyB")]
        [JsonInclude]
        public int PropertyB { get; init; }
    }

    [JsonConverter(typeof(DiscriminatedUnionJsonConverter<EmptyDiscriminator>))]
    [DiscriminatedTypeProperty("type")]
    [DiscriminatedDerivedType(typeof(EmptyDiscriminatorChild), "")]
    private record EmptyDiscriminator
    {
        [JsonConstructor]
        public EmptyDiscriminator()
        {
        }

        [JsonPropertyName("type")]
        [JsonInclude]
        public string Type { get; init; } = string.Empty;
    }

    private record EmptyDiscriminatorChild : EmptyDiscriminator
    {
        [JsonConstructor]
        public EmptyDiscriminatorChild()
            : base()
        {
        }

        [JsonPropertyName("propertyA")]
        [JsonInclude]
        public string PropertyA { get; init; } = "propertyA";
    }

    [JsonConverter(typeof(DiscriminatedUnionJsonConverter<NullForEmptyDiscriminator>))]
    [DiscriminatedTypeProperty("type", PropertyMissingBehavior = DiscriminatorPropertyMissingValueBehavior.ReturnNull)]
    [DiscriminatedDerivedType(typeof(NullForEmptyDiscriminatorChild), "child")]
    private record NullForEmptyDiscriminator
    {
        [JsonConstructor]
        public NullForEmptyDiscriminator()
        {
        }

        [JsonPropertyName("type")]
        [JsonInclude]
        public string Type { get; init; } = string.Empty;
    }

    private record NullForEmptyDiscriminatorChild : NullForEmptyDiscriminator
    {
        [JsonConstructor]
        public NullForEmptyDiscriminatorChild()
            : base()
        {
        }

        [JsonPropertyName("propertyA")]
        [JsonInclude]
        public string PropertyA { get; init; } = "propertyA";
    }

    [JsonConverter(typeof(DiscriminatedUnionJsonConverter<PropertyPresence>))]
    [DiscriminatedTypePresence]
    [DiscriminatedDerivedType(typeof(PropertyPresenceChildA), "propertyA")]
    [DiscriminatedDerivedType(typeof(PropertyPresenceChildB), "propertyB")]
    private record PropertyPresence
    {
        [JsonConstructor]
        public PropertyPresence()
        {
        }
    }

    private record PropertyPresenceChildA : PropertyPresence
    {
        [JsonConstructor]
        public PropertyPresenceChildA()
            : base()
        {
        }

        [JsonPropertyName("propertyA")]
        [JsonInclude]
        public string PropertyA { get; init; } = "propertyA";
    }

    private record PropertyPresenceChildB : PropertyPresence
    {
        [JsonConstructor]
        public PropertyPresenceChildB()
            : base()
        {
        }

        [JsonPropertyName("propertyB")]
        [JsonInclude]
        public int PropertyB { get; init; }
    }

    [JsonConverter(typeof(DiscriminatedUnionJsonConverter<MissingDiscriminatorAttribute>))]
    private record MissingDiscriminatorAttribute
    {
        [JsonConstructor]
        public MissingDiscriminatorAttribute()
        {
        }
    }
}
