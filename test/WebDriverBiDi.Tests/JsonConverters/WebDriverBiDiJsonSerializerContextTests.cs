namespace WebDriverBiDi.JsonConverters;

using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;
using WebDriverBiDi.Protocol;
using TestUtilities;

[TestFixture]
public class WebDriverBiDiJsonSerializerContextTests
{
    [Test]
    public void TestAllCommandParametersAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext
        HashSet<Type> registeredTypes = GetRegisteredSerializableTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;

        // Get a list of all the CommandParameters classes defined.
        // From all of the types in the assmebly, get all that are explicitly subclasses
        // of CommandParameters, are not abstract classes, and are in a namespace that
        // starts with "WebDriverBiDi.". If any type meeting this criteria are not in
        // the list of types registered with the custom JsonSerializerContext, add them
        // to the list of missing types.
        Type[] assemblyTypes = assembly.GetTypes();
        List<Type> missingTypes = [];
        foreach (Type assemblyType in assemblyTypes)
        {
            if (assemblyType.IsAssignableTo(typeof(CommandParameters)) && !assemblyType.IsAbstract &&
                assemblyType != typeof(CommandParameters) && IsLibraryNamespace(assemblyType) &&
                !registeredTypes.Contains(assemblyType))
            {
                missingTypes.Add(assemblyType);
            }
        }

        // There should be no missing types.
        Assert.That(missingTypes, Is.Empty, FormatMissingMessage("CommandParameters", missingTypes));
    }

    [Test]
    public void TestAllCommandResultsHaveResponseMessageInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> registeredTypes = GetRegisteredSerializableTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;
        Type openResponseType = typeof(CommandResponseMessage<>);

        // Get a list of all the CommandResponse classes defined.
        // From all of the types in the assmebly, get all that are explicitly subclasses
        // of CommandParameters, are not abstract classes, and are in a namespace that
        // starts with "WebDriverBiDi.". From that type, get the type of the class's 
        // CommandResponse type and add it to the list.
        List<Type> commandResultTypes = [];
        Type[] assemblyTypes = assembly.GetTypes();
        foreach (Type assemblyType in assemblyTypes)
        {
            if (assemblyType.IsAssignableTo(typeof(CommandParameters)) && !assemblyType.IsAbstract && IsLibraryNamespace(assemblyType))
            {
                Type? commandResultType = GetCommandResultTypeFromParameters(assemblyType);
                if (commandResultType is not null && !commandResultTypes.Contains(commandResultType))
                {
                    commandResultTypes.Add(commandResultType);
                }
            }
        }

        // Given that we now have a list of CommandResponse types, iterate through that
        // list, create a CommandResponseMessage<T>. Add any of the CommandResponseMessage<T>
        // types that are not in the list of types registered with the custom JsonSerializerContext,
        // to the list of missing types.
        List<Type> missingTypes = [];
        foreach (Type commandResultType in commandResultTypes)
        {
            Type commandResponseMessageType = openResponseType.MakeGenericType(commandResultType);
            if (!registeredTypes.Contains(commandResponseMessageType))
            {
                missingTypes.Add(commandResponseMessageType);
            }
        }

        // There should be no missing types.
        Assert.That(missingTypes, Is.Empty, FormatMissingMessage("CommandResponseMessage<T>", missingTypes));
    }

    [Test]
    public void TestAllEventMessagesAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> registeredTypes = GetRegisteredSerializableTypes();

        // Get a list of all the EventMessage classes defined.
        // Create an instance of BiDiDriver to register all of the modules and create
        // EventMessage<T> objects for all of the registered events. The list of these
        // types is stored in the private Values list in the eventMessageTypes field of
        // the Transport class. We introspect into the object using reflection to get
        // this list, as it's not, nor should it be, exposed to consumers of the
        // Transport class.
        TestConnection connection = new();
        Transport transport = new(connection);
        _ = new BiDiDriver(TimeSpan.FromSeconds(1), transport);

        FieldInfo? field = transport.GetType().GetField("eventMessageTypes", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null, "Could not find eventMessageTypes field on Transport");

        ConcurrentDictionary<string, Type> eventMessageTypes = (ConcurrentDictionary<string, Type>)field!.GetValue(transport)!;

        // Add any of the EventMessage<T> types that are not in the list of types registered with
        // the custom JsonSerializerContext to the list of missing types. 
        List<Type> missingTypes = [];
        foreach (Type eventMessageType in eventMessageTypes.Values)
        {
            if (!registeredTypes.Contains(eventMessageType) && !missingTypes.Contains(eventMessageType))
            {
                missingTypes.Add(eventMessageType);
            }
        }

        // There should be no missing types.
        Assert.That(missingTypes, Is.Empty, FormatMissingMessage("EventMessage<T>", missingTypes));
    }

    [Test]
    public void TestAllSerializableEnumsAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> registeredTypes = GetRegisteredSerializableTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;

        // Get a list of all the serializable enums defined.
        // From all of the types in the assmebly, get all that are enum types, are in
        // a namespace that starts with "WebDriverBiDi.", and have a [JsonConverter]
        // attribute. If any type meeting this criteria are not in the list of types
        // registered with the custom JsonSerializerContext, add them to the list of
        // missing types.
        List<Type> missingTypes = [];
        foreach(Type assemblyType in assembly.GetTypes())
        {
            if (assemblyType.IsEnum && IsLibraryNamespace(assemblyType) &&
                assemblyType.GetCustomAttribute<JsonConverterAttribute>() is not null &&
                !registeredTypes.Contains(assemblyType))
            {
                missingTypes.Add(assemblyType);
            }
        }

        // There should be no missing types.
        Assert.That(missingTypes, Is.Empty, FormatMissingMessage("Enum", missingTypes));
    }

    [Test]
    public void TestAllTypesWithCustomConvertersAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> coveredTypes = GetAllCoveredTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;

        // Get a list of all the types using a custom JsonConverter.
        // From all of the types in the assmebly, get all that are not enum types, are in
        // a namespace that starts with "WebDriverBiDi.", and have a [JsonConverter]
        // attribute. If any type meeting this criteria are not in the list of types
        // registered with the custom JsonSerializerContext, add them to the list of
        // missing types.
        List<Type> missingTypes = [];
        foreach (Type assemblyType in assembly.GetTypes())
        {
            if (!assemblyType.IsEnum && IsLibraryNamespace(assemblyType) &&
                assemblyType.GetCustomAttribute<JsonConverterAttribute>() is not null &&
                !coveredTypes.Contains(assemblyType))
            {
                missingTypes.Add(assemblyType);
            }
        }

        // There should be no missing types.
        Assert.That(missingTypes, Is.Empty, FormatMissingMessage("[JsonConverter]", missingTypes));
    }

    [Test]
    public void TestAllDerivedTypesAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> coveredTypes = GetAllCoveredTypes();
        Assembly assembly = typeof(BiDiDriver).Assembly;

        // Get a list of all the types that are polymorphically serializable.
        // From all of the types in the assmebly, get all that have a [JsonDerivedType]
        // attribute. If any type meeting this criteria are not in the list of types
        // registered with the custom JsonSerializerContext, add them to the list of
        // missing types.
        List<Type> missingTypes = [];
        foreach (Type assemblyType in assembly.GetTypes())
        {
            if (!IsLibraryNamespace(assemblyType))
            {
                continue;
            }

            foreach (JsonDerivedTypeAttribute derivedTypeAttribute in assemblyType.GetCustomAttributes<JsonDerivedTypeAttribute>())
            {
                if (!coveredTypes.Contains(derivedTypeAttribute.DerivedType) && !missingTypes.Contains(derivedTypeAttribute.DerivedType))
                {
                    missingTypes.Add(derivedTypeAttribute.DerivedType);
                }
            }
        }

        // There should be no missing types.
        Assert.That(missingTypes, Is.Empty, FormatMissingMessage("[JsonDerivedType]", missingTypes));
    }

    [Test]
    public void TestAllReferencedPropertyTypesAreIncludedInSerializationContext()
    {
        // Get all types registered with the custom JsonSerializerContext.
        HashSet<Type> coveredTypes = GetAllCoveredTypes();

        HashSet<Type> visited = [];
        Queue<Type> toProcess = new(coveredTypes);
        HashSet<Type> allReferencedLibraryTypes = [];

        // Get a list of all types referenced by properties.
        // Recursively perform a breadth-first search through all types registered
        // with the custom JsonSerializerContext. Examine all properties of those
        // classes, and add those types to the list. This list should also include
        // types that are generic parameters to generic types, like List<T>.
        while (toProcess.Count > 0)
        {
            Type current = toProcess.Dequeue();
            if (!visited.Add(current))
            {
                continue;
            }

            if (current.GetCustomAttribute<JsonConverterAttribute>() is not null)
            {
                continue;
            }

            foreach (PropertyInfo property in GetSerializableProperties(current))
            {
                foreach (Type extractedType in ExtractReferencedLibraryTypes(property.PropertyType))
                {
                    allReferencedLibraryTypes.Add(extractedType);
                    if (!visited.Contains(extractedType))
                    {
                        toProcess.Enqueue(extractedType);
                    }
                }
            }
        }

        // Given that we now have a list of all types that should be accessed by the custom
        // JsonSerializerContext, add any of those types that are not registered with the
        // custom context to the list of missing types.
        List<Type> missingTypes = [];
        foreach (Type referencedType in allReferencedLibraryTypes)
        {
            if (!coveredTypes.Contains(referencedType) && !referencedType.IsInterface)
            {
                missingTypes.Add(referencedType);
            }
        }

        // There should be no missing types.
        Assert.That(missingTypes, Is.Empty, FormatMissingMessage("PropertyType", missingTypes));
    }

    private static HashSet<Type> GetRegisteredSerializableTypes()
    {
        // return [.. typeof(WebDriverBiDiJsonSerializerContext)
        //         .GetCustomAttributesData()
        //         .Where(a => a.AttributeType == typeof(JsonSerializableAttribute))
        //         .Select(a => (Type)a.ConstructorArguments[0].Value!)];
        HashSet<Type> set = [];
        IList<CustomAttributeData> customAttributeDataObjects = typeof(WebDriverBiDiJsonSerializerContext).GetCustomAttributesData();
        foreach (CustomAttributeData customAttributeDataObject in customAttributeDataObjects)
        {
            if (customAttributeDataObject.AttributeType == typeof(JsonSerializableAttribute))
            {
                set.Add((Type)customAttributeDataObject.ConstructorArguments[0].Value!);
            }
        }

        return set;
    }

    private static HashSet<Type> GetAllCoveredTypes()
    {
        HashSet<Type> coveredTypes = [];
        IList<CustomAttributeData> customAttributeDataObjects = typeof(WebDriverBiDiJsonSerializerContext).GetCustomAttributesData();
        foreach (CustomAttributeData customAttributeDataObject in customAttributeDataObjects)
        {
            if (customAttributeDataObject.AttributeType == typeof(JsonSerializableAttribute))
            {
                Type registeredType = (Type)customAttributeDataObject.ConstructorArguments[0].Value!;
                coveredTypes.Add(registeredType);
                if (registeredType.IsGenericType)
                {
                    foreach (Type genericArgument in registeredType.GetGenericArguments())
                    {
                        coveredTypes.Add(genericArgument);
                    }
                }
            }
        }

        return coveredTypes;
    }

    private static bool IsLibraryNamespace(Type type)
    {
        return type.Namespace is not null && type.Namespace.StartsWith("WebDriverBiDi", StringComparison.Ordinal);
    }

    private static Type? GetCommandResultTypeFromParameters(Type commandParamsType)
    {
        Type? current = commandParamsType;
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(CommandParameters<>))
            {
                return current.GetGenericArguments()[0];
            }

            current = current.BaseType;
        }

        return null;
    }

    private static List<PropertyInfo> GetSerializableProperties(Type type)
    {
        List<PropertyInfo> result = [];
        HashSet<string> seen = [];
        Type? current = type;

        while (current is not null && current != typeof(object))
        {
            foreach (PropertyInfo property in current.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (!seen.Add(property.Name))
                {
                    continue;
                }

                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                if (property.GetCustomAttribute<JsonExtensionDataAttribute>() is not null)
                {
                    continue;
                }

                JsonIgnoreAttribute? jsonIgnore = property.GetCustomAttribute<JsonIgnoreAttribute>();
                if (jsonIgnore is not null && jsonIgnore.Condition == JsonIgnoreCondition.Always)
                {
                    continue;
                }

                bool hasJsonPropertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>() is not null;
                bool hasJsonInclude = property.GetCustomAttribute<JsonIncludeAttribute>() is not null;

                if (hasJsonPropertyName || hasJsonInclude)
                {
                    result.Add(property);
                }
            }

            current = current.BaseType;
        }

        return result;
    }

    private static List<Type> ExtractReferencedLibraryTypes(Type type)
    {
        List<Type> result = [];
        CollectLibraryTypes(type, result);
        return result;
    }

    private static void CollectLibraryTypes(Type type, List<Type> result)
    {
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying.IsGenericType)
        {
            foreach (Type arg in underlying.GetGenericArguments())
            {
                CollectLibraryTypes(arg, result);
            }

            return;
        }

        if (underlying.IsArray)
        {
            CollectLibraryTypes(underlying.GetElementType()!, result);
            return;
        }

        if (IsLibraryNamespace(underlying))
        {
            result.Add(underlying);
        }
    }

    private static string FormatMissingMessage(string category, List<Type> missingTypes)
    {
        return $"The following {category} types are missing [JsonSerializable] attributes "
               + $"in {nameof(WebDriverBiDiJsonSerializerContext)}:\n"
               + string.Join("\n", missingTypes.Select(t => $"  - {t.FullName}"));
    }
}
