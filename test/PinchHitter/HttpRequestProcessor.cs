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

    /// <summary>
    /// Process an HTTP request, returning a response.
    /// </summary>
    /// <param name="request">The HttpRequest object representing the request.</param>
    /// <returns>An HttpResponse object representing the response.</returns>
    public virtual HttpResponse ProcessRequest(HttpRequest request)
    {
        HttpResponse responseData;
        if (!this.knownResources.ContainsKey(request.Url))
        {
            WebResource notFoundResource = WebResource.CreateHtmlResource("<h1>404 Not Found</h1><div>The requested resource was not found</div>");
            responseData = notFoundResource.CreateHttpResponse(HttpStatusCode.NotFound);
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
                        WebResource forbiddenResource = WebResource.CreateHtmlResource("<h1>400 Invalid request</h1><div>The authorization request was incorrect</div>");
                        responseData = forbiddenResource.CreateHttpResponse(HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        string authorizationHeader = request.Headers["Authorization"][0];
                        if (!resource.TryAuthenticate(authorizationHeader))
                        {
                            WebResource forbiddenResource = WebResource.CreateHtmlResource("<h1>403 Forbidden</h1><div>You do not have the permissions to view this resource</div>");
                            responseData = forbiddenResource.CreateHttpResponse(HttpStatusCode.Forbidden);
                        }
                        else
                        {
                            responseData = resource.CreateHttpResponse(HttpStatusCode.OK);
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
                    responseData = resource.CreateHttpResponse(HttpStatusCode.OK);
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