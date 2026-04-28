// <copyright file="FormSubmissionSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for license information.
// </copyright>
// Code snippets for docs/articles/examples/form-submission.md

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace WebDriverBiDi.Docs.Code.Examples;

using System.Collections.Generic;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for form submission documentation. Compiled at build time to prevent API drift.
/// </summary>
public static class FormSubmissionSamples
{
    /// <summary>
    /// Subscribe to navigation events.
    /// </summary>
    public static async Task SubscribeToNavigationEvents(BiDiDriver driver)
    {
        #region SubscribetoNavigationEvents
        SubscribeCommandParameters subscribe = new SubscribeCommandParameters(
            [
                driver.BrowsingContext.OnNavigationStarted.EventName,
                driver.BrowsingContext.OnLoad.EventName,
            ]
        );
        await driver.Session.SubscribeAsync(subscribe);
        #endregion
    }

    /// <summary>
    /// Find form elements using Script.EvaluateAsync.
    /// </summary>
    public static async Task FindFormElements(BiDiDriver driver, Target target)
    {
        #region FindFormElements
        string findInputScript = "document.querySelector('input[name=\"custname\"]')";
        EvaluateResult inputResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(findInputScript, target, true));
        #endregion
    }

    /// <summary>
    /// Click to focus using PointerSourceActions.
    /// </summary>
    public static async Task ClickToFocus(
        BiDiDriver driver,
        string contextId,
        NodeRemoteValue inputElement)
    {
        #region ClicktoFocus
        PerformActionsCommandParameters clickParams = new PerformActionsCommandParameters(contextId);

        PointerSourceActions mouseSource = new PointerSourceActions
        {
            Parameters = new PointerParameters { PointerType = PointerType.Mouse },
        };
        mouseSource.Actions.Add(new PointerMoveAction
        {
            X = 0,
            Y = 0,
            Origin = Origin.Element(inputElement.ToSharedReference()),
        });
        mouseSource.Actions.Add(new PointerDownAction(0));
        mouseSource.Actions.Add(new PointerUpAction(0));

        clickParams.Actions.Add(mouseSource);
        await driver.Input.PerformActionsAsync(clickParams);
        #endregion
    }

    /// <summary>
    /// Send keyboard input.
    /// </summary>
    public static async Task SendKeyboardInput(
        BiDiDriver driver,
        string contextId,
        string text)
    {
        #region SendKeyboardInput
        KeySourceActions keyboard = new KeySourceActions();
        foreach (char c in text)
        {
            keyboard.Actions.Add(new KeyDownAction(c.ToString()));
            keyboard.Actions.Add(new KeyUpAction(c.ToString()));
        }

        PerformActionsCommandParameters typeParams = new PerformActionsCommandParameters(contextId);
        typeParams.Actions.Add(keyboard);
        await driver.Input.PerformActionsAsync(typeParams);
        #endregion
    }

    /// <summary>
    /// Wait for form submission using event observer.
    /// </summary>
    public static async Task WaitForFormSubmission(BiDiDriver driver)
    {
        #region WaitforFormSubmission
        EventObserver<NavigationEventArgs> navObserver =
            driver.BrowsingContext.OnLoad.AddObserver((e) => { });

        navObserver.StartCapturingTasks();

        // Click submit button...

        Task[] tasks = await navObserver.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
        bool navCompleted = tasks.Length == 1;

        navObserver.StopCapturingTasks();
        #endregion
    }

    /// <summary>
    /// Find input field using LocateNodes.
    /// </summary>
    public static async Task FindInputWithLocateNodes(
        BiDiDriver driver,
        string contextId)
    {
        #region FindInputwithLocateNodes
        // Find input field using CSS selector
        LocateNodesCommandParameters locateParams = new LocateNodesCommandParameters(
            contextId,
            new CssLocator("input[name='custname']"));

        LocateNodesCommandResult locateResult =
            await driver.BrowsingContext.LocateNodesAsync(locateParams);

        if (locateResult.Nodes.Count > 0)
        {
            RemoteValue inputElement = locateResult.Nodes[0];
            // Continue with clicking and typing...
        }
        #endregion
    }

    /// <summary>
    /// Submit with Enter key. Uses Unicode U+E007 for Enter key.
    /// </summary>
    public static async Task SubmitWithEnterKey(BiDiDriver driver, string contextId)
    {
        #region SubmitwithEnterKey
        // After typing in the last field, press Enter
        // Keys.Enter is a hypothetical constant representing the Enter key.
        // In practice, use "\uE007" or appropriate value for your implementation.
        KeySourceActions keyboard = new KeySourceActions();
        keyboard.Actions.Add(new KeyDownAction(Keys.Enter));
        keyboard.Actions.Add(new KeyUpAction(Keys.Enter));

        PerformActionsCommandParameters submitParams = new PerformActionsCommandParameters(contextId);
        submitParams.Actions.Add(keyboard);
        await driver.Input.PerformActionsAsync(submitParams);
        #endregion
    }

    /// <summary>
    /// Submit form with JavaScript.
    /// </summary>
    public static async Task SubmitFormWithJavaScript(BiDiDriver driver, Target target)
    {
        #region SubmitFormwithJavaScript
        // Find and fill fields...

        // Submit form with JavaScript
        string submitScript = "document.querySelector('form').submit()";
        await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(submitScript, target, false));
        #endregion
    }

    /// <summary>
    /// Find and click checkbox.
    /// </summary>
    public static async Task FindAndClickCheckbox(BiDiDriver driver, string contextId)
    {
        #region FindandClickCheckbox
        Target target = new ContextTarget(contextId);
        // Find checkbox
        EvaluateResult checkboxResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('input[type=\"checkbox\"]')",
                target,
                true));

        if (checkboxResult is EvaluateResultSuccess checkboxSuccess &&
            checkboxSuccess.Result is NodeRemoteValue checkboxElement)
        {
            // Click to toggle
            // PointerSource is a hypothetical helper class to build input actions.
            PerformActionsCommandParameters clickParams =
                new PerformActionsCommandParameters(contextId);
            PointerSource mouse = new PointerSource("mouse", PointerType.Mouse);
            mouse.CreatePointerMoveToElement(
                checkboxElement.ToSharedReference(),
                0, 0,
                TimeSpan.Zero);
            mouse.CreatePointerDown(MouseButton.Left);
            mouse.CreatePointerUp(MouseButton.Left);
            clickParams.Actions.Add(mouse.ToSourceActions());
            await driver.Input.PerformActionsAsync(clickParams);
        }
        #endregion
    }

    /// <summary>
    /// Set dropdown value with JavaScript.
    /// </summary>
    public static async Task SetDropdownValue(BiDiDriver driver, Target target)
    {
        #region SetDropdownValue
        // Set dropdown value with JavaScript
        string setSelectScript = @"
            const select = document.querySelector('select[name=""delivery""]');
            select.value = 'express';
            select.dispatchEvent(new Event('change', { bubbles: true }));
        ";

        await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(setSelectScript, target, false));
        #endregion
    }

    /// <summary>
    /// Complete form submission example - full flow from connect through verification.
    /// Assumes driver is already connected. Use https://httpbin.org/forms/post as the form page.
    /// </summary>
    public static async Task RunCompleteFormSubmissionExample()
    {
        #region CompleteFormSubmissionExample
        string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";
        BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

        try
        {
            await driver.StartAsync(webSocketUrl);
            Console.WriteLine("Connected to browser");

            // Subscribe to navigation events to track form submission
            SubscribeCommandParameters subscribe =
                new SubscribeCommandParameters(driver.BrowsingContext.OnNavigationStarted.EventName);
            subscribe.Events.Add(driver.BrowsingContext.OnLoad.EventName);
            await driver.Session.SubscribeAsync(subscribe);

            // Get browsing context
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(
                new GetTreeCommandParameters());
            string contextId = tree.ContextTree[0].BrowsingContextId;

            // Navigate to form page
            Console.WriteLine("Navigating to form...");
            NavigateCommandParameters navParams = new NavigateCommandParameters(
                contextId,
                "https://httpbin.org/forms/post")
            {
                Wait = ReadinessState.Complete
            };
            await driver.BrowsingContext.NavigateAsync(navParams);

            // Find the customer name input field
            Console.WriteLine("Finding form fields...");
            string findInputScript = "document.querySelector('input[name=\"custname\"]')";
            EvaluateCommandParameters evalParams = new EvaluateCommandParameters(
                findInputScript,
                new ContextTarget(contextId),
                true);

            EvaluateResult inputResult = await driver.Script.EvaluateAsync(evalParams);

            if (inputResult is EvaluateResultSuccess inputSuccess)
            {
                RemoteValue inputElementRemoteValue = inputSuccess.Result;
                inputElementRemoteValue.TryConvertTo(out NodeRemoteValue inputElement);
                Console.WriteLine($"Found input element: {inputElement.SharedId}");

                // Click the input to focus it
                Console.WriteLine("Clicking input field...");
                PerformActionsCommandParameters clickParams =
                    new PerformActionsCommandParameters(contextId);

                // PointerSource and KeySource are hypothetical helper classes to build input actions.
                PointerSource mouse = new PointerSource("mouse", PointerType.Mouse);
                mouse.CreatePointerMoveToElement(
                    inputElement.ToSharedReference(),
                    0, 0,
                    TimeSpan.Zero);
                mouse.CreatePointerDown(MouseButton.Left);
                mouse.CreatePointerUp(MouseButton.Left);

                clickParams.Actions.Add(mouse.ToSourceActions());
                await driver.Input.PerformActionsAsync(clickParams);

                // Type the customer name
                Console.WriteLine("Typing customer name...");
                PerformActionsCommandParameters typeParams =
                    new PerformActionsCommandParameters(contextId);

                KeySource keyboard = new KeySource("keyboard");
                string customerName = "John Doe";

                foreach (char c in customerName)
                {
                    keyboard.CreateKeyDown(c.ToString());
                    keyboard.CreateKeyUp(c.ToString());
                }

                typeParams.Actions.Add(keyboard.ToSourceActions());
                await driver.Input.PerformActionsAsync(typeParams);

                // Find and fill the telephone number field
                Console.WriteLine("Filling telephone field...");
                string findPhoneScript = "document.querySelector('input[name=\"custtel\"]')";
                EvaluateResult phoneResult = await driver.Script.EvaluateAsync(
                    new EvaluateCommandParameters(
                        findPhoneScript,
                        new ContextTarget(contextId),
                        true));

                if (phoneResult is EvaluateResultSuccess phoneSuccess)
                {
                    phoneSuccess.Result.TryConvertTo(out NodeRemoteValue? phoneElement);

                    // Click phone field
                    PerformActionsCommandParameters clickPhoneParams =
                        new PerformActionsCommandParameters(contextId);
                    PointerSource mouse2 = new PointerSource("mouse", PointerType.Mouse);
                    mouse2.CreatePointerMoveToElement(
                        phoneElement.ToSharedReference(),
                        0, 0,
                        TimeSpan.Zero);
                    mouse2.CreatePointerDown(MouseButton.Left);
                    mouse2.CreatePointerUp(MouseButton.Left);
                    clickPhoneParams.Actions.Add(mouse2.ToSourceActions());
                    await driver.Input.PerformActionsAsync(clickPhoneParams);

                    // Type phone number
                    PerformActionsCommandParameters typePhoneParams =
                        new PerformActionsCommandParameters(contextId);
                    KeySource keyboard2 = new KeySource("keyboard");
                    string phoneNumber = "555-1234";

                    foreach (char c in phoneNumber)
                    {
                        keyboard2.CreateKeyDown(c.ToString());
                        keyboard2.CreateKeyUp(c.ToString());
                    }

                    typePhoneParams.Actions.Add(keyboard2.ToSourceActions());
                    await driver.Input.PerformActionsAsync(typePhoneParams);
                }

                // Find and click the submit button
                Console.WriteLine("Finding submit button...");
                string findButtonScript = "document.querySelector('button[type=\"submit\"]')";
                EvaluateResult buttonResult = await driver.Script.EvaluateAsync(
                    new EvaluateCommandParameters(
                        findButtonScript,
                        new ContextTarget(contextId),
                        true));

                if (buttonResult is EvaluateResultSuccess buttonSuccess)
                {
                    RemoteValue buttonElementRemoteValue = buttonSuccess.Result;
                    buttonElementRemoteValue.TryConvertTo(out NodeRemoteValue buttonElement);
                    Console.WriteLine($"Found submit button: {buttonElement.SharedId}");

                    // Set up navigation observer
                    EventObserver<NavigationEventArgs> navObserver =
                        driver.BrowsingContext.OnLoad.AddObserver((e) =>
                        {
                            Console.WriteLine($"Navigation complete to: {e.Url}");
                        });

                    navObserver.StartCapturingTasks();

                    // Click submit button
                    Console.WriteLine("Clicking submit button...");
                    PerformActionsCommandParameters clickSubmitParams =
                        new PerformActionsCommandParameters(contextId);
                    PointerSource mouse3 = new PointerSource("mouse", PointerType.Mouse);
                    mouse3.CreatePointerMoveToElement(
                        buttonElement.ToSharedReference(),
                        0, 0,
                        TimeSpan.Zero);
                    mouse3.CreatePointerDown(MouseButton.Left);
                    mouse3.CreatePointerUp(MouseButton.Left);
                    clickSubmitParams.Actions.Add(mouse3.ToSourceActions());
                    await driver.Input.PerformActionsAsync(clickSubmitParams);

                    // Wait for navigation to complete
                    Task[] navTasks = await navObserver.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(10));
                    bool navCompleted = navTasks.Length == 1;
                    navObserver.StopCapturingTasks();

                    if (navCompleted)
                    {
                        Console.WriteLine("✓ Form submitted successfully!");

                        // Verify we're on the results page
                        EvaluateResult urlResult = await driver.Script.EvaluateAsync(
                            new EvaluateCommandParameters(
                                "window.location.href",
                                new ContextTarget(contextId),
                                true));

                        if (urlResult is EvaluateResultSuccess urlSuccess &&
                            urlSuccess.Result is StringRemoteValue urlValue)
                        {
                            string currentUrl = urlValue.Value ?? "No URL";
                            Console.WriteLine($"Current URL: {currentUrl}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("✗ Navigation timeout after form submission");
                    }

                    navObserver.Unobserve();
                }
            }
            else if (inputResult is EvaluateResultException inputException)
            {
                Console.WriteLine($"Error finding input: {inputException.ExceptionDetails.Text}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        catch (WebDriverBiDiException ex)
        {
            Console.WriteLine($"WebDriver BiDi Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await driver.StopAsync();
            Console.WriteLine("Disconnected from browser");
        }
        #endregion
    }

    /// <summary>
    /// Find and click radio button.
    /// </summary>
    public static async Task FindAndClickRadioButton(
        BiDiDriver driver,
        string contextId,
        ContextTarget target)
    {
        #region FindandClickRadioButton
        // Find and click specific radio button
        EvaluateResult radioResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('input[type=\"radio\"][value=\"option1\"]')",
                target,
                true));

        // Click similar to checkbox
        #endregion
    }

    /// <summary>
    /// Type text with newlines into textarea. Note: '\n' creates a new line using Enter key.
    /// </summary>
    public static async Task TypeTextareaWithNewlines(
        BiDiDriver driver,
        string contextId)
    {
        #region TypeTextareawithNewlines
        // Same as text input - click and type
        string longText = "This is a longer message\nthat spans multiple lines.";

        // Note: '\n' creates a new line
        KeySourceActions keyboard = new KeySourceActions();
        foreach (char c in longText)
        {
            if (c == '\n')
            {
                keyboard.Actions.Add(new KeyDownAction("\uE007"));
                keyboard.Actions.Add(new KeyUpAction("\uE007"));
            }
            else
            {
                keyboard.Actions.Add(new KeyDownAction(c.ToString()));
                keyboard.Actions.Add(new KeyUpAction(c.ToString()));
            }
        }

        PerformActionsCommandParameters typeParams = new PerformActionsCommandParameters(contextId);
        typeParams.Actions.Add(keyboard);
        await driver.Input.PerformActionsAsync(typeParams);
        #endregion
    }

    /// <summary>
    /// Verify URL after submission.
    /// </summary>
    public static async Task VerifyUrlAfterSubmission(BiDiDriver driver, Target target)
    {
        #region VerifyURLAfterSubmission
        // Check URL changed
        EvaluateResult urlResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "window.location.href",
                target,
                true));

        string currentUrl = ((EvaluateResultSuccess)urlResult).Result.ConvertTo<StringRemoteValue>().Value;
        if (currentUrl.Contains("/success"))
        {
            Console.WriteLine("Form submitted successfully!");
        }

        // Check for success message
        EvaluateResult messageResult = await driver.Script.EvaluateAsync(
            new EvaluateCommandParameters(
                "document.querySelector('.success-message')?.textContent",
                target,
                true));

        if (messageResult is EvaluateResultSuccess messageSuccess &&
            messageSuccess.Result is StringRemoteValue messageValue)
        {
            string? message = messageValue.Value;
            Console.WriteLine($"Success message: {message}");
        }
        #endregion
    }
}
