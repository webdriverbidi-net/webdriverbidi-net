// <copyright file="HttpRequest.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Represents the data of an HTTP request.
/// </summary>
public class HttpRequest
{
    private readonly Dictionary<string, List<string>> headers = new();
    private string verb = string.Empty;
    private string url = string.Empty;
    private string httpVersion = string.Empty;
    private string body = string.Empty;

    private HttpRequest()
    {
    }

    /// <summary>
    /// Gets the verb of this HTTP request.
    /// </summary>
    public string Verb => this.verb;

    /// <summary>
    /// Gets the relative URL of this HTTP request.
    /// </summary>
    public string Url => this.url;

    /// <summary>
    /// Gets the HTTP version of this HTTP request.
    /// </summary>
    public string HttpVersion => this.httpVersion;

    /// <summary>
    /// Gets the list of headers for this HTTP request.
    /// </summary>
    public Dictionary<string, List<string>> Headers => this.headers;

    /// <summary>
    /// Gets the body of this HTTP request.
    /// </summary>
    public string Body => this.body;

    /// <summary>
    /// Gets a value indicating whether this HTTP request is a WebSocket handshake request.
    /// </summary>
    public bool IsWebSocketHandshakeRequest
    {
        get
        {
            return this.headers.ContainsKey("Connection") && this.headers["Connection"].Contains("Upgrade") &&
                this.headers.ContainsKey("Upgrade") && this.headers["Upgrade"].Contains("websocket") &&
                this.headers.ContainsKey("Sec-WebSocket-Key") && this.headers["Sec-WebSocket-Key"].Count > 0;
        }
    }

    /// <summary>
    /// Parses an incoming HTTP request.
    /// </summary>
    /// <param name="rawRequest">The string containing the HTTP request.</param>
    /// <returns>The parsed HTTP request data.</returns>
    public static HttpRequest Parse(string rawRequest)
    {
        HttpRequest result = new();
        string[] requestLines = rawRequest.Split("\r\n");
        int currentLine = 0;

        string navigationLine = requestLines[currentLine];
        Regex navigationRegex = new(@"(.*)\s+(.*)\s+(.*)");
        if (navigationRegex.IsMatch(navigationLine))
        {
            Match match = navigationRegex.Match(navigationLine);
            result.verb = match.Groups[1].Value;
            result.url = match.Groups[2].Value;
            result.httpVersion = match.Groups[3].Value;
        }

        currentLine++;

        while (requestLines[currentLine].Length > 0)
        {
            string rawHeader = requestLines[currentLine];
            string[] readerInfo = rawHeader.Split(":", 2, StringSplitOptions.TrimEntries);
            if (result.headers.ContainsKey(readerInfo[0]))
            {
                result.headers[readerInfo[0]].Add(readerInfo[1]);
            }
            else
            {
                result.headers[readerInfo[0]] = new() { readerInfo[1] };
            }

            currentLine++;
        }

        StringBuilder bodyBuilder = new();
        for (; currentLine < requestLines.Length; currentLine++)
        {
            if (bodyBuilder.Length > 0)
            {
                bodyBuilder.AppendLine();
            }

            bodyBuilder.Append(requestLines[currentLine]);
        }

        result.body = bodyBuilder.ToString();
        return result;
    }
}