// <copyright file="QuickReferenceSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/quick-reference.md

namespace WebDriverBiDi.Docs.Code;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;

/// <summary>
/// Snippets for quick reference cheat sheet. Compiled at build time to prevent API drift.
/// </summary>
public static class QuickReferenceSamples
{
    /// <summary>
    /// Driver lifecycle.
    /// </summary>
    public static async Task DriverLifecycle()
    {
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));
        await driver.StartAsync("ws://localhost:9222/devtools/browser/YOUR-BROWSER-ID");
        if (driver.IsStarted)
        {
            // ...
        }

        await driver.StopAsync();
        await driver.DisposeAsync();
    }

    /// <summary>
    /// Session status and subscribe.
    /// </summary>
    public static async Task SessionStatusAndSubscribe(BiDiDriver driver, string contextId)
    {
        StatusCommandResult status = await driver.Session.StatusAsync(null);

        SubscribeCommandParameters sub = new SubscribeCommandParameters(
            driver.Network.OnBeforeRequestSent.EventName);
        await driver.Session.SubscribeAsync(sub);

        await driver.Session.EndAsync(null);
    }

    /// <summary>
    /// Browsing context operations.
    /// </summary>
    public static async Task BrowsingContextOperations(BiDiDriver driver, string url)
    {
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(null);
        CreateCommandResult ctx = await driver.BrowsingContext.CreateAsync(
            new CreateCommandParameters(CreateType.Tab));
        string contextId = ctx.BrowsingContextId;

        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, url) { Wait = ReadinessState.Complete });
        await driver.BrowsingContext.CloseAsync(new CloseCommandParameters(contextId));
        await driver.BrowsingContext.CaptureScreenshotAsync(
            new CaptureScreenshotCommandParameters(contextId));
    }

    /// <summary>
    /// Script evaluate and call function.
    /// </summary>
    public static async Task ScriptEvaluate(BiDiDriver driver, string contextId, string expression)
    {
        EvaluateResult r = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(expression, new ContextTarget(contextId), true));
    }

    /// <summary>
    /// Network add intercept and continue.
    /// </summary>
    public static async Task NetworkAddIntercept(BiDiDriver driver, string contextId)
    {
        AddInterceptCommandParameters p = new AddInterceptCommandParameters();
        p.Phases.Add(InterceptPhase.BeforeRequestSent);
        p.BrowsingContextIds = new List<string> { contextId };
        AddInterceptCommandResult result = await driver.Network.AddInterceptAsync(p);
    }

    /// <summary>
    /// Storage get and set cookies.
    /// </summary>
    public static async Task StorageCookies(BiDiDriver driver, string contextId, string name, string value, string domain)
    {
        GetCookiesCommandParameters getParams = new GetCookiesCommandParameters();
        getParams.Partition = new BrowsingContextPartitionDescriptor(contextId);
        GetCookiesCommandResult getResult = await driver.Storage.GetCookiesAsync(getParams);

        await driver.Storage.SetCookieAsync(
            new SetCookieCommandParameters(new PartialCookie(name, BytesValue.FromString(value), domain)));

        DeleteCookiesCommandParameters deleteParams = new DeleteCookiesCommandParameters();
        deleteParams.Partition = new BrowsingContextPartitionDescriptor(contextId);
        await driver.Storage.DeleteCookiesAsync(deleteParams);
    }

    /// <summary>
    /// Input perform actions - mouse click.
    /// </summary>
    public static async Task InputPerformActions(BiDiDriver driver, string contextId)
    {
        PerformActionsCommandParameters p = new PerformActionsCommandParameters(contextId);
        PointerSourceActions mouseSource = new PointerSourceActions
        {
            Parameters = new PointerParameters { PointerType = PointerType.Mouse },
        };
        mouseSource.Actions.Add(new PointerMoveAction { X = 100, Y = 100 });
        mouseSource.Actions.Add(new PointerDownAction(0));
        mouseSource.Actions.Add(new PointerUpAction(0));
        p.Actions.Add(mouseSource);
        await driver.Input.PerformActionsAsync(p);
    }

    /// <summary>
    /// Event subscription pattern.
    /// </summary>
    public static async Task EventSubscriptionPattern(BiDiDriver driver)
    {
        #region EventSubscriptionPattern
        // 1. Add observer
        driver.Network.OnBeforeRequestSent.AddObserver((e) => Console.WriteLine(e.Request.Url));

        // 2. Subscribe via session
        SubscribeCommandParameters sub =
            new SubscribeCommandParameters(driver.Network.OnBeforeRequestSent.EventName);
        await driver.Session.SubscribeAsync(sub);
        #endregion
    }

    /// <summary>
    /// Error configuration.
    /// </summary>
    public static void ErrorConfiguration(BiDiDriver driver)
    {
        #region ErrorConfiguration
        // Fail fast during development
        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate;
        driver.ProtocolErrorBehavior = TransportErrorBehavior.Terminate;

        // Collect for debugging
        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        driver.ProtocolErrorBehavior = TransportErrorBehavior.Collect;
        #endregion
    }
}
