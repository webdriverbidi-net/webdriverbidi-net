// <copyright file="IncomingMessage.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

/// <summary>
/// Represents a message being received over the wire from the WebDriverBiDi protocol.
/// </summary>
public class IncomingMessage : IDisposable
{
    private readonly IMemoryOwner<byte> memoryOwner;
    private readonly Func<JsonDocument, JsonDocument?>? documentTransformer;
    private JsonDocument? document;
    private JsonElement payloadElement = default;
    private IncomingMessageKind messagePacketType = IncomingMessageKind.Uninitialized;
    private string? cachedText;
    private bool isDisposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="IncomingMessage"/> class.
    /// </summary>
    /// <param name="owner">
    /// The <see cref="IMemoryOwner{T}"/> whose <see cref="IMemoryOwner{T}.Memory"/> contains the
    /// incoming message bytes. This instance takes ownership and will dispose it on disposal.
    /// </param>
    /// <param name="length">The length, in bytes, of the incoming message within the owner's buffer.</param>
    /// <param name="documentTransformer">An optional transforming function that converts the parsed JsonDocument object to another, or returns <see langword="null"/> to indicate the message should be silently discarded.</param>
    public IncomingMessage(IMemoryOwner<byte> owner, int length, Func<JsonDocument, JsonDocument?>? documentTransformer = null)
    {
        this.memoryOwner = owner;
        this.MessageLength = length;
        this.documentTransformer = documentTransformer;
    }

    /// <summary>
    /// Gets the raw data of the incoming message as a read-only slice of the owner's buffer.
    /// </summary>
    public ReadOnlyMemory<byte> MessageData => this.memoryOwner.Memory.Slice(0, this.MessageLength);

    /// <summary>
    /// Gets the length, in bytes, of the incoming message.
    /// </summary>
    public int MessageLength { get; }

    /// <summary>
    /// Gets the content of the incoming message as a UTF-8 string.
    /// </summary>
    public string MessageText
    {
        get
        {
#if NET5_0_OR_GREATER
            return this.cachedText ??= Encoding.UTF8.GetString(this.memoryOwner.Memory.Slice(0, this.MessageLength).Span);
#else
            return this.cachedText ??= Encoding.UTF8.GetString(this.memoryOwner.Memory.Slice(0, this.MessageLength).ToArray());
#endif
        }
    }

    /// <summary>
    /// Gets the kind of incoming message. Returns <see cref="IncomingMessageKind.Uninitialized"/> if the message has not been deserialized by the JSON parser.
    /// </summary>
    public IncomingMessageKind MessageKind
    {
        get
        {
            if (this.messagePacketType == IncomingMessageKind.Uninitialized)
            {
                if (this.document is not null)
                {
                    // If the document for this message packet has been successfully
                    // deserialzed and a root element accessed, look for a "type"
                    // property in the root element, and, if its value is a string,
                    // capture that value as the message type. Otherwise, the packet
                    // type is "unknown".
                    if (this.payloadElement.ValueKind != JsonValueKind.Undefined && this.payloadElement.TryGetProperty("type", out JsonElement messageTypeToken) && messageTypeToken.ValueKind == JsonValueKind.String && messageTypeToken.GetString() is string value)
                    {
                        this.messagePacketType = MapMessagePacketType(value);
                    }
                    else
                    {
                        this.messagePacketType = IncomingMessageKind.Unknown;
                    }
                }
            }

            return this.messagePacketType;
        }
    }

    /// <summary>
    /// Releases resources used by this <see cref="IncomingMessage"/>.
    /// </summary>
    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.document?.Dispose();
        this.document = null;
        this.memoryOwner.Dispose();
        this.isDisposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Parses the incoming message data as a JSON document.
    /// </summary>
    public virtual void Parse()
    {
        if (this.document is null && this.messagePacketType != IncomingMessageKind.Filtered)
        {
            JsonDocument doc = JsonDocument.Parse(this.memoryOwner.Memory.Slice(0, this.MessageLength));
            if (this.documentTransformer is null)
            {
                this.document = doc;
            }
            else
            {
                using (doc)
                {
                    JsonDocument? transformed = this.documentTransformer(doc);
                    if (transformed is null)
                    {
                        this.messagePacketType = IncomingMessageKind.Filtered;
                        return;
                    }

                    this.document = transformed;
                }
            }

            this.payloadElement = this.document.RootElement;
        }
    }

    /// <summary>
    /// Attempts to get a command ID from the parsed incoming message payload.
    /// </summary>
    /// <param name="responseId">When this method returns, contains the command ID, if the incoming message payload has one.</param>
    /// <returns><see langword="true"/> if the incoming message has a command ID; otherwise, <see langword="false"/>.</returns>
    internal bool TryGetCommandId(out long responseId)
    {
        bool hasCommandId = false;
        long commandId = 0;
        if (this.payloadElement.TryGetProperty("id", out JsonElement idToken) && idToken.ValueKind == JsonValueKind.Number)
        {
            hasCommandId = idToken.TryGetInt64(out commandId);
        }

        responseId = commandId;
        return hasCommandId;
    }

    /// <summary>
    /// Attempts to get an event name from the parsed incoming message payload.
    /// </summary>
    /// <param name="eventName">When this method returns, contains the name of the event, if the incoming message payload has one.</param>
    /// <returns><see langword="true"/> if the incoming message has an event name; otherwise, <see langword="false"/>.</returns>
    internal bool TryGetEventName(out string eventName)
    {
        eventName = string.Empty;
        if (this.payloadElement.TryGetProperty("method", out JsonElement eventNameToken) && eventNameToken.ValueKind == JsonValueKind.String && eventNameToken.GetString() is string value)
        {
            eventName = value;
            return true;
        }

        eventName = string.Empty;
        return false;
    }

    /// <summary>
    /// Deserializes the payload of the incoming message as a command response.
    /// </summary>
    /// <param name="responseTypeInfo">The <see cref="JsonTypeInfo"/> for the type-specific command response data.</param>
    /// <returns>The deserialized <see cref="CommandResponseMessage"/>.</returns>
    internal CommandResponseMessage DeserializeCommandResponseMessage(JsonTypeInfo responseTypeInfo)
    {
        // Deserialize returns CommandResponseMessage or throws JsonException; null is unreachable
        // for a valid protocol response.
        return (CommandResponseMessage)this.payloadElement.Deserialize(responseTypeInfo)!;
    }

    /// <summary>
    /// Attempts to deserialize the payload of the incoming message as an error response.
    /// </summary>
    /// <param name="typeInfo">The <see cref="JsonTypeInfo"/> for the error response.</param>
    /// <param name="errorResponseMessage">When this method returns, contains the <see cref="ErrorResponseMessage"/> contained in the incoming message.</param>
    /// <returns><see langword="true"/> if the incoming message is a valid error response; otherwise, <see langword="false"/>.</returns>
    internal bool TryGetErrorResponse(JsonTypeInfo<ErrorResponseMessage> typeInfo, [NotNullWhen(true)] out ErrorResponseMessage? errorResponseMessage)
    {
        errorResponseMessage = this.payloadElement.Deserialize(typeInfo);
        return errorResponseMessage is not null;
    }

    /// <summary>
    /// Attempts to deserialize the payload of the incoming message as an event message.
    /// </summary>
    /// <param name="eventTypeInfo">The <see cref="JsonTypeInfo"/> for the type-specifc event data.</param>
    /// <param name="eventMessage">When this method returns, contains the <see cref="EventMessage"/> contained in the incoming message.</param>
    /// <returns><see langword="true"/> if the incoming message contains valid event data; otherwise, <see langword="false"/>.</returns>
    internal bool TryDeserializeEventMessage(JsonTypeInfo eventTypeInfo, [NotNullWhen(true)] out EventMessage? eventMessage)
    {
        if (this.payloadElement.Deserialize(eventTypeInfo) is EventMessage message)
        {
            eventMessage = message;
            return true;
        }

        eventMessage = null;
        return false;
    }

    private static IncomingMessageKind MapMessagePacketType(string messageTypeString)
    {
        return messageTypeString switch
        {
            "success" => IncomingMessageKind.CommandResponse,
            "error" => IncomingMessageKind.ErrorResponse,
            "event" => IncomingMessageKind.Event,
            _ => IncomingMessageKind.Unknown,
        };
    }
}
