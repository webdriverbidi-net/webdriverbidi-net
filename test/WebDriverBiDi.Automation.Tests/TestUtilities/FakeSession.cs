// <copyright file="FakeSession.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation.TestUtilities;

using System.Text.Json.Nodes;
using WebDriverBiDi.Protocol;

/// <summary>
/// A browser's user contexts and browsing contexts, kept for a fake remote end. It answers the session,
/// user context, and browsing context commands from them, and raises the events a browser raises before
/// answering a command that creates, navigates, or closes a browsing context. It finds no elements unless a test
/// answers "browsingContext.locateNodes" itself.
/// </summary>
public sealed class FakeSession
{
    /// <summary>
    /// The ID of the user context every browser has.
    /// </summary>
    public const string DefaultUserContextId = "default";

    private readonly object lockObject = new();
    private readonly List<string> userContextIds = [DefaultUserContextId];
    private readonly List<FakeContext> contexts = [];
    private int identifierCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeSession"/> class.
    /// </summary>
    /// <param name="remoteEnd">The remote end whose commands it answers.</param>
    public FakeSession(FakeRemoteEnd remoteEnd)
    {
        this.RemoteEnd = remoteEnd;
        remoteEnd.AnswerWith("session.new", _ => CreateNewSessionResult());
        remoteEnd.AnswerWith("browser.createUserContext", _ => this.CreateUserContext());
        remoteEnd.AnswerWith("browser.getUserContexts", _ => this.GetUserContexts());
        remoteEnd.AnswerWith("browser.removeUserContext", parameters => this.RemoveUserContext((string)parameters["userContext"]!));
        remoteEnd.AnswerWith("browsingContext.getTree", parameters => this.GetTree((string?)parameters["root"], (int?)parameters["maxDepth"]));
        remoteEnd.AnswerWith("browsingContext.create", parameters => this.CreateContext((string?)parameters["userContext"] ?? DefaultUserContextId));
        remoteEnd.AnswerWith("browsingContext.close", parameters => this.CloseContext((string)parameters["context"]!));
        remoteEnd.AnswerWith("browsingContext.navigate", parameters => this.Navigate((string)parameters["context"]!, (string?)parameters["url"]));
        remoteEnd.AnswerWith("browsingContext.reload", parameters => this.Navigate((string)parameters["context"]!, null));
        remoteEnd.AnswerWith("browsingContext.locateNodes", _ => new JsonObject() { ["nodes"] = new JsonArray() });
    }

    /// <summary>
    /// Gets the remote end whose commands this session answers.
    /// </summary>
    public FakeRemoteEnd RemoteEnd { get; }

    /// <summary>
    /// Gets the IDs of the user contexts, the default one first.
    /// </summary>
    public IReadOnlyList<string> UserContextIds
    {
        get
        {
            lock (this.lockObject)
            {
                return [.. this.userContextIds];
            }
        }
    }

    /// <summary>
    /// Gets the browsing contexts, in the order they were added.
    /// </summary>
    public IReadOnlyList<FakeContext> Contexts
    {
        get
        {
            lock (this.lockObject)
            {
                return [.. this.contexts];
            }
        }
    }

    /// <summary>
    /// Creates a driver connected to a fake remote end answered by a new fake session.
    /// </summary>
    /// <returns>The started driver and the session.</returns>
    public static async Task<(BiDiDriver Driver, FakeSession Session)> ConnectAsync()
    {
        FakeRemoteEnd remoteEnd = new();
        FakeSession session = new(remoteEnd);
        BiDiDriver driver = new(TimeSpan.FromSeconds(10), new Transport(remoteEnd));
        await driver.StartAsync("ws://fake.remote.end/session");
        return (driver, session);
    }

    /// <summary>
    /// Adds a user context, as one created before the session started is.
    /// </summary>
    /// <returns>The ID of the user context.</returns>
    public string AddUserContext()
    {
        lock (this.lockObject)
        {
            string userContextId = this.NextId("user-context");
            this.userContextIds.Add(userContextId);
            return userContextId;
        }
    }

    /// <summary>
    /// Adds a browsing context without raising an event, as one open before the session started is.
    /// </summary>
    /// <param name="userContextId">The ID of its user context.</param>
    /// <param name="parentId">The ID of its parent, or <see langword="null"/> for a top-level context.</param>
    /// <param name="url">Its URL.</param>
    /// <returns>The browsing context.</returns>
    public FakeContext AddContext(string userContextId = DefaultUserContextId, string? parentId = null, string url = "about:blank")
    {
        lock (this.lockObject)
        {
            FakeContext context = new(this.NextId("context"), userContextId, parentId, url);
            this.contexts.Add(context);
            return context;
        }
    }

    /// <summary>
    /// Adds a frame to a browsing context, raising the event a browser raises when a page adds an iframe.
    /// </summary>
    /// <param name="parentId">The ID of the browsing context containing the frame.</param>
    /// <param name="url">The frame's URL.</param>
    /// <returns>The frame's browsing context.</returns>
    public async Task<FakeContext> CreateFrameAsync(string parentId, string url = "about:blank")
    {
        FakeContext frame;
        lock (this.lockObject)
        {
            FakeContext parent = this.contexts.Single(context => context.Id == parentId);
            frame = new FakeContext(this.NextId("context"), parent.UserContextId, parentId, url);
            this.contexts.Add(frame);
        }

        (string method, JsonObject parameters) = CreatedEvent(frame);
        await this.RemoteEnd.RaiseEventAsync(method, parameters);
        return frame;
    }

    /// <summary>
    /// Changes a browsing context's URL through the history API, raising the event a browser raises.
    /// </summary>
    /// <param name="contextId">The ID of the browsing context.</param>
    /// <param name="url">The new URL.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task RaiseHistoryUpdatedAsync(string contextId, string url)
    {
        FakeContext context = this.SetUrl(contextId, url);
        JsonObject parameters = new()
        {
            ["context"] = contextId,
            ["timestamp"] = 1790000000000,
            ["url"] = url,
            ["userContext"] = context.UserContextId,
        };
        return this.RemoteEnd.RaiseEventAsync("browsingContext.historyUpdated", parameters);
    }

    /// <summary>
    /// Raises a navigation event for a browsing context, such as "browsingContext.load".
    /// </summary>
    /// <param name="method">The event's method.</param>
    /// <param name="contextId">The ID of the browsing context.</param>
    /// <param name="url">The URL navigated to.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public Task RaiseNavigationEventAsync(string method, string contextId, string url)
    {
        JsonObject parameters = new()
        {
            ["context"] = contextId,
            ["navigation"] = this.NextId("navigation"),
            ["timestamp"] = 1790000000000,
            ["url"] = url,
        };
        return this.RemoteEnd.RaiseEventAsync(method, parameters);
    }

    private static JsonObject CreateNewSessionResult()
    {
        return new JsonObject()
        {
            ["sessionId"] = "fake-session",
            ["capabilities"] = new JsonObject()
            {
                ["acceptInsecureCerts"] = false,
                ["browserName"] = "fake",
                ["browserVersion"] = "1.0",
                ["platformName"] = "fake",
                ["setWindowRect"] = true,
                ["userAgent"] = "Fake/1.0",
            },
        };
    }

    private static JsonObject SerializeContext(FakeContext context, JsonArray? children)
    {
        JsonObject serialized = new()
        {
            ["context"] = context.Id,
            ["clientWindow"] = $"window-for-{context.Id}",
            ["originalOpener"] = null,
            ["url"] = context.Url,
            ["userContext"] = context.UserContextId,
            ["children"] = children,
        };
        if (context.ParentId is not null)
        {
            serialized["parent"] = context.ParentId;
        }

        return serialized;
    }

    private static (string Method, JsonObject Parameters) CreatedEvent(FakeContext context)
    {
        JsonObject parameters = SerializeContext(context, null);
        parameters["hasPlannedNavigation"] = false;
        return ("browsingContext.contextCreated", parameters);
    }

    private static (string Method, JsonObject Parameters) DestroyedEvent(FakeContext context)
    {
        return ("browsingContext.contextDestroyed", SerializeContext(context, null));
    }

    private JsonObject CreateUserContext()
    {
        lock (this.lockObject)
        {
            string userContextId = this.NextId("user-context");
            this.userContextIds.Add(userContextId);
            return new JsonObject() { ["userContext"] = userContextId };
        }
    }

    private JsonObject GetUserContexts()
    {
        lock (this.lockObject)
        {
            return new JsonObject() { ["userContexts"] = new JsonArray([.. this.userContextIds.Select(id => (JsonNode)new JsonObject() { ["userContext"] = id })]) };
        }
    }

    private FakeResponse RemoveUserContext(string userContextId)
    {
        lock (this.lockObject)
        {
            this.userContextIds.Remove(userContextId);
            return this.RemoveContexts(this.contexts.Where(context => context.UserContextId == userContextId && context.ParentId is null).ToList());
        }
    }

    private FakeResponse CreateContext(string userContextId)
    {
        lock (this.lockObject)
        {
            FakeContext context = new(this.NextId("context"), userContextId, null, "about:blank");
            this.contexts.Add(context);
            return new FakeResponse(new JsonObject() { ["context"] = context.Id, ["userContext"] = userContextId }, [CreatedEvent(context)]);
        }
    }

    // A reload navigates to the context's current URL.
    private FakeResponse Navigate(string contextId, string? url)
    {
        FakeContext context = this.SetUrl(contextId, url);
        JsonObject navigation = new()
        {
            ["context"] = contextId,
            ["navigation"] = this.NextId("navigation"),
            ["timestamp"] = 1790000000000,
            ["url"] = context.Url,
        };
        return new FakeResponse(new JsonObject() { ["navigation"] = navigation["navigation"]!.DeepClone(), ["url"] = context.Url }, [("browsingContext.navigationCommitted", navigation)]);
    }

    private FakeContext SetUrl(string contextId, string? url)
    {
        lock (this.lockObject)
        {
            int index = this.contexts.FindIndex(context => context.Id == contextId);
            FakeContext context = this.contexts[index] with { Url = url ?? this.contexts[index].Url };
            this.contexts[index] = context;
            return context;
        }
    }

    private FakeResponse CloseContext(string contextId)
    {
        lock (this.lockObject)
        {
            return this.RemoveContexts([.. this.contexts.Where(context => context.Id == contextId)]);
        }
    }

    // Called under the lock. A context's descendants are removed with it, and one event reports each top-level removal.
    private FakeResponse RemoveContexts(List<FakeContext> removed)
    {
        List<(string Method, JsonObject Parameters)> events = [];
        foreach (FakeContext context in removed)
        {
            events.Add(DestroyedEvent(context));
            this.RemoveWithDescendants(context.Id);
        }

        return new FakeResponse(new JsonObject(), events);
    }

    private void RemoveWithDescendants(string contextId)
    {
        foreach (FakeContext child in this.contexts.Where(context => context.ParentId == contextId).ToList())
        {
            this.RemoveWithDescendants(child.Id);
        }

        this.contexts.RemoveAll(context => context.Id == contextId);
    }

    private JsonObject GetTree(string? rootId, int? maxDepth)
    {
        lock (this.lockObject)
        {
            IEnumerable<FakeContext> roots = rootId is null ? this.contexts.Where(context => context.ParentId is null) : this.contexts.Where(context => context.Id == rootId);
            return new JsonObject() { ["contexts"] = new JsonArray([.. roots.Select(context => (JsonNode)this.SerializeTree(context, maxDepth))]) };
        }
    }

    // As the protocol does, children beyond the maximum depth are reported as null rather than as an empty list.
    private JsonObject SerializeTree(FakeContext context, int? remainingDepth)
    {
        JsonArray? children = remainingDepth == 0
            ? null
            : new JsonArray([.. this.contexts.Where(child => child.ParentId == context.Id).Select(child => (JsonNode)this.SerializeTree(child, remainingDepth - 1))]);
        return SerializeContext(context, children);
    }

    private string NextId(string prefix)
    {
        return $"{prefix}-{Interlocked.Increment(ref this.identifierCount)}";
    }
}
