// <copyright file="ErrorCode.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Enumeration of error codes that may be returned by the remote end of the WebDriver BiDi protocol in
/// error responses to commands or in unsolicited error events. Each value is associated with the string
/// value used in the JSON error responses.
/// </summary>
/// <remarks>
/// Most values are the error codes the WebDriver BiDi protocol specification defines. The rest are WebDriver
/// classic error codes, which the WebDriver BiDi specification does not define but which remote ends built on
/// a classic WebDriver implementation have been observed to return; each of those says so in its own
/// documentation. An error code string that matches no value is reported as <see cref="UnsetErrorCode"/>.
/// </remarks>
[JsonConverter(typeof(EnumValueJsonConverter<ErrorCode>))]
[StringEnumUnmatchedValue<ErrorCode>(UnsetErrorCode)]
public enum ErrorCode
{
    /// <summary>
    /// Default value indicating that the error code was not set or did not match any known error code string.
    /// </summary>
    UnsetErrorCode,

    /// <summary>
    /// A command failed because the referenced shadow root is no longer attached to the DOM.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("detached shadow root")]
    DetachedShadowRoot,

    /// <summary>
    /// The Element Click command could not be completed because the element receiving the
    /// events is obscuring the element that was requested clicked.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("element click intercepted")]
    ElementClickIntercepted,

    /// <summary>
    /// A command could not be completed because the element is not pointer- or keyboard interactable.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("element not interactable")]
    ElementNotInteractable,

    /// <summary>
    /// Navigation caused the user agent to hit a certificate warning, which is usually the
    /// result of an expired or invalid TLS certificate.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("insecure certificate")]
    InsecureCertificate,

    /// <summary>
    /// An argument sent during a command was invalid or malformed.
    /// </summary>
    [StringEnumValue("invalid argument")]
    InvalidArgument,

    /// <summary>
    /// An illegal attempt was made to set a cookie under a different domain than the current page.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("invalid cookie domain")]
    InvalidCookieDomain,

    /// <summary>
    /// A command could not be completed because the element is in an invalid state, e.g. attempting
    /// to clear an element that isn't both editable and resettable.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("invalid element state")]
    InvalidElementState,

    /// <summary>
    /// An element command failed because an invalid selector was used.
    /// </summary>
    [StringEnumValue("invalid selector")]
    InvalidSelector,

    /// <summary>
    /// The session ID used in the request was not valid.
    /// </summary>
    [StringEnumValue("invalid session id")]
    InvalidSessionId,

    /// <summary>
    /// The web extension data used in the command was not valid.
    /// </summary>
    [StringEnumValue("invalid web extension")]
    InvalidWebExtension,

    /// <summary>
    /// An error occurred while executing JavaScript supplied by the user.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("javascript error")]
    JavascriptError,

    /// <summary>
    /// The target for the pointer move action is not valid for the current view port.
    /// </summary>
    [StringEnumValue("move target out of bounds")]
    MoveTargetOutOfBounds,

    /// <summary>
    /// An attempt was made to operate on an alert that is not present.
    /// </summary>
    [StringEnumValue("no such alert")]
    NoSuchAlert,

    /// <summary>
    /// The ID of the client window was invalid when attempting to set the client
    /// window state.
    /// </summary>
    [StringEnumValue("no such client window")]
    NoSuchClientWindow,

    /// <summary>
    /// No cookie matching the given path name was found amongst the associated
    /// cookies of session's current browsing context's active document.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("no such cookie")]
    NoSuchCookie,

    /// <summary>
    /// No Bluetooth device matching the given device ID was found.
    /// </summary>
    /// <remarks>
    /// This error code comes from the Web Bluetooth specification.
    /// </remarks>
    [StringEnumValue("no such device")]
    NoSuchDevice,

    /// <summary>
    /// The target element for the command was not found.
    /// </summary>
    [StringEnumValue("no such element")]
    NoSuchElement,

    /// <summary>
    /// The target frame for the command was not found.
    /// </summary>
    [StringEnumValue("no such frame")]
    NoSuchFrame,

    /// <summary>
    /// The target handle for the command was not found.
    /// </summary>
    [StringEnumValue("no such handle")]
    NoSuchHandle,

    /// <summary>
    /// The history entry for the command was not found.
    /// </summary>
    [StringEnumValue("no such history entry")]
    NoSuchHistoryEntry,

    /// <summary>
    /// The ID of the network intercept used in the command was not valid.
    /// </summary>
    [StringEnumValue("no such intercept")]
    NoSuchIntercept,

    /// <summary>
    /// The network data collector ID used in the command was not valid.
    /// </summary>
    [StringEnumValue("no such network collector")]
    NoSuchNetworkCollector,

    /// <summary>
    /// The data collected by the network data collector was not valid.
    /// </summary>
    [StringEnumValue("no such network data")]
    NoSuchNetworkData,

    /// <summary>
    /// The node ID used in the command was not found.
    /// </summary>
    [StringEnumValue("no such node")]
    NoSuchNode,

    /// <summary>
    /// No prompt for a Bluetooth device matching the given prompt ID was found.
    /// </summary>
    /// <remarks>
    /// This error code comes from the Web Bluetooth specification.
    /// </remarks>
    [StringEnumValue("no such prompt")]
    NoSuchPrompt,

    /// <summary>
    /// The request ID for the network request used in the command was not found.
    /// </summary>
    [StringEnumValue("no such request")]
    NoSuchRequest,

    /// <summary>
    /// The ID of the screencast used in the command was not found.
    /// </summary>
    [StringEnumValue("no such screencast")]
    NoSuchScreencast,

    /// <summary>
    /// The ID of the script used in the command was not found.
    /// </summary>
    [StringEnumValue("no such script")]
    NoSuchScript,

    /// <summary>
    /// The element does not have a shadow root.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("no such shadow root")]
    NoSuchShadowRoot,

    /// <summary>
    /// The storage partition used in the command was not found.
    /// </summary>
    [StringEnumValue("no such storage partition")]
    NoSuchStoragePartition,

    /// <summary>
    /// The ID of the user context used in the command was not found.
    /// </summary>
    [StringEnumValue("no such user context")]
    NoSuchUserContext,

    /// <summary>
    /// The ID of the web extension used in the command was not found.
    /// </summary>
    [StringEnumValue("no such web extension")]
    NoSuchWebExtension,

    /// <summary>
    /// A command to switch to a window could not be satisfied because the window could not be found.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("no such window")]
    NoSuchWindow,

    /// <summary>
    /// A script did not complete before its timeout expired.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("script timeout error")]
    ScriptTimeoutError,

    /// <summary>
    /// A new session could not be created successfully.
    /// </summary>
    [StringEnumValue("session not created")]
    SessionNotCreated,

    /// <summary>
    /// A command failed because the referenced element is no longer attached to the DOM.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("stale element reference")]
    StaleElementReference,

    /// <summary>
    /// An operation did not complete before its timeout expired.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("timeout")]
    Timeout,

    /// <summary>
    /// The command was unable to capture the screenshot of the current browsing context.
    /// </summary>
    [StringEnumValue("unable to capture screen")]
    UnableToCaptureScreen,

    /// <summary>
    /// The command was unable to close the browser window or tab successfully.
    /// </summary>
    [StringEnumValue("unable to close browser")]
    UnableToCloseBrowser,

    /// <summary>
    /// The command was unable to set the cookie successfully.
    /// </summary>
    [StringEnumValue("unable to set cookie")]
    UnableToSetCookie,

    /// <summary>
    /// The command was unable to set the value of a file input element successfully.
    /// </summary>
    [StringEnumValue("unable to set file input")]
    UnableToSetFileInput,

    /// <summary>
    /// The command was unable to perform the requested operation on the network data.
    /// </summary>
    [StringEnumValue("unavailable network data")]
    UnavailableNetworkData,

    /// <summary>
    /// The command was unable to perform the requested operation on the storage partition, because the
    /// parameters used to specify it were insufficient.
    /// </summary>
    [StringEnumValue("underspecified storage partition")]
    UnderspecifiedStoragePartition,

    /// <summary>
    /// A modal dialog was open, blocking this operation.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("unexpected alert open")]
    UnexpectedAlertOpen,

    /// <summary>
    /// The command was not recognized by the remote end of the WebDriver BiDi protocol, and could not be processed.
    /// </summary>
    [StringEnumValue("unknown command")]
    UnknownCommand,

    /// <summary>
    /// The remote end of the WebDriver BiDi protocol returned an error that did not match any of the other defined error codes.
    /// </summary>
    [StringEnumValue("unknown error")]
    UnknownError,

    /// <summary>
    /// The requested command matched a known URL but did not match any method for that URL.
    /// </summary>
    /// <remarks>
    /// A WebDriver classic error code, not defined by the WebDriver BiDi specification. It is included because
    /// remote ends built on a classic WebDriver implementation have been observed to return it.
    /// </remarks>
    [StringEnumValue("unknown method")]
    UnknownMethod,

    /// <summary>
    /// The command is not supported by the remote end of the WebDriver BiDi protocol.
    /// </summary>
    [StringEnumValue("unsupported operation")]
    UnsupportedOperation,
}
