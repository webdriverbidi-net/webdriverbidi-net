// <copyright file="FakeBrowserServer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TestUtilities;

using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Nodes;
using PinchHitter;

/// <summary>
/// A WebSocket server standing in for a browser, for code that connects through a launcher. Each connection is
/// answered by its own <see cref="FakeSession"/>, found by the name in the connection's URL, and pages are captured as
/// <see cref="Screenshot"/> and recorded as <see cref="Video"/>.
/// </summary>
public sealed class FakeBrowserServer : IAsyncDisposable
{
    /// <summary>
    /// The image every page is captured as: the PNG signature, which is enough for code that only saves it.
    /// </summary>
    public static readonly byte[] Screenshot = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>
    /// The video every screencast is written as: the WebM signature.
    /// </summary>
    public static readonly byte[] Video = [0x1A, 0x45, 0xDF, 0xA3];

    private const string PathPrefix = "/session/";

    private readonly Server server = new();
    private readonly ConcurrentDictionary<string, FakeSession> sessionsByConnection = new();
    private readonly ConcurrentDictionary<string, FakeSession> sessionsByName = new();

    private FakeBrowserServer()
    {
        this.server.OnDataReceived.AddObserver(this.OnDataReceivedAsync);
    }

    /// <summary>
    /// Starts a server.
    /// </summary>
    /// <returns>The started server.</returns>
    public static async Task<FakeBrowserServer> StartAsync()
    {
        FakeBrowserServer fakeServer = new();
        await fakeServer.server.StartAsync();
        return fakeServer;
    }

    /// <summary>
    /// Gets the URL at which a connection is answered by the session of a name.
    /// </summary>
    /// <param name="name">The session's name, made of letters, digits, and '-'.</param>
    /// <returns>The URL.</returns>
    public Uri UrlFor(string name)
    {
        return new Uri($"ws://localhost:{this.server.Port}{PathPrefix}{name}");
    }

    /// <summary>
    /// Gets the session of a name, which answers the connection to its URL.
    /// </summary>
    /// <param name="name">The session's name.</param>
    /// <returns>The session.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when nothing has connected to the name's URL.</exception>
    public FakeSession SessionFor(string name)
    {
        return this.sessionsByName[name];
    }

    /// <summary>
    /// Answers a session's screenshot commands with <see cref="Screenshot"/>, as it does unless told otherwise.
    /// </summary>
    /// <param name="session">The session.</param>
    public static void AnswerScreenshots(FakeSession session)
    {
        session.RemoteEnd.AnswerWith("browsingContext.captureScreenshot", new JsonObject() { ["data"] = Convert.ToBase64String(Screenshot) });
    }

    /// <summary>
    /// Answers a session's screencast commands as a browser on this machine does: a screencast started with a folder
    /// is written there, as <see cref="Video"/>, when it stops.
    /// </summary>
    /// <param name="session">The session.</param>
    public static void AnswerScreencasts(FakeSession session)
    {
        ConcurrentDictionary<string, string> paths = new();
        int count = 0;
        session.RemoteEnd.AnswerWith("browsingContext.startScreencast", parameters =>
        {
            string screencast = $"screencast-{Interlocked.Increment(ref count)}";
            string path = Path.Combine((string?)parameters["destinationFolder"] ?? Path.GetTempPath(), $"{screencast}.webm");
            paths[screencast] = path;
            return new JsonObject() { ["screencast"] = screencast, ["path"] = path };
        });
        session.RemoteEnd.AnswerWith("browsingContext.stopScreencast", parameters =>
        {
            string path = paths[(string)parameters["screencast"]!];
            File.WriteAllBytes(path, Video);
            return new JsonObject() { ["path"] = path };
        });
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.server.StopAsync();
    }

    private async Task OnDataReceivedAsync(ServerDataReceivedEventArgs e)
    {
        // The HTTP upgrade request, whose path names the session, arrives before the WebSocket frames holding commands.
        if (!e.Data.TrimStart().StartsWith('{'))
        {
            string path = e.Data.Split(' ', 3)[1];
            if (path.StartsWith(PathPrefix, StringComparison.Ordinal))
            {
                await this.ConnectAsync(e.ConnectionId, path[PathPrefix.Length..]);
            }

            return;
        }

        await this.sessionsByConnection[e.ConnectionId].RemoteEnd.SendDataAsync(Encoding.UTF8.GetBytes(e.Data));
    }

    private async Task ConnectAsync(string connectionId, string name)
    {
        FakeRemoteEnd remoteEnd = new();
        FakeSession session = new(remoteEnd);
        AnswerScreenshots(session);
        AnswerScreencasts(session);
        remoteEnd.OnDataReceived.AddObserver(e => this.server.SendWebSocketDataAsync(connectionId, Encoding.UTF8.GetString(e.Data.Span)));
        await remoteEnd.StartAsync("ws://fake.browser.server/session");
        this.sessionsByConnection[connectionId] = session;
        this.sessionsByName[name] = session;
    }
}
