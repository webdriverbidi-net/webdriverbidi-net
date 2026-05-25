namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

public class DiscriminatedUnionJsonConverterTests
{
    [Fact]
    public void TestCanDeserializeDerivedType()
    {
        string json = """
                      {
                        "type": "typeA",
                        "propertyA": "Value for A"
                      }
                      """;
        ParentTypeWithChildTypes? result = JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json);
        Assert.NotNull(result);
        Assert.IsType<DerivedTypeA>(result);
        DerivedTypeA derivedA = (DerivedTypeA)result;
        Assert.Equal("typeA", derivedA.Type);
        Assert.Equal("Value for A", derivedA.PropertyA);
    }

    [Fact]
    public void TestDeserializingDerivedTypeWithInvalidDiscriminatorValueThrows()
    {
        string json = """
                      {
                        "type": "typeC",
                        "propertyC": true
                      }
                      """;
        Assert.Contains("JSON for 'ParentTypeWithChildTypes' type property contains unknown value 'typeC'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json)).Message);
    }

    [Fact]
    public void TestDeserializingDerivedTypeWithMissingDiscriminatorPropertyThrows()
    {
        string json = """
                      {
                        "propertyC": true
                      }
                      """;
        Assert.Contains("JSON for 'ParentTypeWithChildTypes' must contain a 'type' property", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json)).Message);
    }

    [Fact]
    public void TestDeserializingBaseTypeWithInvalidJsonSchemaThrows()
    {
        string json = """
                      [
                        "type",
                        "propertyC"
                      ]
                      """;
        Assert.Contains("JSON for 'ParentTypeWithChildTypes' must be an object, but starting token was StartArray", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json)).Message);
    }

    [Fact]
    public void TestDeserializingDerivedTypeWithInvalidDiscriminatorValueTypeThrows()
    {
        string json = """
                      {
                        "type": 123,
                        "propertyC": true
                      }
                      """;
        Assert.Contains("JSON 'type' property must be a string", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ParentTypeWithChildTypes>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWithEmptyDiscriminatorValueThrows()
    {
        string json = """
                      {
                        "type": "asdf",
                        "propertyC": true
                      }
                      """;
        Assert.Contains("must have a non-empty Discriminator", Assert.ThrowsAny<InvalidOperationException>(() => JsonSerializer.Deserialize<EmptyDiscriminator>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWithMissingDiscriminatorValueReturnsNullForAttributeValue()
    {
        string json = """
                      {
                        "propertyC": true
                      }
                      """;
        NullForEmptyDiscriminator? result = JsonSerializer.Deserialize<NullForEmptyDiscriminator>(json);
        Assert.Null(result);
    }

    [Fact]
    public void TestCanDeserializeFallbackDerivedType()
    {
        string json = """
                      {
                        "type": "someUnknownType",
                        "propertyA": "Value for A"
                      }
                      """;
        ParentTypeWithFallback? result = JsonSerializer.Deserialize<ParentTypeWithFallback>(json);
        Assert.NotNull(result);
        Assert.IsType<FallbackDerivedType>(result);
        FallbackDerivedType fallbackResult = (FallbackDerivedType)result;
        Assert.Equal("someUnknownType", fallbackResult.Type);
        Assert.Equal("Value for A", fallbackResult.PropertyA);
    }

    [Fact]
    public void TestCanDeserializeBasedOnPropertyPresence()
    {
        string jsonA = """
                      {
                        "propertyA": "Value for A"
                      }
                      """;
        PropertyPresence? resultA = JsonSerializer.Deserialize<PropertyPresence>(jsonA);
        Assert.NotNull(resultA);
        Assert.IsType<PropertyPresenceChildA>(resultA);
        PropertyPresenceChildA childAResult = (PropertyPresenceChildA)resultA;
        Assert.Equal("Value for A", childAResult.PropertyA);

        string jsonB = """
                      {
                        "propertyB": 123
                      }
                      """;
        PropertyPresence? resultB = JsonSerializer.Deserialize<PropertyPresence>(jsonB);
        Assert.NotNull(resultB);
        Assert.IsType<PropertyPresenceChildB>(resultB);
        PropertyPresenceChildB childBResult = (PropertyPresenceChildB)resultB;
        Assert.Equal(123, childBResult.PropertyB);
    }

    [Fact]
    public void TestDeserializingBasedOnPropertyPresenceWithInvalidValueThrows()
    {
        string jsonA = """
                      {
                        "propertyC": "Value for C"
                      }
                      """;
        Assert.Contains("one of the following properties", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PropertyPresence>(jsonA)).Message);
    }

    [Fact]
    public void TestCanSerializeDerivedType()
    {
        ParentTypeWithChildTypes derivedB = new DerivedTypeB
        {
            Type = "typeB",
            PropertyB = 42
        };
        string json = JsonSerializer.Serialize(derivedB);
        Assert.NotNull(json);
        Assert.NotEmpty(json);
    }

    [Fact]
    public void TestDeserializingTypeWithMissingDiscriminatedTypePropertyAttributeThrows()
    {
        string json = """
                      {
                        "type": "child"
                      }
                      """;
        Assert.Contains("must have a [DiscriminatedTypeProperty] or [DiscriminatedTypePresence] attribute", Assert.ThrowsAny<InvalidOperationException>(() => JsonSerializer.Deserialize<MissingDiscriminatorAttribute>(json)).Message);
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
