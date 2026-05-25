namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetGeolocationOverrideCoordinatesCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new();
        Assert.Equal("emulation.setGeolocationOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("coordinates"));
        JToken? coordinates = serialized["coordinates"];
        Assert.NotNull(coordinates);
        Assert.Equal(JTokenType.Null, coordinates.Type);
        Assert.Null(coordinates.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithCoordinates()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new()
        {
            Coordinates = new(123.45, -67.89),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("coordinates"));
        JToken? coordinatesToken = serialized["coordinates"];
        Assert.NotNull(coordinatesToken);
        Assert.Equal(JTokenType.Object, coordinatesToken.Type);
        JObject? coordinatesObject = coordinatesToken as JObject;
        Assert.NotNull(coordinatesObject);
        Assert.Equal(2, coordinatesObject.Count);

        Assert.True(coordinatesObject.ContainsKey("longitude"));
        JToken? longitude = coordinatesObject["longitude"];
        Assert.NotNull(longitude);
        Assert.Equal(JTokenType.Float, longitude.Type);
        Assert.Equal(123.45, longitude.Value<double>());

        Assert.True(coordinatesObject.ContainsKey("latitude"));
        JToken? latitude = coordinatesObject["latitude"];
        Assert.NotNull(latitude);
        Assert.Equal(JTokenType.Float, latitude.Type);
        Assert.Equal(-67.89, latitude.Value<double>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new()
        {
            Contexts =
            [
                "context1",
                "context2",
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("coordinates"));
        JToken? coordinates = serialized["coordinates"];
        Assert.NotNull(coordinates);
        Assert.Equal(JTokenType.Null, coordinates.Type);
        Assert.Null(coordinates.Value<JObject?>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        Assert.Equal(JTokenType.Array, contextsToken.Type);
        JArray? contextsArray = contextsToken.Value<JArray>();
        Assert.NotNull(contextsArray);
        Assert.Equal(2, contextsArray.Count);
        Assert.Equal(JTokenType.String, contextsArray[0].Type);
        Assert.Equal("context1", contextsArray[0].Value<string>());
        Assert.Equal(JTokenType.String, contextsArray[1].Type);
        Assert.Equal("context2", contextsArray[1].Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithUserContexts()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new()
        {
            UserContexts =
            [
                "userContext1",
                "userContext2",
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("coordinates"));
        JToken? coordinates = serialized["coordinates"];
        Assert.NotNull(coordinates);
        Assert.Equal(JTokenType.Null, coordinates.Type);
        Assert.Null(coordinates.Value<JObject?>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContextsToken = serialized["userContexts"];
        Assert.NotNull(userContextsToken);
        Assert.Equal(JTokenType.Array, userContextsToken.Type);
        JArray? userContextsArray = userContextsToken.Value<JArray>();
        Assert.NotNull(userContextsArray);
        Assert.Equal(2, userContextsArray.Count);
        Assert.Equal(JTokenType.String, userContextsArray[0].Type);
        Assert.Equal("userContext1", userContextsArray[0].Value<string>());
        Assert.Equal(JTokenType.String, userContextsArray[1].Type);
        Assert.Equal("userContext2", userContextsArray[1].Value<string>());
    }
}
