namespace WebDriverBiDi.Storage;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;
using WebDriverBiDi.Network;

[TestFixture]
public class GetCookiesCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""cookies"": [ { ""name"": ""cookieName"", ""value"": { ""type"": ""string"", ""value"": ""cookieValue"" }, ""domain"": ""cookieDomain"", ""path"": ""cookiePath"", ""size"": 123, ""httpOnly"": false, ""secure"": true, ""sameSite"": ""lax"", ""expiry"": " + milliseconds + @" } ], ""partition"": { ""userContext"": ""myUserContext"", ""sourceOrigin"": ""mySourceOrigin"", ""extraPropertyName"": ""extraPropertyValue"" } }";
        GetCookiesCommandResult? result = JsonSerializer.Deserialize<GetCookiesCommandResult>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Cookies, Is.Not.Null);
            Assert.That(result.Cookies, Has.Count.EqualTo(1));
            Assert.That(result.Cookies[0].Name, Is.EqualTo("cookieName"));
            Assert.That(result.Cookies[0].Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(result.Cookies[0].Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(result.Cookies[0].Domain, Is.EqualTo("cookieDomain"));
            Assert.That(result.Cookies[0].Path, Is.EqualTo("cookiePath"));
            Assert.That(result.Cookies[0].Size, Is.EqualTo(123));
            Assert.That(result.Cookies[0].HttpOnly, Is.False);
            Assert.That(result.Cookies[0].Secure, Is.True);
            Assert.That(result.Cookies[0].SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(result.Cookies[0].Expires, Is.EqualTo(expireTime));
            Assert.That(result!.Partition, Is.Not.Null);
            Assert.That(result.Partition.UserContextId, Is.EqualTo("myUserContext"));
            Assert.That(result.Partition.SourceOrigin, Is.EqualTo("mySourceOrigin"));
            Assert.That(result.Partition.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(result.Partition.AdditionalData, Contains.Key("extraPropertyName"));
            Assert.That(result.Partition.AdditionalData["extraPropertyName"], Is.EqualTo("extraPropertyValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithNoCookieData()
    {
        string json = @"{ ""cookies"": [], ""partition"": {} }";
        GetCookiesCommandResult? result = JsonSerializer.Deserialize<GetCookiesCommandResult>(json, deserializationOptions);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Cookies, Is.Not.Null);
            Assert.That(result.Cookies, Is.Empty);
            Assert.That(result!.Partition, Is.Not.Null);
            Assert.That(result.Partition.UserContextId, Is.Null);
            Assert.That(result.Partition.SourceOrigin, Is.Null);
            Assert.That(result.Partition.AdditionalData, Is.Empty);
        });
    }

    [Test]
    public void TestDeserializingWithMissingCookiesThrows()
    {
        string json = @"{ ""partition"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<GetCookiesCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidCookiesDataTypeThrows()
    {
        string json = @"{ ""cookies"": ""invalidCookieArrayType"", ""partition"": {} }";
        Assert.That(() => JsonSerializer.Deserialize<GetCookiesCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingPartitionThrows()
    {
        string json = @"{ ""cookies"": [] }";
        Assert.That(() => JsonSerializer.Deserialize<GetCookiesCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidPartitionDataTypeThrows()
    {
        string json = @"{ ""cookies"": [], ""partition"": ""invalidPartitionType"" }";
        Assert.That(() => JsonSerializer.Deserialize<GetCookiesCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
