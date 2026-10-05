# Tracing

A trace records what a browser did, so that a failure can be understood after the fact: each action taken on its pages, with its timing, parameters, log, and error, and the line of code that called it; snapshots of the pages' DOM before and after each action; a filmstrip of screenshots; and the pages' console messages, errors, and network traffic. Dramaturge writes traces in the format of [Playwright's traces](https://playwright.dev/docs/trace-viewer), so Playwright's trace viewer opens them.

## Recording a Trace

`RecordTraceAsync` starts recording a browser's trace: every page of the browser, including pages it opens later, such as popups. Disposing the recording, or `StopAsync`, ends it and writes the trace, a zip file.

[!code-csharp[Record a Trace](../code/TracingSamples.cs#RecordTrace)]

A browser records one trace at a time; starting another while one is recording throws `InvalidOperationException`. To trace a test run, the [test framework packages](test-frameworks.md#traces) can record one for each failed test.

`StopAsync` returns the trace's path, and stopping again returns the same path. Stopping waits, for at most the group's `NavigationTimeout`, for requests still in flight; a request still in flight then is written as failed.

[!code-csharp[Stop a Trace](../code/TracingSamples.cs#StopTrace)]

## What Is Recorded

Every trace has:

- **Actions:** every action Dramaturge takes for the code, from a locator's click to a page's navigation, a browser's cookies, a route's answer, and each `Expect` assertion, with its parameters, its start and end, and the error it threw. An action that waits logs what it is waiting for and what it saw, such as "the element was not visible".
- **Callers:** the lines of code that called each action, for the viewer's Source tab, where the code's source files are on this machine and its debugging symbols are present.
- **Network:** the browser's requests, with their responses and bodies.
- **Console:** the pages' console messages and uncaught errors.

`TraceRecordingOptions` adds more:

| Option | Records |
| --- | --- |
| `Snapshots` | A snapshot of the page's DOM before and after each action, and, for an action on an element, as it acts, with the element marked and the point it clicked |
| `Screenshots` | A screenshot of the page after each action and each time it loads a document, for the viewer's filmstrip |
| `Sources` | The source files of the code that called each action, so the viewer can show them |
| `Title` | The title the viewer shows for the trace |

Snapshots make the viewer most useful, and cost an extra script call or two around each action; screenshots cost one more. The time a snapshot takes before an action does not count against the action's timeout.

## Opening a Trace

Open the zip file in Playwright's trace viewer:

- at [trace.playwright.dev](https://trace.playwright.dev), which loads the trace in the browser without uploading it; or
- with `npx playwright show-trace trace.zip`, which needs Node.js.

Dramaturge writes version 9 of the trace format, which Playwright 1.63 introduced. A viewer from Playwright 1.63 or later opens it; an older viewer reports that the trace is from a newer version.

## Limitations

- A snapshot records the DOM, form state, scroll positions, open shadow roots, and stylesheets that the page's scripts built. It does not record a canvas's pixels, closed shadow roots, or changes that script made to the rules of a linked stylesheet.
- The point an action clicked is recorded only for an element in the page's main frame; in a frame, the element is still marked.
- A route's or a dialog's actions are recorded without snapshots or screenshots: the page's scripts cannot run while it waits on the request or the dialog. Actions on a browser, such as opening a page or reading cookies, have none either.
- A call stack reaches back only as far as the code that last resumed from an `await`, so an action called from a helper method may not show the helper's caller.
- Without debugging symbols, as in a native AOT application, actions have no callers.
