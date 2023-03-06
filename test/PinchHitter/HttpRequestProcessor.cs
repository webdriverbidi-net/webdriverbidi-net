// <copyright file="HttpRequestProcessor.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Net;
using System.Text;

/// <summary>
/// A processor for managing HTTP requests, including generating responses
/// based on registered resource.
/// </summary>
public class HttpRequestProcessor
{
    private readonly Dictionary<string, WebResource> knownResources = new();
    private readonly WebResource notFoundResource = WebResource.CreateHtmlResource("<h1>404 Not Found</h1><div>The requested resource was not found</div>");
    private readonly WebResource invalidRequestResource = WebResource.CreateHtmlResource("<h1>400 Invalid request</h1><div>The authorization request was incorrect</div>");
    private readonly WebResource forbiddenResource = WebResource.CreateHtmlResource("<h1>403 Forbidden</h1><div>You do not have the permissions to view this resource</div>");

    /// <summary>
    /// Process an HTTP request, returning a response.
    /// </summary>
    /// <param name="request">The HttpRequest object representing the request.</param>
    /// <returns>An HttpResponse object representing the response.</returns>
    public virtual HttpResponse ProcessRequest(HttpRequest request)
    {
        HttpResponse responseData;
        if (request.IsWebSocketHandshakeRequest)
        {
            WebResource resource = WebResource.CreateWebSocketHandshakeResponse(request.Headers["Sec-WebSocket-Key"][0]);
            responseData = resource.CreateHttpResponse();
        }
        else
        {
            if (!this.knownResources.ContainsKey(request.Url))
            {
                responseData = this.notFoundResource.CreateHttpResponse(HttpStatusCode.NotFound);
            }
            else
            {
                WebResource resource = this.knownResources[request.Url];
                if (resource.RequiresAuthentication)
                {
                    if (!request.Headers.ContainsKey("Authorization"))
                    {
                        WebResource unauthorizedResource = WebResource.CreateHtmlResource(string.Empty);
                        responseData = unauthorizedResource.CreateHttpResponse(HttpStatusCode.Unauthorized);
                        responseData.Headers["Www-Authenticate"] = new List<string>() { "Basic" };
                    }
                    else
                    {
                        if (request.Headers["Authorization"].Count == 0)
                        {
                            responseData = this.invalidRequestResource.CreateHttpResponse(HttpStatusCode.BadRequest);
                        }
                        else
                        {
                            string authorizationHeader = request.Headers["Authorization"][0];
                            if (!resource.TryAuthenticate(authorizationHeader))
                            {
                                responseData = this.forbiddenResource.CreateHttpResponse(HttpStatusCode.Forbidden);
                            }
                            else
                            {
                                responseData = resource.CreateHttpResponse();
                            }
                        }
                    }
                }
                else
                {
                    if (resource.IsRedirect)
                    {
                        responseData = resource.CreateHttpResponse(HttpStatusCode.MovedPermanently);
                        responseData.Headers["Location"] = new List<string>() { Encoding.UTF8.GetString(resource.Data) };
                        responseData.Headers["Content-Length"] = new List<string>() { "0" };
                        responseData.BodyContent = Array.Empty<byte>();
                    }
                    else
                    {
                        responseData = resource.CreateHttpResponse();
                    }
                }
            }
        }

        return responseData;
    }

    /// <summary>
    /// Registers a resource with this web server to be returned when requested.
    /// </summary>
    /// <param name="url">The relative URL associated with this resource.</param>
    /// <param name="resource">The web resource to return when requested.</param>
    public virtual void RegisterResource(string url, WebResource resource)
    {
        this.knownResources[url] = resource;
    }
}