# Form Submission Example

This example demonstrates how to find form elements, fill them out, and submit forms using WebDriverBiDi.NET-Relaxed.

## Overview

This example shows:
- Locating form elements with CSS selectors
- Clicking elements to focus them
- Sending keyboard input
- Submitting forms
- Waiting for navigation after submission

## Complete Example

```csharp
using System;
using System.Threading.Tasks;
using WebDriverBiDi;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

namespace FormSubmissionExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string webSocketUrl = "ws://localhost:9222/devtools/browser/YOUR-ID-HERE";
            BiDiDriver driver = new BiDiDriver(TimeSpan.FromSeconds(30));

            try
            {
                await driver.StartAsync(webSocketUrl);
                Console.WriteLine("Connected to browser");

                // Subscribe to navigation events to track form submission
                SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
                subscribe.Events.Add(driver.BrowsingContext.OnNavigationStarted.EventName);
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
                    RemoteValue inputElement = inputSuccess.Result;
                    Console.WriteLine($"Found input element: {inputElement.SharedId}");

                    // Click the input to focus it
                    Console.WriteLine("Clicking input field...");
                    PerformActionsCommandParameters clickParams = 
                        new PerformActionsCommandParameters(contextId);
                    
                    PointerSource mouse = new PointerSource("mouse", PointerType.Mouse);
                    mouse.CreatePointerMoveToElement(
                        inputElement.ToSharedReference(), 
                        0, 0, 
                        TimeSpan.Zero);
                    mouse.CreatePointerDown(MouseButton.Left);
                    mouse.CreatePointerUp(MouseButton.Left);
                    
                    clickParams.Actions.Add(mouse);
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
                    
                    typeParams.Actions.Add(keyboard);
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
                        RemoteValue phoneElement = phoneSuccess.Result;

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
                        clickPhoneParams.Actions.Add(mouse2);
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
                        
                        typePhoneParams.Actions.Add(keyboard2);
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
                        RemoteValue buttonElement = buttonSuccess.Result;
                        Console.WriteLine($"Found submit button: {buttonElement.SharedId}");

                        // Set up navigation observer
                        EventObserver<NavigationEventArgs> navObserver = 
                            driver.BrowsingContext.OnLoad.AddObserver((e) =>
                            {
                                Console.WriteLine($"Navigation complete to: {e.Url}");
                            });

                        navObserver.SetCheckpoint();

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
                        clickSubmitParams.Actions.Add(mouse3);
                        await driver.Input.PerformActionsAsync(clickSubmitParams);

                        // Wait for navigation to complete
                        bool navCompleted = navObserver.WaitForCheckpoint(TimeSpan.FromSeconds(10));
                        
                        if (navCompleted)
                        {
                            Console.WriteLine("✓ Form submitted successfully!");

                            // Verify we're on the results page
                            EvaluateResult urlResult = await driver.Script.EvaluateAsync(
                                new EvaluateCommandParameters(
                                    "window.location.href",
                                    new ContextTarget(contextId),
                                    true));

                            if (urlResult is EvaluateResultSuccess urlSuccess)
                            {
                                string currentUrl = urlSuccess.Result.ValueAs<string>();
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
        }
    }
}
```

## Step-by-Step Breakdown

### 1. Subscribe to Navigation Events

```csharp
SubscribeCommandParameters subscribe = new SubscribeCommandParameters();
subscribe.Events.Add(driver.BrowsingContext.OnNavigationStarted.EventName);
subscribe.Events.Add(driver.BrowsingContext.OnLoad.EventName);
await driver.Session.SubscribeAsync(subscribe);
```

This allows us to track when the form submission causes navigation.

### 2. Find Form Elements

```csharp
string findInputScript = "document.querySelector('input[name=\"custname\"]')";
EvaluateResult inputResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(findInputScript, target, true));
```

Use JavaScript to find elements. You can also use `LocateNodesAsync` with CSS selectors.

### 3. Click to Focus

```csharp
PointerSource mouse = new PointerSource("mouse", PointerType.Mouse);
mouse.CreatePointerMoveToElement(inputElement.ToSharedReference(), 0, 0, TimeSpan.Zero);
mouse.CreatePointerDown(MouseButton.Left);
mouse.CreatePointerUp(MouseButton.Left);

PerformActionsCommandParameters clickParams = new PerformActionsCommandParameters(contextId);
clickParams.Actions.Add(mouse);
await driver.Input.PerformActionsAsync(clickParams);
```

Clicking the input field focuses it for keyboard input.

### 4. Send Keyboard Input

```csharp
KeySource keyboard = new KeySource("keyboard");
foreach (char c in text)
{
    keyboard.CreateKeyDown(c.ToString());
    keyboard.CreateKeyUp(c.ToString());
}

PerformActionsCommandParameters typeParams = new PerformActionsCommandParameters(contextId);
typeParams.Actions.Add(keyboard);
await driver.Input.PerformActionsAsync(typeParams);
```

Each character requires a key down and key up action.

### 5. Wait for Form Submission

```csharp
EventObserver<NavigationEventArgs> navObserver = 
    driver.BrowsingContext.OnLoad.AddObserver((e) => { });

navObserver.SetCheckpoint();

// Click submit button...

bool navCompleted = navObserver.WaitForCheckpoint(TimeSpan.FromSeconds(10));
```

Use an event observer to wait for the navigation to complete.

## Alternative Approach: Using LocateNodes

Instead of JavaScript evaluation, you can use the BrowsingContext module's `LocateNodesAsync`:

```csharp
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
```

## Alternative Approach: Submitting with Enter Key

Instead of clicking the submit button, you can press Enter:

```csharp
// After typing in the last field, press Enter
KeySource keyboard = new KeySource("keyboard");
keyboard.CreateKeyDown(Keys.Enter);
keyboard.CreateKeyUp(Keys.Enter);

PerformActionsCommandParameters submitParams = new PerformActionsCommandParameters(contextId);
submitParams.Actions.Add(keyboard);
await driver.Input.PerformActionsAsync(submitParams);
```

## Alternative Approach: Direct JavaScript Submission

For simple scenarios, you can submit the form directly with JavaScript:

```csharp
// Find and fill fields...

// Submit form with JavaScript
string submitScript = "document.querySelector('form').submit()";
await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(submitScript, target, false));
```

**Note**: This bypasses validation and doesn't trigger the same events as a user clicking submit.

## Handling Different Input Types

### Checkbox

```csharp
// Find checkbox
EvaluateResult checkboxResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('input[type=\"checkbox\"]')",
        target,
        true));

if (checkboxResult is EvaluateResultSuccess checkboxSuccess)
{
    // Click to toggle
    PerformActionsCommandParameters clickParams = 
        new PerformActionsCommandParameters(contextId);
    PointerSource mouse = new PointerSource("mouse", PointerType.Mouse);
    mouse.CreatePointerMoveToElement(
        checkboxSuccess.Result.ToSharedReference(), 
        0, 0, 
        TimeSpan.Zero);
    mouse.CreatePointerDown(MouseButton.Left);
    mouse.CreatePointerUp(MouseButton.Left);
    clickParams.Actions.Add(mouse);
    await driver.Input.PerformActionsAsync(clickParams);
}
```

### Radio Button

```csharp
// Find and click specific radio button
EvaluateResult radioResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('input[type=\"radio\"][value=\"option1\"]')",
        target,
        true));

// Click similar to checkbox
```

### Select Dropdown

```csharp
// Set dropdown value with JavaScript
string setSelectScript = @"
    const select = document.querySelector('select[name=""delivery""]');
    select.value = 'express';
    select.dispatchEvent(new Event('change', { bubbles: true }));
";

await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(setSelectScript, target, false));
```

### Textarea

```csharp
// Same as text input - click and type
string longText = "This is a longer message\nthat spans multiple lines.";

// Note: '\n' creates a new line
foreach (char c in longText)
{
    if (c == '\n')
    {
        keyboard.CreateKeyDown(Keys.Enter);
        keyboard.CreateKeyUp(Keys.Enter);
    }
    else
    {
        keyboard.CreateKeyDown(c.ToString());
        keyboard.CreateKeyUp(c.ToString());
    }
}
```

## Verification After Submission

After form submission, verify the results:

```csharp
// Check URL changed
EvaluateResult urlResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "window.location.href",
        target,
        true));

string currentUrl = ((EvaluateResultSuccess)urlResult).Result.ValueAs<string>();
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

if (messageResult is EvaluateResultSuccess messageSuccess)
{
    string? message = messageSuccess.Result.ValueAs<string>();
    Console.WriteLine($"Success message: {message}");
}
```

## Best Practices

1. **Wait for elements**: Ensure elements exist before interacting
2. **Focus before typing**: Always click/focus input fields before sending keys
3. **Use event observers**: Track navigation after form submission
4. **Handle errors**: Check for `EvaluateResultException` when finding elements
5. **Verify submission**: Check URL or page content after submission
6. **Release actions**: Call `ReleaseActionsAsync` between form interactions in tests

## Common Issues

### Text Not Appearing

**Problem**: Text typed but doesn't appear in field.

**Solution**: Ensure the field is focused by clicking it first.

### Form Not Submitting

**Problem**: Click submit button but nothing happens.

**Solution**: 
- Verify the button is the correct element
- Check for JavaScript errors in console
- Ensure form validation passes

### Navigation Timeout

**Problem**: Form submits but navigation times out.

**Solution**:
- Increase timeout duration
- Check if form submission is AJAX (no navigation)
- Look for error messages on the page

## Next Steps

- [Input Module](../modules/input.md): Complete input simulation guide
- [Script Module](../modules/script.md): JavaScript execution patterns
- [Common Scenarios](common-scenarios.md): More examples

