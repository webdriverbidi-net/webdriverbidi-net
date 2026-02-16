// <copyright file="WebDriverBiDiJsonSerializerContext.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using WebDriverBiDi.Bluetooth;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Speculation;
using WebDriverBiDi.Storage;
using WebDriverBiDi.UserAgentClientHints;
using WebDriverBiDi.WebExtension;

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
// ── Protocol ──
[JsonSerializable(typeof(Command))]
[JsonSerializable(typeof(ErrorResponseMessage))]

// ── Bluetooth module (command responses and event handlers) ──
[JsonSerializable(typeof(CommandResponseMessage<DisableSimulationCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<HandleRequestDevicePromptCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateAdapterCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateAdvertisementCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateCharacteristicCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateCharacteristicResponseCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateDescriptorCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateDescriptorResponseCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateGattConnectionResponseCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateGattDisconnectionCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulatePreconnectedPeripheralCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SimulateServiceCommandResult>))]
[JsonSerializable(typeof(EventMessage<CharacteristicEventGeneratedEventArgs>))]
[JsonSerializable(typeof(EventMessage<DescriptorEventGeneratedEventArgs>))]
[JsonSerializable(typeof(EventMessage<GattConnectionAttemptedEventArgs>))]
[JsonSerializable(typeof(EventMessage<RequestDevicePromptUpdatedEventArgs>))]

// ── Browser module (command responses) ──
[JsonSerializable(typeof(CommandResponseMessage<Browser.CloseCommandResult>), TypeInfoPropertyName = "BrowserCloseCommandResponse")]
[JsonSerializable(typeof(CommandResponseMessage<CreateUserContextCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<GetClientWindowsCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<GetUserContextsCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<RemoveUserContextCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetClientWindowStateCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetDownloadBehaviorCommandResult>))]

// ── BrowsingContext module (command responses and event handlers) ──
[JsonSerializable(typeof(CommandResponseMessage<ActivateCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<CaptureScreenshotCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<BrowsingContext.CloseCommandResult>), TypeInfoPropertyName = "BrowsingContextCloseCommandResponse")]
[JsonSerializable(typeof(CommandResponseMessage<CreateCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<GetTreeCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<HandleUserPromptCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<LocateNodesCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<NavigateCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<PrintCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<ReloadCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetViewportCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<TraverseHistoryCommandResult>))]
[JsonSerializable(typeof(EventMessage<BrowsingContextInfo>))]
[JsonSerializable(typeof(EventMessage<NavigationEventArgs>))]
[JsonSerializable(typeof(EventMessage<DownloadWillBeginEventArgs>))]
[JsonSerializable(typeof(EventMessage<DownloadEndEventArgs>))]
[JsonSerializable(typeof(EventMessage<HistoryUpdatedEventArgs>))]
[JsonSerializable(typeof(EventMessage<UserPromptClosedEventArgs>))]
[JsonSerializable(typeof(EventMessage<UserPromptOpenedEventArgs>))]

// ── Emulation module (command responses) ──
[JsonSerializable(typeof(CommandResponseMessage<SetForcedColorsModeThemeOverrideCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetGeolocationOverrideCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetLocaleOverrideCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetNetworkConditionsCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetScreenOrientationOverrideCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetScreenSettingsOverrideCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetScriptingEnabledCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetTimeZoneOverrideCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetTouchOverrideCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetUserAgentOverrideCommandResult>))]

// ── Input module (command responses and event handlers) ──
[JsonSerializable(typeof(CommandResponseMessage<PerformActionsCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<ReleaseActionsCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetFilesCommandResult>))]
[JsonSerializable(typeof(EventMessage<FileDialogInfo>))]

// -- Log module (event handlers) --
[JsonSerializable(typeof(EventMessage<LogEntry>))]

// ── Network module (command responses and event handlers) ──
[JsonSerializable(typeof(CommandResponseMessage<AddDataCollectorCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<AddInterceptCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<ContinueRequestCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<ContinueResponseCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<ContinueWithAuthCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<DisownDataCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<FailRequestCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<GetDataCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<ProvideResponseCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<RemoveDataCollectorCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<RemoveInterceptCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetCacheBehaviorCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetExtraHeadersCommandResult>))]
[JsonSerializable(typeof(EventMessage<AuthRequiredEventArgs>))]
[JsonSerializable(typeof(EventMessage<BeforeRequestSentEventArgs>))]
[JsonSerializable(typeof(EventMessage<FetchErrorEventArgs>))]
[JsonSerializable(typeof(EventMessage<ResponseStartedEventArgs>))]
[JsonSerializable(typeof(EventMessage<ResponseCompletedEventArgs>))]

// ── Permissions module (command responses) ──
[JsonSerializable(typeof(CommandResponseMessage<SetPermissionCommandResult>))]

// ── Script module (command responses and event handlers) ──
[JsonSerializable(typeof(CommandResponseMessage<AddPreloadScriptCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<DisownCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<EvaluateResult>))]
[JsonSerializable(typeof(CommandResponseMessage<GetRealmsCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<RemovePreloadScriptCommandResult>))]
[JsonSerializable(typeof(EventMessage<MessageEventArgs>))]
[JsonSerializable(typeof(EventMessage<RealmDestroyedEventArgs>))]
[JsonSerializable(typeof(EventMessage<RealmInfo>))]

// ── Session module (command responses) ──
[JsonSerializable(typeof(CommandResponseMessage<EndCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<NewCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<StatusCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SubscribeCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<UnsubscribeCommandResult>))]

// ── Speculation module (event handlers) ──
[JsonSerializable(typeof(EventMessage<PrefetchStatusUpdatedEventArgs>))]

// ── Storage module (command responses) ──
[JsonSerializable(typeof(CommandResponseMessage<DeleteCookiesCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<GetCookiesCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<SetCookieCommandResult>))]

// -- UserAgentClientHints module (command responses) --
[JsonSerializable(typeof(CommandResponseMessage<SetClientHintsOverrideCommandResult>))]

// ── WebExtension module (command responses) ──
[JsonSerializable(typeof(CommandResponseMessage<InstallCommandResult>))]
[JsonSerializable(typeof(CommandResponseMessage<UninstallCommandResult>))]

// ── Bluetooth module ──
[JsonSerializable(typeof(AdapterState))]
[JsonSerializable(typeof(BluetoothManufacturerData))]
[JsonSerializable(typeof(CharacteristicEventGeneratedType))]
[JsonSerializable(typeof(CharacteristicProperties))]
[JsonSerializable(typeof(DescriptorEventGeneratedType))]
[JsonSerializable(typeof(DisableSimulationCommandParameters))]
[JsonSerializable(typeof(HandleRequestDevicePromptAcceptCommandParameters))]
[JsonSerializable(typeof(HandleRequestDevicePromptCommandParameters))]
[JsonSerializable(typeof(HandleRequestDevicePromptCancelCommandParameters))]
[JsonSerializable(typeof(RequestDeviceInfo))]
[JsonSerializable(typeof(ScanRecord))]
[JsonSerializable(typeof(SimulateAdapterCommandParameters))]
[JsonSerializable(typeof(SimulateAdvertisementCommandParameters))]
[JsonSerializable(typeof(SimulateAdvertisementScanEntry))]
[JsonSerializable(typeof(SimulateCharacteristicCommandParameters))]
[JsonSerializable(typeof(SimulateCharacteristicResponseCommandParameters))]
[JsonSerializable(typeof(SimulateCharacteristicResponseType))]
[JsonSerializable(typeof(SimulateCharacteristicType))]
[JsonSerializable(typeof(SimulateDescriptorCommandParameters))]
[JsonSerializable(typeof(SimulateDescriptorResponseCommandParameters))]
[JsonSerializable(typeof(SimulateDescriptorResponseType))]
[JsonSerializable(typeof(SimulateDescriptorType))]
[JsonSerializable(typeof(SimulateGattConnectionResponseCommandParameters))]
[JsonSerializable(typeof(SimulateGattDisconnectionCommandParameters))]
[JsonSerializable(typeof(SimulatePreconnectedPeripheralCommandParameters))]
[JsonSerializable(typeof(SimulateServiceCommandParameters))]
[JsonSerializable(typeof(SimulateServiceType))]

// ── Browser module ──
[JsonSerializable(typeof(ClientWindowInfo))]
[JsonSerializable(typeof(ClientWindowState))]
[JsonSerializable(typeof(Browser.CloseCommandParameters), TypeInfoPropertyName = "BrowserCloseCommandParameters")]
[JsonSerializable(typeof(Browser.CloseCommandResult), TypeInfoPropertyName = "BrowserCloseCommandResult")]
[JsonSerializable(typeof(CreateUserContextCommandParameters))]
[JsonSerializable(typeof(DownloadBehavior))]
[JsonSerializable(typeof(DownloadBehaviorAllowed))]
[JsonSerializable(typeof(DownloadBehaviorDenied))]
[JsonSerializable(typeof(DownloadBehaviorType))]
[JsonSerializable(typeof(GetClientWindowsCommandParameters))]
[JsonSerializable(typeof(GetUserContextsCommandParameters))]
[JsonSerializable(typeof(RemoveUserContextCommandParameters))]
[JsonSerializable(typeof(SetClientWindowStateCommandParameters))]
[JsonSerializable(typeof(SetDownloadBehaviorCommandParameters))]
[JsonSerializable(typeof(DownloadBehaviorAllowed))]
[JsonSerializable(typeof(DownloadBehaviorDenied))]
[JsonSerializable(typeof(UserContextInfo))]

// ── BrowsingContext module ──
[JsonSerializable(typeof(AccessibilityLocator))]
[JsonSerializable(typeof(ActivateCommandParameters))]
[JsonSerializable(typeof(BoxClipRectangle))]
[JsonSerializable(typeof(BrowsingContextInfo))]
[JsonSerializable(typeof(CaptureScreenshotCommandParameters))]
[JsonSerializable(typeof(ClipRectangle))]
[JsonSerializable(typeof(BrowsingContext.CloseCommandParameters), TypeInfoPropertyName = "BrowsingContextCloseCommandParameters")]
[JsonSerializable(typeof(BrowsingContext.CloseCommandResult), TypeInfoPropertyName = "BrowsingContextCloseCommandResult")]
[JsonSerializable(typeof(ContextLocator))]
[JsonSerializable(typeof(CreateCommandParameters))]
[JsonSerializable(typeof(CreateType))]
[JsonSerializable(typeof(CssLocator))]
[JsonSerializable(typeof(DownloadEndStatus))]
[JsonSerializable(typeof(ElementClipRectangle))]
[JsonSerializable(typeof(GetTreeCommandParameters))]
[JsonSerializable(typeof(HandleUserPromptCommandParameters))]
[JsonSerializable(typeof(ImageFormat))]
[JsonSerializable(typeof(InnerTextLocator))]
[JsonSerializable(typeof(InnerTextMatchType))]
[JsonSerializable(typeof(LocateNodesCommandParameters))]
[JsonSerializable(typeof(Locator))]
[JsonSerializable(typeof(NavigateCommandParameters))]
[JsonSerializable(typeof(PrintCommandParameters))]
[JsonSerializable(typeof(PrintMarginParameters))]
[JsonSerializable(typeof(PrintOrientation))]
[JsonSerializable(typeof(PrintPageParameters))]
[JsonSerializable(typeof(ReadinessState))]
[JsonSerializable(typeof(ReloadCommandParameters))]
[JsonSerializable(typeof(ScreenshotOrigin))]
[JsonSerializable(typeof(SetViewportCommandParameters))]
[JsonSerializable(typeof(TraverseHistoryCommandParameters))]
[JsonSerializable(typeof(UserPromptType))]
[JsonSerializable(typeof(Viewport))]
[JsonSerializable(typeof(XPathLocator))]

// ── Emulation module ──
[JsonSerializable(typeof(ForcedColorsModeTheme))]
[JsonSerializable(typeof(GeolocationCoordinates))]
[JsonSerializable(typeof(GeolocationPositionError))]
[JsonSerializable(typeof(NetworkConditions))]
[JsonSerializable(typeof(NetworkConditionsOffline))]
[JsonSerializable(typeof(ScreenArea))]
[JsonSerializable(typeof(ScreenOrientation))]
[JsonSerializable(typeof(ScreenOrientationNatural))]
[JsonSerializable(typeof(ScreenOrientationType))]
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

// ── Input module ──
[JsonSerializable(typeof(ElementOrigin))]
[JsonSerializable(typeof(FileDialogInfo))]
[JsonSerializable(typeof(KeyDownAction))]
[JsonSerializable(typeof(KeySourceActions))]
[JsonSerializable(typeof(KeyUpAction))]
[JsonSerializable(typeof(NoneSourceActions))]
[JsonSerializable(typeof(Origin))]
[JsonSerializable(typeof(PauseAction))]
[JsonSerializable(typeof(PerformActionsCommandParameters))]
[JsonSerializable(typeof(PointerAction))]
[JsonSerializable(typeof(PointerDownAction))]
[JsonSerializable(typeof(PointerMoveAction))]
[JsonSerializable(typeof(PointerParameters))]
[JsonSerializable(typeof(PointerSourceActions))]
[JsonSerializable(typeof(PointerType))]
[JsonSerializable(typeof(PointerUpAction))]
[JsonSerializable(typeof(ReleaseActionsCommandParameters))]
[JsonSerializable(typeof(SetFilesCommandParameters))]
[JsonSerializable(typeof(SourceActions))]
[JsonSerializable(typeof(WheelSourceActions))]
[JsonSerializable(typeof(WheelScrollAction))]

// -- Log module --
[JsonSerializable(typeof(LogEntry))]
[JsonSerializable(typeof(LogLevel))]

// ── Network module ──
[JsonSerializable(typeof(AddDataCollectorCommandParameters))]
[JsonSerializable(typeof(AddInterceptCommandParameters))]
[JsonSerializable(typeof(AuthChallenge))]
[JsonSerializable(typeof(AuthCredentials))]
[JsonSerializable(typeof(BytesValue))]
[JsonSerializable(typeof(BytesValueType))]
[JsonSerializable(typeof(CacheBehavior))]
[JsonSerializable(typeof(CollectorType))]
[JsonSerializable(typeof(ContinueRequestCommandParameters))]
[JsonSerializable(typeof(ContinueResponseCommandParameters))]
[JsonSerializable(typeof(ContinueWithAuthActionType))]
[JsonSerializable(typeof(ContinueWithAuthCommandParameters))]
[JsonSerializable(typeof(Cookie))]
[JsonSerializable(typeof(CookieHeader))]
[JsonSerializable(typeof(CookieSameSiteValue))]
[JsonSerializable(typeof(DataType))]
[JsonSerializable(typeof(DisownDataCommandParameters))]
[JsonSerializable(typeof(FailRequestCommandParameters))]
[JsonSerializable(typeof(FetchTimingInfo))]
[JsonSerializable(typeof(GetDataCommandParameters))]
[JsonSerializable(typeof(Header))]
[JsonSerializable(typeof(Initiator))]
[JsonSerializable(typeof(InitiatorType))]
[JsonSerializable(typeof(InterceptPhase))]
[JsonSerializable(typeof(ProvideResponseCommandParameters))]
[JsonSerializable(typeof(ReadOnlyHeader))]
[JsonSerializable(typeof(RemoveDataCollectorCommandParameters))]
[JsonSerializable(typeof(RemoveInterceptCommandParameters))]
[JsonSerializable(typeof(RequestData))]
[JsonSerializable(typeof(ResponseContent))]
[JsonSerializable(typeof(ResponseData))]
[JsonSerializable(typeof(SetCacheBehaviorCommandParameters))]
[JsonSerializable(typeof(SetCookieHeader))]
[JsonSerializable(typeof(SetExtraHeadersCommandParameters))]
[JsonSerializable(typeof(UrlPattern))]
[JsonSerializable(typeof(UrlPatternPattern))]
[JsonSerializable(typeof(UrlPatternString))]
[JsonSerializable(typeof(UrlPatternType))]

// ── Permissions module ──
[JsonSerializable(typeof(PermissionDescriptor))]
[JsonSerializable(typeof(PermissionState))]
[JsonSerializable(typeof(SetPermissionCommandParameters))]

// ── Script module ──
[JsonSerializable(typeof(AddPreloadScriptCommandParameters))]
[JsonSerializable(typeof(ArgumentValue))]
[JsonSerializable(typeof(CallFunctionCommandParameters))]
[JsonSerializable(typeof(ChannelProperties))]
[JsonSerializable(typeof(ChannelValue))]
[JsonSerializable(typeof(ContextTarget))]
[JsonSerializable(typeof(DisownCommandParameters))]
[JsonSerializable(typeof(EvaluateCommandParameters))]
[JsonSerializable(typeof(EvaluateResult))]
[JsonSerializable(typeof(EvaluateResultSuccess))]
[JsonSerializable(typeof(EvaluateResultException))]
[JsonSerializable(typeof(EvaluateResultType))]
[JsonSerializable(typeof(ExceptionDetails))]
[JsonSerializable(typeof(GetRealmsCommandParameters))]
[JsonSerializable(typeof(IncludeShadowTreeSerializationOption))]
[JsonSerializable(typeof(LocalValue))]
[JsonSerializable(typeof(NodeAttributes))]
[JsonSerializable(typeof(NodeProperties))]
[JsonSerializable(typeof(RealmInfo))]
[JsonSerializable(typeof(RealmTarget))]
[JsonSerializable(typeof(RealmType))]
[JsonSerializable(typeof(RegularExpressionValue))]
[JsonSerializable(typeof(RemoteObjectReference))]
[JsonSerializable(typeof(RemoteReference))]
[JsonSerializable(typeof(RemoteValue))]
[JsonSerializable(typeof(RemovePreloadScriptCommandParameters))]
[JsonSerializable(typeof(ResultOwnership))]
[JsonSerializable(typeof(SerializationOptions))]
[JsonSerializable(typeof(ShadowRootMode))]
[JsonSerializable(typeof(SharedReference))]
[JsonSerializable(typeof(Source))]
[JsonSerializable(typeof(StackFrame))]
[JsonSerializable(typeof(StackTrace))]
[JsonSerializable(typeof(Target))]
[JsonSerializable(typeof(WindowProxyProperties))]
[JsonSerializable(typeof(WindowRealmInfo))]

// ── Session module ──
[JsonSerializable(typeof(AutoDetectProxyConfiguration))]
[JsonSerializable(typeof(CapabilitiesRequest))]
[JsonSerializable(typeof(CapabilityRequest))]
[JsonSerializable(typeof(DirectProxyConfiguration))]
[JsonSerializable(typeof(EndCommandParameters))]
[JsonSerializable(typeof(ManualProxyConfiguration))]
[JsonSerializable(typeof(NewCommandParameters))]
[JsonSerializable(typeof(PacProxyConfiguration))]
[JsonSerializable(typeof(ProxyConfiguration))]
[JsonSerializable(typeof(ProxyType))]
[JsonSerializable(typeof(StatusCommandParameters))]
[JsonSerializable(typeof(SubscribeCommandParameters))]
[JsonSerializable(typeof(SystemProxyConfiguration))]
[JsonSerializable(typeof(UnsubscribeCommandParameters))]
[JsonSerializable(typeof(UnsubscribeByAttributesCommandParameters))]
[JsonSerializable(typeof(UnsubscribeByIdsCommandParameters))]
[JsonSerializable(typeof(UserPromptHandler))]
[JsonSerializable(typeof(UserPromptHandlerType))]

// ── Speculation module ──
[JsonSerializable(typeof(PreloadingStatus))]

// ── Storage module ──
[JsonSerializable(typeof(BrowsingContextPartitionDescriptor))]
[JsonSerializable(typeof(CookieFilter))]
[JsonSerializable(typeof(DeleteCookiesCommandParameters))]
[JsonSerializable(typeof(GetCookiesCommandParameters))]
[JsonSerializable(typeof(PartialCookie))]
[JsonSerializable(typeof(PartitionDescriptor))]
[JsonSerializable(typeof(PartitionKey))]
[JsonSerializable(typeof(SetCookieCommandParameters))]
[JsonSerializable(typeof(StorageKeyPartitionDescriptor))]

// -- UserAgentClientHints module (command responses) --
[JsonSerializable(typeof(BrandVersion))]
[JsonSerializable(typeof(ClientHintsMetadata))]
[JsonSerializable(typeof(SetClientHintsOverrideCommandParameters))]

// ── WebExtension module ──
[JsonSerializable(typeof(ExtensionArchivePath))]
[JsonSerializable(typeof(ExtensionBase64Encoded))]
[JsonSerializable(typeof(ExtensionData))]
[JsonSerializable(typeof(ExtensionPath))]
[JsonSerializable(typeof(InstallCommandParameters))]
[JsonSerializable(typeof(UninstallCommandParameters))]

[ExcludeFromCodeCoverage]
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
        RuntimeHelpers.RunClassConstructor(typeof(PreloadingStatus[]).TypeHandle);
    }
}
