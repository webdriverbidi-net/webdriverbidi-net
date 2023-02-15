// <copyright file="WebResource.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Text;

/// <summary>
/// Represents a resource that can be served to a web client.
/// </summary>
public class WebResource
{
    private readonly List<WebAuthenticator> authenticators = new();
    private readonly byte[] data;
    private string mimeType = "text/html";

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
    /// Gets a read-only list of the registered authenticators for this resource.
    /// </summary>
    public IList<WebAuthenticator> Authenticators => this.authenticators.AsReadOnly();

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
    /// Adds an authenticator for this resource.
    /// </summary>
    /// <param name="authenticator">The authenticator to add.</param>
    public void AddAuthenticator(WebAuthenticator authenticator)
    {
        this.authenticators.Add(authenticator);
    }
}