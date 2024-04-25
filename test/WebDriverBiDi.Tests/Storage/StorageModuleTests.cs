namespace WebDriverBiDi.Storage;

using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

[TestFixture]
public class StorageModuleTests()
{
    [Test]
    public async Task TestGetCookiesCommand()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": { ""cookies"": [ { ""name"": ""cookieName"", ""value"": { ""type"": ""string"", ""value"": ""cookieValue"" }, ""domain"": ""cookieDomain"", ""path"": ""cookiePath"", ""size"": 123, ""httpOnly"": false, ""secure"": true, ""sameSite"": ""lax"", ""expiry"": " + milliseconds + @" } ], ""partition"": { ""userContext"": ""myUserContext"", ""sourceOrigin"": ""mySourceOrigin"" } } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        StorageModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        var task = module.GetCookiesAsync(new GetCookiesCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Cookies, Has.Count.EqualTo(1));
            Assert.That(result.Cookies[0].Name, Is.EqualTo("cookieName"));
            Assert.That(result.Cookies[0].Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(result.Cookies[0].Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(result.Cookies[0].Domain, Is.EqualTo("cookieDomain"));
            Assert.That(result.Cookies[0].Path, Is.EqualTo("cookiePath"));
            Assert.That(result.Cookies[0].Size, Is.EqualTo(123));
            Assert.That(result.Cookies[0].Secure, Is.True);
            Assert.That(result.Cookies[0].HttpOnly, Is.False);
            Assert.That(result.Cookies[0].SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(result.Cookies[0].Expires, Is.EqualTo(expireTime));
            Assert.That(result.Partition.UserContextId, Is.EqualTo("myUserContext"));
            Assert.That(result.Partition.SourceOrigin, Is.EqualTo("mySourceOrigin"));
        });
    }

    [Test]
    public async Task TestSetCookieCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": { ""partition"": { ""userContext"": ""myUserContext"", ""sourceOrigin"": ""mySourceOrigin"" } } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        StorageModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        var task = module.SetCookieAsync(new SetCookieCommandParameters(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain")));
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Partition.UserContextId, Is.EqualTo("myUserContext"));
            Assert.That(result!.Partition.SourceOrigin, Is.EqualTo("mySourceOrigin"));
        });
    }

    [Test]
    public async Task TestDeleteCookiesCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            string responseJson = @"{ ""type"": ""success"", ""id"": " + e.SentCommandId + @", ""result"": { ""partition"": { ""userContext"": ""myUserContext"", ""sourceOrigin"": ""mySourceOrigin"" } } }";
            connection.RaiseDataReceivedEvent(responseJson);
        };

        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), new Transport(connection));
        StorageModule module = new(driver);
        await driver.StartAsync("ws:localhost");

        var task = module.DeleteCookiesAsync(new DeleteCookiesCommandParameters());
        task.Wait(TimeSpan.FromSeconds(1));
        var result = task.Result;

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Partition.UserContextId, Is.EqualTo("myUserContext"));
            Assert.That(result!.Partition.SourceOrigin, Is.EqualTo("mySourceOrigin"));
        });
    }
}
