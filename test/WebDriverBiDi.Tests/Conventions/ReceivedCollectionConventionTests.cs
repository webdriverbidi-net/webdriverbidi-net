namespace WebDriverBiDi.Conventions;

using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

/// <summary>
/// Enforces that every collection the transport can populate from a remote-end payload rejects a JSON
/// <see langword="null"/> element or value.
/// </summary>
/// <remarks>
/// <para>
/// No array or map production in the WebDriver BiDi specification admits a <c>null</c> element, but
/// <see cref="System.Text.Json.JsonSerializer"/> does not enforce that: nullable annotations are not
/// honored for the elements of a collection, so without a converter a <c>null</c> element is admitted
/// into a list whose element type is non-nullable and surfaces later as a
/// <see cref="NullReferenceException"/> in consumer code rather than as a protocol failure the transport
/// can contain. <see cref="NonNullElementListJsonConverter{T}"/> and
/// <see cref="NonNullValueDictionaryJsonConverter{TValue}"/> close that hole, and this test is what keeps
/// a newly added collection from missing one: nothing else fails when the attribute is absent.
/// </para>
/// <para>
/// Extension data is deliberately exempt. A vendor may legitimately send a <c>null</c> in an
/// <c>Extensible</c> object, and the library preserves it.
/// </para>
/// </remarks>
public class ReceivedCollectionConventionTests
{
    [Fact]
    public void TestReceivedCollectionsRejectNullElements()
    {
        List<string> offenders = [];
        foreach ((Type owner, PropertyInfo property) in GetDeserializableCollectionProperties())
        {
            Type collectionType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            Type[] typeArguments = collectionType.GetGenericArguments();
            Type elementType = typeArguments[^1];
            Type expectedConverter = collectionType.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                ? typeof(NonNullValueDictionaryJsonConverter<>).MakeGenericType(elementType)
                : typeof(NonNullElementListJsonConverter<>).MakeGenericType(elementType);

            JsonConverterAttribute? converter = property.GetCustomAttribute<JsonConverterAttribute>();
            if (converter is null)
            {
                offenders.Add($"{owner.FullName}.{property.Name} (no [JsonConverter]; expected {Describe(expectedConverter)})");
            }
            else if (converter.ConverterType != expectedConverter)
            {
                offenders.Add($"{owner.FullName}.{property.Name} (has {Describe(converter.ConverterType)}; expected {Describe(expectedConverter)})");
            }
        }

        Assert.True(offenders.Count == 0, $"Every collection deserialized from a remote-end payload must reject a null element, because System.Text.Json admits one into a non-nullable element type. Apply the converter named for each member below. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void TestSweepFindsTheCollectionsItIsMeantToGuard()
    {
        // The sweep walks a type graph, so a mistake in the walk would silently guard nothing and the
        // test above would pass vacuously. Assert that it still reaches a representative member of each
        // shape it is responsible for: a command result list, a list nested inside a received object, a
        // list on an event payload, a list on a type that is both sent and received, and the one map.
        HashSet<string> found = [.. GetDeserializableCollectionProperties().Select(entry => $"{entry.Owner.FullName}.{entry.Property.Name}")];
        string[] expected =
        [
            "WebDriverBiDi.Script.GetRealmsCommandResult.SerializableRealms",
            "WebDriverBiDi.Script.StackTrace.SerializableCallFrames",
            "WebDriverBiDi.Network.BaseNetworkEventArgs.SerializableIntercepts",
            "WebDriverBiDi.Session.ManualProxyConfiguration.SerializableNoProxyAddresses",
            "WebDriverBiDi.Script.NodeProperties.SerializableAttributes",
        ];
        foreach (string member in expected)
        {
            Assert.Contains(member, found);
        }

        Assert.True(found.Count >= 15, $"Expected the sweep to reach every received collection in the library, but it found only {found.Count}.");
    }

    private static string Describe(Type? converterType)
    {
        if (converterType is null)
        {
            return "none";
        }

        return converterType.IsGenericType
            ? $"{converterType.Name.Split('`')[0]}<{string.Join(", ", converterType.GetGenericArguments().Select(argument => argument.Name))}>"
            : converterType.Name;
    }

    /// <summary>
    /// Gets every property the serializer can populate, on any type reachable from a command result or a
    /// registered event payload, whose type is a list or string-keyed dictionary of a reference type.
    /// </summary>
    /// <returns>The owning type and property for each such member.</returns>
    private static IEnumerable<(Type Owner, PropertyInfo Property)> GetDeserializableCollectionProperties()
    {
        const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        Assembly assembly = typeof(CommandResult).Assembly;
        List<(Type, PropertyInfo)> results = [];
        HashSet<Type> visited = [];
        Queue<Type> pending = new(GetReceivedPayloadTypes(assembly));
        while (pending.Count > 0)
        {
            Type current = pending.Dequeue();
            if (current.IsPrimitive || current == typeof(string) || current.Namespace?.StartsWith("System", StringComparison.Ordinal) == true || !visited.Add(current))
            {
                continue;
            }

            // A payload member typed as an abstract base (a discriminated union such as RemoteValue or
            // RealmInfo) is deserialized to one of its derived types, so those must be walked too.
            foreach (Type derived in assembly.GetTypes())
            {
                if (current.IsAssignableFrom(derived) && derived != current)
                {
                    pending.Enqueue(derived);
                }
            }

            // Members are collected from their declaring type, so a base class that no member is typed
            // as must still be walked; otherwise a member declared on a shared base (such as
            // BaseNetworkEventArgs.SerializableIntercepts, inherited by all five network events) is
            // never examined and this convention silently guards nothing for it.
            if (current.BaseType is Type baseType && baseType.Assembly == assembly)
            {
                pending.Enqueue(baseType);
            }

            foreach (PropertyInfo property in current.GetProperties(MemberFlags))
            {
                Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (!propertyType.IsGenericType || !typeof(IEnumerable).IsAssignableFrom(propertyType))
                {
                    pending.Enqueue(propertyType);
                    continue;
                }

                foreach (Type typeArgument in propertyType.GetGenericArguments())
                {
                    pending.Enqueue(typeArgument);
                }

                if (IsGuardedCollection(property, propertyType))
                {
                    results.Add((current, property));
                }
            }
        }

        return results;
    }

    private static bool IsGuardedCollection(PropertyInfo property, Type propertyType)
    {
        // Only a member the serializer can assign is populated from the payload; a read-only projection
        // (the public IList<T> wrapper over the backing member) is not deserialized.
        if (property.SetMethod is null)
        {
            return false;
        }

        // Extension data legitimately carries nulls, and its value type (object or JsonElement) is not a
        // protocol type in any case.
        if (property.GetCustomAttribute<JsonExtensionDataAttribute>() is not null)
        {
            return false;
        }

        Type definition = propertyType.GetGenericTypeDefinition();
        if (definition != typeof(List<>) && definition != typeof(Dictionary<,>))
        {
            return false;
        }

        if (definition == typeof(Dictionary<,>) && propertyType.GetGenericArguments()[0] != typeof(string))
        {
            return false;
        }

        Type elementType = propertyType.GetGenericArguments()[^1];
        if (elementType == typeof(object) || elementType == typeof(JsonElement) || elementType.IsValueType)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the payload types the transport deserializes: every command result, and the data type of
    /// every event registered by the driver's modules.
    /// </summary>
    /// <param name="assembly">The library assembly.</param>
    /// <returns>The set of payload types.</returns>
    private static IEnumerable<Type> GetReceivedPayloadTypes(Assembly assembly)
    {
        HashSet<Type> payloadTypes = [];
        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(CommandResult).IsAssignableFrom(type) && !type.IsAbstract)
            {
                payloadTypes.Add(type);
            }
        }

        // Reading the registry rather than listing event args types by hand means an event added later
        // is swept automatically, exactly as ExtensionDataConventionTests does.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        _ = new BiDiDriver(TimeSpan.FromSeconds(1), transport);
        FieldInfo? field = transport.GetType().GetField("eventMessageTypes", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        IDictionary? registry = field.GetValue(transport) as IDictionary;
        Assert.NotNull(registry);
        foreach (object? registration in registry.Values)
        {
            Type? messageType = registration?.GetType().GetProperty("EventMessageType")?.GetValue(registration) as Type;
            Assert.NotNull(messageType);
            payloadTypes.Add(messageType.GetGenericArguments()[0]);
        }

        return payloadTypes;
    }
}
