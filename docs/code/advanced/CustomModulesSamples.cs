// <copyright file="CustomModulesSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/advanced/custom-modules.md

#pragma warning disable CS0649, CS1591, CS8600, CS8602, CS8618, CS8604, CS8619

namespace WebDriverBiDi.Docs.Code.Advanced;

using System.Text.Json.Serialization;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;

/// <summary>
/// Snippets for custom modules documentation.
/// </summary>
public static class CustomModulesSamples
{
}

/// <summary>
/// Example minimal custom module.
/// </summary>
#region ModuleStructure
public class CustomModule : Module
{
    public const string CustomModuleName = "custom";

    public CustomModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => CustomModuleName;
}
#endregion

/// <summary>
/// Example command parameters - use override for MethodName.
/// </summary>
#region CommandParameters
public class MyCommandParameters : CommandParameters<MyCommandResult>
{
    public MyCommandParameters(string contextId, string value)
    {
        this.ContextId = contextId;
        this.Value = value;
    }

    [JsonPropertyName("context")]
    public string ContextId { get; }

    [JsonPropertyName("value")]
    public string Value { get; }

    public override string MethodName => "myCustom.myCommand";
}
#endregion

/// <summary>
/// Example command result.
/// </summary>
#region CommandResult
public record MyCommandResult : CommandResult
{
    [JsonPropertyName("success")]
    public bool Success { get; private set; }

    [JsonPropertyName("data")]
    public string Data { get; private set; } = string.Empty;
}
#endregion

/// <summary>
/// Example module with command method.
/// </summary>
#region CommandMethod
public class MyCustomModule : Module
{
    public const string MyCustomModuleName = "myCustom";

    public MyCustomModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => MyCustomModuleName;

    public async Task<MyCommandResult> MyCommandAsync(MyCommandParameters parameters)
    {
        return await this.Driver.ExecuteCommandAsync<MyCommandResult>(parameters);
    }
}
#endregion

public static class Registrar
{
    /// <summary>
    /// Register and use custom module.
    /// </summary>
    public static async Task RegisterAndUseModule(
        string webSocketUrl)
    {
#region RegisterandUseModule
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Register custom module (must be done before calling StartAsync)
        MyCustomModule customModule = new MyCustomModule(driver);
        driver.RegisterModule(customModule);
        await driver.StartAsync(webSocketUrl);

        // Access module
        var myModule = driver.GetModule<MyCustomModule>("myCustom");
#endregion
    }
}

/// <summary>
/// Page utilities module - WaitForElementAsync.
/// </summary>
#region PageUtilitiesModule
// Command Parameters
public class WaitForElementCommandParameters : CommandParameters<WaitForElementCommandResult>
{
    public override string MethodName => "script.evaluate";  // Use existing protocol command

    public WaitForElementCommandParameters(string contextId, string selector, int timeoutMs)
    {
        this.ContextId = contextId;
        this.Selector = selector;
        this.TimeoutMs = timeoutMs;
        
        // Build JavaScript that waits for element
        this.Expression = $$"""
            new Promise((resolve) => {
                const checkElement = () => {
                    const element = document.querySelector('{{selector}}');
                    if (element) {
                        resolve({ found: true, tagName: element.tagName });
                    } else {
                        setTimeout(checkElement, 100);
                    }
                };
                checkElement();
                setTimeout(() => resolve({ found: false }), {timeoutMs});
            })
            """;
        
        this.Target = new ContextTarget(contextId);
        this.AwaitPromise = true;
    }

    [JsonPropertyName("expression")]
    public string Expression { get; }

    [JsonPropertyName("target")]
    public ContextTarget Target { get; }

    [JsonPropertyName("awaitPromise")]
    public bool AwaitPromise { get; }

    [JsonIgnore]
    public string ContextId { get; }

    [JsonIgnore]
    public string Selector { get; }

    [JsonIgnore]
    public int TimeoutMs { get; }
}

// Command Result (uses standard EvaluateResult)
public record WaitForElementCommandResult : CommandResult
{
    [JsonPropertyName("result")]
    public RemoteValue? Result { get; private set; }
}

// Custom Module
public class PageUtilitiesModule : Module
{
    public const string PageUtilitiesModuleName = "pageUtilities";

    public PageUtilitiesModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => PageUtilitiesModuleName;

    /// <summary>
    /// Waits for an element to appear on the page.
    /// </summary>
    /// <param name="contextId">The browsing context ID.</param>
    /// <param name="selector">CSS selector for the element.</param>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <returns>True if element found, false otherwise.</returns>
    public async Task<bool> WaitForElementAsync(
        string contextId, 
        string selector, 
        TimeSpan timeout)
    {
        WaitForElementCommandParameters parameters = 
            new WaitForElementCommandParameters(
                contextId, 
                selector, 
                (int)timeout.TotalMilliseconds);

        EvaluateResult result = await this.Driver.ExecuteCommandAsync<EvaluateResult>(
            parameters);

        if (result is EvaluateResultSuccess success)
        {
            RemoteValueDictionary data = success.Result.ValueAs<RemoteValueDictionary>();
            return data["found"].ValueAs<bool>();
        }

        return false;
    }

    public async Task<string?> GetElementTextAsync(string contextId, string selector)
    {
        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            $"document.querySelector('{selector}')?.textContent",
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await this.Driver.ExecuteCommandAsync<EvaluateResult>(parameters);

        if (result is EvaluateResultSuccess success)
        {
            return success.Result.ValueAs<string>();
        }

        return null;
    }

    public async Task<bool> IsElementVisibleAsync(string contextId, string selector)
    {
        string script = $$"""
            (() => {
                const element = document.querySelector('{{selector}}');
                if (!element) return false;
                const style = window.getComputedStyle(element);
                return style.display !== 'none' && 
                        style.visibility !== 'hidden' && 
                        element.offsetParent !== null;
            })()
            """;

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            script,
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await this.Driver.ExecuteCommandAsync<EvaluateResult>(parameters);

        if (result is EvaluateResultSuccess success)
        {
            return success.Result.ValueAs<bool>();
        }

        return false;
    }

    public async Task<Dictionary<string, object>> GetPageMetricsAsync(string contextId)
    {
        string script = """
            ({
                title: document.title,
                url: window.location.href,
                linkCount: document.querySelectorAll('a').length,
                imageCount: document.querySelectorAll('img').length,
                scriptCount: document.querySelectorAll('script').length,
                styleCount: document.querySelectorAll('link[rel="stylesheet"]').length,
                readyState: document.readyState,
                height: document.documentElement.scrollHeight,
                width: document.documentElement.scrollWidth
            })
            """;

        EvaluateCommandParameters parameters = new EvaluateCommandParameters(
            script,
            new ContextTarget(contextId),
            true);

        EvaluateResult result = await this.Driver.ExecuteCommandAsync<EvaluateResult>(parameters);

        if (result is EvaluateResultSuccess success)
        {
            return ToDictionary(success.Result.ValueAs<RemoteValueDictionary>());
        }

        return new Dictionary<string, object>();
    }

    private static Dictionary<string, object> ToDictionary(RemoteValueDictionary dict)
    {
        Dictionary<string, object> result = new();
        foreach (KeyValuePair<object, RemoteValue> kvp in dict)
        {
            result[kvp.Key.ToString() ?? ""] = kvp.Value.Type switch
            {
                "string" => kvp.Value.ValueAs<string>() ?? "",
                "number" => kvp.Value.Value is long l ? (object)l : kvp.Value.ValueAs<double>(),
                "boolean" => kvp.Value.ValueAs<bool>(),
                "null" or "undefined" => (object?)null!,
                _ => kvp.Value.Value ?? (object)""
            };
        }
        return result;
    }
}
#endregion

public static class PageUtilitiesModuleUse
{
    public static async Task UsePageUtilitiesModule(string webSocketUrl)
    {
#region UsingPageUtilitiesModule
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        // Register custom module (must be done before calling StartAsync)
        PageUtilitiesModule pageUtils = new PageUtilitiesModule(driver);
        driver.RegisterModule(pageUtils);
        await driver.StartAsync(webSocketUrl);

        // Get context
        GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new());
        string contextId = tree.ContextTree[0].BrowsingContextId;

        // Navigate
        await driver.BrowsingContext.NavigateAsync(
            new NavigateCommandParameters(contextId, "https://example.com")
            { Wait = ReadinessState.Complete });

        // Use custom module
        bool elementFound = await pageUtils.WaitForElementAsync(
            contextId, 
            ".content", 
            TimeSpan.FromSeconds(10));

        if (elementFound)
        {
            string? text = await pageUtils.GetElementTextAsync(contextId, ".content");
            Console.WriteLine($"Content: {text}");
            
            bool isVisible = await pageUtils.IsElementVisibleAsync(contextId, ".content");
            Console.WriteLine($"Visible: {isVisible}");
        }

        // Get page metrics
        var metrics = await pageUtils.GetPageMetricsAsync(contextId);
        Console.WriteLine($"Links: {metrics["linkCount"]}");
        Console.WriteLine($"Images: {metrics["imageCount"]}");
#endregion
    }
}

/// <summary>
/// Test utilities module - TakeFullPageScreenshotAsync.
/// </summary>
#region TestUtilitiesModule
public class TestUtilitiesModule : Module
{
    public const string TestUtilitiesModuleName = "testUtilities";

    public TestUtilitiesModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => TestUtilitiesModuleName;

    public async Task<byte[]> TakeFullPageScreenshotAsync(string contextId)
    {
        // Get page dimensions
        string script = """
            ({
                width: Math.max(
                    document.documentElement.scrollWidth,
                    document.body.scrollWidth
                ),
                height: Math.max(
                    document.documentElement.scrollHeight,
                    document.body.scrollHeight
                )
            })
            """;

        EvaluateResult result = await this.Driver.ExecuteCommandAsync<EvaluateResult>(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success)
        {
            RemoteValueDictionary dimensions = success.Result.ValueAs<RemoteValueDictionary>();
            long width = dimensions["width"].ValueAs<long>();
            long height = dimensions["height"].ValueAs<long>();

            // Capture screenshot with full page dimensions
            CaptureScreenshotCommandParameters screenshotParams = 
                new CaptureScreenshotCommandParameters(contextId)
                {
                    Clip = new BoxClipRectangle
                    {
                        X = 0,
                        Y = 0,
                        Width = width,
                        Height = height
                    }
                };

            CaptureScreenshotCommandResult screenshot =
                await this.Driver.ExecuteCommandAsync<CaptureScreenshotCommandResult>(screenshotParams);

            return Convert.FromBase64String(screenshot.Data);
        }

        throw new Exception("Failed to get page dimensions");
    }

    public async Task<List<string>> GetAllLinksAsync(string contextId)
    {
        string script = "Array.from(document.querySelectorAll('a')).map(a => a.href)";

        EvaluateResult result = await this.Driver.ExecuteCommandAsync<EvaluateResult>(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), true));

        if (result is EvaluateResultSuccess success)
        {
            RemoteValueList links = success.Result.ValueAs<RemoteValueList>();
            return links.Select(l => l.ValueAs<string>()).ToList();
        }

        return new List<string>();
    }

    public async Task HighlightElementAsync(string contextId, string selector)
    {
        string script = $$"""
            (() => {
                const element = document.querySelector('{{selector}}');
                if (element) {
                    element.style.border = '3px solid red';
                    element.style.backgroundColor = 'yellow';
                    return true;
                }
                return false;
            })()
            """;

        await this.Driver.ExecuteCommandAsync<EvaluateResult>(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), false));
    }

    public async Task InjectCSSAsync(string contextId, string css)
    {
        string script = $$"""
            (() => {
                const style = document.createElement('style');
                style.textContent = `{{css}}`;
                document.head.appendChild(style);
            })()
            """;

        await this.Driver.ExecuteCommandAsync<EvaluateResult>(
            new EvaluateCommandParameters(script, new ContextTarget(contextId), false));
    }
}
#endregion

/// <summary>
/// Custom events module with observable event.
/// </summary>
#region CustomEventsModule
public class CustomEventsModule : Module
{
    public const string CustomModuleName = "customModule";
    private const string CustomEventName = "custom.eventOccurred";

    public CustomEventsModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
        // Register event with driver
        this.RegisterObservableEvent<CustomEventArgs>(this.OnCustomEvent);
    }

    public override string ModuleName => CustomModuleName;

    public ObservableEvent<CustomEventArgs> OnCustomEvent { get; } = 
        new ObservableEvent<CustomEventArgs>(CustomEventName);
}

public record CustomEventArgs : WebDriverBiDiEventArgs
{
    [JsonPropertyName("data")]
    public string Data { get; private set; } = string.Empty;
}
#endregion

/// <summary>
/// Custom event args.
/// </summary>
public record CustomModulesCustomEventArgs : WebDriverBiDiEventArgs
{
    [JsonPropertyName("data")]
    public string Data { get; private set; } = string.Empty;
}

/// <summary>
/// Namespace command pattern.
/// </summary>
#region NamespaceCommand
public class CustomModulesNamespacedCommandParameters : CommandParameters<MyCommandResult>
{
    [JsonIgnore]
    public override string MethodName => "myCompany.myModule.myCommand";  // Clear namespace

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    public CustomModulesNamespacedCommandParameters(string value)
    {
        this.Value = value;
    }
}
#endregion

public class Waiter
{
    /// <summary>
    /// Optional timeout with default.
    /// </summary>
#region OptionalTimeoutDefault
    public async Task<bool> WaitForElementAsync(
        string contextId, 
        string selector, 
        TimeSpan? timeout = null)  // Optional timeout
    {
        timeout = timeout ?? TimeSpan.FromSeconds(30);  // Default
        // ...
        return true; // Placeholder
    }
#endregion

    private readonly IBiDiCommandExecutor Driver;

    /// <summary>
    /// Handle errors gracefully in GetElementTextAsync.
    /// </summary>
#region ErrorHandlinginGetElementText
    public async Task<string?> GetElementTextAsync(string contextId, string selector)
    {
        try
        {
            EvaluateCommandParameters parameters = new EvaluateCommandParameters(
                $"document.querySelector('{selector}')?.textContent",
                new ContextTarget(contextId),
                true);

            EvaluateResult result = await this.Driver.ExecuteCommandAsync<EvaluateResult>(parameters);
            
            if (result is EvaluateResultSuccess success)
            {
                return success.Result.ValueAs<string>();
            }
            else if (result is EvaluateResultException exception)
            {
                Console.WriteLine($"Script error: {exception.ExceptionDetails.Text}");
            }
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"Command error: {ex.Message}");
        }
        
        return null;  // Graceful fallback
    }
#endregion
}

/// <summary>
/// Experimental module with protocol extension.
/// </summary>
#region ExperimentalModule
public class ExperimentalCommandParameters : CommandParameters<ExperimentalCommandResult>
{
    public ExperimentalCommandParameters(string parameter)
    {
        // Use actual protocol command name
        this.Parameter = parameter;
    }

    public override string MethodName => "experimental.newCommand";

    [JsonPropertyName("parameter")]
    public string Parameter { get; }
}

public record ExperimentalCommandResult : CommandResult
{
    [JsonPropertyName("result")]
    public string Result { get; private set; } = string.Empty;
}

// Implement in module
public class ExperimentalModule : Module
{
    public const string ExperimentalModuleName = "experimental";

    public ExperimentalModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
    }

    public override string ModuleName => ExperimentalModuleName;

    public async Task<ExperimentalCommandResult> NewCommandAsync(
        ExperimentalCommandParameters parameters)
    {
        return await this.Driver.ExecuteCommandAsync<ExperimentalCommandResult>(
            parameters);
    }
}
#endregion

#pragma warning restore CS1591, CS8600, CS8602, CS8618
