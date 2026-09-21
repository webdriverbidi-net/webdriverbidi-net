// <copyright file="QuickReferenceRowSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Compiled counterparts for the table rows in docs/articles/quick-reference.md.
//
// A cheat-sheet row is a code span inside a table cell, which DocFX cannot fill from a region, so the
// row's text is duplicated here instead. docs/tools/validate-doc-regions.sh requires every row to
// appear in this repository's snippet code verbatim (whitespace aside), so a row that stops compiling,
// or that is edited without its counterpart, fails the build rather than misleading a reader.

namespace WebDriverBiDi.Docs.Code;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.DigitalCredentials;
using WebDriverBiDi.Input;
using WebDriverBiDi.Network;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;

/// <summary>
/// One method per row of the quick reference cheat sheet, holding that row's code verbatim.
/// </summary>
public static class QuickReferenceRowSamples
{
    /// <summary>
    /// Row: create driver.
    /// </summary>
    public static void CreateDriver()
    {
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Row: start connection.
    /// </summary>
    /// <param name="driver">The driver to start.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task StartConnection(BiDiDriver driver)
    {
        await driver.StartAsync("ws://localhost:9515/session/YOUR-SESSION-ID");
    }

    /// <summary>
    /// Row: check if started.
    /// </summary>
    /// <param name="driver">The driver to test.</param>
    public static void CheckIfStarted(BiDiDriver driver)
    {
        bool isStarted = driver.IsStarted;
    }

    /// <summary>
    /// Row: stop connection.
    /// </summary>
    /// <param name="driver">The driver to stop.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task StopConnection(BiDiDriver driver)
    {
        await driver.StopAsync();
    }

    /// <summary>
    /// Row: dispose.
    /// </summary>
    /// <param name="driver">The driver to dispose.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Dispose(BiDiDriver driver)
    {
        await driver.DisposeAsync();
    }

    /// <summary>
    /// Row: check status.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CheckStatus(BiDiDriver driver)
    {
        await driver.Session.StatusAsync();
    }

    /// <summary>
    /// Row: subscribe to events.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SubscribeToEvents(BiDiDriver driver)
    {
        SubscribeCommandParameters sub = new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName);
        await driver.Session.SubscribeAsync(sub);
    }

    /// <summary>
    /// Row: end session.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task EndSession(BiDiDriver driver)
    {
        await driver.Session.EndAsync();
    }

    /// <summary>
    /// Row: get context tree.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task GetContextTree(BiDiDriver driver)
    {
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync();
    }

    /// <summary>
    /// Row: create context.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CreateContext(BiDiDriver driver)
    {
        CreateCommandResult ctx = await driver.BrowsingContext.CreateAsync(new CreateCommandParameters(CreateType.Tab));
    }

    /// <summary>
    /// Row: navigate.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context to navigate.</param>
    /// <param name="url">The URL to navigate to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Navigate(BiDiDriver driver, string contextId, string url)
    {
        await driver.BrowsingContext.NavigateAsync(new NavigateCommandParameters(contextId, url) { Wait = ReadinessState.Complete });
    }

    /// <summary>
    /// Row: close context.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context to close.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CloseContext(BiDiDriver driver, string contextId)
    {
        await driver.BrowsingContext.CloseAsync(new CloseCommandParameters(contextId));
    }

    /// <summary>
    /// Row: capture screenshot.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context to capture.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CaptureScreenshot(BiDiDriver driver, string contextId)
    {
        await driver.BrowsingContext.CaptureScreenshotAsync(new CaptureScreenshotCommandParameters(contextId));
    }

    /// <summary>
    /// Row: evaluate expression.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context to evaluate in.</param>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task EvaluateExpression(BiDiDriver driver, string contextId, string expression)
    {
        EvaluateResult r = await driver.Script.EvaluateAsync(new EvaluateCommandParameters(expression, new ContextTarget(contextId), true));
    }

    /// <summary>
    /// Row: call function.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context to call the function in.</param>
    /// <param name="functionDeclaration">The function to call.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task CallFunction(BiDiDriver driver, string contextId, string functionDeclaration)
    {
        EvaluateResult r = await driver.Script.CallFunctionAsync(new CallFunctionCommandParameters(functionDeclaration, new ContextTarget(contextId), true));
    }

    /// <summary>
    /// Row: add preload script.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="script">The script to preload.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task AddPreloadScript(BiDiDriver driver, string script)
    {
        AddPreloadScriptCommandResult r = await driver.Script.AddPreloadScriptAsync(new AddPreloadScriptCommandParameters(script));
    }

    /// <summary>
    /// Row: get realms.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task GetRealms(BiDiDriver driver)
    {
        GetRealmsCommandResult realms = await driver.Script.GetRealmsAsync();
    }

    /// <summary>
    /// Row: add intercept.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task AddIntercept(BiDiDriver driver)
    {
        AddInterceptCommandParameters p = new AddInterceptCommandParameters(InterceptPhase.BeforeRequestSent);
        await driver.Network.AddInterceptAsync(p);
    }

    /// <summary>
    /// Row: add data collector.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context to collect data for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task AddDataCollector(BiDiDriver driver, string contextId)
    {
        AddDataCollectorCommandParameters p = new AddDataCollectorCommandParameters(1024 * 1024, DataType.Response);
        p.Contexts.Add(contextId);
        await driver.Network.AddDataCollectorAsync(p);
    }

    /// <summary>
    /// Row: continue request.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="requestId">The request to continue.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ContinueRequest(BiDiDriver driver, string requestId)
    {
        await driver.Network.ContinueRequestAsync(new ContinueRequestCommandParameters(requestId));
    }

    /// <summary>
    /// Row: provide response.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="requestId">The request to respond to.</param>
    /// <param name="body">The response body.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ProvideResponse(BiDiDriver driver, string requestId, string body)
    {
        ProvideResponseCommandParameters p = new ProvideResponseCommandParameters(requestId) { Body = BytesValue.FromString(body) };
        await driver.Network.ProvideResponseAsync(p);
    }

    /// <summary>
    /// Row: get cookies.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context whose cookies to read.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task GetCookies(BiDiDriver driver, string contextId)
    {
        GetCookiesCommandParameters p = new GetCookiesCommandParameters();
        p.Partition = new BrowsingContextPartitionDescriptor(contextId);
        GetCookiesCommandResult r = await driver.Storage.GetCookiesAsync(p);
    }

    /// <summary>
    /// Row: set cookie.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="domain">The cookie domain.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SetCookie(BiDiDriver driver, string name, string value, string domain)
    {
        await driver.Storage.SetCookieAsync(new SetCookieCommandParameters(new PartialCookie(name, BytesValue.FromString(value), domain)));
    }

    /// <summary>
    /// Row: delete cookies.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context whose cookies to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DeleteCookies(BiDiDriver driver, string contextId)
    {
        DeleteCookiesCommandParameters p = new DeleteCookiesCommandParameters();
        p.Partition = new BrowsingContextPartitionDescriptor(contextId);
        await driver.Storage.DeleteCookiesAsync(p);
    }

    /// <summary>
    /// Row: perform actions.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="contextId">The browsing context to act in.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task PerformActions(BiDiDriver driver, string contextId)
    {
        PerformActionsCommandParameters p = new PerformActionsCommandParameters(contextId);
        PointerSourceActions mouse = new PointerSourceActions();
        mouse.Actions.Add(new PointerMoveAction { X = 100, Y = 100 });
        p.Actions.Add(mouse);
        await driver.Input.PerformActionsAsync(p);
    }

    /// <summary>
    /// Row: decline credential request.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task DeclineCredentialRequest(BiDiDriver driver)
    {
        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(new SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction.Decline));
    }

    /// <summary>
    /// Row: respond with credential.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="credentialResponse">The response the virtual wallet returns.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RespondWithCredential(BiDiDriver driver, Dictionary<string, object?> credentialResponse)
    {
        SetVirtualWalletBehaviorCommandParameters p = new SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction.Respond) { Protocol = "openid4vp-v1-unsigned", Response = credentialResponse };
        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(p);
    }

    /// <summary>
    /// Row: leave request pending.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task LeaveRequestPending(BiDiDriver driver)
    {
        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(new SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction.Wait));
    }

    /// <summary>
    /// Row: clear wallet behavior.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ClearWalletBehavior(BiDiDriver driver)
    {
        await driver.DigitalCredentials.SetVirtualWalletBehaviorAsync(new SetVirtualWalletBehaviorCommandParameters(VirtualWalletAction.Clear));
    }
}
