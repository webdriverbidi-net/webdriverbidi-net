# Speculation Module

The Speculation module provides control over navigation prefetching and prerendering based on the [Speculation Rules API](https://wicg.github.io/nav-speculation/prefetch.html).

## Overview

The Speculation module allows you to:

- Add prefetch rules for faster navigation
- Configure prerendering for instant page loads
- Remove speculation rules
- Test speculative loading behavior

## Accessing the Module

```csharp
SpeculationModule speculation = driver.Speculation;
```

## Prefetching

Prefetching downloads resources in advance but doesn't render the page.

### Add Prefetch Rules

```csharp
AddSpeculationRulesCommandParameters params =
    new AddSpeculationRulesCommandParameters(contextId);

params.Rules = @"{
    ""prefetch"": [
        {
            ""source"": ""list"",
            ""urls"": [""https://example.com/page1"", ""https://example.com/page2""]
        }
    ]
}";

AddSpeculationRulesCommandResult result =
    await driver.Speculation.AddSpeculationRulesAsync(params);

Console.WriteLine($"Speculation rules added: {result.RulesId}");
```

### Prefetch with Eagerness

```csharp
AddSpeculationRulesCommandParameters params =
    new AddSpeculationRulesCommandParameters(contextId);

params.Rules = @"{
    ""prefetch"": [
        {
            ""source"": ""list"",
            ""urls"": [""https://example.com/important-page""],
            ""eagerness"": ""eager""
        }
    ]
}";

await driver.Speculation.AddSpeculationRulesAsync(params);
```

Eagerness levels:
- `immediate` - Start immediately
- `eager` - Start soon
- `moderate` - Start when likely needed
- `conservative` - Start when very likely needed

## Prerendering

Prerendering downloads and fully renders the page in the background.

### Add Prerender Rules

```csharp
AddSpeculationRulesCommandParameters params =
    new AddSpeculationRulesCommandParameters(contextId);

params.Rules = @"{
    ""prerender"": [
        {
            ""source"": ""list"",
            ""urls"": [""https://example.com/important-page""]
        }
    ]
}";

await driver.Speculation.AddSpeculationRulesAsync(params);
Console.WriteLine("Prerender rules added");
```

### Prerender with Document Rules

```csharp
AddSpeculationRulesCommandParameters params =
    new AddSpeculationRulesCommandParameters(contextId);

params.Rules = @"{
    ""prerender"": [
        {
            ""source"": ""document"",
            ""where"": {
                ""selector_matches"": "".prerender-link""
            }
        }
    ]
}";

await driver.Speculation.AddSpeculationRulesAsync(params);
```

## Removing Speculation Rules

### Remove by Rules ID

```csharp
RemoveSpeculationRulesCommandParameters params =
    new RemoveSpeculationRulesCommandParameters(rulesId);

await driver.Speculation.RemoveSpeculationRulesAsync(params);
Console.WriteLine("Speculation rules removed");
```

## Common Patterns

### Testing Prefetch Performance

```csharp
// Navigate to initial page
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Add prefetch rules
AddSpeculationRulesCommandParameters speculationParams =
    new AddSpeculationRulesCommandParameters(contextId);
speculationParams.Rules = @"{
    ""prefetch"": [{
        ""source"": ""list"",
        ""urls"": [""https://example.com/next-page""]
    }]
}";

AddSpeculationRulesCommandResult speculationResult =
    await driver.Speculation.AddSpeculationRulesAsync(speculationParams);

// Wait for prefetch
await Task.Delay(2000);

// Navigate to prefetched page (should be faster)
DateTime startTime = DateTime.Now;
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com/next-page")
    { Wait = ReadinessState.Complete });
TimeSpan navigationTime = DateTime.Now - startTime;

Console.WriteLine($"Navigation time: {navigationTime.TotalMilliseconds}ms");

// Clean up
await driver.Speculation.RemoveSpeculationRulesAsync(
    new RemoveSpeculationRulesCommandParameters(speculationResult.RulesId));
```

### Testing Prerender Activation

```csharp
// Navigate to starting page
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Add prerender rules
AddSpeculationRulesCommandParameters params =
    new AddSpeculationRulesCommandParameters(contextId);
params.Rules = @"{
    ""prerender"": [{
        ""source"": ""list"",
        ""urls"": [""https://example.com/prerendered-page""],
        ""eagerness"": ""immediate""
    }]
}";

AddSpeculationRulesCommandResult result =
    await driver.Speculation.AddSpeculationRulesAsync(params);

// Wait for prerender to complete
await Task.Delay(3000);

// Navigate to prerendered page (should be instant)
DateTime startTime = DateTime.Now;
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com/prerendered-page")
    { Wait = ReadinessState.Complete });
TimeSpan activationTime = DateTime.Now - startTime;

Console.WriteLine($"Prerender activation time: {activationTime.TotalMilliseconds}ms");

// Clean up
await driver.Speculation.RemoveSpeculationRulesAsync(
    new RemoveSpeculationRulesCommandParameters(result.RulesId));
```

### Testing Multiple Speculation Rules

```csharp
List<string> rulesIds = new List<string>();

// Add prefetch rules
AddSpeculationRulesCommandParameters prefetchParams =
    new AddSpeculationRulesCommandParameters(contextId);
prefetchParams.Rules = @"{
    ""prefetch"": [{
        ""source"": ""list"",
        ""urls"": [""https://example.com/page1"", ""https://example.com/page2""]
    }]
}";
var prefetchResult = await driver.Speculation.AddSpeculationRulesAsync(prefetchParams);
rulesIds.Add(prefetchResult.RulesId);

// Add prerender rules
AddSpeculationRulesCommandParameters prerenderParams =
    new AddSpeculationRulesCommandParameters(contextId);
prerenderParams.Rules = @"{
    ""prerender"": [{
        ""source"": ""list"",
        ""urls"": [""https://example.com/important""]
    }]
}";
var prerenderResult = await driver.Speculation.AddSpeculationRulesAsync(prerenderParams);
rulesIds.Add(prerenderResult.RulesId);

// Test navigation performance
// ...

// Clean up all rules
foreach (var id in rulesIds)
{
    await driver.Speculation.RemoveSpeculationRulesAsync(
        new RemoveSpeculationRulesCommandParameters(id));
}
```

### Testing Document-Based Rules

```csharp
// Add speculation rules that match page elements
AddSpeculationRulesCommandParameters params =
    new AddSpeculationRulesCommandParameters(contextId);
params.Rules = @"{
    ""prefetch"": [
        {
            ""source"": ""document"",
            ""where"": {
                ""and"": [
                    { ""href_matches"": ""/product/*"" },
                    { ""selector_matches"": "".product-link"" }
                ]
            },
            ""eagerness"": ""moderate""
        }
    ]
}";

AddSpeculationRulesCommandResult result =
    await driver.Speculation.AddSpeculationRulesAsync(params);

// Navigate to page with matching links
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com/products")
    { Wait = ReadinessState.Complete });

// Links matching the rules should be prefetched automatically
await Task.Delay(2000);

// Test performance when clicking a product link
// ...

// Clean up
await driver.Speculation.RemoveSpeculationRulesAsync(
    new RemoveSpeculationRulesCommandParameters(result.RulesId));
```

## Speculation Rules JSON Format

### Basic Structure

```json
{
  "prefetch": [...],
  "prerender": [...]
}
```

### List Source Rules

```json
{
  "prefetch": [
    {
      "source": "list",
      "urls": ["https://example.com/page1", "https://example.com/page2"],
      "eagerness": "eager"
    }
  ]
}
```

### Document Source Rules

```json
{
  "prerender": [
    {
      "source": "document",
      "where": {
        "href_matches": "/product/*",
        "selector_matches": ".important-link"
      },
      "eagerness": "moderate"
    }
  ]
}
```

## Browser Support

| Browser | Prefetch | Prerender |
|---------|----------|-----------|
| Chrome/Edge | ⚠️ Experimental | ⚠️ Experimental |
| Firefox | ❌ Not supported | ❌ Not supported |
| Safari | ❌ Not supported | ❌ Not supported |

**Note**: Speculation Rules support is experimental and may not be available in all browsers.

## Enabling Speculation Support

### Chrome/Edge

Launch browser with experimental features enabled:

```
--enable-features=SpeculationRulesPrefetchProxy
```

## Best Practices

1. **Test performance impact**: Measure actual navigation speed improvements
2. **Use appropriate eagerness**: Don't prefetch everything immediately
3. **Monitor resource usage**: Prefetching consumes bandwidth and memory
4. **Clean up rules**: Remove speculation rules when no longer needed
5. **Test on real networks**: Performance benefits vary by connection speed
6. **Consider privacy**: Prefetching triggers resource loads and tracking

## Common Issues

### Speculation Rules Not Supported

**Problem**: Module throws "not supported" errors.

**Solution**:
- Use Chrome 108+ or Edge 108+
- Enable experimental web platform features
- Launch browser with required feature flags
- Check that Speculation Rules API is available

### Prefetch Not Working

**Problem**: Resources are not being prefetched.

**Solution**:
- Verify URL format in rules is correct
- Check network tab to confirm prefetch requests
- Ensure eagerness level is appropriate
- Wait sufficient time for prefetch to complete

### Prerender Not Activating

**Problem**: Prerendered page loads slowly.

**Solution**:
- Verify prerender completed before navigation
- Check browser console for prerender errors
- Some pages may block prerendering (e.g., auth required)
- Monitor prerender state with Performance API

### Rules Not Applying to Page

**Problem**: Document-based rules don't match elements.

**Solution**:
- Verify selector syntax is correct
- Check that matching elements exist on the page
- Use browser DevTools to test selectors
- Review href_matches patterns for accuracy

## Next Steps

- [Network Module](network.md): Monitoring network performance
- [Browser Module](browser.md): Browser-level operations
- [Browsing Context Module](browsing-context.md): Navigation management
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [Speculation Rules API Specification](https://wicg.github.io/nav-speculation/prefetch.html)
- [Chrome Prerender Documentation](https://developer.chrome.com/blog/prerender-pages/)
- [Web Performance Working Group](https://www.w3.org/webperf/)
- [Resource Hints Specification](https://www.w3.org/TR/resource-hints/)
