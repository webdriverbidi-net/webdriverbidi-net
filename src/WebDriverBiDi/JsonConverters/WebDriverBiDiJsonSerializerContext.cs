// <copyright file="WebDriverBiDiJsonSerializerContext.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using WebDriverBiDi.Bluetooth;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Input;
using WebDriverBiDi.Network;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;
using WebDriverBiDi.WebExtension;

// ── Protocol ──
[JsonSerializable(typeof(Command))]

// ── Bluetooth module ──
[JsonSerializable(typeof(DisableSimulationCommandParameters))]
[JsonSerializable(typeof(HandleRequestDevicePromptCommandParameters))]
[JsonSerializable(typeof(HandleRequestDevicePromptAcceptCommandParameters))]
[JsonSerializable(typeof(HandleRequestDevicePromptCancelCommandParameters))]
[JsonSerializable(typeof(SimulateAdapterCommandParameters))]
[JsonSerializable(typeof(SimulateAdvertisementCommandParameters))]
[JsonSerializable(typeof(SimulateCharacteristicCommandParameters))]
[JsonSerializable(typeof(SimulateCharacteristicResponseCommandParameters))]
[JsonSerializable(typeof(SimulateDescriptorCommandParameters))]
[JsonSerializable(typeof(SimulateDescriptorResponseCommandParameters))]
[JsonSerializable(typeof(SimulateGattConnectionResponseCommandParameters))]
[JsonSerializable(typeof(SimulateGattDisconnectionCommandParameters))]
[JsonSerializable(typeof(SimulatePreconnectedPeripheralCommandParameters))]
[JsonSerializable(typeof(SimulateServiceCommandParameters))]
[JsonSerializable(typeof(BluetoothManufacturerData))]
[JsonSerializable(typeof(CharacteristicProperties))]
[JsonSerializable(typeof(ScanRecord))]
[JsonSerializable(typeof(SimulateAdvertisementScanEntry))]
[JsonSerializable(typeof(AdapterState))]
[JsonSerializable(typeof(CharacteristicEventGeneratedType))]
[JsonSerializable(typeof(DescriptorEventGeneratedType))]
[JsonSerializable(typeof(SimulateCharacteristicResponseType))]
[JsonSerializable(typeof(SimulateCharacteristicType))]
[JsonSerializable(typeof(SimulateDescriptorResponseType))]
[JsonSerializable(typeof(SimulateDescriptorType))]
[JsonSerializable(typeof(SimulateServiceType))]

// ── Browser module ──
[JsonSerializable(typeof(Browser.CloseCommandParameters), TypeInfoPropertyName = "BrowserCloseCommandParameters")]
[JsonSerializable(typeof(CreateUserContextCommandParameters))]
[JsonSerializable(typeof(GetClientWindowsCommandParameters))]
[JsonSerializable(typeof(GetUserContextsCommandParameters))]
[JsonSerializable(typeof(RemoveUserContextCommandParameters))]
[JsonSerializable(typeof(SetClientWindowStateCommandParameters))]
[JsonSerializable(typeof(SetDownloadBehaviorCommandParameters))]
[JsonSerializable(typeof(DownloadBehavior))]
[JsonSerializable(typeof(DownloadBehaviorAllowed))]
[JsonSerializable(typeof(DownloadBehaviorDenied))]
[JsonSerializable(typeof(ClientWindowState))]
[JsonSerializable(typeof(DownloadBehaviorType))]

// ── BrowsingContext module ──
[JsonSerializable(typeof(ActivateCommandParameters))]
[JsonSerializable(typeof(CaptureScreenshotCommandParameters))]
[JsonSerializable(typeof(BrowsingContext.CloseCommandParameters), TypeInfoPropertyName = "BrowsingContextCloseCommandParameters")]
[JsonSerializable(typeof(CreateCommandParameters))]
[JsonSerializable(typeof(GetTreeCommandParameters))]
[JsonSerializable(typeof(HandleUserPromptCommandParameters))]
[JsonSerializable(typeof(LocateNodesCommandParameters))]
[JsonSerializable(typeof(NavigateCommandParameters))]
[JsonSerializable(typeof(PrintCommandParameters))]
[JsonSerializable(typeof(ReloadCommandParameters))]
[JsonSerializable(typeof(SetViewportCommandParameters))]
[JsonSerializable(typeof(TraverseHistoryCommandParameters))]
[JsonSerializable(typeof(Viewport))]
[JsonSerializable(typeof(Locator))]
[JsonSerializable(typeof(AccessibilityLocator))]
[JsonSerializable(typeof(ContextLocator))]
[JsonSerializable(typeof(CssLocator))]
[JsonSerializable(typeof(InnerTextLocator))]
[JsonSerializable(typeof(XPathLocator))]
[JsonSerializable(typeof(ClipRectangle))]
[JsonSerializable(typeof(BoxClipRectangle))]
[JsonSerializable(typeof(ElementClipRectangle))]
[JsonSerializable(typeof(PrintMarginParameters))]
[JsonSerializable(typeof(PrintPageParameters))]
[JsonSerializable(typeof(CreateType))]
[JsonSerializable(typeof(DownloadEndStatus))]
[JsonSerializable(typeof(InnerTextMatchType))]
[JsonSerializable(typeof(PrintOrientation))]
[JsonSerializable(typeof(ReadinessState))]
[JsonSerializable(typeof(ScreenshotOrigin))]
[JsonSerializable(typeof(UserPromptType))]

// ── Emulation module ──
[JsonSerializable(typeof(SetForcedColorsModeThemeOverrideCommandParameters))]
[JsonSerializable(typeof(SetGeolocationOverrideCommandParameters))]
[JsonSerializable(typeof(SetGeolocationOverrideCoordinatesCommandParameters))]
[JsonSerializable(typeof(SetGeolocationOverrideErrorCommandParameters))]
[JsonSerializable(typeof(SetLocaleOverrideCommandParameters))]
[JsonSerializable(typeof(SetNetworkConditionsCommandParameters))]
[JsonSerializable(typeof(SetScreenOrientationOverrideCommandParameters))]
[JsonSerializable(typeof(SetScreenSettingsOverrideCommandParameters))]
[JsonSerializable(typeof(SetScriptingEnabledCommandParameters))]
[JsonSerializable(typeof(SetTimeZoneOverrideCommandParameters))]
[JsonSerializable(typeof(SetTouchOverrideCommandParameters))]
[JsonSerializable(typeof(SetUserAgentOverrideCommandParameters))]
[JsonSerializable(typeof(GeolocationCoordinates))]
[JsonSerializable(typeof(GeolocationPositionError))]
[JsonSerializable(typeof(NetworkConditions))]
[JsonSerializable(typeof(NetworkConditionsOffline))]
[JsonSerializable(typeof(ScreenArea))]
[JsonSerializable(typeof(ScreenOrientation))]
[JsonSerializable(typeof(ForcedColorsModeTheme))]
[JsonSerializable(typeof(ScreenOrientationNatural))]
[JsonSerializable(typeof(ScreenOrientationType))]

// ── Input module ──
[JsonSerializable(typeof(PerformActionsCommandParameters))]
[JsonSerializable(typeof(ReleaseActionsCommandParameters))]
[JsonSerializable(typeof(SetFilesCommandParameters))]
[JsonSerializable(typeof(SourceActions))]
[JsonSerializable(typeof(KeySourceActions))]
[JsonSerializable(typeof(NoneSourceActions))]
[JsonSerializable(typeof(PointerSourceActions))]
[JsonSerializable(typeof(WheelSourceActions))]
[JsonSerializable(typeof(KeyDownAction))]
[JsonSerializable(typeof(KeyUpAction))]
[JsonSerializable(typeof(PauseAction))]
[JsonSerializable(typeof(PointerDownAction))]
[JsonSerializable(typeof(PointerMoveAction))]
[JsonSerializable(typeof(PointerUpAction))]
[JsonSerializable(typeof(WheelScrollAction))]
[JsonSerializable(typeof(PointerAction))]
[JsonSerializable(typeof(PointerParameters))]
[JsonSerializable(typeof(Origin))]
[JsonSerializable(typeof(ElementOrigin))]
[JsonSerializable(typeof(FileDialogInfo))]
[JsonSerializable(typeof(PointerType))]

// ── Network module ──
[JsonSerializable(typeof(AddDataCollectorCommandParameters))]
[JsonSerializable(typeof(AddInterceptCommandParameters))]
[JsonSerializable(typeof(ContinueRequestCommandParameters))]
[JsonSerializable(typeof(ContinueResponseCommandParameters))]
[JsonSerializable(typeof(ContinueWithAuthCommandParameters))]
[JsonSerializable(typeof(DisownDataCommandParameters))]
[JsonSerializable(typeof(FailRequestCommandParameters))]
[JsonSerializable(typeof(GetDataCommandParameters))]
[JsonSerializable(typeof(ProvideResponseCommandParameters))]
[JsonSerializable(typeof(RemoveDataCollectorCommandParameters))]
[JsonSerializable(typeof(RemoveInterceptCommandParameters))]
[JsonSerializable(typeof(SetCacheBehaviorCommandParameters))]
[JsonSerializable(typeof(SetExtraHeadersCommandParameters))]
[JsonSerializable(typeof(UrlPattern))]
[JsonSerializable(typeof(UrlPatternPattern))]
[JsonSerializable(typeof(UrlPatternString))]
[JsonSerializable(typeof(BytesValue))]
[JsonSerializable(typeof(Header))]
[JsonSerializable(typeof(CookieHeader))]
[JsonSerializable(typeof(SetCookieHeader))]
[JsonSerializable(typeof(AuthCredentials))]
[JsonSerializable(typeof(BytesValueType))]
[JsonSerializable(typeof(CacheBehavior))]
[JsonSerializable(typeof(CollectorType))]
[JsonSerializable(typeof(ContinueWithAuthActionType))]
[JsonSerializable(typeof(CookieSameSiteValue))]
[JsonSerializable(typeof(DataType))]
[JsonSerializable(typeof(InitiatorType))]
[JsonSerializable(typeof(InterceptPhase))]
[JsonSerializable(typeof(UrlPatternType))]

// ── Permissions module ──
[JsonSerializable(typeof(SetPermissionCommandParameters))]
[JsonSerializable(typeof(PermissionDescriptor))]
[JsonSerializable(typeof(PermissionState))]

// ── Script module ──
[JsonSerializable(typeof(AddPreloadScriptCommandParameters))]
[JsonSerializable(typeof(CallFunctionCommandParameters))]
[JsonSerializable(typeof(DisownCommandParameters))]
[JsonSerializable(typeof(EvaluateCommandParameters))]
[JsonSerializable(typeof(GetRealmsCommandParameters))]
[JsonSerializable(typeof(RemovePreloadScriptCommandParameters))]
[JsonSerializable(typeof(Target))]
[JsonSerializable(typeof(ContextTarget))]
[JsonSerializable(typeof(RealmTarget))]
[JsonSerializable(typeof(ArgumentValue))]
[JsonSerializable(typeof(LocalValue))]
[JsonSerializable(typeof(RemoteReference))]
[JsonSerializable(typeof(RemoteObjectReference))]
[JsonSerializable(typeof(SharedReference))]
[JsonSerializable(typeof(ChannelValue))]
[JsonSerializable(typeof(ChannelProperties))]
[JsonSerializable(typeof(SerializationOptions))]
[JsonSerializable(typeof(EvaluateResultType))]
[JsonSerializable(typeof(IncludeShadowTreeSerializationOption))]
[JsonSerializable(typeof(RealmType))]
[JsonSerializable(typeof(ResultOwnership))]
[JsonSerializable(typeof(ShadowRootMode))]

// ── Session module ──
[JsonSerializable(typeof(EndCommandParameters))]
[JsonSerializable(typeof(NewCommandParameters))]
[JsonSerializable(typeof(StatusCommandParameters))]
[JsonSerializable(typeof(SubscribeCommandParameters))]
[JsonSerializable(typeof(UnsubscribeCommandParameters))]
[JsonSerializable(typeof(UnsubscribeByAttributesCommandParameters))]
[JsonSerializable(typeof(UnsubscribeByIdsCommandParameters))]
[JsonSerializable(typeof(CapabilitiesRequest))]
[JsonSerializable(typeof(CapabilityRequest))]
[JsonSerializable(typeof(ProxyConfiguration))]
[JsonSerializable(typeof(AutoDetectProxyConfiguration))]
[JsonSerializable(typeof(DirectProxyConfiguration))]
[JsonSerializable(typeof(ManualProxyConfiguration))]
[JsonSerializable(typeof(PacProxyConfiguration))]
[JsonSerializable(typeof(SystemProxyConfiguration))]
[JsonSerializable(typeof(UserPromptHandler))]
[JsonSerializable(typeof(ProxyType))]
[JsonSerializable(typeof(UserPromptHandlerType))]

// ── Storage module ──
[JsonSerializable(typeof(DeleteCookiesCommandParameters))]
[JsonSerializable(typeof(GetCookiesCommandParameters))]
[JsonSerializable(typeof(SetCookieCommandParameters))]
[JsonSerializable(typeof(PartialCookie))]
[JsonSerializable(typeof(Storage.CookieFilter))]
[JsonSerializable(typeof(PartitionDescriptor))]
[JsonSerializable(typeof(BrowsingContextPartitionDescriptor))]
[JsonSerializable(typeof(StorageKeyPartitionDescriptor))]
[JsonSerializable(typeof(PartitionKey))]

// ── WebExtension module ──
[JsonSerializable(typeof(InstallCommandParameters))]
[JsonSerializable(typeof(UninstallCommandParameters))]
[JsonSerializable(typeof(ExtensionData))]
[JsonSerializable(typeof(ExtensionArchivePath))]
[JsonSerializable(typeof(ExtensionBase64Encoded))]
[JsonSerializable(typeof(ExtensionPath))]

/// <summary>
/// Source-generated JSON serializer context providing AOT-compatible metadata
/// for all types that flow through WebDriverBiDi command serialization.
/// <para>
/// AOT consumers should set this as their <c>TypeInfoResolver</c>:
/// <code>
/// var options = new JsonSerializerOptions
/// {
///     TypeInfoResolver = WebDriverBiDiJsonSerializerContext.Default,
/// };
/// </code>
/// </para>
/// </summary>
public partial class WebDriverBiDiJsonSerializerContext : JsonSerializerContext
{
    // Root enum array types for AOT. EnumValueJsonConverter<T> calls
    // Enum.GetValues(typeof(T)) which requires T[] at runtime.
    // A static constructor ensures the AOT compiler generates these array types.
    static WebDriverBiDiJsonSerializerContext()
    {
        // Bluetooth enums
        RuntimeHelpers.RunClassConstructor(typeof(AdapterState[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CharacteristicEventGeneratedType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(DescriptorEventGeneratedType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SimulateCharacteristicResponseType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SimulateCharacteristicType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SimulateDescriptorResponseType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SimulateDescriptorType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(SimulateServiceType[]).TypeHandle);

        // Browser enums
        RuntimeHelpers.RunClassConstructor(typeof(ClientWindowState[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(DownloadBehaviorType[]).TypeHandle);

        // BrowsingContext enums
        RuntimeHelpers.RunClassConstructor(typeof(CreateType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(DownloadEndStatus[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(InnerTextMatchType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(PrintOrientation[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ReadinessState[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ScreenshotOrigin[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(UserPromptType[]).TypeHandle);

        // Emulation enums
        RuntimeHelpers.RunClassConstructor(typeof(ForcedColorsModeTheme[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ScreenOrientationNatural[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ScreenOrientationType[]).TypeHandle);

        // Input enums
        RuntimeHelpers.RunClassConstructor(typeof(PointerType[]).TypeHandle);

        // Log enums
        RuntimeHelpers.RunClassConstructor(typeof(Log.LogLevel[]).TypeHandle);

        // Network enums
        RuntimeHelpers.RunClassConstructor(typeof(BytesValueType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CacheBehavior[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CollectorType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ContinueWithAuthActionType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CookieSameSiteValue[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(DataType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(InitiatorType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(InterceptPhase[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(UrlPatternType[]).TypeHandle);

        // Permissions enums
        RuntimeHelpers.RunClassConstructor(typeof(PermissionState[]).TypeHandle);

        // Script enums
        RuntimeHelpers.RunClassConstructor(typeof(EvaluateResultType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(IncludeShadowTreeSerializationOption[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(RealmType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ResultOwnership[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(ShadowRootMode[]).TypeHandle);

        // Session enums
        RuntimeHelpers.RunClassConstructor(typeof(ProxyType[]).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(UserPromptHandlerType[]).TypeHandle);

        // Speculation enums
        RuntimeHelpers.RunClassConstructor(typeof(Speculation.PreloadingStatus[]).TypeHandle);
    }
}
