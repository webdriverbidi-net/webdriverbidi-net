namespace WebDriverBiDi;

using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;

/// <summary>
/// Enforces the shape rules for list-typed properties on every <see cref="CommandParameters"/> type:
/// lists are never settable, optional lists are omitted while empty through an internal
/// <c>Serializable*</c> property, and the only nullable-settable lists are the ones for which the
/// protocol gives a present-but-empty array a meaning distinct from omission.
/// </summary>
public class CommandParametersConventionTests
{
    /// <summary>
    /// The optional lists that are deliberately nullable and settable. An entry belongs here only when the
    /// command's remote end steps in the WebDriver BiDi specification branch on the field's presence and
    /// replace existing state before consuming the array (e.g. network.continueRequest: "If command parameters
    /// contains "headers": Let headers be an empty header list"), so that sending <c>[]</c> differs from omitting
    /// the field. Cite the step in the property's XML remarks when adding an entry.
    /// </summary>
    private static readonly HashSet<string> NullableSettableListAllowList =
    [
        "WebDriverBiDi.Network.ContinueRequestCommandParameters.Headers",
        "WebDriverBiDi.Network.ContinueRequestCommandParameters.Cookies",
        "WebDriverBiDi.Network.ContinueResponseCommandParameters.Headers",
        "WebDriverBiDi.Network.ContinueResponseCommandParameters.Cookies",
        "WebDriverBiDi.Network.ProvideResponseCommandParameters.Headers",
        "WebDriverBiDi.Network.ProvideResponseCommandParameters.Cookies",
    ];

    private static readonly NullabilityInfoContext NullabilityContext = new();

    [Fact]
    public void TestListPropertiesAreNotSettableUnlessAllowListed()
    {
        List<string> offenders = [];
        foreach (PropertyInfo property in GetListProperties())
        {
            if (!NullableSettableListAllowList.Contains(Key(property)) && property.SetMethod is { IsPublic: true })
            {
                offenders.Add(Key(property));
            }
        }

        Assert.True(offenders.Count == 0, $"List properties on CommandParameters types must be read-only (required lists) or read-only with a Serializable* shim (optional lists); add the property to the allow list only if the specification distinguishes an empty array from an omitted field. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void TestReadOnlyListPropertiesAreNonNullable()
    {
        List<string> offenders = [];
        foreach (PropertyInfo property in GetListProperties())
        {
            if (!NullableSettableListAllowList.Contains(Key(property)) && NullabilityContext.Create(property).ReadState != NullabilityState.NotNull)
            {
                offenders.Add(Key(property));
            }
        }

        Assert.True(offenders.Count == 0, $"Read-only list properties must be non-nullable and initialized. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void TestOptionalReadOnlyListPropertiesHaveSerializableShim()
    {
        List<string> offenders = [];
        foreach (PropertyInfo property in GetListProperties())
        {
            if (NullableSettableListAllowList.Contains(Key(property)))
            {
                continue;
            }

            JsonIgnoreAttribute? ignore = property.GetCustomAttribute<JsonIgnoreAttribute>();
            if (ignore is null || ignore.Condition != JsonIgnoreCondition.Always)
            {
                // A required list: serialized directly, so it must carry the JSON property name itself.
                if (property.GetCustomAttribute<JsonPropertyNameAttribute>() is null)
                {
                    offenders.Add($"{Key(property)} (required list without [JsonPropertyName])");
                }

                continue;
            }

            // An optional list: the public property is ignored and an internal Serializable* property emits
            // null (omitting the field) while the list is empty.
            PropertyInfo? shim = property.DeclaringType!.GetProperty($"Serializable{property.Name}", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (shim is null)
            {
                offenders.Add($"{Key(property)} (no Serializable{property.Name} shim)");
                continue;
            }

            if (shim.GetCustomAttribute<JsonPropertyNameAttribute>() is null || shim.GetCustomAttribute<JsonIncludeAttribute>() is null)
            {
                offenders.Add($"{Key(property)} (shim missing [JsonPropertyName] or [JsonInclude])");
                continue;
            }

            JsonIgnoreAttribute? shimIgnore = shim.GetCustomAttribute<JsonIgnoreAttribute>();
            if (shimIgnore is null || shimIgnore.Condition != JsonIgnoreCondition.WhenWritingNull)
            {
                offenders.Add($"{Key(property)} (shim must use JsonIgnoreCondition.WhenWritingNull)");
            }
        }

        Assert.True(offenders.Count == 0, $"Optional read-only lists must be paired with an internal Serializable* property that omits the field while the list is empty. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void TestAllowListedPropertiesAreNullableAndSettable()
    {
        Dictionary<string, PropertyInfo> listProperties = GetListProperties().ToDictionary(Key);
        List<string> offenders = [];
        foreach (string entry in NullableSettableListAllowList)
        {
            if (!listProperties.TryGetValue(entry, out PropertyInfo? property))
            {
                offenders.Add($"{entry} (no such list property; remove the stale allow list entry)");
                continue;
            }

            if (property.SetMethod is not { IsPublic: true })
            {
                offenders.Add($"{entry} (not settable; remove it from the allow list)");
            }

            if (NullabilityContext.Create(property).WriteState != NullabilityState.Nullable)
            {
                offenders.Add($"{entry} (not nullable)");
            }

            JsonIgnoreAttribute? ignore = property.GetCustomAttribute<JsonIgnoreAttribute>();
            if (property.GetCustomAttribute<JsonPropertyNameAttribute>() is null || ignore is null || ignore.Condition != JsonIgnoreCondition.WhenWritingNull)
            {
                offenders.Add($"{entry} (must carry [JsonPropertyName] and [JsonIgnore(Condition = WhenWritingNull)])");
            }
        }

        Assert.True(offenders.Count == 0, $"Allow-listed lists must be nullable, settable, and omitted only when null. Offenders: {string.Join(", ", offenders)}");
    }

    private static string Key(PropertyInfo property)
    {
        return $"{property.DeclaringType!.FullName}.{property.Name}";
    }

    private static IEnumerable<PropertyInfo> GetListProperties()
    {
        foreach (Type type in typeof(CommandParameters).Assembly.GetTypes())
        {
            if (!typeof(CommandParameters).IsAssignableFrom(type))
            {
                continue;
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Type propertyType = property.PropertyType;
                if (propertyType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(propertyType) || typeof(IDictionary).IsAssignableFrom(propertyType))
                {
                    continue;
                }

                yield return property;
            }
        }
    }
}
