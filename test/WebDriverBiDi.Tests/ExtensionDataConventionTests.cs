namespace WebDriverBiDi;

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
        foreach (Type type in typeof(CommandResult).Assembly.GetTypes())
        {
            if (typeof(CommandResult).IsAssignableFrom(type) && DeclaresExtensionData(type))
            {
                offenders.Add(type.FullName ?? type.Name);
            }
        }

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
        return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Any(property => property.GetCustomAttribute<JsonExtensionDataAttribute>() is not null);
    }
}
