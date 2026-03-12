# Form Submission Example

This example demonstrates how to find form elements, fill them out, and submit forms using WebDriverBiDi.NET.

## Overview

This example shows:
- Locating form elements with CSS selectors
- Clicking elements to focus them
- Sending keyboard input
- Submitting forms
- Waiting for navigation after submission

## Complete Example

[!code-csharp[Complete Form Submission Example](../../code/examples/FormSubmissionSamples.cs#CompleteFormSubmissionExample)]

The example above assumes the driver is already connected. To run a full program, connect to the browser first with `driver.StartAsync(webSocketUrl)`, then call the form submission logic.

## Step-by-Step Breakdown

### 1. Subscribe to Navigation Events

[!code-csharp[Subscribe to Navigation Events](../../code/examples/FormSubmissionSamples.cs#SubscribetoNavigationEvents)]

This allows us to track when the form submission causes navigation.

### 2. Find Form Elements

[!code-csharp[Find Form Elements](../../code/examples/FormSubmissionSamples.cs#FindFormElements)]

Use JavaScript to find elements. You can also use `LocateNodesAsync` with CSS selectors.

### 3. Click to Focus

[!code-csharp[Click to Focus](../../code/examples/FormSubmissionSamples.cs#ClicktoFocus)]

Clicking the input field focuses it for keyboard input.

### 4. Send Keyboard Input

[!code-csharp[Send Keyboard Input](../../code/examples/FormSubmissionSamples.cs#SendKeyboardInput)]

Each character requires a key down and key up action.

### 5. Wait for Form Submission

[!code-csharp[Wait for Form Submission](../../code/examples/FormSubmissionSamples.cs#WaitforFormSubmission)]

Use an event observer to wait for the navigation to complete.

## Alternative Approach: Using LocateNodes

Instead of JavaScript evaluation, you can use the BrowsingContext module's `LocateNodesAsync`:

[!code-csharp[Find Input with LocateNodes](../../code/examples/FormSubmissionSamples.cs#FindInputwithLocateNodes)]

## Alternative Approach: Submitting with Enter Key

Instead of clicking the submit button, you can press Enter:

[!code-csharp[Submit with Enter Key](../../code/examples/FormSubmissionSamples.cs#SubmitwithEnterKey)]

## Alternative Approach: Direct JavaScript Submission

For simple scenarios, you can submit the form directly with JavaScript:

[!code-csharp[Submit Form with JavaScript](../../code/examples/FormSubmissionSamples.cs#SubmitFormwithJavaScript)]

**Note**: This bypasses validation and doesn't trigger the same events as a user clicking submit.

## Handling Different Input Types

### Checkbox

[!code-csharp[Find and Click Checkbox](../../code/examples/FormSubmissionSamples.cs#FindandClickCheckbox)]

### Radio Button

[!code-csharp[Find and Click Radio Button](../../code/examples/FormSubmissionSamples.cs#FindandClickRadioButton)]

### Select Dropdown

[!code-csharp[Set Dropdown Value](../../code/examples/FormSubmissionSamples.cs#SetDropdownValue)]

### Textarea

[!code-csharp[Type Textarea with Newlines](../../code/examples/FormSubmissionSamples.cs#TypeTextareawithNewlines)]

Note: `\n` creates a new line using the Enter key (Unicode U+E007).

## Verification After Submission

After form submission, verify the results:

[!code-csharp[Verify URL After Submission](../../code/examples/FormSubmissionSamples.cs#VerifyURLAfterSubmission)]

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

