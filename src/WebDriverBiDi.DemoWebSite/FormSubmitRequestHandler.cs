// <copyright file="FormSubmitRequestHandler.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.DemoWebSite;

using System.Net;
using PinchHitter;

/// <summary>
/// Handles the form submission, processing the data sent.
/// </summary>
public class FormSubmitRequestHandler : WebResourceRequestHandler
{
    private static readonly string contentTemplate = WebContent.AsHtmlDocument(@"<h1>Data sent!</h1><div>Data submitted via the form: <span class=""form-data"">{0}</span></div>", @"<link rel=""stylesheet"" href=""./style/result.css"" />");

    /// <summary>
    /// Initializes a new instance of the <see cref="FormSubmitRequestHandler"/> class.
    /// </summary>
    public FormSubmitRequestHandler()
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
        Dictionary<string, string> formData = this.ParseRequestBody(request.Body);
        HttpResponse response = base.CreateHttpResponse(request.Id, HttpStatusCode.OK);
        response.TextBodyContent = string.Format(response.TextBodyContent, formData["dataToSend"]);
        response.Headers["Content-Length"][0] = response.BodyContent.Length.ToString();
        return Task.FromResult<HttpResponse>(response);
    }

    private Dictionary<string, string> ParseRequestBody(string requestBody)
    {
        Dictionary<string, string> formData = new();
        string[] lines = requestBody.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] entry = line.Split('=', 2);
            formData[entry[0]] = entry[1];
        }

        return formData;
    }
}