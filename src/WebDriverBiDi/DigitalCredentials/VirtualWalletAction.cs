// <copyright file="VirtualWalletAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.DigitalCredentials;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Enumerated value describing actions for the wallet containing digital credentials.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<VirtualWalletAction>))]
public enum VirtualWalletAction
{
    /// <summary>
    /// Value indicating the virtual wallet simulates a user cancellation or rejection, aborting the request.
    /// </summary>
    Decline,

    /// <summary>
    /// Value indicating the virtual wallet simulates a successful user interaction and returns the predefined credential response.
    /// </summary>
    Respond,

    /// <summary>
    /// Value indicating the virtual wallet simulates an active, pending prompt, effectively leaving
    /// the active promise unsettled to test timeouts or concurrent request handling.
    /// </summary>
    Wait,

    /// <summary>
    /// Value indicating the command should clear the active virtual wallet behavior.
    /// </summary>
    Clear,
}
