namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

[TestFixture]
public class AddInterceptCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""intercept"": ""myInterceptId"" }";
        AddInterceptCommandResult? result = JsonConvert.DeserializeObject<AddInterceptCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.InterceptId, Is.EqualTo("myInterceptId"));
    }

    [Test]
    public void TestDeserializingWithMissingDataThrows()
    {
        string json = @"{}";
        Assert.That(() => JsonConvert.DeserializeObject<AddInterceptCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDataTypeThrows()
    {
        string json = @"{ ""intercept"": {} }";
        Assert.That(() => JsonConvert.DeserializeObject<AddInterceptCommandResult>(json), Throws.InstanceOf<JsonReaderException>());
    }
}