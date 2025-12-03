// <copyright file="ElementStateInspector.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

using System.Reflection;
using WebDriverBiDi.Script;

/// <summary>
/// Provides the code for the Acquiesence element state inspection library to be loaded into the
/// browser via a preload script.
/// </summary>
public class ElementStateInspector
{
    private readonly string sandboxName = "webdriverbidi";
    private readonly string inspectorObjectName = "webdriverInspector";
    private readonly BiDiDriver driver;
    private string preloadScriptId = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementStateInspector"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> object to which to add the inspector.</param>
    public ElementStateInspector(BiDiDriver driver)
    {
        this.driver = driver;
    }

    /// <summary>
    /// Adds the inspector to the driver as a preload script.
    /// </summary>
    /// <returns>A <see cref="Task"/> object containing information about the asynchronous operation.</returns>
    public async Task AddInspectorAsync()
    {
        string libraryScript = string.Empty;
        Assembly executingAssembly = Assembly.GetExecutingAssembly();
        using (Stream resourceStream = executingAssembly.GetManifestResourceStream("acquiescence-library"))
        {
            using StreamReader reader = new(resourceStream);
            libraryScript = reader.ReadToEnd();
        }

        string preloadScriptContent = $$"""
            {{libraryScript}}
            window.acquiescence = Acquiescence
            const { ElementStateInspector } = window.acquiescence;
            window.{{this.inspectorObjectName}} = new ElementStateInspector();
            """;

        AddPreloadScriptCommandParameters preloadScriptParams = new($"() => {{{preloadScriptContent}}}")
        {
            Sandbox = this.sandboxName,
        };
        AddPreloadScriptCommandResult addScriptResult = await this.driver.Script.AddPreloadScriptAsync(preloadScriptParams);
        this.preloadScriptId = addScriptResult.PreloadScriptId;
    }

    /// <summary>
    /// Removes the inspector from the driver.
    /// </summary>
    /// <returns>A <see cref="Task"/> object containing information about the asynchronous operation.</returns>
    public async Task RemoveInspectorAsync()
    {
        if (!string.IsNullOrEmpty(this.preloadScriptId))
        {
            await this.driver.Script.RemovePreloadScriptAsync(new RemovePreloadScriptCommandParameters(this.preloadScriptId));
        }
    }

    /// <summary>
    /// Waits for an element to be ready for interaction ready up to a timeout.
    /// </summary>
    /// <param name="contextId">The ID of the browsing context containing the element.</param>
    /// <param name="element">The <see cref="SharedReference"/> representing the element to wait to be ready for interaction.</param>
    /// <param name="interactionType">A <see cref="InteractionType"/> value representing the type of interaction for which we are waiting to be ready to perform..</param>
    /// <param name="timeout">The <see cref="TimeSpan"/> representing the maximum time to wait.</param>
    /// <returns>A <see cref="Task"/> object containing information about the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when an error occurs during the execution of the wait, including exceeding the timeout.</exception>
    public async Task WaitForInteractionReadyAsync(string contextId, SharedReference element, InteractionType interactionType, TimeSpan timeout)
    {
        string waitFunctionDefinition = $"(e, interactionType, timeout) => window.{this.inspectorObjectName}.waitForInteractionReady(e, interactionType, timeout)";
        ContextTarget contextTarget = new(contextId)
        {
            Sandbox = this.sandboxName,
        };
        CallFunctionCommandParameters waitFunctionParams = new(waitFunctionDefinition, contextTarget, true);
        waitFunctionParams.Arguments.Add(element);
        waitFunctionParams.Arguments.Add(LocalValue.String(interactionType.ToString().ToLowerInvariant()));
        waitFunctionParams.Arguments.Add(LocalValue.Number(timeout.TotalMilliseconds));
        EvaluateResult waitScriptResult = await this.driver.Script.CallFunctionAsync(waitFunctionParams);
        if (waitScriptResult.ResultType == EvaluateResultType.Exception)
        {
            // If the "waitForInteractionReady" script times out or experiences an
            // unexpected error, throw an exception
            throw new WebDriverBiDiException(((EvaluateResultException)waitScriptResult).ExceptionDetails.Text);
        }
    }

    /// <summary>
    /// Gets a value indicating whether an element is visible.
    /// </summary>
    /// <param name="contextId">The ID of the browsing context containing the element.</param>
    /// <param name="element">The <see cref="SharedReference"/> representing the element to check for visibility.</param>
    /// <returns><see langword="true"/> if the element is visible; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when an error occurs during the execution of the visibility script.</exception>
    public async Task<bool> IsElementVisibleAsync(string contextId, SharedReference element)
    {
        string visibleFunctionDefinition = $"(e) => window.{this.inspectorObjectName}.isElementVisible(e)";
        ContextTarget contextTarget = new(contextId)
        {
            Sandbox = this.sandboxName,
        };
        CallFunctionCommandParameters visibilityFunctionParams = new(visibleFunctionDefinition, contextTarget, true);
        visibilityFunctionParams.Arguments.Add(element);
        EvaluateResult visibilityScriptResult = await this.driver.Script.CallFunctionAsync(visibilityFunctionParams);
        if (visibilityScriptResult.ResultType == EvaluateResultType.Exception)
        {
            // If the "waitForInteractionReady" script times out or experiences an
            // unexpected error, throw an exception
            throw new WebDriverBiDiException(((EvaluateResultException)visibilityScriptResult).ExceptionDetails.Text);
        }

        // We know this is not an exception, so we can safely cast to ElementResultSuccess.
        EvaluateResultSuccess visibilityScriptResultSuccess = (EvaluateResultSuccess)visibilityScriptResult;
        return visibilityScriptResultSuccess.Result.ValueAs<bool>();
    }

    /// <summary>
    /// Gets a value indicating whether an element is enabled.
    /// </summary>
    /// <param name="contextId">The ID of the browsing context containing the element.</param>
    /// <param name="element">The <see cref="SharedReference"/> representing the element to check if it is enabled.</param>
    /// <returns><see langword="true"/> if the element is enabled; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown when an error occurs during the execution of the enablement detection script.</exception>
    public async Task<bool> IsElementEnabledAsync(string contextId, SharedReference element)
    {
        string visibleFunctionDefinition = $"(e) => window.{this.inspectorObjectName}.isElementDisabled(e)";
        ContextTarget contextTarget = new(contextId)
        {
            Sandbox = this.sandboxName,
        };
        CallFunctionCommandParameters visibilityFunctionParams = new(visibleFunctionDefinition, contextTarget, true);
        visibilityFunctionParams.Arguments.Add(element);
        EvaluateResult visibilityScriptResult = await this.driver.Script.CallFunctionAsync(visibilityFunctionParams);
        if (visibilityScriptResult.ResultType == EvaluateResultType.Exception)
        {
            // If the "waitForInteractionReady" script times out or experiences an
            // unexpected error, throw an exception
            throw new WebDriverBiDiException(((EvaluateResultException)visibilityScriptResult).ExceptionDetails.Text);
        }

        // We know this is not an exception, so we can safely cast to ElementResultSuccess.
        EvaluateResultSuccess visibilityScriptResultSuccess = (EvaluateResultSuccess)visibilityScriptResult;
        return !visibilityScriptResultSuccess.Result.ValueAs<bool>();
    }
}
