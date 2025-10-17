namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SimulateCharacteristicResponseCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Read, 0);
        Assert.That(properties.MethodName, Is.EqualTo("bluetooth.simulateCharacteristicResponse"));
    }

    [Test]
    public void TestCanSerializeParametersForCharacteristicReadResponse()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Read, 0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("read"));
            Assert.That(serialized, Contains.Key("code"));
            Assert.That(serialized["code"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["code"]!.Value<uint>(), Is.Zero);
       });
    }

    [Test]
    public void TestCanSerializeParametersForCharacteristicWriteResponse()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Write, 0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("write"));
            Assert.That(serialized, Contains.Key("code"));
            Assert.That(serialized["code"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["code"]!.Value<uint>(), Is.Zero);
       });
    }

    [Test]
    public void TestCanSerializeParametersForCharacteristicSubscribeResponse()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.SubscribeToNotifications, 0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("subscribe-to-notifications"));
            Assert.That(serialized, Contains.Key("code"));
            Assert.That(serialized["code"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["code"]!.Value<uint>(), Is.Zero);
       });
    }

    [Test]
    public void TestCanSerializeParametersForCharacteristicUnsubscribeResponse()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.UnsubscribeFromNotifications, 0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("unsubscribe-from-notifications"));
            Assert.That(serialized, Contains.Key("code"));
            Assert.That(serialized["code"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["code"]!.Value<uint>(), Is.Zero);
       });
    }

    [Test]
    public void TestCanSerializeParametersForCharacteristicResponseWithEmptyData()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Read, 0)
        {
            Data = [],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(7));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("read"));
            Assert.That(serialized, Contains.Key("code"));
            Assert.That(serialized["code"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["code"]!.Value<uint>(), Is.Zero);
            Assert.That(serialized, Contains.Key("data"));
            Assert.That(serialized["data"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? dataArray = serialized["data"] as JArray;
            Assert.That(dataArray, Is.Empty);
       });
    }

    [Test]
    public void TestCanSerializeParametersForCharacteristicResponseWithData()
    {
        SimulateCharacteristicResponseCommandParameters properties = new("myContext", "myAddress", "myServiceUuid", "myCharacteristicUuid", SimulateCharacteristicResponseType.Read, 0)
        {
            Data = [123, 456],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(7));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("address"));
            Assert.That(serialized["address"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["address"]!.Value<string>(), Is.EqualTo("myAddress"));
            Assert.That(serialized, Contains.Key("serviceUuid"));
            Assert.That(serialized["serviceUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["serviceUuid"]!.Value<string>(), Is.EqualTo("myServiceUuid"));
            Assert.That(serialized, Contains.Key("characteristicUuid"));
            Assert.That(serialized["characteristicUuid"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["characteristicUuid"]!.Value<string>(), Is.EqualTo("myCharacteristicUuid"));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("read"));
            Assert.That(serialized, Contains.Key("code"));
            Assert.That(serialized["code"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["code"]!.Value<uint>(), Is.Zero);
            Assert.That(serialized, Contains.Key("data"));
            Assert.That(serialized["data"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? dataArray = serialized["data"] as JArray;
            Assert.That(dataArray, Has.Count.EqualTo(2));
            Assert.That(dataArray![0].Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(dataArray[0].Value<uint>(), Is.EqualTo(123));
            Assert.That(dataArray![1].Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(dataArray[1].Value<uint>(), Is.EqualTo(456));
       });
    }
}
