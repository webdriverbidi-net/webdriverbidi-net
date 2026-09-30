// <copyright file="ScriptHost.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using System.Reflection;
using WebDriverBiDi.Script;

/// <summary>
/// Runs the library's scripts in a sandbox, isolated from the page's own scripts, with the Acquiescence element
/// state library and the page actions installed: by a preload script in documents loaded after the group starts,
/// and on first use in documents loaded before it.
/// </summary>
internal sealed class ScriptHost
{
    private const string InspectorName = "webdriverbidiAutomationInspector";
    private const string ActionsName = "webdriverbidiAutomationActions";
    private const string MissingMessage = "webdriverbidi-automation: scripts not installed";

    private static readonly Lazy<string> InstallFunction = new(() =>
        $"() => {{\n{ReadResource("acquiescence-library")}\nwindow.{InspectorName} = new Acquiescence.ElementStateInspector();\nwindow.{ActionsName} = {ReadResource("page-actions")};\n}}");

    private readonly BiDiDriver driver;
    private readonly string sandboxName;
    private string? preloadScriptId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptHost"/> class.
    /// </summary>
    /// <param name="driver">The driver that runs the scripts.</param>
    /// <param name="sandboxName">The name of the sandbox in which they run.</param>
    public ScriptHost(BiDiDriver driver, string sandboxName)
    {
        this.driver = driver;
        this.sandboxName = sandboxName;
    }

    /// <summary>
    /// Gets a value indicating whether the preload script has been added.
    /// </summary>
    public bool HasPreloadScript => this.preloadScriptId is not null;

    /// <summary>
    /// Adds the preload script that installs the library in each new document.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>A task that completes when the script is added.</returns>
    public async Task AddPreloadScriptAsync(CancellationToken cancellationToken)
    {
        AddPreloadScriptCommandParameters parameters = new(InstallFunction.Value) { Sandbox = this.sandboxName };
        this.preloadScriptId = (await this.driver.Script.AddPreloadScriptAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false)).PreloadScriptId;
    }

    /// <summary>
    /// Removes the preload script.
    /// </summary>
    /// <returns>A task that completes when the script is removed.</returns>
    public Task RemovePreloadScriptAsync()
    {
        return this.driver.Script.RemovePreloadScriptAsync(new RemovePreloadScriptCommandParameters(this.preloadScriptId!));
    }

    /// <summary>
    /// Calls a function in the sandbox of a browsing context, passing the library's inspector as its first
    /// argument, and installing the scripts first if the document does not have them.
    /// </summary>
    /// <param name="contextId">The ID of the browsing context.</param>
    /// <param name="functionDeclaration">The function, taking the inspector and then the arguments.</param>
    /// <param name="arguments">The arguments after the inspector.</param>
    /// <param name="budget">The time the call may take.</param>
    /// <returns>The function's result.</returns>
    public Task<RemoteValue> CallAsync(string contextId, string functionDeclaration, IReadOnlyList<LocalValue> arguments, TimeBudget budget)
    {
        return this.CallWithAsync(InspectorName, contextId, functionDeclaration, arguments, budget);
    }

    /// <summary>
    /// Calls a function in the sandbox of a browsing context, passing the page actions as its first argument, and
    /// installing the scripts first if the document does not have them.
    /// </summary>
    /// <param name="contextId">The ID of the browsing context.</param>
    /// <param name="functionDeclaration">The function, taking the page actions and then the arguments.</param>
    /// <param name="arguments">The arguments after the page actions.</param>
    /// <param name="budget">The time the call may take.</param>
    /// <returns>The function's result.</returns>
    public Task<RemoteValue> CallActionsAsync(string contextId, string functionDeclaration, IReadOnlyList<LocalValue> arguments, TimeBudget budget)
    {
        return this.CallWithAsync(ActionsName, contextId, functionDeclaration, arguments, budget);
    }

    private static string ReadResource(string name)
    {
        using Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(name)!;
        using StreamReader reader = new(resource);
        return reader.ReadToEnd();
    }

    private async Task<RemoteValue> CallWithAsync(string globalName, string contextId, string functionDeclaration, IReadOnlyList<LocalValue> arguments, TimeBudget budget)
    {
        string wrapped = $"(...args) => {{ const installed = window.{globalName}; if (!installed) {{ throw new Error('{MissingMessage}'); }} return ({functionDeclaration})(installed, ...args); }}";
        try
        {
            return await this.CallFunctionAsync(contextId, wrapped, arguments, budget).ConfigureAwait(false);
        }
        catch (ScriptException ex) when (ex.Message.Contains(MissingMessage))
        {
            await this.CallFunctionAsync(contextId, InstallFunction.Value, [], budget).ConfigureAwait(false);
            return await this.CallFunctionAsync(contextId, wrapped, arguments, budget).ConfigureAwait(false);
        }
    }

    private Task<RemoteValue> CallFunctionAsync(string contextId, string functionDeclaration, IReadOnlyList<LocalValue> arguments, TimeBudget budget)
    {
        return this.driver.Script.CallFunctionAsync(contextId, functionDeclaration, arguments, this.sandboxName, budget.Remaining, budget.CancellationToken);
    }
}
