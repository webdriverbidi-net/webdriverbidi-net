namespace WebDriverBiDi.Permissions;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetPermissionsCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Granted, "https://example.com");
        Assert.Equal("permissions.setPermission", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Granted, "https://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("descriptor"));
        JToken? descriptorToken = serialized["descriptor"];
        Assert.NotNull(descriptorToken);
        Assert.Equal(JTokenType.Object, descriptorToken.Type);
        JObject? descriptor = descriptorToken as JObject;
        Assert.NotNull(descriptor);
        Assert.Single(descriptor);
        Assert.True(descriptor.ContainsKey("name"));
        JToken? descriptorName = descriptor["name"];
        Assert.NotNull(descriptorName);
        Assert.Equal(JTokenType.String, descriptorName.Type);
        Assert.Equal("myPermission", descriptorName.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("granted", state.Value<string>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("https://example.com", origin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDescriptorConstructor()
    {
        SetPermissionCommandParameters properties = new(new PermissionDescriptor("myPermission"), PermissionState.Granted, "https://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("descriptor"));
        JToken? descriptorToken = serialized["descriptor"];
        Assert.NotNull(descriptorToken);
        Assert.Equal(JTokenType.Object, descriptorToken.Type);
        JObject? descriptor = descriptorToken as JObject;
        Assert.NotNull(descriptor);
        Assert.Single(descriptor);
        Assert.True(descriptor.ContainsKey("name"));
        JToken? descriptorName = descriptor["name"];
        Assert.NotNull(descriptorName);
        Assert.Equal(JTokenType.String, descriptorName.Type);
        Assert.Equal("myPermission", descriptorName.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("granted", state.Value<string>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("https://example.com", origin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithEmbeddedOrigin()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Granted, "https://example.com")
        {
            EmbeddedOrigin = "myEmbeddedOrigin"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("descriptor"));
        JToken? descriptorToken = serialized["descriptor"];
        Assert.NotNull(descriptorToken);
        Assert.Equal(JTokenType.Object, descriptorToken.Type);
        JObject? descriptor = descriptorToken as JObject;
        Assert.NotNull(descriptor);
        Assert.Single(descriptor);
        Assert.True(descriptor.ContainsKey("name"));
        JToken? descriptorName = descriptor["name"];
        Assert.NotNull(descriptorName);
        Assert.Equal(JTokenType.String, descriptorName.Type);
        Assert.Equal("myPermission", descriptorName.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("granted", state.Value<string>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("https://example.com", origin.Value<string>());

        Assert.True(serialized.ContainsKey("embeddedOrigin"));
        JToken? embeddedOrigin = serialized["embeddedOrigin"];
        Assert.NotNull(embeddedOrigin);
        Assert.Equal(JTokenType.String, embeddedOrigin.Type);
        Assert.Equal("myEmbeddedOrigin", embeddedOrigin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithUserContext()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Granted, "https://example.com")
        {
            UserContextId = "myContext"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("descriptor"));
        JToken? descriptorToken = serialized["descriptor"];
        Assert.NotNull(descriptorToken);
        Assert.Equal(JTokenType.Object, descriptorToken.Type);
        JObject? descriptor = descriptorToken as JObject;
        Assert.NotNull(descriptor);
        Assert.Single(descriptor);
        Assert.True(descriptor.ContainsKey("name"));
        JToken? descriptorName = descriptor["name"];
        Assert.NotNull(descriptorName);
        Assert.Equal(JTokenType.String, descriptorName.Type);
        Assert.Equal("myPermission", descriptorName.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("granted", state.Value<string>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("https://example.com", origin.Value<string>());

        Assert.True(serialized.ContainsKey("userContext"));
        JToken? userContext = serialized["userContext"];
        Assert.NotNull(userContext);
        Assert.Equal(JTokenType.String, userContext.Type);
        Assert.Equal("myContext", userContext.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithPermissionDenied()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Denied, "https://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("descriptor"));
        JToken? descriptorToken = serialized["descriptor"];
        Assert.NotNull(descriptorToken);
        Assert.Equal(JTokenType.Object, descriptorToken.Type);
        JObject? descriptor = descriptorToken as JObject;
        Assert.NotNull(descriptor);
        Assert.Single(descriptor);
        Assert.True(descriptor.ContainsKey("name"));
        JToken? descriptorName = descriptor["name"];
        Assert.NotNull(descriptorName);
        Assert.Equal(JTokenType.String, descriptorName.Type);
        Assert.Equal("myPermission", descriptorName.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("denied", state.Value<string>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("https://example.com", origin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithPermissionPrompt()
    {
        SetPermissionCommandParameters properties = new("myPermission", PermissionState.Prompt, "https://example.com");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("descriptor"));
        JToken? descriptorToken = serialized["descriptor"];
        Assert.NotNull(descriptorToken);
        Assert.Equal(JTokenType.Object, descriptorToken.Type);
        JObject? descriptor = descriptorToken as JObject;
        Assert.NotNull(descriptor);
        Assert.Single(descriptor);
        Assert.True(descriptor.ContainsKey("name"));
        JToken? descriptorName = descriptor["name"];
        Assert.NotNull(descriptorName);
        Assert.Equal(JTokenType.String, descriptorName.Type);
        Assert.Equal("myPermission", descriptorName.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("prompt", state.Value<string>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("https://example.com", origin.Value<string>());
    }
}
