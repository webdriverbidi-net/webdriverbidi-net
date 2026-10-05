# Pages and Frames

A `Page` is a tab or a window. Its main document is its `MainFrame`, and each iframe is a `Frame` of its own; `Page.Frames` lists them. The navigation, script, and locator methods below are on both `Page`, where they act on the main frame, and `Frame`.

## Navigating

[!code-csharp[Navigation](../code/PagesSamples.cs#Navigation)]

A navigation waits until the new document is loaded, or until the `ReadinessState` given: `Interactive` for the DOM, or `None` to return once the navigation is committed. A link click navigates without waiting for the new page, so wrap it in `RunAndWaitForNavigationAsync` when the next step needs that page, or follow it with `WaitForUrlAsync` or `WaitForLoadStateAsync`. `WaitForUrlAsync` takes a string for an exact URL, a `Regex`, or a condition.

`SetContentAsync` replaces the document's content with HTML of your own, and `BringToFrontAsync` and `CloseAsync` focus and close the page.

## Scripts

[!code-csharp[Scripts](../code/PagesSamples.cs#Scripts)]

`EvaluateAsync` calls a JavaScript function, given as a function declaration, in the page's own realm, with arguments as WebDriver BiDi `LocalValue`s. It awaits a returned promise. `EvaluateAsync<T>` converts the result to strings, Booleans, numbers, `DateTime`, arrays, `List<T>`, and string-keyed `Dictionary<string, T>`; `object` gives a tree of those; and the plain `EvaluateAsync` returns the `RemoteValue` itself. A function that throws throws `ScriptException`.

`WaitForFunctionAsync` calls a function until it returns a value that is truthy in JavaScript, and calls it again in the new document if the page navigates.

### Before the Page's Own Scripts

[!code-csharp[Init Scripts](../code/PagesSamples.cs#InitScripts)]

An init script runs in every document the page loads from then on, before the document's own scripts.

## Console Messages and Errors

[!code-csharp[Console and Errors](../code/PagesSamples.cs#ConsoleAndErrors)]

`OnConsoleMessage` reports each `console` call, with its method, level, formatted text, and arguments; `OnPageError` reports each uncaught exception. Both include the frame that raised them.

## Dialogs

What happens to an alert, a confirmation, a prompt, or a warning before leaving the page is decided by the user prompt handler. By default the browser dismisses it. To answer dialogs yourself, leave them open, with the handler `Ignore`, and handle `OnDialog`:

[!code-csharp[Dialogs](../code/PagesSamples.cs#Dialogs)]

Answer the dialog from the observer, without awaiting the action that opened it: that action cannot finish while the dialog is open. `OnDialog` also reports a dialog the browser handled itself, whose `Handler` says what was done, and answering that one throws.

> **Firefox:** Firefox ignores the handler given to a new user context ([bug 1975279](https://bugzilla.mozilla.org/show_bug.cgi?id=1975279)). Set it for the whole session instead, with `WithSessionCapability("unhandledPromptBehavior", new UserPromptHandler() { Default = UserPromptHandlerType.Ignore })` on the launcher.

## Popups

[!code-csharp[Popups](../code/PagesSamples.cs#Popups)]

`OnPopup` reports a page this page opened, which also appears in its browser's `Pages` and `OnPageCreated`, with this page as its `Opener`.

## Downloads

[!code-csharp[Downloads](../code/PagesSamples.cs#Downloads)]

`AllowDownloadsAsync`, `DenyDownloadsAsync`, and `ResetDownloadBehaviorAsync` set where a browser saves downloads. `RunAndWaitForDownloadAsync` returns the download an action starts, and `OnDownload` reports every one; `WaitForEndAsync` waits for it to complete, fail, or be canceled.

## Screenshots, PDFs, and the Viewport

[!code-csharp[Capture](../code/PagesSamples.cs#Capture)]

A page screenshot captures the viewport, the whole page, or a clip, as PNG unless another `ImageFormat` is given. An element's screenshot captures the element, whether or not it is scrolled into view. `PdfAsync` prints the page as a PDF. `SetViewportSizeAsync` sets the size of the page's viewport in CSS pixels, until `ResetViewportSizeAsync`.

> **Chrome:** Chrome captures screenshots only of top-level browsing contexts, so a screenshot of an element in an iframe fails in Chrome.

## Video

[!code-csharp[Video](../code/PagesSamples.cs#Video)]

`RecordVideoAsync` records the page until the recording is stopped or disposed. `VideoRecordingOptions` sets the video's width, height, and frame rate, which the browser may adjust to keep the viewport's shape; without them, the browser records the whole viewport at a rate of its own. The browser chooses the format, such as WebM. `StopAsync` returns the video's path, and stopping again returns the same path.

The browser writes the file. A browser on this machine that writes it elsewhere, as Firefox 157 does to its downloads folder, has it moved to the path given, replacing any file there. A browser on another machine, such as a remote grid's, leaves the file on that machine, and `StopAsync` and `Path` give its path there.

> **Chrome:** Chrome cannot record video yet, so `RecordVideoAsync` throws `NotSupportedException`.
