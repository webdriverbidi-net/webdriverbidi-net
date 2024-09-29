// <copyright file="ChromiumTransport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using System.Text.Json;
using System.Text.Json.Serialization;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// A Transport class for driving Chromium-based browsers without use of an external driver binary.
/// </summary>
public class ChromiumTransport : Transport
{
    private string sessionId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromiumTransport"/> class.
    /// </summary>
    public ChromiumTransport()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromiumTransport"/> class.
    /// </summary>
    /// <param name="connection">The connection used to communicate with the protocol remote end.</param>
    public ChromiumTransport(Connection connection)
        : base(connection)
    {
    }

    /// <summary>
    /// Asynchronously connects to the remote end web socket.
    /// </summary>
    /// <param name="websocketUri">The URI used to connect to the web socket.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task ConnectAsync(string websocketUri)
    {
        await this.ConnectToBiDi(websocketUri);
        await base.ConnectAsync(websocketUri);
    }

    /// <summary>
    /// Serializes a command for transmission across the WebSocket connection.
    /// </summary>
    /// <param name="command">The command to serialize.</param>
    /// <returns>The serialized JSON string representing the command.</returns>
    protected override byte[] SerializeCommand(Command command)
    {
        // Calling base.SerializeCommand yields the Command object serialized
        // as a string. This string must be passed as an argument to a function,
        // so we call JsonSerializer.Serialize again to enclose the serialized
        // Command object in quotes, and properly escape any embedded quotes in
        // the properties of the Command object.
        string serializedCommand = JsonSerializer.Serialize(base.SerializeCommand(command));
        DevToolsProtocolCommand wrapperCommand = new(this.GetNextCommandId(), "Runtime.evaluate");
        wrapperCommand.Parameters["expression"] = @$"window.onBidiMessage({serializedCommand})";
        wrapperCommand.SessionId = this.sessionId;
        return JsonSerializer.SerializeToUtf8Bytes(wrapperCommand);
    }

    /// <summary>
    /// Deserializes an incoming message from the WebSocket connection.
    /// </summary>
    /// <param name="messageData">The message data to deserialize.</param>
    /// <returns>A JsonElement representing the parsed message.</returns>
    /// <exception cref="JsonException">
    /// Thrown when there is a syntax error in the incoming JSON.
    /// </exception>
    protected override JsonElement DeserializeMessage(byte[] messageData)
    {
        // Incoming BiDi messages are received by listening to the Runtime.bindingCalled
        // event, where the binding name is "sendBidiResponse". The BiDi message is
        // the "payload" property of that event.
        JsonElement deserialized = base.DeserializeMessage(messageData);
        if (deserialized.TryGetProperty("method", out JsonElement methodNameElement))
        {
            string? methodName = methodNameElement.GetString();
            if (methodName is not null && methodName == "Runtime.bindingCalled" && deserialized.TryGetProperty("params", out JsonElement valueElement))
            {
                JsonElement bindingNameElement = valueElement.GetProperty("name");
                string? bindingName = bindingNameElement.GetString();
                if (bindingName is not null && bindingName == "sendBidiResponse")
                {
                    JsonElement payloadElement = valueElement.GetProperty("payload");
                    string? payload = payloadElement.GetString();
                    if (payload is not null)
                    {
                        return JsonDocument.Parse(payload).RootElement;
                    }
                }
            }
        }

        return default;
    }

    private async Task ConnectToBiDi(string websocketUri)
    {
        ManualResetEventSlim syncEvent = new(false);
        JsonDocument? document = null;
        string targetId = string.Empty;
        EventObserver<ConnectionDataReceivedEventArgs> observer = this.Connection.OnDataReceived.AddObserver((e) =>
        {
            document = JsonDocument.Parse(e.Data);
            Console.WriteLine(e.Data);
            if (!document.RootElement.TryGetProperty("id", out _))
            {
                // Only return data from command responses; ignore events.
                return;
            }

            syncEvent.Set();
        });

        DevToolsProtocolCommand command = new(this.GetNextCommandId(), "Target.getTargets");
        await this.Connection.StartAsync(websocketUri);
        await this.Connection.SendDataAsync(JsonSerializer.SerializeToUtf8Bytes(command));
        syncEvent.Wait(TimeSpan.FromSeconds(3));
        syncEvent.Reset();
        if (document is not null)
        {
            targetId = document.RootElement.GetProperty("result").GetProperty("targetInfos")[0].GetProperty("targetId").GetString()!;
            Console.WriteLine($"Found target id {targetId}");
        }

        if (!string.IsNullOrEmpty(targetId))
        {
            command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Target.attachToTarget");
            command.Parameters["targetId"] = targetId;
            command.Parameters["flatten"] = true;
            await this.Connection.SendDataAsync(JsonSerializer.SerializeToUtf8Bytes(command));
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();

            JsonElement result = document!.RootElement.GetProperty("result");
            this.sessionId = result.GetProperty("sessionId").GetString()!;

            command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Target.createTarget");
            command.Parameters["url"] = "about:blank";
            await this.Connection.SendDataAsync(JsonSerializer.SerializeToUtf8Bytes(command));
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();

            command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.addBinding");
            command.Parameters["name"] = "sendBidiResponse";
            command.SessionId = this.sessionId;
            await this.Connection.SendDataAsync(JsonSerializer.SerializeToUtf8Bytes(command));
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();
        }

        observer.Unobserve();
    }

    private class DevToolsProtocolCommand
    {
        private readonly long id;
        private readonly string method;
        private Dictionary<string, object> parameters = new();
        private string? sessionId = null;

        public DevToolsProtocolCommand(long id, string method)
        {
            this.id = id;
            this.method = method;
        }

        [JsonPropertyName("id")]
        public long Id => this.id;

        [JsonPropertyName("method")]
        public string Method => this.method;

        [JsonPropertyName("params")]
        public Dictionary<string, object> Parameters => this.parameters;

        [JsonPropertyName("sessionId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SessionId { get => this.sessionId; set => this.sessionId = value; }
    }
}
