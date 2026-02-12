// <copyright file="ChromiumTransport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WebDriverBiDi;
using WebDriverBiDi.Protocol;

/// <summary>
/// A Transport class for driving Chromium-based browsers without use of an external driver binary.
/// </summary>
public class ChromiumTransport : Transport
{
    private string sessionId = string.Empty;
    private string mapperTabTargetId = string.Empty;

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
        await base.ConnectAsync(websocketUri);
        await this.InitializeBiDi();
    }

    /// <summary>
    /// Serializes a command for transmission across the WebSocket connection.
    /// </summary>
    /// <param name="command">The command to serialize.</param>
    /// <returns>The serialized JSON string representing the command.</returns>
    protected override byte[] SerializeCommand(Command command)
    {
        // Calling base.SerializeCommand yields the Command object serialized
        // as a byte array. The string represented by this byte array must be
        // passed as an argument to a function, so we decode the byte array
        // into a string and call JsonSerializer.Serialize again to enclose
        // the serialized Command object in quotes, and properly escape any
        // embedded quotes in the properties of the Command object. Note
        // carefully that this does double-convert from byte array to string
        // and back, and that is intentional given the usage here.
        byte[] commandBytes = base.SerializeCommand(command);
        string serializedCommand = JsonEncodeString(Encoding.UTF8.GetString(commandBytes));
        DevToolsProtocolCommand wrapperCommand = new(this.GetNextCommandId(), "Runtime.evaluate");
        wrapperCommand.Parameters["expression"] = @$"window.onBidiMessage({serializedCommand})";
        wrapperCommand.SessionId = this.sessionId;
        return wrapperCommand.SerializeToUtf8Bytes();
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

    private async Task InitializeBiDi()
    {
        ManualResetEventSlim syncEvent = new(false);
        JsonDocument? document = null;
        EventObserver<ConnectionDataReceivedEventArgs> observer = this.Connection.OnDataReceived.AddObserver((e) =>
        {
            document = JsonDocument.Parse(e.Data);
            if (!document.RootElement.TryGetProperty("id", out _))
            {
                // Only return data from command responses; ignore events.
                return;
            }

            syncEvent.Set();
        });

        // Create a hidden tab in the browser to host the BiDi-to-CDP mapper code.
        DevToolsProtocolCommand command = new(this.GetNextCommandId(), "Target.createTarget");
        command.Parameters["url"] = "about:blank";
        command.Parameters["hidden"] = true;
        await this.Connection.SendDataAsync(command.SerializeToUtf8Bytes());
        syncEvent.Wait(TimeSpan.FromSeconds(3));
        syncEvent.Reset();
        if (document is not null)
        {
            // Capture the session ID for the CDP session and the target ID of the created mapper tab.
            JsonElement result = document.RootElement.GetProperty("result");
            this.mapperTabTargetId = document.RootElement.GetProperty("result").GetProperty("targetId").GetString()!;
            Console.WriteLine($"Created mapper tab with target id {this.mapperTabTargetId}");
        }

        if (!string.IsNullOrEmpty(this.mapperTabTargetId))
        {
            // Attach to the target, and capture the session ID.
            command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Target.attachToTarget");
            command.Parameters["targetId"] = this.mapperTabTargetId;
            command.Parameters["flatten"] = true;
            await this.Connection.SendDataAsync(command.SerializeToUtf8Bytes()).ConfigureAwait(false);
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();

            JsonElement result = document!.RootElement.GetProperty("result");
            this.sessionId = result.GetProperty("sessionId").GetString()!;

            // Send a click event to the target so that the beforeunload event
            // will not be fired upon close.
            command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.evaluate");
            command.Parameters["expression"] = "document.body.click()";
            command.Parameters["userGesture"] = true;
            await this.Connection.SendDataAsync(command.SerializeToUtf8Bytes()).ConfigureAwait(false);
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();

            // Enable the Runtime CDP domain.
            command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.enable");
            command.SessionId = this.sessionId;
            await this.Connection.SendDataAsync(command.SerializeToUtf8Bytes()).ConfigureAwait(false);
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();

            // Expose CDP for the mapper tab.
            command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Target.exposeDevToolsProtocol");
            command.Parameters["bindingName"] = "cdp";
            command.Parameters["targetId"] = this.mapperTabTargetId;
            await this.Connection.SendDataAsync(command.SerializeToUtf8Bytes()).ConfigureAwait(false);
            syncEvent.Wait(TimeSpan.FromSeconds(3));
            syncEvent.Reset();

            // Load the mapper tab source code from the resources of this assembly.
            // This source code can be generated by building the chromium-bidi project
            // (https://github.com/GoogleChromeLabs/chromium-bidi). It is stored for
            // convenience in this project in the third_party directory.
            string mapperScript = string.Empty;
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            using (Stream resourceStream = executingAssembly.GetManifestResourceStream("chromium-bidi-mapper"))
            {
                using StreamReader reader = new(resourceStream);
                mapperScript = reader.ReadToEnd();
            }

            if (!string.IsNullOrEmpty(mapperScript))
            {
                // Load the source code for the BiDi-to-CDP mapper into the target tab.
                command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.evaluate");
                command.Parameters["expression"] = mapperScript;
                command.SessionId = this.sessionId;
                await this.Connection.SendDataAsync(command.SerializeToUtf8Bytes()).ConfigureAwait(false);
                syncEvent.Wait(TimeSpan.FromSeconds(3));
                syncEvent.Reset();

                // Start the BiDi-to-CDP mapper code.
                command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.evaluate");
                command.Parameters["expression"] = @$"window.runMapperInstance(""{this.mapperTabTargetId}"")";
                command.Parameters["awaitPromise"] = true;
                command.SessionId = this.sessionId;
                await this.Connection.SendDataAsync(command.SerializeToUtf8Bytes()).ConfigureAwait(false);
                syncEvent.Wait(TimeSpan.FromSeconds(3));
                syncEvent.Reset();

                // Add a binding to be notified when a response is sent from the BiDi-to-CDP mapper code.
                command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.addBinding");
                command.Parameters["name"] = "sendBidiResponse";
                command.SessionId = this.sessionId;
                await this.Connection.SendDataAsync(command.SerializeToUtf8Bytes());
                syncEvent.Wait(TimeSpan.FromSeconds(3));
                syncEvent.Reset();
            }
        }

        observer.Unobserve();
    }

    /// <summary>
    /// Produces a JSON-encoded (quoted and escaped) string using Utf8JsonWriter,
    /// replacing the AOT-incompatible <c>JsonSerializer.Serialize(string)</c>.
    /// </summary>
    private static string JsonEncodeString(string value)
    {
        using MemoryStream stream = new();
        using (Utf8JsonWriter writer = new(stream))
        {
            writer.WriteStringValue(value);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private class DevToolsProtocolCommand
    {
        private readonly long id;
        private readonly string method;
        private Dictionary<string, object> parameters = [];
        private string? sessionId = null;

        public DevToolsProtocolCommand(long id, string method)
        {
            this.id = id;
            this.method = method;
        }

        public long Id => this.id;

        public string Method => this.method;

        public Dictionary<string, object> Parameters => this.parameters;

        public string? SessionId { get => this.sessionId; set => this.sessionId = value; }

        /// <summary>
        /// Serializes this command to UTF-8 JSON bytes using Utf8JsonWriter,
        /// avoiding the AOT-incompatible <c>JsonSerializer.SerializeToUtf8Bytes</c>.
        /// </summary>
        public byte[] SerializeToUtf8Bytes()
        {
            using MemoryStream stream = new();
            using (Utf8JsonWriter writer = new(stream))
            {
                writer.WriteStartObject();
                writer.WriteNumber("id", this.id);
                writer.WriteString("method", this.method);
                writer.WriteStartObject("params");
                foreach (KeyValuePair<string, object> kvp in this.parameters)
                {
                    switch (kvp.Value)
                    {
                        case string s:
                            writer.WriteString(kvp.Key, s);
                            break;
                        case bool b:
                            writer.WriteBoolean(kvp.Key, b);
                            break;
                        case long l:
                            writer.WriteNumber(kvp.Key, l);
                            break;
                        default:
                            writer.WriteString(kvp.Key, kvp.Value?.ToString() ?? string.Empty);
                            break;
                    }
                }

                writer.WriteEndObject();
                if (this.sessionId is not null)
                {
                    writer.WriteString("sessionId", this.sessionId);
                }

                writer.WriteEndObject();
            }

            return stream.ToArray();
        }
    }
}
