namespace WebDriverBiDi.Conventions;

using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Enforces that every recursive path through the library's serialized types tests the remaining stack at
/// each level.
/// </summary>
/// <remarks>
/// <para>
/// A protocol value such as a <c>script.RemoteValue</c> can nest inside itself as deeply as the transport's
/// maximum JSON depth allows, and reading it recurses once per level. The library tests the remaining stack
/// at each level, in <c>JsonConverterUtilities.ReadNestedValue</c>, so that a value too deep for the current
/// thread fails with a catchable exception, which the transport answers by reading the message again on a
/// thread with a larger stack. A recursive path the serializer follows by itself, through plain members,
/// performs no such test, and a value nested deeply enough along it overflows the stack and terminates the
/// process.
/// </para>
/// <para>
/// Every level of a recursive path must therefore pass through a converter that reads through
/// <c>ReadNestedValue</c>. This test builds the graph of the types the serializer reads through members,
/// collection elements and union derived types, removes the edges that pass through such a converter, and
/// requires what remains to have no cycle. A new recursive member that fails it needs
/// <see cref="NestedValueJsonConverter{T}"/>, or a collection converter that reads through
/// <c>ReadNestedValue</c>.
/// </para>
/// </remarks>
public class RecursionConventionTests
{
    // Every converter here reads the values it contains through JsonConverterUtilities.ReadNestedValue, which
    // tests the remaining stack.
    private static readonly HashSet<Type> StackTestingConverters =
    [
        typeof(DiscriminatedUnionJsonConverter<>),
        typeof(NestedValueJsonConverter<>),
        typeof(NonNullElementListJsonConverter<>),
        typeof(NonNullValueDictionaryJsonConverter<>),
        typeof(RemoteValueDictionaryJsonConverter),
        typeof(RemoteValueListJsonConverter),
    ];

    [Fact]
    public void TestEveryRecursivePathTestsTheStackAtEachLevel()
    {
        Dictionary<Type, List<Edge>> graph = BuildGraph();

        // Guard against a walk that finds nothing to check: the recursive paths the protocol is known to have
        // must be found on a cycle. Whether each one tests the stack is what the assertion below decides.
        List<Edge> edgesOnCycles = [.. graph.Values.SelectMany(edges => edges).Where(edge => Reaches(graph, edge.To, edge.From, includeTestedEdges: true))];
        Assert.Contains(edgesOnCycles, edge => edge.Via == "NodeProperties.ShadowRoot");
        Assert.Contains(edgesOnCycles, edge => edge.Via == "NodeProperties.SerializableChildren");
        Assert.Contains(edgesOnCycles, edge => edge.Via == "BrowsingContextInfo.SerializableChildren");

        List<string> untestedCycles = [];
        foreach (Edge edge in graph.Values.SelectMany(edges => edges).Where(edge => !edge.TestsStack))
        {
            if (Reaches(graph, edge.To, edge.From, includeTestedEdges: false))
            {
                untestedCycles.Add($"{edge.Via} ({edge.From.Name} -> {edge.To.Name})");
            }
        }

        Assert.True(untestedCycles.Count == 0, $"These members lie on a recursive path that never tests the remaining stack, so a deeply nested value can overflow it; apply NestedValueJsonConverter<T> to one member on each such path:{Environment.NewLine}{string.Join(Environment.NewLine, untestedCycles)}");
    }

    private static Dictionary<Type, List<Edge>> BuildGraph()
    {
        Assembly assembly = typeof(BiDiDriver).Assembly;
        Dictionary<Type, List<Edge>> graph = [];
        foreach (Type type in assembly.GetTypes().Where(type => type.IsClass && !type.IsGenericTypeDefinition))
        {
            List<Edge> edges = [];
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!IsSerialized(property))
                {
                    continue;
                }

                Type? memberConverter = property.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType;
                foreach (Type reached in GetReachableTypes(property.PropertyType, assembly))
                {
                    Type? converter = memberConverter ?? reached.GetCustomAttribute<JsonConverterAttribute>(false)?.ConverterType;
                    edges.Add(new Edge(type, reached, $"{property.DeclaringType!.Name}.{property.Name}", TestsStack(converter)));
                }
            }

            // A union reads each derived type through the union's converter.
            foreach (DiscriminatedDerivedTypeAttribute derivedType in type.GetCustomAttributes<DiscriminatedDerivedTypeAttribute>(false))
            {
                edges.Add(new Edge(type, derivedType.DerivedType, $"{type.Name} (derived type {derivedType.DerivedType.Name})", TestsStack(type.GetCustomAttribute<JsonConverterAttribute>(false)?.ConverterType)));
            }

            // Polymorphism the serializer resolves by itself performs no test.
            foreach (JsonDerivedTypeAttribute derivedType in type.GetCustomAttributes<JsonDerivedTypeAttribute>(false))
            {
                edges.Add(new Edge(type, derivedType.DerivedType, $"{type.Name} (derived type {derivedType.DerivedType.Name})", false));
            }

            graph[type] = edges;
        }

        return graph;
    }

    private static bool TestsStack(Type? converterType)
    {
        if (converterType is null)
        {
            return false;
        }

        Type definition = converterType.IsGenericType ? converterType.GetGenericTypeDefinition() : converterType;
        return StackTestingConverters.Contains(definition);
    }

    private static bool Reaches(Dictionary<Type, List<Edge>> graph, Type from, Type target, bool includeTestedEdges)
    {
        HashSet<Type> visited = [];
        Stack<Type> pending = new([from]);
        while (pending.Count > 0)
        {
            Type current = pending.Pop();
            if (current == target)
            {
                return true;
            }

            if (!visited.Add(current) || !graph.TryGetValue(current, out List<Edge>? edges))
            {
                continue;
            }

            foreach (Edge edge in edges.Where(edge => includeTestedEdges || !edge.TestsStack))
            {
                pending.Push(edge.To);
            }
        }

        return false;
    }

    private static bool IsSerialized(PropertyInfo property)
    {
        if (property.GetIndexParameters().Length > 0 || property.GetCustomAttribute<JsonIgnoreAttribute>() is { Condition: JsonIgnoreCondition.Always })
        {
            return false;
        }

        return property.GetMethod is { IsPublic: true } || property.GetCustomAttribute<JsonIncludeAttribute>() is not null;
    }

    private static IEnumerable<Type> GetReachableTypes(Type type, Assembly assembly)
    {
        Type candidate = Nullable.GetUnderlyingType(type) ?? type;
        if (candidate.IsArray)
        {
            candidate = candidate.GetElementType()!;
        }
        else if (candidate.IsGenericType && typeof(IEnumerable).IsAssignableFrom(candidate))
        {
            foreach (Type argument in candidate.GetGenericArguments())
            {
                foreach (Type reached in GetReachableTypes(argument, assembly))
                {
                    yield return reached;
                }
            }

            yield break;
        }

        if (candidate.Assembly == assembly && candidate.IsClass)
        {
            yield return candidate;
        }
    }

    private sealed record Edge(Type From, Type To, string Via, bool TestsStack);
}
