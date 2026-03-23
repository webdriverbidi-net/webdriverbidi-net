// <copyright file="RemoteValueType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

/// <summary>
/// Enumeration of the types of remote values that can be returned from the browser.
/// </summary>
public enum RemoteValueType
{
    /// <summary>
    /// The RemoteValue represents the Javascript 'undefined' value.
    /// </summary>
    Undefined,

    /// <summary>
    /// The RemoteValue represents the Javascript 'null' value.
    /// </summary>
    Null,

    /// <summary>
    /// The RemoteValue represents a string value from the remote end.
    /// </summary>
    String,

    /// <summary>
    /// The RemoteValue represents a number value from the remote end.
    /// </summary>
    Number,

    /// <summary>
    /// The RemoteValue represents a boolean value from the remote end.
    /// </summary>
    Boolean,

    /// <summary>
    /// The RemoteValue represents a large integer value from the remote end.
    /// </summary>
    BigInt,

    /// <summary>
    /// The RemoteValue represents a JavaScript Symbol value from the remote end.
    /// </summary>
    Symbol,

    /// <summary>
    /// The RemoteValue represents a JavaScript array value from the remote end.
    /// </summary>
    Array,

    /// <summary>
    /// The RemoteValue represents a JavaScript object value from the remote end.
    /// </summary>
    Object,

    /// <summary>
    /// The RemoteValue represents a JavaScript Function object from the remote end.
    /// </summary>
    Function,

    /// <summary>
    /// The RemoteValue represents a JavaScript Regular Expression object from the remote end.
    /// </summary>
    RegExp,

    /// <summary>
    /// The RemoteValue represents a JavaScript Date object from the remote end.
    /// </summary>
    Date,

    /// <summary>
    /// The RemoteValue represents a JavaScript Map object from the remote end.
    /// </summary>
    Map,

    /// <summary>
    /// The RemoteValue represents a JavaScript Set object from the remote end.
    /// </summary>
    Set,

    /// <summary>
    /// The RemoteValue represents a JavaScript WeakMap object from the remote end.
    /// </summary>
    WeakMap,

    /// <summary>
    /// The RemoteValue represents a JavaScript WeakSet object from the remote end.
    /// </summary>
    WeakSet,

    /// <summary>
    /// The RemoteValue represents a JavaScript Generator object from the remote end.
    /// </summary>
    Generator,

    /// <summary>
    /// The RemoteValue represents a JavaScript Error object from the remote end.
    /// </summary>
    Error,

    /// <summary>
    /// The RemoteValue represents a JavaScript Proxy object from the remote end.
    /// </summary>
    Proxy,

    /// <summary>
    /// The RemoteValue represents a JavaScript Promise object from the remote end.
    /// </summary>
    Promise,

    /// <summary>
    /// The RemoteValue represents a JavaScript TypedArray object from the remote end.
    /// </summary>
    TypedArray,

    /// <summary>
    /// The RemoteValue represents a JavaScript ArrayBuffer object from the remote end.
    /// </summary>
    ArrayBuffer,

    /// <summary>
    /// The RemoteValue represents a JavaScript NodeList object from the remote end.
    /// </summary>
    NodeList,

    /// <summary>
    /// The RemoteValue represents a JavaScript HtmlCollection object from the remote end.
    /// </summary>
    HtmlCollection,

    /// <summary>
    /// The RemoteValue represents a JavaScript DOM Node object from the remote end.
    /// </summary>
    Node,

    /// <summary>
    /// The RemoteValue represents a JavaScript Window object from the remote end.
    /// </summary>
    Window,
}
