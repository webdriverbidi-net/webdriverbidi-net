namespace WebDriverBiDi.TestUtilities;

using Newtonsoft.Json.Linq;

public static class JsonExtensions
{
    public static Dictionary<string, object?> ToParsedDictionary(this JToken token)
    {
        return ParseToken(token) as Dictionary<string, object?> ?? [];
    }

    private static object? ParseToken(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Array:
                List<object?> listObject = [];
                foreach (JToken arrayElement in ((JArray)token).Values())
                {
                    listObject.Add(ParseToken(arrayElement));
                }

                return listObject;

            case JTokenType.Object:
                Dictionary<string, object?> dictObject = [];
                foreach(JProperty property in ((JObject)token).Properties())
                {
                    dictObject[property.Name] = ParseToken(property.Value);
                }
                return dictObject;
            
            case JTokenType.Boolean:
                return token.ToObject<bool>();

            case JTokenType.Integer:
                return token.ToObject<int>();

            case JTokenType.Float:
                return token.ToObject<double>();

            case JTokenType.String:
                return token.ToString();
        }

        return null;
    }
}
