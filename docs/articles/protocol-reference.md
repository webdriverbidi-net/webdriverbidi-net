# WebDriver BiDi Protocol Reference

This page provides a comprehensive cross-reference between the [W3C WebDriver BiDi specification](https://w3c.github.io/webdriver-bidi/) protocol methods and their implementation in WebDriverBiDi.NET.

For each protocol method, this reference links to:
- The API documentation for command parameters and result types
- The module guide with usage examples and best practices

## How to Use This Reference

1. **Find a protocol method** from the W3C spec (e.g., `browsingContext.navigate`)
2. **Look up the row** in the appropriate module table below
3. **Click the API link** to see the command parameters, result type, and method signature
4. **Click the Guide link** to see usage examples and patterns

## Module List

- [Session Module](#session-module)
- [Browser Module](#browser-module)
- [Browsing Context Module](#browsing-context-module)
- [Network Module](#network-module)
- [Script Module](#script-module)
- [Input Module](#input-module)
- [Storage Module](#storage-module)
- [Log Module](#log-module)
- [Bluetooth Module](#bluetooth-module)
- [Emulation Module](#emulation-module)
- [Permissions Module](#permissions-module)
- [User Agent Client Hints Module](#user-agent-client-hints-module)
- [Web Extension Module](#web-extension-module)

---

## Session Module

Commands for managing WebDriver BiDi sessions and event subscriptions.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `session.end` | [EndCommandParameters](xref:WebDriverBiDi.Session.EndCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Session Guide](modules/session.md) |
| `session.new` | [NewCommandParameters](xref:WebDriverBiDi.Session.NewCommandParameters) | [NewCommandResult](xref:WebDriverBiDi.Session.NewCommandResult) | [Session Guide](modules/session.md#creating-a-new-session) |
| `session.status` | [StatusCommandParameters](xref:WebDriverBiDi.Session.StatusCommandParameters) | [StatusCommandResult](xref:WebDriverBiDi.Session.StatusCommandResult) | [Session Guide](modules/session.md#session-status) |
| `session.subscribe` | [SubscribeCommandParameters](xref:WebDriverBiDi.Session.SubscribeCommandParameters) | [SubscribeCommandResult](xref:WebDriverBiDi.Session.SubscribeCommandResult) | [Session Guide](modules/session.md#event-subscription) |
| `session.unsubscribe` | [UnsubscribeCommandParameters](xref:WebDriverBiDi.Session.UnsubscribeCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Session Guide](modules/session.md#event-subscription) |

---

## Browser Module

Commands for browser-level operations including user contexts and client windows.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `browser.close` | [CloseCommandParameters](xref:WebDriverBiDi.Browser.CloseCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browser Guide](modules/browser.md#closing-browser) |
| `browser.createUserContext` | [CreateUserContextCommandParameters](xref:WebDriverBiDi.Browser.CreateUserContextCommandParameters) | [CreateUserContextCommandResult](xref:WebDriverBiDi.Browser.CreateUserContextCommandResult) | [Browser Guide](modules/browser.md#user-contexts) |
| `browser.getClientWindows` | [GetClientWindowsCommandParameters](xref:WebDriverBiDi.Browser.GetClientWindowsCommandParameters) | [GetClientWindowsCommandResult](xref:WebDriverBiDi.Browser.GetClientWindowsCommandResult) | [Browser Guide](modules/browser.md#client-windows) |
| `browser.getUserContexts` | [GetUserContextsCommandParameters](xref:WebDriverBiDi.Browser.GetUserContextsCommandParameters) | [GetUserContextsCommandResult](xref:WebDriverBiDi.Browser.GetUserContextsCommandResult) | [Browser Guide](modules/browser.md#user-contexts) |
| `browser.removeUserContext` | [RemoveUserContextCommandParameters](xref:WebDriverBiDi.Browser.RemoveUserContextCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browser Guide](modules/browser.md#user-contexts) |
| `browser.setClientWindowState` | [SetClientWindowStateCommandParameters](xref:WebDriverBiDi.Browser.SetClientWindowStateCommandParameters) | [SetClientWindowStateCommandResult](xref:WebDriverBiDi.Browser.SetClientWindowStateCommandResult) | [Browser Guide](modules/browser.md#client-windows) |
| `browser.setDownloadBehavior` | [SetDownloadBehaviorCommandParameters](xref:WebDriverBiDi.Browser.SetDownloadBehaviorCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browser Guide](modules/browser.md#download-behavior) |

---

## Browsing Context Module

Commands for managing browsing contexts (tabs/windows), navigation, and capturing content.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `browsingContext.activate` | [ActivateCommandParameters](xref:WebDriverBiDi.BrowsingContext.ActivateCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.captureScreenshot` | [CaptureScreenshotCommandParameters](xref:WebDriverBiDi.BrowsingContext.CaptureScreenshotCommandParameters) | [CaptureScreenshotCommandResult](xref:WebDriverBiDi.BrowsingContext.CaptureScreenshotCommandResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.close` | [CloseCommandParameters](xref:WebDriverBiDi.BrowsingContext.CloseCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.create` | [CreateCommandParameters](xref:WebDriverBiDi.BrowsingContext.CreateCommandParameters) | [CreateCommandResult](xref:WebDriverBiDi.BrowsingContext.CreateCommandResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.getTree` | [GetTreeCommandParameters](xref:WebDriverBiDi.BrowsingContext.GetTreeCommandParameters) | [GetTreeCommandResult](xref:WebDriverBiDi.BrowsingContext.GetTreeCommandResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.handleUserPrompt` | [HandleUserPromptCommandParameters](xref:WebDriverBiDi.BrowsingContext.HandleUserPromptCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.locateNodes` | [LocateNodesCommandParameters](xref:WebDriverBiDi.BrowsingContext.LocateNodesCommandParameters) | [LocateNodesCommandResult](xref:WebDriverBiDi.BrowsingContext.LocateNodesCommandResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.navigate` | [NavigateCommandParameters](xref:WebDriverBiDi.BrowsingContext.NavigateCommandParameters) | [NavigateCommandResult](xref:WebDriverBiDi.BrowsingContext.NavigateCommandResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.print` | [PrintCommandParameters](xref:WebDriverBiDi.BrowsingContext.PrintCommandParameters) | [PrintCommandResult](xref:WebDriverBiDi.BrowsingContext.PrintCommandResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.reload` | [ReloadCommandParameters](xref:WebDriverBiDi.BrowsingContext.ReloadCommandParameters) | [NavigateCommandResult](xref:WebDriverBiDi.BrowsingContext.NavigateCommandResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.setBypassCSP` | [SetBypassCSPCommandParameters](xref:WebDriverBiDi.BrowsingContext.SetBypassCSPCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.setViewport` | [SetViewportCommandParameters](xref:WebDriverBiDi.BrowsingContext.SetViewportCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.traverseHistory` | [TraverseHistoryCommandParameters](xref:WebDriverBiDi.BrowsingContext.TraverseHistoryCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Browsing Context Guide](modules/browsing-context.md) |

### Browsing Context Events

| Protocol Event | EventArgs API | Module Guide |
|----------------|---------------|--------------|
| `browsingContext.contextCreated` | [BrowsingContextEventArgs](xref:WebDriverBiDi.BrowsingContext.BrowsingContextEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.contextDestroyed` | [BrowsingContextEventArgs](xref:WebDriverBiDi.BrowsingContext.BrowsingContextEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.domContentLoaded` | [NavigationEventArgs](xref:WebDriverBiDi.BrowsingContext.NavigationEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.downloadEnd` | [DownloadEndEventArgs](xref:WebDriverBiDi.BrowsingContext.DownloadEndEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.downloadWillBegin` | [DownloadWillBeginEventArgs](xref:WebDriverBiDi.BrowsingContext.DownloadWillBeginEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.fragmentNavigated` | [NavigationEventArgs](xref:WebDriverBiDi.BrowsingContext.NavigationEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.load` | [NavigationEventArgs](xref:WebDriverBiDi.BrowsingContext.NavigationEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.navigationAborted` | [NavigationEventArgs](xref:WebDriverBiDi.BrowsingContext.NavigationEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.navigationCommitted` | [NavigationEventArgs](xref:WebDriverBiDi.BrowsingContext.NavigationEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.navigationFailed` | [NavigationEventArgs](xref:WebDriverBiDi.BrowsingContext.NavigationEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.navigationStarted` | [NavigationEventArgs](xref:WebDriverBiDi.BrowsingContext.NavigationEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.userPromptClosed` | [UserPromptClosedEventArgs](xref:WebDriverBiDi.BrowsingContext.UserPromptClosedEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |
| `browsingContext.userPromptOpened` | [UserPromptOpenedEventArgs](xref:WebDriverBiDi.BrowsingContext.UserPromptOpenedEventArgs) | [Browsing Context Guide](modules/browsing-context.md) |

---

## Network Module

Commands for intercepting, modifying, and monitoring network traffic.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `network.addDataCollector` | [AddDataCollectorCommandParameters](xref:WebDriverBiDi.Network.AddDataCollectorCommandParameters) | [AddDataCollectorCommandResult](xref:WebDriverBiDi.Network.AddDataCollectorCommandResult) | [Network Guide](modules/network.md) |
| `network.addIntercept` | [AddInterceptCommandParameters](xref:WebDriverBiDi.Network.AddInterceptCommandParameters) | [AddInterceptCommandResult](xref:WebDriverBiDi.Network.AddInterceptCommandResult) | [Network Guide](modules/network.md) |
| `network.continueRequest` | [ContinueRequestCommandParameters](xref:WebDriverBiDi.Network.ContinueRequestCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.continueResponse` | [ContinueResponseCommandParameters](xref:WebDriverBiDi.Network.ContinueResponseCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.continueWithAuth` | [ContinueWithAuthCommandParameters](xref:WebDriverBiDi.Network.ContinueWithAuthCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.disownData` | [DisownDataCommandParameters](xref:WebDriverBiDi.Network.DisownDataCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.failRequest` | [FailRequestCommandParameters](xref:WebDriverBiDi.Network.FailRequestCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.getData` | [GetDataCommandParameters](xref:WebDriverBiDi.Network.GetDataCommandParameters) | [GetDataCommandResult](xref:WebDriverBiDi.Network.GetDataCommandResult) | [Network Guide](modules/network.md) |
| `network.provideResponse` | [ProvideResponseCommandParameters](xref:WebDriverBiDi.Network.ProvideResponseCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.removeDataCollector` | [RemoveDataCollectorCommandParameters](xref:WebDriverBiDi.Network.RemoveDataCollectorCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.removeIntercept` | [RemoveInterceptCommandParameters](xref:WebDriverBiDi.Network.RemoveInterceptCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.setCacheBehavior` | [SetCacheBehaviorCommandParameters](xref:WebDriverBiDi.Network.SetCacheBehaviorCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |
| `network.setExtraHeaders` | [SetExtraHeadersCommandParameters](xref:WebDriverBiDi.Network.SetExtraHeadersCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Network Guide](modules/network.md) |

### Network Events

| Protocol Event | EventArgs API | Module Guide |
|----------------|---------------|--------------|
| `network.authRequired` | [AuthRequiredEventArgs](xref:WebDriverBiDi.Network.AuthRequiredEventArgs) | [Network Guide](modules/network.md) |
| `network.beforeRequestSent` | [BeforeRequestSentEventArgs](xref:WebDriverBiDi.Network.BeforeRequestSentEventArgs) | [Network Guide](modules/network.md) |
| `network.fetchError` | [FetchErrorEventArgs](xref:WebDriverBiDi.Network.FetchErrorEventArgs) | [Network Guide](modules/network.md) |
| `network.responseCompleted` | [ResponseCompletedEventArgs](xref:WebDriverBiDi.Network.ResponseCompletedEventArgs) | [Network Guide](modules/network.md) |
| `network.responseStarted` | [ResponseStartedEventArgs](xref:WebDriverBiDi.Network.ResponseStartedEventArgs) | [Network Guide](modules/network.md) |

---

## Script Module

Commands for script evaluation, realm management, and preload scripts.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `script.addPreloadScript` | [AddPreloadScriptCommandParameters](xref:WebDriverBiDi.Script.AddPreloadScriptCommandParameters) | [AddPreloadScriptCommandResult](xref:WebDriverBiDi.Script.AddPreloadScriptCommandResult) | [Script Guide](modules/script.md) |
| `script.callFunction` | [CallFunctionCommandParameters](xref:WebDriverBiDi.Script.CallFunctionCommandParameters) | [EvaluateResult](xref:WebDriverBiDi.Script.EvaluateResult) | [Script Guide](modules/script.md) |
| `script.disown` | [DisownCommandParameters](xref:WebDriverBiDi.Script.DisownCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Script Guide](modules/script.md) |
| `script.evaluate` | [EvaluateCommandParameters](xref:WebDriverBiDi.Script.EvaluateCommandParameters) | [EvaluateResult](xref:WebDriverBiDi.Script.EvaluateResult) | [Script Guide](modules/script.md) |
| `script.getRealms` | [GetRealmsCommandParameters](xref:WebDriverBiDi.Script.GetRealmsCommandParameters) | [GetRealmsCommandResult](xref:WebDriverBiDi.Script.GetRealmsCommandResult) | [Script Guide](modules/script.md) |
| `script.removePreloadScript` | [RemovePreloadScriptCommandParameters](xref:WebDriverBiDi.Script.RemovePreloadScriptCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Script Guide](modules/script.md) |

### Script Events

| Protocol Event | EventArgs API | Module Guide |
|----------------|---------------|--------------|
| `script.message` | [MessageEventArgs](xref:WebDriverBiDi.Script.MessageEventArgs) | [Script Guide](modules/script.md) |
| `script.realmCreated` | [RealmInfo](xref:WebDriverBiDi.Script.RealmInfo) | [Script Guide](modules/script.md) |
| `script.realmDestroyed` | [RealmDestroyedEventArgs](xref:WebDriverBiDi.Script.RealmDestroyedEventArgs) | [Script Guide](modules/script.md) |

---

## Input Module

Commands for simulating user input actions.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `input.performActions` | [PerformActionsCommandParameters](xref:WebDriverBiDi.Input.PerformActionsCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Input Guide](modules/input.md) |
| `input.releaseActions` | [ReleaseActionsCommandParameters](xref:WebDriverBiDi.Input.ReleaseActionsCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Input Guide](modules/input.md) |
| `input.setFiles` | [SetFilesCommandParameters](xref:WebDriverBiDi.Input.SetFilesCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Input Guide](modules/input.md) |

---

## Storage Module

Commands for managing cookies and other storage mechanisms.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `storage.deleteCookies` | [DeleteCookiesCommandParameters](xref:WebDriverBiDi.Storage.DeleteCookiesCommandParameters) | [DeleteCookiesCommandResult](xref:WebDriverBiDi.Storage.DeleteCookiesCommandResult) | [Storage Guide](modules/storage.md) |
| `storage.getCookies` | [GetCookiesCommandParameters](xref:WebDriverBiDi.Storage.GetCookiesCommandParameters) | [GetCookiesCommandResult](xref:WebDriverBiDi.Storage.GetCookiesCommandResult) | [Storage Guide](modules/storage.md) |
| `storage.setCookie` | [SetCookieCommandParameters](xref:WebDriverBiDi.Storage.SetCookieCommandParameters) | [SetCookieCommandResult](xref:WebDriverBiDi.Storage.SetCookieCommandResult) | [Storage Guide](modules/storage.md) |

---

## Log Module

Monitoring console log entries.

### Log Events

| Protocol Event | EventArgs API | Module Guide |
|----------------|---------------|--------------|
| `log.entryAdded` | [EntryAddedEventArgs](xref:WebDriverBiDi.Log.EntryAddedEventArgs) | [Log Guide](modules/log.md) |

---

## Bluetooth Module

Commands for simulating Bluetooth adapter and device behavior.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `bluetooth.disableSimulation` | [DisableSimulationCommandParameters](xref:WebDriverBiDi.Bluetooth.DisableSimulationCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.handleRequestDevicePrompt` | [HandleRequestDevicePromptCommandParameters](xref:WebDriverBiDi.Bluetooth.HandleRequestDevicePromptCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateAdapter` | [SimulateAdapterCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateAdapterCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateAdvertisement` | [SimulateAdvertisementCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateAdvertisementCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateCharacteristic` | [SimulateCharacteristicCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateCharacteristicCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateCharacteristicResponse` | [SimulateCharacteristicResponseCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateCharacteristicResponseCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateDescriptor` | [SimulateDescriptorCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateDescriptorCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateDescriptorResponse` | [SimulateDescriptorResponseCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateDescriptorResponseCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateGattConnectionResponse` | [SimulateGattConnectionResponseCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateGattConnectionResponseCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateGattDisconnection` | [SimulateGattDisconnectionCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateGattDisconnectionCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulatePreconnectedPeripheral` | [SimulatePreconnectedPeripheralCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulatePreconnectedPeripheralCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |
| `bluetooth.simulateService` | [SimulateServiceCommandParameters](xref:WebDriverBiDi.Bluetooth.SimulateServiceCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Bluetooth Guide](modules/bluetooth.md) |

### Bluetooth Events

| Protocol Event | EventArgs API | Module Guide |
|----------------|---------------|--------------|
| `bluetooth.requestDevicePromptUpdated` | [RequestDevicePromptUpdatedEventArgs](xref:WebDriverBiDi.Bluetooth.RequestDevicePromptUpdatedEventArgs) | [Bluetooth Guide](modules/bluetooth.md) |

---

## Emulation Module

Commands for device emulation and environment overrides.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `emulation.setForcedColorsModeThemeOverride` | [SetForcedColorsModeThemeOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetForcedColorsModeThemeOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setGeolocationOverride` | [SetGeolocationOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetGeolocationOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setLocaleOverride` | [SetLocaleOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetLocaleOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setNetworkConditions` | [SetNetworkConditionsCommandParameters](xref:WebDriverBiDi.Emulation.SetNetworkConditionsCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setScreenOrientationOverride` | [SetScreenOrientationOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetScreenOrientationOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setScreenSettingsOverride` | [SetScreenSettingsOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetScreenSettingsOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setScriptingEnabled` | [SetScriptingEnabledCommandParameters](xref:WebDriverBiDi.Emulation.SetScriptingEnabledCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setScrollbarTypeOverride` | [SetScrollbarTypeOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetScrollbarTypeOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setTimezoneOverride` | [SetTimeZoneOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetTimeZoneOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setTouchOverride` | [SetTouchOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetTouchOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |
| `emulation.setUserAgentOverride` | [SetUserAgentOverrideCommandParameters](xref:WebDriverBiDi.Emulation.SetUserAgentOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Emulation Guide](modules/emulation.md) |

---

## Permissions Module

Commands for simulating permission grants and denials.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `permissions.setPermission` | [SetPermissionCommandParameters](xref:WebDriverBiDi.Permissions.SetPermissionCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Permissions Guide](modules/permissions.md) |

---

## User Agent Client Hints Module

Commands for overriding User-Agent Client Hints.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `userAgentClientHints.setClientHintsOverride` | [SetClientHintsOverrideCommandParameters](xref:WebDriverBiDi.UserAgentClientHints.SetClientHintsOverrideCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [User Agent Client Hints Guide](modules/user-agent-client-hints.md) |

---

## Web Extension Module

Commands for managing browser extensions.

| Protocol Method | Command API | Result API | Module Guide |
|-----------------|-------------|------------|--------------|
| `webExtension.install` | [InstallCommandParameters](xref:WebDriverBiDi.WebExtension.InstallCommandParameters) | [InstallCommandResult](xref:WebDriverBiDi.WebExtension.InstallCommandResult) | [Web Extension Guide](modules/webextension.md) |
| `webExtension.uninstall` | [UninstallCommandParameters](xref:WebDriverBiDi.WebExtension.UninstallCommandParameters) | [EmptyResult](xref:WebDriverBiDi.Protocol.EmptyResult) | [Web Extension Guide](modules/webextension.md) |

---

## See Also

- [W3C WebDriver BiDi Specification](https://w3c.github.io/webdriver-bidi/)
- [Module Guides](modules/additional-modules.md)
- [API Reference](../api/WebDriverBiDi.yml)
- [Architecture Overview](architecture.md)
