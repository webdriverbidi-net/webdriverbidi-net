# WebExtension Module

The WebExtension module allows you to manage browser extensions programmatically, enabling automated testing of extension functionality.

## Overview

The WebExtension module allows you to:

- Install browser extensions
- Uninstall extensions
- Test extension functionality
- Automate extension-based workflows

## Accessing the Module

[!code-csharp[Accessing Module](../../code/modules/WebExtensionModuleSamples.cs#AccessingModule)]

## Timeout and Cancellation

All commands in this module accept optional `timeoutOverride` and `CancellationToken` parameters. Use `timeoutOverride` to set a per-command timeout (defaults to `BiDiDriver.DefaultCommandTimeout` when omitted). Use `CancellationToken` for cooperative cancellation. See the [API Design Guide](../advanced/api-design.md#timeout-and-cancellation) for details and examples.

## Installing Extensions

### Install from Path

[!code-csharp[Install from Path](../../code/modules/WebExtensionModuleSamples.cs#InstallfromPath)]

### Install Unpacked Extension

[!code-csharp[Install Unpacked Extension](../../code/modules/WebExtensionModuleSamples.cs#InstallUnpackedExtension)]

## Uninstalling Extensions

### Uninstall by ID

[!code-csharp[Uninstall by ID](../../code/modules/WebExtensionModuleSamples.cs#UninstallbyID)]

## Common Patterns

### Testing with Extension

[!code-csharp[Testing with Extension](../../code/modules/WebExtensionModuleSamples.cs#TestingwithExtension)]

### Testing Extension Content Scripts

[!code-csharp[Testing Extension Content Scripts](../../code/modules/WebExtensionModuleSamples.cs#TestingExtensionContentScripts)]

### Testing Multiple Extensions

[!code-csharp[Testing Multiple Extensions](../../code/modules/WebExtensionModuleSamples.cs#TestingMultipleExtensions)]

### Testing Extension Permissions

[!code-csharp[Testing Extension Permissions](../../code/modules/WebExtensionModuleSamples.cs#TestingExtensionPermissions)]

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
