// <copyright file="WebResource.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Net;
using System.Text;

/// <summary>
/// Represents a resource that can be served to a web client.
/// </summary>
public class WebResource
{
    private readonly List<WebAuthenticator> authenticators = new();
    private readonly byte[] data;
    private string mimeType = "text/html;charset=utf-8";
    private bool isRedirect = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebResource"/> class with a string.
    /// </summary>
    /// <param name="data">A string representing the data of this resource to be served. The string will be converted to a byte array using UTF-8 encoding.</param>
    public WebResource(string data)
        : this(Encoding.UTF8.GetBytes(data))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebResource"/> class with a byte array.
    /// </summary>
    /// <param name="data">A byte array representing the data of this resource to be served.</param>
    public WebResource(byte[] data)
    {
        this.data = data;
    }

    /// <summary>
    /// Gets the data for this resource as an array of bytes.
    /// </summary>
    public byte[] Data => this.data;

    /// <summary>
    /// Gets or sets the MIME type of this resource.
    /// </summary>
    public string MimeType { get => this.mimeType; set => this.mimeType = value; }

    /// <summary>
    /// Gets a value indicating whether this resource requires authentication.
    /// </summary>
    public bool RequiresAuthentication => this.authenticators.Count > 0;

    /// <summary>
    /// Gets or sets a value indicating whether this resource redirects to another.
    /// </summary>
    public bool IsRedirect { get => this.isRedirect; set => this.isRedirect = value; }

    /// <summary>
    /// Creates a WebResource representing an HTML page.
    /// </summary>
    /// <param name="bodyContent">The content of the body tag, not including the tag itself.</param>
    /// <param name="headContent">The content of the head tag, not including the tag itself.</param>
    /// <returns>The WebResource representing the HTML page.</returns>
    public static WebResource CreateHtmlResource(string bodyContent, string headContent = "")
    {
        return new($"<html><head>{headContent}</head><body>{bodyContent}</body></html>");
    }

    /// <summary>
    /// Creates a WebResource representing the HTTP response for a WebSocket upgrade request.
    /// </summary>
    /// <param name="websocketAcceptResponseHash">The WebSocket handshake response hash as a base64 encoded string.</param>
    /// <returns>The WebResource representing the HTTP response for a WebSocket upgrade request.</returns>
    public static WebResource CreateWebSocketHandshakeResponse(string websocketAcceptResponseHash)
    {
        return new WebSocketResponseWebResource(websocketAcceptResponseHash);
    }

    /// <summary>
    /// Adds an authenticator for this resource.
    /// </summary>
    /// <param name="authenticator">The authenticator to add.</param>
    public void AddAuthenticator(WebAuthenticator authenticator)
    {
        this.authenticators.Add(authenticator);
    }

    /// <summary>
    /// Attempts to authenticate this resource.
    /// </summary>
    /// <param name="authorizationHeader">The value of the Authorization header in the HTTP request that requests this resource.</param>
    /// <returns><see langword="true" /> if the resource is authenticated; otherwise, <see langword="false" />.</returns>
    public bool TryAuthenticate(string authorizationHeader)
    {
        if (!this.RequiresAuthentication)
        {
            return true;
        }

        foreach (WebAuthenticator authenticator in this.authenticators)
        {
            if (authenticator.IsAuthenticated(authorizationHeader))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates an HttpResponse from this resource with a default status code.
    /// </summary>
    /// <returns>The HTTP response to be transmitted.</returns>
    public virtual HttpResponse CreateHttpResponse()
    {
        return this.CreateHttpResponse(HttpStatusCode.OK);
    }

    /// <summary>
    /// Creates an HttpResponse object from this resource.
    /// </summary>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <returns>The HTTP response to be transmitted.</returns>
    public HttpResponse CreateHttpResponse(HttpStatusCode statusCode)
    {
        HttpResponse response = new()
        {
            StatusCode = statusCode,
        };
        response.Headers["Connection"] = new List<string>() { "keep-alive" };
        response.Headers["Server"] = new List<string>() { "PinchHitter/0.1 .NET/6.0" };
        response.Headers["Date"] = new List<string>() { DateTime.UtcNow.ToString("ddd, dd MMM yyy HH:mm:ss GMT") };
        response.Headers["Content-Type"] = new List<string>() { this.mimeType };
        response.Headers["Content-Length"] = new List<string>() { this.data.Length.ToString() };
        response.BodyContent = this.data;
        return response;
    }
}