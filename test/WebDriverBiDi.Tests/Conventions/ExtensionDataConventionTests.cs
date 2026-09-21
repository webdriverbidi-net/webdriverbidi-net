namespace WebDriverBiDi.Conventions;

using System.Reflection;
using System.Text.Json.Serialization;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

/// <summary>
/// Enforces where extension data is captured: the transport captures it generically at the root of every
/// command result and event payload, so root payload types must not declare their own
/// <see cref="JsonExtensionDataAttribute"/> member (nested <c>Extensible</c> productions such as
/// <c>Cookie</c> or <c>RequestData</c> do declare one).
/// </summary>
public class ExtensionDataConventionTests
{
    [Fact]
    public void TestCommandResultTypesDoNotDeclareExtensionData()
    {
        List<string> offenders = [];
        int resultCount = 0;
        foreach (Type type in typeof(CommandResult).Assembly.GetTypes())
        {
            if (!typeof(CommandResult).IsAssignableFrom(type))
            {
                continue;
            }

            resultCount++;
            if (DeclaresExtensionData(type))
            {
                offenders.Add(type.FullName ?? type.Name);
            }
        }

        // A floor rather than an inventory, so adding a command does not break it. It fails if the walk
        // stops finding result types, which would otherwise let the check above pass by sweeping nothing
        // at all. There are 87 today.
        Assert.True(resultCount >= 70, $"The command result sweep found only {resultCount} result types; the walk is broken.");
        Assert.True(offenders.Count == 0, $"Command result types are payload roots; the transport captures their extension data. Remove [JsonExtensionData] from: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void TestRegisteredEventPayloadTypesDoNotDeclareExtensionData()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        _ = new BiDiDriver(TimeSpan.FromSeconds(1), transport);

        FieldInfo? field = transport.GetType().GetField("eventMessageTypes", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        System.Collections.IDictionary? registry = field.GetValue(transport) as System.Collections.IDictionary;
        Assert.NotNull(registry);

        // A floor rather than an inventory, so registering another event does not break it. It fails if
        // the registry walk reaches nothing — a renamed field, or a driver that registers no events —
        // which would otherwise let the check below pass vacuously. The modules register 29 today.
        Assert.True(registry.Count >= 25, $"The event payload sweep found only {registry.Count} registered events; the walk is broken.");

        List<string> offenders = [];
        foreach (object? registration in registry.Values)
        {
            Type? messageType = registration?.GetType().GetProperty("EventMessageType")?.GetValue(registration) as Type;
            Assert.NotNull(messageType);
            Type payloadType = messageType.GetGenericArguments()[0];
            if (DeclaresExtensionData(payloadType))
            {
                offenders.Add(payloadType.FullName ?? payloadType.Name);
            }
        }

        Assert.True(offenders.Count == 0, $"Event payload types are payload roots; the transport captures their extension data. Remove [JsonExtensionData] from: {string.Join(", ", offenders)}");
    }

    private static bool DeclaresExtensionData(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        // Walk the full type hierarchy so an extension-data member declared on a base payload type — for
        // example BaseNetworkEventArgs or NavigationEventArgs, from which the concrete event payloads
        // derive — is also caught, and scan both properties and fields (the attribute is valid on either).
        for (Type? current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            bool onProperty = current.GetProperties(flags)
                .Any(member => member.GetCustomAttribute<JsonExtensionDataAttribute>() is not null);
            bool onField = current.GetFields(flags)
                .Any(member => member.GetCustomAttribute<JsonExtensionDataAttribute>() is not null);
            if (onProperty || onField)
            {
                return true;
            }
        }

        return false;
    }
}
