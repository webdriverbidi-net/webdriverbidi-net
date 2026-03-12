# Speculation Module

The Speculation module provides **monitoring** of prefetch status updates from the browser. It is defined in the [Prefetch specification's Automated testing section](https://wicg.github.io/nav-speculation/prefetch.html#automated-testing) as an extension to WebDriver BiDi.

> **Scope note**: The WebDriver BiDi spec defines only the `speculation.prefetchStatusUpdated` event. There are no commands to add or remove speculation rules—those are configured by the page itself via the [Speculation Rules API](https://wicg.github.io/nav-speculation/prefetch.html) (e.g., `<script type="speculationrules">` in HTML). This module is intentionally limited to what the spec defines.

## Overview

The Speculation module allows you to:

- Subscribe to prefetch status updates (`pending`, `ready`, `success`, `failure`)
- Monitor when the browser prefetches or prerenders resources
- Observe prefetch lifecycle for testing and diagnostics

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/SpeculationModuleSamples.cs#AccessingModule)]

## Prefetch Status Event

The `OnPrefetchStatusUpdated` event fires when the browser updates the status of a prefetched or prerendered resource. Subscribe to it to observe prefetch lifecycle.

### Subscribe to Prefetch Status Updates

[!code-csharp[Subscribe to Prefetch Status](../../code/modules/SpeculationModuleSamples.cs#SubscribetoPrefetchStatus)]

### PreloadingStatus Values

| Status | Description |
|--------|-------------|
| `Pending` | The prefetch has begun |
| `Ready` | The prefetch is complete and available |
| `Success` | The prefetch was activated (e.g., user navigated to it) |
| `Failure` | The prefetch failed or expired |

### Event Payload

Each event provides:

- `BrowsingContextId` – The browsing context where the prefetch occurred
- `Url` – The URL that was prefetched
- `Status` – The current `PreloadingStatus`

## Example: Monitoring Prefetch During Navigation

[!code-csharp[Monitor Prefetch During Navigation](../../code/modules/SpeculationModuleSamples.cs#MonitorPrefetchDuringNavigation)]

## Browser Support

| Browser | Prefetch Status Event |
|---------|------------------------|
| Chrome/Edge | ⚠️ Experimental |
| Firefox | ❌ Not supported |
| Safari | ❌ Not supported |

**Note**: Support is experimental. Enable with `--enable-features=SpeculationRulesPrefetchProxy` for Chrome/Edge.

## Speculation Rules (Page-Configured)

Prefetch and prerender rules are **not** set via WebDriver BiDi. Pages configure them using the [Speculation Rules API](https://wicg.github.io/nav-speculation/prefetch.html), for example:

```html
<script type="speculationrules">
{
  "prefetch": [{
    "source": "list",
    "urls": ["https://example.com/next-page"]
  }]
}
</script>
```

The Speculation module lets you **observe** when the browser acts on these rules—it does not create or modify them.

## Next Steps

- [Network Module](network.md): Monitoring network performance
- [Browser Module](browser.md): Browser-level operations
- [Browsing Context Module](browsing-context.md): Navigation management
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [Prefetch Specification – Automated testing](https://wicg.github.io/nav-speculation/prefetch.html#automated-testing) – WebDriver BiDi speculation module definition
- [Speculation Rules API](https://wicg.github.io/nav-speculation/prefetch.html) – How pages configure prefetch/prerender
