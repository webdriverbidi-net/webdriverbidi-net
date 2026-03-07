# WebExtension Module

The WebExtension module allows you to manage browser extensions programmatically, enabling automated testing of extension functionality.

## Overview

The WebExtension module allows you to:

- Install browser extensions
- Uninstall extensions
- Test extension functionality
- Automate extension-based workflows

## Accessing the Module

```csharp
WebExtensionModule webExtension = driver.WebExtension;
```

## Installing Extensions

### Install from Path

```csharp
InstallCommandParameters params = new InstallCommandParameters();
params.ExtensionPath = "/path/to/extension.crx";

InstallCommandResult result = await driver.WebExtension.InstallAsync(params);
string extensionId = result.ExtensionId;

Console.WriteLine($"Extension installed: {extensionId}");
```

### Install Unpacked Extension

```csharp
// Install from unpacked extension directory
InstallCommandParameters params = new InstallCommandParameters();
params.ExtensionPath = "/path/to/extension-directory";

InstallCommandResult result = await driver.WebExtension.InstallAsync(params);
Console.WriteLine($"Extension installed: {result.ExtensionId}");
```

## Uninstalling Extensions

### Uninstall by ID

```csharp
UninstallCommandParameters params = new UninstallCommandParameters(extensionId);
await driver.WebExtension.UninstallAsync(params);

Console.WriteLine("Extension uninstalled");
```

## Common Patterns

### Testing with Extension

```csharp
// Install extension
InstallCommandParameters installParams = new InstallCommandParameters();
installParams.ExtensionPath = "/path/to/my-extension";

InstallCommandResult installResult =
    await driver.WebExtension.InstallAsync(installParams);
string extensionId = installResult.ExtensionId;

Console.WriteLine($"Extension installed: {extensionId}");

// Navigate and test
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Test extension functionality
EvaluateResult result = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.querySelector('[data-extension-injected]') !== null",
        new ContextTarget(contextId),
        true));

if (result is EvaluateResultSuccess success)
{
    bool extensionActive = success.Result.ValueAs<bool>();
    Console.WriteLine($"Extension active: {extensionActive}");
}

// Clean up
await driver.WebExtension.UninstallAsync(
    new UninstallCommandParameters(extensionId));
```

### Testing Extension Content Scripts

```csharp
// Install extension with content script
InstallCommandParameters params = new InstallCommandParameters();
params.ExtensionPath = "/path/to/content-script-extension";

InstallCommandResult result = await driver.WebExtension.InstallAsync(params);

// Navigate to test page
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Wait for content script to inject
await Task.Delay(1000);

// Check if content script modified the page
EvaluateResult evalResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        "document.body.dataset.contentScriptLoaded",
        new ContextTarget(contextId),
        true));

if (evalResult is EvaluateResultSuccess success)
{
    string loaded = success.Result.ValueAs<string>();
    Console.WriteLine($"Content script loaded: {loaded}");
}

// Clean up
await driver.WebExtension.UninstallAsync(
    new UninstallCommandParameters(result.ExtensionId));
```

### Testing Multiple Extensions

```csharp
List<string> extensionIds = new List<string>();

// Install multiple extensions
string[] extensionPaths = new[]
{
    "/path/to/extension1",
    "/path/to/extension2",
    "/path/to/extension3"
};

foreach (var path in extensionPaths)
{
    InstallCommandParameters params = new InstallCommandParameters();
    params.ExtensionPath = path;

    InstallCommandResult result = await driver.WebExtension.InstallAsync(params);
    extensionIds.Add(result.ExtensionId);

    Console.WriteLine($"Installed: {result.ExtensionId}");
}

// Run tests with all extensions active
// ...

// Clean up all extensions
foreach (var id in extensionIds)
{
    await driver.WebExtension.UninstallAsync(
        new UninstallCommandParameters(id));
}
```

### Testing Extension Permissions

```csharp
// Install extension that requires permissions
InstallCommandParameters params = new InstallCommandParameters();
params.ExtensionPath = "/path/to/permission-extension";

InstallCommandResult result = await driver.WebExtension.InstallAsync(params);

// Navigate to page where extension needs permissions
await driver.BrowsingContext.NavigateAsync(
    new NavigateCommandParameters(contextId, "https://example.com")
    { Wait = ReadinessState.Complete });

// Test that extension has required permissions
EvaluateResult evalResult = await driver.Script.EvaluateAsync(
    new EvaluateCommandParameters(
        @"new Promise((resolve) => {
            chrome.permissions.contains({
                permissions: ['storage']
            }, (result) => resolve(result));
        })",
        new ContextTarget(contextId),
        true));

if (evalResult is EvaluateResultSuccess success)
{
    bool hasPermission = success.Result.ValueAs<bool>();
    Console.WriteLine($"Extension has storage permission: {hasPermission}");
}

// Uninstall
await driver.WebExtension.UninstallAsync(
    new UninstallCommandParameters(result.ExtensionId));
```

## Extension Formats

### Chrome/Edge

- **CRX files**: Packaged extension files (.crx)
- **Unpacked**: Directory containing manifest.json and extension files

### Firefox

- **XPI files**: Firefox extension packages (.xpi)
- **Unpacked**: Directory with manifest.json

### Safari

- **App Extensions**: Safari extensions are packaged differently
- **Limited BiDi support**: WebExtension module has limited Safari support

## Browser Support

| Browser | Support Level | Format |
|---------|---------------|--------|
| Chrome/Edge | ✅ Full support | CRX, unpacked |
| Firefox | ⚠️ Different API | XPI, unpacked |
| Safari | ⚠️ Limited | App extensions |

## Best Practices

1. **Use unpacked extensions for development**: Easier to modify and debug
2. **Store extension ID**: Save the ID returned from InstallAsync for cleanup
3. **Wait for extension initialization**: Add delays after installation if needed
4. **Clean up after tests**: Always uninstall extensions to prevent conflicts
5. **Test with real extension builds**: Use production-ready extension packages

## Common Issues

### Extension Installation Fails

**Problem**: Cannot install browser extension.

**Solution**:
- Verify extension path is correct and absolute
- Check extension file format (CRX for Chrome/Edge)
- Ensure manifest.json is valid
- Try loading as unpacked extension for development
- Check browser console for extension errors

### Extension Not Active

**Problem**: Extension installs but doesn't work.

**Solution**:
- Wait for extension to initialize after installation
- Check if page URL matches extension's content script patterns
- Verify extension permissions in manifest.json
- Reload the page after extension installation

### Extension ID Not Found

**Problem**: Cannot uninstall extension by ID.

**Solution**:
- Save the extension ID returned from InstallAsync
- Don't manually construct extension IDs
- Ensure extension is still installed before uninstalling

### Manifest Version Issues

**Problem**: Extension manifest version incompatible.

**Solution**:
- Use Manifest V3 for Chrome/Edge (preferred)
- Manifest V2 support varies by browser version
- Update extension manifest to supported version
- Check browser's extension documentation

## Next Steps

- [Bluetooth Module](bluetooth.md): Web Bluetooth API control
- [Permissions Module](permissions.md): Managing extension permissions
- [Browser Module](browser.md): Browser-level operations
- [API Reference](../../api/index.md): Complete API documentation

## Further Reading

- [Chrome Extensions Documentation](https://developer.chrome.com/docs/extensions/)
- [Firefox Extensions Documentation](https://developer.mozilla.org/en-US/docs/Mozilla/Add-ons/WebExtensions)
- [Manifest V3 Migration Guide](https://developer.chrome.com/docs/extensions/mv3/intro/)
- [Extension Development Best Practices](https://developer.chrome.com/docs/extensions/mv3/intro/mv3-overview/)
