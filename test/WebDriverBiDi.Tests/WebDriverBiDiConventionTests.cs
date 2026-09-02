namespace WebDriverBiDi;

using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;

/// <summary>
/// Enforces conventions that hold across the whole library and that no compiler check can express.
/// </summary>
/// <remarks>
/// <para>
/// The shape rules for list-typed properties on every <see cref="CommandParameters"/> type: lists
/// are never settable, optional lists are omitted while empty through an internal
/// <c>Serializable*</c> property, and the only nullable-settable lists are the ones for which the
/// protocol gives a present-but-empty array a meaning distinct from omission.
/// </para>
/// <para>
/// The declaration rules for module events: every observable event a module exposes carries an
/// <see cref="ObservableEventNameAttribute"/> naming the protocol event it corresponds to, and that
/// name agrees both with the event's own <see cref="ObservableEvent{T}.EventName"/> and with the
/// module it belongs to.
/// </para>
/// </remarks>
public class WebDriverBiDiConventionTests
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

        // This test enforces the non-nullable annotation; initialization is enforced separately by the
        // compiler, which treats CS8618 (uninitialized non-nullable member) as an error in the library.
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

    [Fact]
    public void TestModuleObservableEventsCarryMatchingObservableEventNameAttribute()
    {
        // The attribute is what the BIDI005, BIDI015, and BIDI027 analyzers read to know which
        // protocol event a property corresponds to, so an event that lacks it, or carries a name
        // that disagrees with the event itself, silently degrades those diagnostics rather than
        // failing anything. Nothing else checks it.
        List<string> offenders = [];
        foreach ((Module module, PropertyInfo property) in GetModuleObservableEventProperties())
        {
            string key = Key(property);
            ObservableEventNameAttribute? attribute = property.GetCustomAttribute<ObservableEventNameAttribute>();
            if (attribute is null)
            {
                offenders.Add($"{key} (no [ObservableEventName])");
                continue;
            }

            // The runtime name is the one the transport actually dispatches on, so the attribute is
            // only useful insofar as it agrees with it.
            object? observableEvent = property.GetValue(module);
            Assert.NotNull(observableEvent);
            string runtimeEventName = (string)observableEvent.GetType().GetProperty("EventName")!.GetValue(observableEvent)!;
            if (attribute.EventName != runtimeEventName)
            {
                offenders.Add($"{key} ([ObservableEventName(\"{attribute.EventName}\")] but EventName is \"{runtimeEventName}\")");
                continue;
            }

            // Every event name in the protocol is qualified by the module that raises it. The
            // module-name constants are public, so a copy-paste that builds one module's event name
            // from another module's constant compiles cleanly; this is what catches it.
            string expectedPrefix = $"{module.ModuleName}.";
            if (!runtimeEventName.StartsWith(expectedPrefix, StringComparison.Ordinal))
            {
                offenders.Add($"{key} (\"{runtimeEventName}\" is not qualified by its own module, \"{module.ModuleName}\")");
            }
        }

        Assert.True(offenders.Count == 0, $"Observable events on modules must carry an [ObservableEventName] whose value matches the event's own EventName and is qualified by the declaring module. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void TestEveryModuleObservableEventIsCoveredByTheAttributeSweep()
    {
        // The sweep above reaches events through the driver's module properties, so a module the
        // driver does not expose would be skipped silently and its events would go unchecked.
        // Comparing against the modules actually declared in the library closes that hole.
        HashSet<Type> sweptModuleTypes = [.. GetDriverModules().Select(module => module.GetType())];
        List<string> unreachable = [];
        foreach (Type type in typeof(Module).Assembly.GetTypes())
        {
            if (type.IsAbstract || !typeof(Module).IsAssignableFrom(type))
            {
                continue;
            }

            if (!sweptModuleTypes.Contains(type))
            {
                unreachable.Add(type.FullName!);
            }
        }

        Assert.True(unreachable.Count == 0, $"Every module in the library must be reachable from BiDiDriver, or its observable events escape the [ObservableEventName] sweep. Unreachable: {string.Join(", ", unreachable)}");
    }

    private static string Key(PropertyInfo property)
    {
        return $"{property.DeclaringType!.FullName}.{property.Name}";
    }

    /// <summary>
    /// Gets the module instances a <see cref="BiDiDriver"/> constructs. The driver is the only place
    /// they can be taken from: constructing a second instance of a module against the same driver
    /// throws, because the module registers its event names with the driver as it is built.
    /// </summary>
    private static IEnumerable<Module> GetDriverModules()
    {
        BiDiDriver driver = new();
        foreach (PropertyInfo property in typeof(BiDiDriver).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (typeof(Module).IsAssignableFrom(property.PropertyType) && property.GetValue(driver) is Module module)
            {
                yield return module;
            }
        }
    }

    private static IEnumerable<(Module Module, PropertyInfo Property)> GetModuleObservableEventProperties()
    {
        foreach (Module module in GetDriverModules())
        {
            foreach (PropertyInfo property in module.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ObservableEvent<>))
                {
                    yield return (module, property);
                }
            }
        }
    }

    [Fact]
    public void TestSpecRangeAttributesAreWellFormed()
    {
        List<PropertyInfo> rangedProperties = GetSpecRangeProperties().ToList();

        // Guard against the sweep silently finding nothing (for example if the attribute were renamed
        // or every application removed): the library has many spec-ranged command-parameter properties.
        Assert.True(rangedProperties.Count >= 15, $"Expected the library to contain multiple [SpecRange]-decorated properties, but found {rangedProperties.Count}.");

        List<string> offenders = [];
        foreach (PropertyInfo property in rangedProperties)
        {
            SpecRangeAttribute attribute = property.GetCustomAttribute<SpecRangeAttribute>()!;
            string propertyName = $"{property.DeclaringType!.FullName}.{property.Name}";

            if (attribute.Minimum > attribute.Maximum)
            {
                offenders.Add($"{propertyName} (minimum {attribute.Minimum} is greater than maximum {attribute.Maximum})");
            }

            // With an exclusive maximum, a minimum equal to the maximum would describe an empty range.
            if (attribute.MaximumExclusive && attribute.Minimum == attribute.Maximum)
            {
                offenders.Add($"{propertyName} (minimum {attribute.Minimum} equals the exclusive maximum {attribute.Maximum}, describing an empty range)");
            }

            // A reset sentinel is by definition a value outside the specification range; if it fell
            // inside the range it would be indistinguishable from an ordinary valid value. A value
            // equal to an exclusive maximum is outside the range, so it is a legal sentinel.
            bool sentinelBelowUpperBound = attribute.MaximumExclusive ? attribute.SentinelValue < attribute.Maximum : attribute.SentinelValue <= attribute.Maximum;
            if (attribute.HasSentinel && attribute.SentinelValue >= attribute.Minimum && sentinelBelowUpperBound)
            {
                offenders.Add($"{propertyName} (sentinel {attribute.SentinelValue} is inside the valid range [{attribute.Minimum}, {attribute.Maximum}])");
            }
        }

        Assert.True(offenders.Count == 0, $"[SpecRange] attributes must declare a valid range with the minimum no greater than the maximum, and any sentinel must fall outside that range. Offenders: {string.Join(", ", offenders)}");
    }

    private static IEnumerable<PropertyInfo> GetSpecRangeProperties()
    {
        foreach (Type type in typeof(CommandParameters).Assembly.GetTypes())
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.GetCustomAttribute<SpecRangeAttribute>() is not null)
                {
                    yield return property;
                }
            }
        }
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
