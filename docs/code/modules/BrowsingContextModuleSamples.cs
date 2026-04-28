// <copyright file="BrowsingContextModuleSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/modules/browsing-context.md

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace WebDriverBiDi.Docs.Code.Modules;

using System.Collections.Generic;
using System.Security.Policy;
using WebDriverBiDi;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using BrowserCloseParams = WebDriverBiDi.Browser.CloseCommandParameters;
using CloseCommandParameters = WebDriverBiDi.BrowsingContext.CloseCommandParameters;

/// <summary>
/// Snippets for BrowsingContext module documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class BrowsingContextModuleSamples
{
    /// <summary>
    /// Accessing the module.
    /// </summary>
    public static void AccessingModule(BiDiDriver driver)
    {
        #region AccessingModule
        BrowsingContextModule browsingContext = driver.BrowsingContext;
        #endregion
    }

    /// <summary>
    /// Get all contexts.
    /// </summary>
    public static async Task GetAllContexts(BiDiDriver driver)
    {
        #region GetAllContexts
        GetTreeCommandParameters parameters = new GetTreeCommandParameters();
        GetTreeCommandResult result = await driver.BrowsingContext.GetTreeAsync(parameters);

        foreach (BrowsingContextInfo context in result.ContextTree)
        {
            Console.WriteLine($"Context ID: {context.BrowsingContextId}");
            Console.WriteLine($"URL: {context.Url}");
            Console.WriteLine($"Parent: {context.Parent ?? "none"}");
            Console.WriteLine($"Children: {context.Children.Count}");
        }
        #endregion
    }

    /// <summary>
    /// Get specific context.
    /// </summary>
    public static async Task GetSpecificContext(BiDiDriver driver, string contextId)
    {
        #region GetSpecificContext
        GetTreeCommandParameters parameters = new GetTreeCommandParameters()
        {
            RootBrowsingContextId = contextId  // Only get this context and its descendants
        };
        GetTreeCommandResult result = await driver.BrowsingContext.GetTreeAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Get only top-level contexts.
    /// </summary>
    public static async Task GetOnlyTopLevelContexts(BiDiDriver driver)
    {
        #region GetOnlyTop-LevelContexts
        GetTreeCommandParameters parameters = new GetTreeCommandParameters()
        {
            MaxDepth = 0  // Don't include child contexts (iframes)
        };
        GetTreeCommandResult result = await driver.BrowsingContext.GetTreeAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Create a new tab.
    /// </summary>
    public static async Task CreateNewTab(BiDiDriver driver)
    {
        #region CreateNewTab
        CreateCommandParameters parameters = new CreateCommandParameters(CreateType.Tab);
        CreateCommandResult result = await driver.BrowsingContext.CreateAsync(parameters);

        string newTabId = result.BrowsingContextId;
        Console.WriteLine($"Created tab: {newTabId}");
        #endregion
    }

    /// <summary>
    /// Create a new window.
    /// </summary>
    public static async Task CreateNewWindow(BiDiDriver driver)
    {
        #region CreateNewWindow
        CreateCommandParameters parameters = new CreateCommandParameters(CreateType.Window);
        CreateCommandResult result = await driver.BrowsingContext.CreateAsync(parameters);

        string newWindowId = result.BrowsingContextId;
        #endregion
    }

    public static async Task CreateContextInUserContext(BiDiDriver driver)
    {
        #region CreateContextinUserContext
        // First create a user context
        CreateUserContextCommandResult userContext =
            await driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters());

        // Create tab in that user context
        CreateCommandParameters parameters = new CreateCommandParameters(CreateType.Tab)
        {
            UserContextId = userContext.UserContextId
        };
        CreateCommandResult result = await driver.BrowsingContext.CreateAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Basic navigation.
    /// </summary>
    public static async Task BasicNavigation(BiDiDriver driver, string contextId)
    {
        #region BasicNavigation
        NavigateCommandParameters parameters = new NavigateCommandParameters(
            contextId,
            "https://example.com");

        NavigateCommandResult result = await driver.BrowsingContext.NavigateAsync(parameters);

        Console.WriteLine($"Navigation ID: {result.NavigationId}");
        Console.WriteLine($"URL: {result.Url}");
        #endregion
    }

    /// <summary>
    /// Wait for page load.
    /// </summary>
    public static async Task WaitForPageLoad(BiDiDriver driver, string contextId)
    {
        #region WaitforPageLoad
        NavigateCommandParameters parameters = new NavigateCommandParameters(
            contextId,
            "https://example.com")
        {
            Wait = ReadinessState.Complete  // Wait for full page load
        };
        await driver.BrowsingContext.NavigateAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Navigation with timeout.
    /// </summary>
    public static async Task NavigationWithTimeout(BiDiDriver driver, string contextId)
    {
        #region NavigationwithTimeout
        NavigateCommandParameters parameters = new NavigateCommandParameters(
            contextId,
            "https://example.com")
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.NavigateAsync(
            parameters,
            TimeSpan.FromSeconds(30));  // Fail if not loaded in 30 seconds
        #endregion
    }

    /// <summary>
    /// Back/forward navigation.
    /// </summary>
    public static async Task BackForwardNavigation(BiDiDriver driver, string contextId)
    {
        #region Back/ForwardNavigation
        // Navigate back
        TraverseHistoryCommandParameters backParams =
            new TraverseHistoryCommandParameters(contextId, -1);
        await driver.BrowsingContext.TraverseHistoryAsync(backParams);

        // Navigate forward
        TraverseHistoryCommandParameters forwardParams =
            new TraverseHistoryCommandParameters(contextId, 1);
        await driver.BrowsingContext.TraverseHistoryAsync(forwardParams);
        #endregion
    }

    /// <summary>
    /// Reload page.
    /// </summary>
    public static async Task ReloadPage(BiDiDriver driver, string contextId)
    {
        #region ReloadPage
        ReloadCommandParameters parameters = new ReloadCommandParameters(contextId);
        await driver.BrowsingContext.ReloadAsync(parameters);

        // Or wait for complete reload
        ReloadCommandParameters reloadParams = new ReloadCommandParameters(contextId)
        {
            Wait = ReadinessState.Complete
        };
        await driver.BrowsingContext.ReloadAsync(reloadParams);
        #endregion
    }

    /// <summary>
    /// Close a tab.
    /// </summary>
    public static async Task CloseTab(BiDiDriver driver, string contextId)
    {
        #region CloseTab
        CloseCommandParameters parameters = new CloseCommandParameters(contextId);
        await driver.BrowsingContext.CloseAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Close all tabs in user context.
    /// </summary>
    public static async Task CloseAllTabsInUserContext(BiDiDriver driver, string userContextId)
    {
        #region CloseAllTabsinUserContext
        // Get all contexts in user context
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
            new GetTreeCommandParameters());

        foreach (BrowsingContextInfo context in tree.ContextTree)
        {
            if (context.UserContextId == userContextId)
            {
                await driver.BrowsingContext.CloseAsync(
                    new CloseCommandParameters(context.BrowsingContextId));
            }
        }
        #endregion
    }

    /// <summary>
    /// Locate by CSS selector.
    /// </summary>
    public static async Task LocateByCssSelector(BiDiDriver driver, string contextId)
    {
        #region LocatebyCSSSelector
        LocateNodesCommandParameters parameters = new LocateNodesCommandParameters(
            contextId,
            new CssLocator("button.submit"));

        LocateNodesCommandResult result = await driver.BrowsingContext.LocateNodesAsync(parameters);

        foreach (NodeRemoteValue node in result.Nodes)
        {
            Console.WriteLine($"Found element: {node.SharedId}");
        }
        #endregion
    }

    /// <summary>
    /// Locate by XPath.
    /// </summary>
    public static async Task LocateByXPath(BiDiDriver driver, string contextId)
    {
        #region LocatebyXPath
        LocateNodesCommandParameters parameters = new LocateNodesCommandParameters(
            contextId,
            new XPathLocator("//button[@type='submit']"));

        LocateNodesCommandResult result = await driver.BrowsingContext.LocateNodesAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Locate with maximum results.
    /// </summary>
    public static async Task LocateWithMaxResults(BiDiDriver driver, string contextId)
    {
        #region LocatewithMaximumResults
        LocateNodesCommandParameters parameters = new LocateNodesCommandParameters(
            contextId,
            new CssLocator("input"))
        {
            MaxNodeCount = 5  // Return at most 5 elements
        };
        LocateNodesCommandResult result = await driver.BrowsingContext.LocateNodesAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Locate within element.
    /// </summary>
    public static async Task LocateWithinElement(BiDiDriver driver, string contextId)
    {
        #region LocateWithinElement
        LocateNodesCommandResult parentResult = await driver.BrowsingContext.LocateNodesAsync(
            new LocateNodesCommandParameters(contextId, new CssLocator("#container")));

        parentResult.Nodes[0].TryConvertTo(out NodeRemoteValue? parent);

        LocateNodesCommandParameters parameters = new LocateNodesCommandParameters(
            contextId,
            new CssLocator("button"))
        {
        };
        parameters.StartNodes.Add(parent.ToSharedReference());
        LocateNodesCommandResult result = await driver.BrowsingContext.LocateNodesAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Set viewport size.
    /// </summary>
    public static async Task SetViewportSize(BiDiDriver driver, string contextId)
    {
        #region SetViewportSize
        SetViewportCommandParameters parameters = new SetViewportCommandParameters
        {
            BrowsingContextId = contextId,
            Viewport = new Viewport
            {
                Width = 800,
                Height = 600
            },
            DevicePixelRatio = 1.0
        };

        await driver.BrowsingContext.SetViewportAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Reset viewport to default.
    /// </summary>
    public static async Task ResetViewportToDefault(BiDiDriver driver, string contextId)
    {
        #region ResetViewportToDefault
        SetViewportCommandParameters parameters = new SetViewportCommandParameters
        {
            BrowsingContextId = contextId,
            Viewport = SetViewportCommandParameters.ResetToDefaultViewport
        };

        await driver.BrowsingContext.SetViewportAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Enable CSP bypass.
    /// </summary>
    public static async Task EnableCSPBypass(BiDiDriver driver, string contextId)
    {
        #region EnableCSPBypass
        SetBypassCSPCommandParameters parameters = new SetBypassCSPCommandParameters
        {
            Contexts = new List<string> { contextId },
            Bypass = true
        };

        await driver.BrowsingContext.SetBypassCSPAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Clear CSP bypass override.
    /// </summary>
    public static async Task ClearCSPBypassOverride(BiDiDriver driver, string contextId)
    {
        #region ClearCSPBypassOverride
        SetBypassCSPCommandParameters parameters = SetBypassCSPCommandParameters.ResetBypassCSP;
        parameters.Contexts = new List<string> { contextId };

        await driver.BrowsingContext.SetBypassCSPAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Screenshot of entire viewport.
    /// </summary>
    public static async Task ScreenshotOfViewport(BiDiDriver driver, string contextId)
    {
        #region ScreenshotofViewport
        CaptureScreenshotCommandParameters parameters =
            new CaptureScreenshotCommandParameters(contextId);

        CaptureScreenshotCommandResult result =
            await driver.BrowsingContext.CaptureScreenshotAsync(parameters);

        // result.Data is base64-encoded PNG
        byte[] imageBytes = Convert.FromBase64String(result.Data);
        await File.WriteAllBytesAsync("screenshot.png", imageBytes);
        #endregion
    }

    /// <summary>
    /// Screenshot of specific element.
    /// </summary>
    public static async Task ScreenshotOfElement(BiDiDriver driver, string contextId)
    {
        #region ScreenshotofElement
        // First locate the element
        LocateNodesCommandResult locateResult = await driver.BrowsingContext.LocateNodesAsync(
            new LocateNodesCommandParameters(contextId, new CssLocator("#chart")));

        locateResult.Nodes[0].TryConvertTo(out NodeRemoteValue? element);

        // Capture element screenshot
        CaptureScreenshotCommandParameters parameters =
            new CaptureScreenshotCommandParameters(contextId)
            {
                Clip = new ElementClipRectangle(element.ToSharedReference())
            };

        CaptureScreenshotCommandResult result =
            await driver.BrowsingContext.CaptureScreenshotAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Clipped screenshot.
    /// </summary>
    public static async Task ClippedScreenshot(BiDiDriver driver, string contextId)
    {
        #region ClippedScreenshot
        CaptureScreenshotCommandParameters parameters =
            new CaptureScreenshotCommandParameters(contextId)
            {
                Clip = new BoxClipRectangle()
                {
                    X = 100,
                    Y = 100,
                    Width = 800,
                    Height = 600
                }
            };

        CaptureScreenshotCommandResult result =
            await driver.BrowsingContext.CaptureScreenshotAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Print to PDF.
    /// </summary>
    public static async Task PrintToPdf(BiDiDriver driver, string contextId)
    {
        #region PrinttoPDF
        PrintCommandParameters parameters = new PrintCommandParameters(contextId);
        PrintCommandResult result = await driver.BrowsingContext.PrintAsync(parameters);

        // result.Data is base64-encoded PDF
        byte[] pdfBytes = Convert.FromBase64String(result.Data);
        await File.WriteAllBytesAsync("page.pdf", pdfBytes);
        #endregion
    }

    /// <summary>
    /// PDF with custom settings.
    /// </summary>
    public static async Task PdfWithCustomSettings(BiDiDriver driver, string contextId)
    {
        #region PDFwithCustomSettings
        PrintCommandParameters parameters = new PrintCommandParameters(contextId)
        {
            Orientation = PrintOrientation.Landscape,
            Scale = 0.8,
            Background = true,  // Print background colors/images
            Page = new PrintPageParameters()
            {
                Height = 11,    // Inches
                Width = 8.5,
            },
            Margins = new PrintMarginParameters()
            {
                Top = 0.5,
                Bottom = 0.5,
                Left = 0.5,
                Right = 0.5
            },
        };
        PrintCommandResult result = await driver.BrowsingContext.PrintAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Accept alert/confirm.
    /// </summary>
    public static async Task AcceptAlert(BiDiDriver driver, string contextId)
    {
        #region AcceptAlert
        driver.BrowsingContext.OnUserPromptOpened.AddObserver((UserPromptOpenedEventArgs e) =>
        {
            Console.WriteLine($"Prompt: {e.Message}");
        });

        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.BrowsingContext.OnUserPromptOpened.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        // When prompt appears, handle it
        HandleUserPromptCommandParameters handleParams =
            new HandleUserPromptCommandParameters(contextId);
        handleParams.Accept = true;  // Click OK/Accept

        await driver.BrowsingContext.HandleUserPromptAsync(handleParams);
        #endregion
    }

    /// <summary>
    /// Dismiss prompt.
    /// </summary>
    public static async Task DismissPrompt(BiDiDriver driver, string contextId)
    {
        #region DismissPrompt
        HandleUserPromptCommandParameters parameters =
            new HandleUserPromptCommandParameters(contextId);
        parameters.Accept = false;  // Click Cancel
        await driver.BrowsingContext.HandleUserPromptAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Enter text in prompt.
    /// </summary>
    public static async Task EnterTextInPrompt(BiDiDriver driver, string contextId)
    {
        #region EnterTextinPrompt
        // For prompt() dialogs that accept user input
        HandleUserPromptCommandParameters parameters =
            new HandleUserPromptCommandParameters(contextId);
        parameters.Accept = true;
        parameters.UserText = "My input text";
        await driver.BrowsingContext.HandleUserPromptAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Bring tab to foreground.
    /// </summary>
    public static async Task ActivateTab(BiDiDriver driver, string contextId)
    {
        #region ActivateTab
        ActivateCommandParameters parameters = new ActivateCommandParameters(contextId);
        await driver.BrowsingContext.ActivateAsync(parameters);
        #endregion
    }

    /// <summary>
    /// Navigation events.
    /// </summary>
    public static void NavigationEvents(BiDiDriver driver)
    {
        #region NavigationEvents
        // Page load complete
        driver.BrowsingContext.OnLoad.AddObserver((NavigationEventArgs e) =>
        {
            Console.WriteLine($"Page loaded: {e.Url}");
        });

        // DOM ready
        driver.BrowsingContext.OnDomContentLoaded.AddObserver((NavigationEventArgs e) =>
        {
            Console.WriteLine($"DOM ready: {e.Url}");
        });

        // Navigation started
        driver.BrowsingContext.OnNavigationStarted.AddObserver((NavigationEventArgs e) =>
        {
            Console.WriteLine($"Navigation started to: {e.Url}");
        });

        // Navigation failed
        driver.BrowsingContext.OnNavigationFailed.AddObserver((NavigationEventArgs e) =>
        {
            Console.WriteLine($"Navigation failed: {e.Url}");
        });

        // Navigation committed to the session history
        driver.BrowsingContext.OnNavigationCommitted.AddObserver((NavigationEventArgs e) =>
        {
            Console.WriteLine($"Navigation committed: {e.Url}");
        });

        // History entry updated via pushState/replaceState
        driver.BrowsingContext.OnHistoryUpdated.AddObserver((HistoryUpdatedEventArgs e) =>
        {
            Console.WriteLine($"History updated: {e.Url}");
        });

        // Download about to begin
        driver.BrowsingContext.OnDownloadWillBegin.AddObserver((DownloadWillBeginEventArgs e) =>
        {
            Console.WriteLine($"Download starting: {e.SuggestedFileName} from {e.Url}");
        });

        // Download completed or failed
        driver.BrowsingContext.OnDownloadEnd.AddObserver((DownloadEndEventArgs e) =>
        {
            Console.WriteLine($"Download ended with status: {e.Status}");
            if (e.FilePath != null)
            {
                Console.WriteLine($"Saved to: {e.FilePath}");
            }
        });
        #endregion
    }

    /// <summary>
    /// Context lifecycle events.
    /// </summary>
    public static void ContextLifecycleEvents(BiDiDriver driver)
    {
        #region ContextLifecycleEvents
        // New tab/window/iframe created
        driver.BrowsingContext.OnContextCreated.AddObserver((BrowsingContextEventArgs e) =>
        {
            Console.WriteLine($"Context created: {e.BrowsingContextId}");
            Console.WriteLine($"URL: {e.Url}");
            Console.WriteLine($"Type: {e.OriginalOpener ?? "user-initiated"}");
        });

        // Tab/window closed
        driver.BrowsingContext.OnContextDestroyed.AddObserver((BrowsingContextEventArgs e) =>
        {
            Console.WriteLine($"Context destroyed: {e.BrowsingContextId}");
        });
        #endregion
    }

    /// <summary>
    /// User prompt events.
    /// </summary>
    public static void UserPromptEvents(BiDiDriver driver)
    {
        #region UserPromptEvents
        // Alert/confirm/prompt opened
        driver.BrowsingContext.OnUserPromptOpened.AddObserver((UserPromptOpenedEventArgs e) =>
        {
            Console.WriteLine($"Prompt type: {e.PromptType}");
            Console.WriteLine($"Message: {e.Message}");
        });

        // Prompt closed
        driver.BrowsingContext.OnUserPromptClosed.AddObserver((UserPromptClosedEventArgs e) =>
        {
            Console.WriteLine($"Prompt closed with accept={e.IsAccepted}");
            if (e.UserText != null)
            {
                Console.WriteLine($"User entered: {e.UserText}");
            }
        });
        #endregion
    }

    /// <summary>
    /// Wait for page load pattern.
    /// </summary>
    public static async Task WaitForPageLoadPattern(
        BiDiDriver driver,
        string contextId,
        string url)
    {
        #region WaitforPageLoadPattern
        SubscribeCommandParameters subscribe =
            new SubscribeCommandParameters(driver.BrowsingContext.OnLoad.EventName);
        await driver.Session.SubscribeAsync(subscribe);

        EventObserver<NavigationEventArgs> observer =
            driver.BrowsingContext.OnLoad.AddObserver((e) => { });

        observer.StartCapturing();
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, url));

        Task[] tasks = await observer.WaitForAsync(1, TimeSpan.FromSeconds(30));
        bool loaded = tasks.Length == 1;
        observer.StopCapturing();

        if (!loaded)
        {
            Console.WriteLine("Page load timeout!");
        }
        #endregion
    }

    /// <summary>
    /// Multi-tab pattern.
    /// </summary>
    public static async Task MultiTabPattern(BiDiDriver driver)
    {
        #region Multi-TabPattern
        // Open multiple tabs
        List<string> contextIds = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            CreateCommandResult result = await driver.BrowsingContext.CreateAsync(
                new CreateCommandParameters(CreateType.Tab));
            contextIds.Add(result.BrowsingContextId);
        }

        // Navigate each tab
        foreach (string contextId in contextIds)
        {
            await driver.BrowsingContext.NavigateAsync(
                new NavigateCommandParameters(contextId, $"https://example.com/page{contextIds.IndexOf(contextId)}"));
        }

        // Close all tabs
        foreach (string contextId in contextIds)
        {
            await driver.BrowsingContext.CloseAsync(
                new CloseCommandParameters(contextId));
        }
        #endregion
    }
}
