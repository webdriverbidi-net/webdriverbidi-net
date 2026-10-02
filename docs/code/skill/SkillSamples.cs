// <copyright file="SkillSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>

// Compiled counterparts for the code blocks in skills/webdriverbidi-net, which coding agents read as plain files, so
// their samples cannot be region references. Each block names the region it mirrors in a
// '<!-- readme-csharp: path#Region -->' marker, and docs/tools/validate-doc-regions.sh compares the two.

namespace WebDriverBiDi.Docs.Code.Skill;

using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for the WebDriverBiDi.NET agent skill. Compiled at build time to prevent API drift.
/// </summary>
public static class SkillSamples
{
    /// <summary>
    /// Connecting to a browser that has no session yet.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ConnectWithNewSession()
    {
        #region ConnectWithNewSession
        // Firefox started with --remote-debugging-port=9222 serves WebDriver BiDi at /session, with no session yet.
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30));
        await driver.StartAsync("ws://localhost:9222/session");
        await driver.Session.NewSessionAsync(new NewCommandParameters());
        try
        {
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;
            await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters(contextId, "https://example.com") { Wait = ReadinessState.Complete });
        }
        finally
        {
            await driver.StopAsync();
        }
        #endregion
    }

    /// <summary>
    /// Connecting to a session a driver executable created.
    /// </summary>
    /// <param name="webSocketUrl">The webSocketUrl capability of the driver's new-session response.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ConnectToDriverSession(string webSocketUrl)
    {
        #region ConnectToDriverSession
        // The session already exists, so NewSessionAsync must not be called.
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(30));
        await driver.StartAsync(webSocketUrl);
        #endregion
    }

    /// <summary>
    /// Sending a command with a timeout and a cancellation token.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="contextId">A browsing context.</param>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Commands(BiDiDriver driver, string contextId, CancellationToken cancellationToken)
    {
        #region Commands
        NavigateCommandParameters navigate = new(contextId, "https://example.com/report") { Wait = ReadinessState.Interactive };
        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(navigate, TimeSpan.FromMinutes(2), cancellationToken);
        Console.WriteLine(result.Url);
        #endregion
    }

    /// <summary>
    /// Receiving events.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Events(BiDiDriver driver)
    {
        #region Events
        // 1. Add the observer. A handler that awaits anything runs asynchronously, or it blocks every message.
        using EventObserver<BeforeRequestSentEventArgs> observer = driver.Network.OnBeforeRequestSent.AddObserver(
            async e =>
            {
                await Task.Yield();
                await File.AppendAllTextAsync("requests.log", e.Request.Url + Environment.NewLine);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // 2. Subscribe, naming the event through its observable event; without this, the browser sends nothing.
        SubscribeCommandResult subscription = await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName));
        #endregion
    }

    /// <summary>
    /// Waiting for asynchronous handlers to finish.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="navigate">A navigation that raises log entries.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CapturedTasks(BiDiDriver driver, NavigateCommandParameters navigate)
    {
        #region CapturedTasks
        using EventObserver<EntryAddedEventArgs> observer = driver.Log.OnEntryAdded.AddObserver(
            async e =>
            {
                await Task.Yield();
                await File.AppendAllTextAsync("console.log", e.Text + Environment.NewLine);
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        await driver.Session.SubscribeAsync(new SubscribeCommandParameters(driver.Log.OnEntryAdded.EventName));

        observer.StartCapturingTasks();
        await driver.BrowsingContext.NavigateAsync(navigate);

        // True if two entries arrived and their handlers finished within ten seconds.
        bool done = await observer.WaitForCapturedTasksCompleteAsync(2, TimeSpan.FromSeconds(10));
        observer.StopCapturingTasks();
        #endregion
    }

    /// <summary>
    /// Running a script and reading its result.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="contextId">A browsing context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Scripts(BiDiDriver driver, string contextId)
    {
        #region Scripts
        EvaluateResult result = await driver.Script.EvaluateAsync(new EvaluateCommandParameters("document.title", new ContextTarget(contextId), true));
        if (result is EvaluateResultException failure)
        {
            throw new InvalidOperationException($"The script threw: {failure.ExceptionDetails.Text}");
        }

        RemoteValue value = ((EvaluateResultSuccess)result).Result;

        // As<T>() converts, or throws if the value is not that type; TryAs<T>() tests without throwing.
        string title = value.As<StringRemoteValue>().Value;
        if (value.TryAs(out NumberRemoteValue? number))
        {
            Console.WriteLine(number.Value);
        }
        #endregion
    }

    /// <summary>
    /// Surfacing handler and protocol errors during development.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ErrorBehaviors()
    {
        #region ErrorBehaviors
        BiDiDriver driver = new(TimeSpan.FromSeconds(30));

        // By default these errors are only reported through the driver's diagnostic events. Terminate makes the next
        // command throw them.
        driver.TransportConfiguration.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
        driver.TransportConfiguration.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;
        driver.TransportConfiguration.UnknownMessageBehavior = TransportErrorBehavior.Terminate;
        driver.TransportConfiguration.UnexpectedErrorBehavior = TransportErrorBehavior.Terminate;
        #endregion
        await driver.DisposeAsync();
    }

    /// <summary>
    /// Handling a command the browser rejects.
    /// </summary>
    /// <param name="driver">A connected driver.</param>
    /// <param name="navigate">A navigation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CommandErrors(BiDiDriver driver, NavigateCommandParameters navigate)
    {
        #region CommandErrors
        try
        {
            await driver.BrowsingContext.NavigateAsync(navigate);
        }
        catch (WebDriverBiDiCommandException ex) when (ex.ErrorCode == ErrorCode.NoSuchFrame)
        {
            // The browser rejected the command; ErrorCode is the protocol's error.
            Console.WriteLine(ex.Message);
        }
        catch (WebDriverBiDiTimeoutException)
        {
            // No response came within the timeout.
        }
        #endregion
    }
}
