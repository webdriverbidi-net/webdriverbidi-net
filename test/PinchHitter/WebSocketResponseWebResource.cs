// <copyright file="WebSocketResponseWebResource.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Net;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// A WebResource used to create a response for a request to upgrade a connection to use the WebSocket protocol.
/// </summary>
public class WebSocketResponseWebResource : WebResource
{
    // A special GUID used in the WebSocket handshake for upgrading an HTTP
    // connection to use the WebSocket protocol. Specified by RFC 6455.
    private static readonly string WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

    private readonly string websocketAcceptResponseHash;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketResponseWebResource"/> class.
    /// </summary>
    /// <param name="websocketSecureKey">The value of the Sec-WebSocket-Key header in the HTTP request.</param>
    internal WebSocketResponseWebResource(string websocketSecureKey)
        : base(string.Empty)
    {
        // 1. Obtain the passed-in value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
        // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
        // 3. Compute SHA-1 and Base64 hash of the new value
        // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
        byte[] websocketSecureResponseBytes = Encoding.UTF8.GetBytes($"{websocketSecureKey.Trim()}{WebSocketGuid}");
        byte[] websocketResponseHash = SHA1.Create().ComputeHash(websocketSecureResponseBytes);
        this.websocketAcceptResponseHash = Convert.ToBase64String(websocketResponseHash);
    }

    /// <summary>
    /// Creates an HttpResponse from this resource with a default status code.
    /// </summary>
    /// <returns>The HTTP response to be transmitted.</returns>
    public override HttpResponse CreateHttpResponse()
    {
        HttpResponse response = this.CreateHttpResponse(HttpStatusCode.SwitchingProtocols);
        response.Headers["Connection"] = new List<string>() { "Upgrade" };
        response.Headers["Upgrade"] = new List<string>() { "websocket" };
        response.Headers["Sec-WebSocket-Accept"] = new List<string>() { this.websocketAcceptResponseHash };
        return response;
    }
}