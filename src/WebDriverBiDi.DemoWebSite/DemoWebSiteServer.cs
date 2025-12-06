// <copyright file="DemoSiteServer.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.DemoWebSite;

using PinchHitter;

/// <summary>
/// Creates and launches the demo web site, and populates it with content.
/// </summary>
public class DemoWebSiteServer
{
    private static readonly string contentDirectoryName = "content";
    private readonly Server webServer = new();

    /// <summary>
    /// Gets the port on which the server is listening.
    /// </summary>
    public int Port => this.webServer.Port;

    /// <summary>
    /// Launches the server.
    /// </summary>
    public void Launch()
    {
        StoredContentRegistrar.RegisterDirectory(webServer, string.Empty, Path.Combine(AppContext.BaseDirectory, contentDirectoryName));
        this.webServer.RegisterHandler("/", new RedirectRequestHandler("/index.html"));
        this.webServer.RegisterHandler("/processForm", HttpMethod.Post, new FormSubmitRequestHandler());
        this.webServer.RegisterHandler("/cookiePage.html", HttpMethod.Get, new CookiePageRequestHandler());
        this.webServer.Start();
        Console.WriteLine($"Demo web server listening on port {this.Port}");
    }

    /// <summary>
    /// Shuts down the server.
    /// </summary>
    public void Shutdown()
    {
        this.webServer.Stop();
    }
}
