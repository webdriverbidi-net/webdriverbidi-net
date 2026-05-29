// <copyright file="ChromiumTransport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

using System.Buffers;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<long, DevToolsProtocolCommand> initializationCommandDictionary = new();
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
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the transport is already connected to a remote end.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public override async Task ConnectAsync(string websocketUri, CancellationToken cancellationToken = default)
    {
        await base.ConnectAsync(websocketUri);
        await this.InitializeBiDiAsync();
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
    /// Creates an <see cref="IncomingMessage"/> object for the data received by this <see cref="Transport"/>.
    /// </summary>
    /// <param name="owner">
    /// The <see cref="IMemoryOwner{T}"/> whose buffer contains the incoming message data.
    /// Ownership transfers to the returned <see cref="IncomingMessage"/>, which will dispose it on disposal.
    /// </param>
    /// <param name="length">The length, in bytes, of the incoming message within the data buffer.</param>
    /// <returns>The <see cref="IncomingMessage"/> object for the data received.</returns>
    protected override IncomingMessage CreateIncomingMessage(IMemoryOwner<byte> owner, int length)
    {
        return new IncomingMessage(owner, length, this.ProcessMessageDocument);
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

    private async Task InitializeBiDiAsync(bool hideMapperTab = true)
    {
        // The await using contruct ensures the observer is disposed properly.
        await using EventObserver<ConnectionDataReceivedEventArgs> observer = this.Connection.OnDataReceived.AddObserver((e) =>
        {
            JsonDocument document = JsonDocument.Parse(e.Data);
            if (!document.RootElement.TryGetProperty("id", out JsonElement idElement))
            {
                // Only return data from command responses; ignore events.
                return;
            }

            if (idElement.TryGetInt64(out long responseId) && this.initializationCommandDictionary.TryGetValue(responseId, out DevToolsProtocolCommand command))
            {
                // Only set the result of the task if the response corresponds to
                // a known command ID.
                command.TaskCompletionSource.SetResult(document);
            }
        });

        // Create a hidden tab in the browser to host the BiDi-to-CDP mapper code.
        DevToolsProtocolCommand command = new(this.GetNextCommandId(), "Target.createTarget");
        command.Parameters["url"] = "about:blank";
        command.Parameters["hidden"] = hideMapperTab;
        JsonElement result = await this.ExecuteInitializationCommandAsync(command).ConfigureAwait(false);

        this.mapperTabTargetId = result.GetProperty("targetId").GetString() ?? string.Empty;
        if (string.IsNullOrEmpty(this.mapperTabTargetId))
        {
            throw new WebDriverBiDiException("Could not capture target ID of BiDi mapper tab.");
        }

        // Attach to the target, and capture the session ID.
        command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Target.attachToTarget");
        command.Parameters["targetId"] = this.mapperTabTargetId;
        command.Parameters["flatten"] = true;
        result = await this.ExecuteInitializationCommandAsync(command).ConfigureAwait(false);

        this.sessionId = result.GetProperty("sessionId").GetString() ?? string.Empty;
        if (string.IsNullOrEmpty(this.sessionId))
        {
            throw new WebDriverBiDiException("Could not capture session ID of CDP -> BiDi session.");
        }

        // Enable the Runtime CDP domain.
        command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.enable");
        command.SessionId = this.sessionId;
        await this.ExecuteInitializationCommandAsync(command).ConfigureAwait(false);

        // Send a click event to the target so that the beforeunload event
        // will not be fired upon close.
        command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.evaluate");
        command.Parameters["expression"] = "document.body.click()";
        command.Parameters["userGesture"] = true;
        command.SessionId = this.sessionId;
        await this.ExecuteInitializationCommandAsync(command).ConfigureAwait(false);

        // Expose CDP for the mapper tab.
        command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Target.exposeDevToolsProtocol");
        command.Parameters["bindingName"] = "cdp";
        command.Parameters["targetId"] = this.mapperTabTargetId;
        await this.ExecuteInitializationCommandAsync(command).ConfigureAwait(false);

        // Load the mapper tab source code from the resources of this assembly.
        // This source code can be generated by building the chromium-bidi project
        // (https://github.com/GoogleChromeLabs/chromium-bidi). It is stored for
        // convenience in this project in the third_party directory.
        string mapperScript = string.Empty;
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        using Stream resourceStream = executingAssembly.GetManifestResourceStream("chromium-bidi-mapper");
        if (resourceStream is null)
        {
            throw new InvalidOperationException("Unable to find the Chromium BiDi mapper script as an embedded resource.");
        }

        using StreamReader reader = new(resourceStream);
        mapperScript = reader.ReadToEnd();
        if (string.IsNullOrEmpty(mapperScript))
        {
            throw new InvalidOperationException("Found an embedded resource for the Chromium BiDi mapper script, but the resource was empty.");
        }

        // Load the source code for the BiDi-to-CDP mapper into the target tab.
        command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.evaluate");
        command.Parameters["expression"] = mapperScript;
        command.SessionId = this.sessionId;
        await this.ExecuteInitializationCommandAsync(command).ConfigureAwait(false);

        // Start the BiDi-to-CDP mapper code.
        command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.evaluate");
        command.Parameters["expression"] = @$"window.runMapperInstance(""{this.mapperTabTargetId}"")";
        command.Parameters["awaitPromise"] = true;
        command.SessionId = this.sessionId;
        await this.ExecuteInitializationCommandAsync(command).ConfigureAwait(false);

        // Add a binding to be notified when a response is sent from the BiDi-to-CDP mapper code.
        command = new DevToolsProtocolCommand(this.GetNextCommandId(), "Runtime.addBinding");
        command.Parameters["name"] = "sendBidiResponse";
        command.SessionId = this.sessionId;
        await this.ExecuteInitializationCommandAsync(command).ConfigureAwait(false);
    }

    private async Task<JsonElement> ExecuteInitializationCommandAsync(DevToolsProtocolCommand command, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(3);
        int retryCount = 0;
        DevToolsProtocolCommand currentCommand = command;
        CancellationTokenSource cancellationTokenSource = new();
        Task timeoutTask = Task.Delay(timeout.Value, cancellationTokenSource.Token);
        while (!cancellationTokenSource.IsCancellationRequested && !timeoutTask.IsCompleted)
        {
            this.initializationCommandDictionary.TryAdd(currentCommand.Id, currentCommand);
            await this.Connection.SendDataAsync(currentCommand.SerializeToUtf8Bytes()).ConfigureAwait(false);
            Task completedTask = await Task.WhenAny(timeoutTask, currentCommand.TaskCompletionSource.Task).ConfigureAwait(false);
            if (completedTask != timeoutTask)
            {
                this.initializationCommandDictionary.TryRemove(currentCommand.Id, out _);
                JsonDocument document = await currentCommand.TaskCompletionSource.Task;
                if (document.RootElement.TryGetProperty("result", out JsonElement result))
                {
                    cancellationTokenSource.Cancel();
                    return result;
                }
                else
                {
                    // The command result was an error. Copy the command parameters and retry,
                    // up until the timeout.
                    retryCount++;
                    currentCommand = new DevToolsProtocolCommand(this.GetNextCommandId(), command.Method);
                    foreach (KeyValuePair<string, object> entry in command.Parameters)
                    {
                        currentCommand.Parameters[entry.Key] = entry.Value;
                        if (!string.IsNullOrEmpty(command.SessionId))
                        {
                            currentCommand.SessionId = command.SessionId;
                        }
                    }
                }
            }
        }

        throw new WebDriverBiDiException($"Unable to execute BiDi initialization command '{command.Method}' within {timeout.Value.TotalMilliseconds} seconds (retried {retryCount} times)");
    }

    private JsonDocument ProcessMessageDocument(JsonDocument deserializedDocument)
    {
        JsonElement deserialized = deserializedDocument.RootElement;
        if (!deserialized.TryGetProperty("method", out JsonElement methodNameElement))
        {
            throw new WebDriverBiDiSerializationException("No 'method' property in JSON");
        }

        string methodName = methodNameElement.GetString() ?? throw new WebDriverBiDiSerializationException("'method' property in JSON is not a string");
        if (methodName != "Runtime.bindingCalled")
        {
            throw new WebDriverBiDiSerializationException("'method' property value was not 'Runtime.bindingCalled");
        }

        if (!deserialized.TryGetProperty("params", out JsonElement valueElement))
        {
            throw new WebDriverBiDiSerializationException("No 'params' property in JSON");
        }

        JsonElement bindingNameElement = valueElement.GetProperty("name");
        string bindingName = bindingNameElement.GetString() ?? throw new WebDriverBiDiSerializationException("'params' object does not have a 'name' property with a string value");
        if (bindingName != "sendBidiResponse")
        {
            throw new WebDriverBiDiSerializationException("'params' object value of 'name' property is not 'sendBidiResponse");
        }

        JsonElement payloadElement = valueElement.GetProperty("payload");
        string payload = payloadElement.GetString() ?? throw new WebDriverBiDiSerializationException("'params' object does not have a 'payload' property with a string value");
        return JsonDocument.Parse(payload);
    }

    private class DevToolsProtocolCommand
    {
        private readonly string method;
        private readonly Dictionary<string, object> parameters = [];
        private readonly TaskCompletionSource<JsonDocument> taskCompletionSource = new();
        private readonly long id = 0;
        private string? sessionId = null;

        public DevToolsProtocolCommand(long commandId, string method)
        {
            this.id = commandId;
            this.method = method;
        }

        public long Id => this.id;

        public string Method => this.method;

        public Dictionary<string, object> Parameters => this.parameters;

        public string? SessionId { get => this.sessionId; set => this.sessionId = value; }

        public TaskCompletionSource<JsonDocument> TaskCompletionSource => this.taskCompletionSource;

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
                        case string stringValue:
                            writer.WriteString(kvp.Key, stringValue);
                            break;
                        case bool booleanValue:
                            writer.WriteBoolean(kvp.Key, booleanValue);
                            break;
                        case long longValue:
                            writer.WriteNumber(kvp.Key, longValue);
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
