// <copyright file="CookiePageRequestHandler.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.DemoWebSite;

using System.Globalization;
using System.Net;
using System.Text;
using PinchHitter;

/// <summary>
/// Handles the form submission, processing the data sent.
/// </summary>
public class CookiePageRequestHandler : WebResourceRequestHandler
{
    private static readonly string contentTemplate = WebContent.AsHtmlDocument("""
        <h1>Cookies Received!</h1>
        <div>
          Cookies submitted via the request:
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Value</th>
                <th>Domain</th>
                <th>Path</th>
                <th>Expires</th>
                <th>Is secure?</th>
                <th>Is HTTP only?</th>
                <th>Same site value</th>
              <tr>
            </thead>
            <tbody>
              {0}
            </tbody>
          </table>
        </div>
        """,
        """
        <link rel="stylesheet" href="./style/main.css" />
        <link rel="stylesheet" href="./style/cookie-page.css" />
        """
    );

    /// <summary>
    /// Initializes a new instance of the <see cref="CookiePageRequestHandler"/> class.
    /// </summary>
    public CookiePageRequestHandler()
        : base(contentTemplate)
    {
    }

    /// <summary>
    /// Processes an HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request to handle.</param>
    /// <param name="additionalData">Additional data passed into the method for handling requests.</param>
    /// <returns>The response to the HTTP request.</returns>
    protected override Task<HttpResponse> ProcessRequestAsync(HttpRequest request, params object[] additionalData)
    {
        List<Dictionary<string, string>> cookies = [];
        if (request.Headers.TryGetValue("Cookie", out List<string>? cookieHeaderValues))
        {
            foreach (string cookieHeaderValue in cookieHeaderValues)
            {
                cookies.AddRange(ParseCookieHeader(cookieHeaderValue));
            }
        }

        Dictionary<string, string>? serverCookie = cookies.Find(cookie => cookie["name"] == "ServerSuppliedCookie");
        if (serverCookie is null)
        {
            cookies.Add(new Dictionary<string, string>()
            {
                { "name", "ServerSuppliedCookie" },
                { "value", "ServerCookieValue" },
                { "expires", DateTime.UtcNow.AddDays(1).ToString("ddd, dd MMM yyy HH:mm:ss", CultureInfo.InvariantCulture) + " GMT" },
                { "server", string.Empty },
            });
        }
        else
        {
            serverCookie["expires"] = DateTime.UtcNow.AddDays(1).ToString("ddd, dd MMM yyy HH:mm:ss", CultureInfo.InvariantCulture) + " GMT";
            serverCookie["server"] = string.Empty;
        }

        HttpResponse response = base.CreateHttpResponse(request.Id, HttpStatusCode.OK);
        response.Headers["Set-Cookie"] = new List<string>();
        foreach (Dictionary<string, string> cookieAttributes in cookies)
        {
            response.Headers["Set-Cookie"].Add(FormatSetCookieHeader(cookieAttributes));
        }

        response.TextBodyContent = string.Format(response.TextBodyContent, FormatCookieDisplay(cookies));
        response.Headers["Content-Length"][0] = response.BodyContent.Length.ToString();
        return Task.FromResult<HttpResponse>(response);
    }

    private static string FormatCookieDisplay(List<Dictionary<string, string>> cookies)
    {
        StringBuilder cookieDisplayBuilder = new();
        foreach (Dictionary<string, string> cookieAttributes in cookies)
        {
            bool isServerCookie = cookieAttributes.ContainsKey("server");
            cookieDisplayBuilder.AppendLine($"      <tr{(isServerCookie ? " class=\"server-cookie\"" : string.Empty)}>");
            if (cookieAttributes.TryGetValue("name", out string? nameValue))
            {
                cookieDisplayBuilder.AppendLine($"        <td>{nameValue}</td>");
            }
            else
            {
                cookieDisplayBuilder.AppendLine($"        <td></td>");
            }

            if (cookieAttributes.TryGetValue("value", out string? valueValue))
            {
                cookieDisplayBuilder.AppendLine($"        <td>{valueValue}</td>");
            }
            else
            {
                cookieDisplayBuilder.AppendLine($"        <td></td>");
            }

            if (cookieAttributes.TryGetValue("domain", out string? domainValue))
            {
                cookieDisplayBuilder.AppendLine($"        <td>{domainValue}</td>");
            }
            else
            {
                cookieDisplayBuilder.AppendLine($"        <td></td>");
            }

            if (cookieAttributes.TryGetValue("path", out string? pathValue))
            {
                cookieDisplayBuilder.AppendLine($"        <td>{pathValue}</td>");
            }
            else
            {
                cookieDisplayBuilder.AppendLine($"        <td></td>");
            }

            if (cookieAttributes.TryGetValue("expires", out string? expiresValue))
            {
                cookieDisplayBuilder.AppendLine($"        <td>{expiresValue}</td>");
            }
            else
            {
                cookieDisplayBuilder.AppendLine($"        <td></td>");
            }

            if (cookieAttributes.ContainsKey("httponly"))
            {
                cookieDisplayBuilder.AppendLine($"        <td>is set</td>");
            }
            else
            {
                cookieDisplayBuilder.AppendLine($"        <td></td>");
            }

            if (cookieAttributes.ContainsKey("secure"))
            {
                cookieDisplayBuilder.AppendLine($"        <td>is set</td>");
            }
            else
            {
                cookieDisplayBuilder.AppendLine($"        <td></td>");
            }

            if (cookieAttributes.TryGetValue("samesite", out string? sameSite))
            {
                cookieDisplayBuilder.AppendLine($"        <td>{sameSite}</td>");
            }
            else
            {
                cookieDisplayBuilder.AppendLine($"        <td></td>");
            }

            cookieDisplayBuilder.AppendLine("      </tr>");
        }

        return cookieDisplayBuilder.ToString();
    }

    private static string FormatSetCookieHeader(Dictionary<string, string> cookieAttributes)
    {
        StringBuilder cookieHeaderBuilder = new();
        cookieHeaderBuilder.Append($"{cookieAttributes["name"]}={cookieAttributes["value"]}");
        foreach (KeyValuePair<string, string> cookieAttribute in cookieAttributes)
        {
            if (!cookieAttribute.Key.Equals("name", StringComparison.InvariantCultureIgnoreCase) && !cookieAttribute.Key.Equals("value", StringComparison.InvariantCultureIgnoreCase))
            {
                if (cookieAttribute.Key.Equals("httponly", StringComparison.InvariantCultureIgnoreCase) || cookieAttribute.Key.Equals("secure", StringComparison.InvariantCultureIgnoreCase))
                {
                    cookieHeaderBuilder.Append($";{cookieAttribute.Key}");
                }
                else
                {
                    cookieHeaderBuilder.Append($";{cookieAttribute.Key}={cookieAttribute.Value}");
                }
            }
        }

        return cookieHeaderBuilder.ToString();
    }

    private static List<Dictionary<string, string>> ParseCookieHeader(string cookieHeaderValue)
    {
        List<Dictionary<string, string>> cookieList = [];
        string[] crumbs = cookieHeaderValue.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (string crumb in crumbs)
        {
            Dictionary<string, string> cookie = new(StringComparer.OrdinalIgnoreCase);
            string[] crumbParts = crumb.Trim().Split('=', 2);
            string attributeName = crumbParts[0];
            if (crumbParts.Length > 1)
            {
                string attributeValue = crumbParts[1];
                switch (attributeName.ToLowerInvariant())
                {
                    case "domain":
                    case "path":
                    case "expires":
                    case "samesite":
                        cookie[attributeName] = attributeValue;
                        break;
                    default:
                        cookie["name"] = attributeName;
                        cookie["value"] = attributeValue;
                        break;
                }
            }
            else
            {
                if (attributeName.Equals("httponly", StringComparison.InvariantCultureIgnoreCase))
                {
                    cookie["httponly"] = "isSet";
                }

                if (attributeName.Equals("secure", StringComparison.InvariantCultureIgnoreCase))
                {
                    cookie["secure"] = "isSet";
                }
            }

            cookieList.Add(cookie);
        }

        return cookieList;
    }
}
