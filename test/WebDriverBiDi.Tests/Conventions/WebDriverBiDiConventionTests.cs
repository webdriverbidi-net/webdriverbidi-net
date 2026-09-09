namespace WebDriverBiDi.Conventions;

using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;

// In this namespace the file-level using of System.Reflection would otherwise make the bare name
// 'Module' bind to System.Reflection.Module rather than to the library's protocol module base class.
using Module = WebDriverBiDi.Module;

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
    /// build the resulting state from that field alone, so that sending <c>[]</c> means "none" and differs
    /// from omitting the field, which leaves the intercepted request or response as it was. Describe the
    /// behaviour in the property's XML remarks when adding an entry; describe it rather than quoting the
    /// specification, because the steps differ per command and a verbatim quotation drifts out of date.
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

    /// <summary>
    /// The module commands whose <see cref="CommandParameters"/> argument may be omitted. A command belongs
    /// here exactly when its parameters type can be constructed with <c>new()</c> and offers no public static
    /// reset member, which is what
    /// <see cref="TestModuleCommandParametersAreOptionalExactlyWhenTheRuleAllows"/> verifies. The set is
    /// listed as well as derived because these commands are enumerated in the documentation, so a change
    /// here is a prompt to change the tables there too.
    /// </summary>
    private static readonly HashSet<string> OptionalParametersCommands =
    [
        "BrowserModule.CloseAsync",
        "BrowserModule.CreateUserContextAsync",
        "BrowserModule.GetClientWindowsAsync",
        "BrowserModule.GetUserContextsAsync",
        "BrowsingContextModule.GetTreeAsync",
        "ScriptModule.GetRealmsAsync",
        "SessionModule.EndAsync",
        "SessionModule.NewSessionAsync",
        "SessionModule.StatusAsync",
        "StorageModule.DeleteCookiesAsync",
        "StorageModule.GetCookiesAsync",
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
    public void TestEveryModuleObservableEventIsCoveredByExaminingAttributes()
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

    [Fact]
    public void TestModuleCommandParametersAreOptionalExactlyWhenTheRuleAllows()
    {
        // A module command may let its parameters object be omitted only where omitting it is unambiguous.
        // The type must be constructible with new(), so that a default instance can stand in for "no
        // options"; and it must expose no public static reset member, because for those commands the method
        // name alone would not say whether a value is being set or reset — SetSomeCondition() reads as
        // though a condition is being established, where
        // SetSomeCondition(SetSomeConditionCommandParameters.ResetSomeCondition) states the intent. Every
        // other command requires its parameters object. Nothing but review enforced this before.
        List<string> offenders = [];
        List<string> optionalCommands = [];
        int commandCount = 0;
        foreach ((Module module, MethodInfo method, ParameterInfo parameter) in GetModuleCommandMethods())
        {
            commandCount++;
            string key = $"{module.GetType().Name}.{method.Name}";
            Type parametersType = parameter.ParameterType;

            // The optional form is a nullable parameter defaulted to null, which the module turns into a
            // default instance with `commandParameters ?? new()`.
            bool isOptional = parameter.HasDefaultValue
                && parameter.DefaultValue is null
                && NullabilityContext.Create(parameter).WriteState == NullabilityState.Nullable;
            if (isOptional)
            {
                optionalCommands.Add(key);
            }

            // GetConstructor considers public instance constructors only, so a type whose parameterless
            // constructor is protected — as the base of a discriminated set of parameters types tends to
            // be — is correctly reported as not constructible here.
            bool isConstructible = !parametersType.IsAbstract && parametersType.GetConstructor(Type.EmptyTypes) is not null;
            bool hasResetMember = HasPublicStaticResetMember(parametersType);
            bool shouldBeOptional = isConstructible && !hasResetMember;
            if (isOptional == shouldBeOptional)
            {
                continue;
            }

            string reason = shouldBeOptional
                ? $"{parametersType.Name} is constructible and offers no reset member, so the parameters object should be optional"
                : hasResetMember
                    ? $"{parametersType.Name} offers a public static reset member, so the parameters object must be required"
                    : $"{parametersType.Name} cannot be constructed with new(), so the parameters object must be required";
            offenders.Add($"{key} ({reason})");
        }

        Assert.True(offenders.Count == 0, $"A module command must accept an optional parameters object exactly when its parameters type is constructible with new() and offers no public static reset member. Offenders: {string.Join(", ", offenders)}");

        // Guard against the sweep silently reaching nothing and passing vacuously.
        Assert.True(commandCount >= 60, $"Expected the sweep to reach every module command in the library, but it found only {commandCount}.");

        // The commands that take optional parameters are enumerated in the documentation (the tables in
        // docs/articles/advanced/api-design.md and docs/articles/core-concepts.md). Nothing else keeps those
        // tables honest, so changing the set here is deliberately a two-step edit.
        List<string> unexpected = [.. optionalCommands.Where(command => !OptionalParametersCommands.Contains(command)).OrderBy(command => command, StringComparer.Ordinal)];
        List<string> missing = [.. OptionalParametersCommands.Where(command => !optionalCommands.Contains(command)).OrderBy(command => command, StringComparer.Ordinal)];
        Assert.True(unexpected.Count == 0 && missing.Count == 0, $"The set of commands taking optional parameters changed. Update this list and the documented tables together. Newly optional: {string.Join(", ", unexpected)}. No longer optional: {string.Join(", ", missing)}.");
    }

    [Fact]
    public void TestEveryModuleCommandTakesATimeoutOverrideAndCancellationToken()
    {
        // Every module command ends with the same two optional arguments, so that any command can be
        // given a per-call timeout and can be canceled. BIDI004 and BIDI013 report call sites that omit
        // the token, which only makes sense while every command offers one. A command added without
        // them compiles and ships, and nothing else notices.
        List<string> offenders = [];
        int commandCount = 0;
        foreach ((Module module, MethodInfo method, ParameterInfo _) in GetModuleCommandMethods())
        {
            commandCount++;
            string name = $"{module.GetType().Name}.{method.Name}";
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 3)
            {
                offenders.Add($"{name} takes {parameters.Length} parameters; expected the command parameters, a timeout override and a cancellation token");
                continue;
            }

            if (parameters[1].ParameterType != typeof(TimeSpan?) || parameters[1].Name != "timeoutOverride" || !parameters[1].IsOptional)
            {
                offenders.Add($"{name} second parameter is '{parameters[1].ParameterType.Name} {parameters[1].Name}' (optional: {parameters[1].IsOptional}); expected an optional 'TimeSpan? timeoutOverride'");
            }

            if (parameters[2].ParameterType != typeof(CancellationToken) || parameters[2].Name != "cancellationToken" || !parameters[2].IsOptional)
            {
                offenders.Add($"{name} third parameter is '{parameters[2].ParameterType.Name} {parameters[2].Name}' (optional: {parameters[2].IsOptional}); expected an optional 'CancellationToken cancellationToken'");
            }
        }

        // A floor rather than an inventory, so adding a command does not break it. It fails if the
        // reflection walk stops finding commands, which would otherwise let the check above pass by
        // sweeping nothing at all. There are 82 today.
        Assert.True(commandCount >= 80, $"The command sweep found only {commandCount} module commands; the walk is broken.");
        Assert.True(offenders.Count == 0, $"Every module command must end with 'TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default'. Offenders:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    [Fact]
    public void TestEveryCommandMethodNameIsQualifiedByItsOwnModule()
    {
        // A command's MethodName is the wire method, and the protocol spells it "<module>.<command>".
        // The module segment must be the module the parameters type belongs to: a copy-paste that
        // leaves the wrong prefix produces a command the remote end rejects, or worse, routes to a
        // different module's command, and no compiler check can see it.
        Dictionary<string, string> moduleNameByNamespace = [];
        foreach (Module module in GetDriverModules())
        {
            moduleNameByNamespace[module.GetType().Namespace!] = module.ModuleName;
        }

        List<string> offenders = [];
        int parametersTypeCount = 0;
        foreach (Type type in typeof(CommandParameters).Assembly.GetTypes())
        {
            if (type.IsAbstract || !typeof(CommandParameters).IsAssignableFrom(type))
            {
                continue;
            }

            parametersTypeCount++;

            if (!moduleNameByNamespace.TryGetValue(type.Namespace ?? string.Empty, out string? expectedModuleName))
            {
                offenders.Add($"{type.FullName} is a CommandParameters type in a namespace that owns no module, so its method name cannot be checked");
                continue;
            }

            // The property is a constant expression on every parameters type, so it can be read
            // without running a constructor; several of them require arguments.
            string methodName = ((CommandParameters)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type)).MethodName;
            string[] segments = methodName.Split('.');
            if (segments.Length != 2 || segments[0].Length == 0 || segments[1].Length == 0)
            {
                offenders.Add($"{type.FullName} declares MethodName '{methodName}'; expected exactly '<module>.<command>'");
                continue;
            }

            if (segments[0] != expectedModuleName)
            {
                offenders.Add($"{type.FullName} declares MethodName '{methodName}', whose module segment is '{segments[0]}'; the module in its namespace is '{expectedModuleName}'");
            }
        }

        // A floor, for the same reason as the command sweep above. There are 88 today.
        Assert.True(parametersTypeCount >= 85, $"The parameters-type sweep found only {parametersTypeCount} types; the walk is broken.");
        Assert.True(offenders.Count == 0, $"Every command's MethodName must be '<module>.<command>' with the module segment naming the module that owns it. Offenders:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    [Fact]
    public void TestEveryObservableEventPropertyIsNamedWithTheOnPrefix()
    {
        // Callers discover events by name, and every one of them reads "driver.<Module>.On<Event>".
        // The sweep covers the transport-level types as well as the modules, because Connection,
        // Transport and BiDiDriver expose observable events of their own on the same footing.
        List<string> offenders = [];
        int eventPropertyCount = 0;
        foreach ((Module module, PropertyInfo property) in GetModuleObservableEventProperties())
        {
            eventPropertyCount++;
            if (!IsOnPrefixed(property.Name))
            {
                offenders.Add($"{module.GetType().Name}.{property.Name}");
            }
        }

        foreach (Type type in new[] { typeof(WebDriverBiDi.Protocol.Connection), typeof(WebDriverBiDi.Protocol.Transport), typeof(BiDiDriver) })
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(ObservableEvent<>))
                {
                    eventPropertyCount++;
                    if (!IsOnPrefixed(property.Name))
                    {
                        offenders.Add($"{type.Name}.{property.Name}");
                    }
                }
            }
        }

        // A floor, for the same reason as the sweeps above: 29 module events plus the transport-level
        // ones today.
        Assert.True(eventPropertyCount >= 30, $"The observable-event sweep found only {eventPropertyCount} properties; the walk is broken.");
        Assert.True(offenders.Count == 0, $"Every ObservableEvent<T> property must be named On<Event>. Offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void TestReceivedTypesExposeNoMutableCollectionType()
    {
        // A received type hands the caller data the library deserialized. Exposing the concrete
        // List<>, Dictionary<,> or HashSet<> would let a caller mutate what the library parsed, and
        // would freeze the storage choice into the public API; the library projects them read-only
        // instead. The backing storage stays internal, so only public members are examined.
        HashSet<Type> mutableDefinitions = [typeof(List<>), typeof(Dictionary<,>), typeof(HashSet<>)];
        List<string> offenders = [];
        int receivedTypeCount = 0;
        foreach (Type type in typeof(CommandResult).Assembly.GetTypes())
        {
            if (!IsReceivedType(type))
            {
                continue;
            }

            receivedTypeCount++;
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (propertyType.IsGenericType && mutableDefinitions.Contains(propertyType.GetGenericTypeDefinition()))
                {
                    offenders.Add($"{type.FullName}.{property.Name} is declared as {propertyType.Name}");
                }
            }
        }

        // A floor, for the same reason as the sweeps above. There are 112 today.
        Assert.True(receivedTypeCount >= 105, $"The received-type sweep found only {receivedTypeCount} types; the walk is broken.");
        Assert.True(offenders.Count == 0, $"A received type must expose collections as read-only projections, not as List<>, Dictionary<,> or HashSet<>. Offenders:{Environment.NewLine}{string.Join(Environment.NewLine, offenders)}");
    }

    private static bool IsOnPrefixed(string propertyName)
    {
        return propertyName.Length > 2 && propertyName.StartsWith("On", StringComparison.Ordinal) && char.IsUpper(propertyName[2]);
    }

    private static bool HasPublicStaticResetMember(Type parametersType)
    {
        // Inherited statics count: a derived parameters type whose reset helper lives on its base (as
        // SetGeolocationOverrideCoordinatesCommandParameters' does) is still a resettable command.
        const BindingFlags MemberFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        bool onProperty = parametersType.GetProperties(MemberFlags).Any(member => member.Name.StartsWith("Reset", StringComparison.Ordinal));
        bool onField = parametersType.GetFields(MemberFlags).Any(member => member.Name.StartsWith("Reset", StringComparison.Ordinal));
        return onProperty || onField;
    }

    private static IEnumerable<(Module Module, MethodInfo Method, ParameterInfo Parameter)> GetModuleCommandMethods()
    {
        foreach (Module module in GetDriverModules())
        {
            foreach (MethodInfo method in module.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).OrderBy(method => method.Name, StringComparer.Ordinal))
            {
                // A module's command methods are exactly its public methods whose first parameter is the
                // command's parameters object; everything else it exposes is an observable event property.
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length > 0 && typeof(CommandParameters).IsAssignableFrom(parameters[0].ParameterType))
                {
                    yield return (module, method, parameters[0]);
                }
            }
        }
    }

    [Fact]
    public void TestJsonIncludeIsUsedOnlyWhereNecessary()
    {
        // Verifies that [JsonInclude] appears only where it changes what the serializer does.
        List<string> offenders = [];
        foreach (Type type in typeof(CommandParameters).Assembly.GetTypes())
        {
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.GetCustomAttribute<JsonIncludeAttribute>() is null)
                {
                    continue;
                }

                // A non-public member needs the attribute to be seen at all.
                MethodInfo? getter = property.GetMethod;
                MethodInfo? setter = property.SetMethod;
                bool isMemberNonPublic = (getter is null || !getter.IsPublic) && (setter is null || !setter.IsPublic);
                if (isMemberNonPublic)
                {
                    continue;
                }

                // A public member needs it only when one of its accessors is non-public. A get-only public
                // property has no setter to make accessible, so the attribute does nothing for it either.
                bool hasNonPublicAccessor = (getter is not null && !getter.IsPublic) || (setter is not null && !setter.IsPublic);
                if (!hasNonPublicAccessor)
                {
                    offenders.Add(Key(property));
                }
            }
        }

        Assert.True(offenders.Count == 0, $"[JsonInclude] is a no-op on a public member with public accessors and must be removed; keep it only on a non-public member or one with a non-public accessor. Offenders: {string.Join(", ", offenders)}");
    }

    private static string Key(PropertyInfo property)
    {
        return $"{property.DeclaringType!.FullName}.{property.Name}";
    }

    [Fact]
    public void TestReceivedTypesHaveNoPublicSetters()
    {
        // Collect all types received from the remote end, and validate
        // no properties have public setters.
        List<string> offenders = [];
        foreach (Type type in typeof(CommandParameters).Assembly.GetTypes())
        {
            if (!IsReceivedType(type))
            {
                continue;
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                MethodInfo? setter = property.SetMethod;
                if (setter is null || !setter.IsPublic)
                {
                    continue;
                }

                // An init accessor is a public setter as far as reflection is concerned, but it can only be
                // used in an object initializer, so it does not let a caller mutate an object it was handed.
                bool isInitOnly = setter.ReturnParameter.GetRequiredCustomModifiers()
                    .Any(modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit");
                if (isInitOnly)
                {
                    continue;
                }

                offenders.Add(Key(property));
            }
        }

        Assert.True(offenders.Count == 0, $"Types received from the remote end must not expose a public setter; use an internal or init accessor so the deserializer can populate the property without letting a caller mutate the object afterwards. Offenders: {string.Join(", ", offenders)}");
    }

    /// <summary>
    /// Determines whether a type is one the remote end sends to the consumer.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns><see langword="true"/> if the type is a command result or event arguments type that is subject to the immutability rule.</returns>
    private static bool IsReceivedType(Type type)
    {
        if (!typeof(CommandResult).IsAssignableFrom(type) && !typeof(WebDriverBiDiEventArgs).IsAssignableFrom(type))
        {
            return false;
        }

        // Transport-level event arguments are raised by the library itself rather than deserialized from the
        // remote end, and callers construct them; they are outside the rule this test enforces.
        return type.Namespace != "WebDriverBiDi.Protocol";
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
            // Inherited properties are swept deliberately. An ObservableEvent<T> declared on an
            // intermediate base class is still an event the module raises, and it must carry the
            // attribute like any other; restricting the sweep to declared properties would let it
            // escape. The hierarchy is flat today, so this widens nothing, but it stops the hole
            // from opening the moment a base module is introduced.
            Dictionary<string, PropertyInfo> mostDerivedByName = [];
            foreach (PropertyInfo property in module.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!property.PropertyType.IsGenericType || property.PropertyType.GetGenericTypeDefinition() != typeof(ObservableEvent<>))
                {
                    continue;
                }

                // Reflection reports a property shadowed by a "new" declaration once per declaring
                // type. Only the most derived one is the property the module actually exposes, so
                // keeping the others would report phantom offenders against hidden declarations.
                if (!mostDerivedByName.TryGetValue(property.Name, out PropertyInfo? existing) || existing.DeclaringType!.IsAssignableFrom(property.DeclaringType))
                {
                    mostDerivedByName[property.Name] = property;
                }
            }

            foreach (PropertyInfo property in mostDerivedByName.Values.OrderBy(eventProperty => eventProperty.Name, StringComparer.Ordinal))
            {
                yield return (module, property);
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

            // With either bound exclusive, a minimum equal to the maximum would describe an empty range.
            if ((attribute.MaximumExclusive || attribute.MinimumExclusive) && attribute.Minimum == attribute.Maximum)
            {
                offenders.Add($"{propertyName} (minimum {attribute.Minimum} equals the maximum {attribute.Maximum} with an exclusive bound, describing an empty range)");
            }

            // A reset sentinel is by definition a value outside the specification range; if it fell
            // inside the range it would be indistinguishable from an ordinary valid value. A value
            // equal to an exclusive bound is outside the range, so it is a legal sentinel.
            bool sentinelAboveLowerBound = attribute.MinimumExclusive ? attribute.SentinelValue > attribute.Minimum : attribute.SentinelValue >= attribute.Minimum;
            bool sentinelBelowUpperBound = attribute.MaximumExclusive ? attribute.SentinelValue < attribute.Maximum : attribute.SentinelValue <= attribute.Maximum;
            if (attribute.HasSentinel && sentinelAboveLowerBound && sentinelBelowUpperBound)
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
