// <copyright file="ContinueWithAuthActionType.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// The enumerated value of actions allowed when using the network.continueWithAuth command.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ContinueWithAuthActionType>))]
public enum ContinueWithAuthActionType
{
    /// <summary>
    /// The command will perform the default action.
    /// </summary>
    Default,

    /// <summary>
    /// The command will cancel the auth request.
    /// </summary>
    Cancel,

    /// <summary>
    /// The command will use the provided credentials.
    /// </summary>
    [JsonEnumValue("provideCredentials")]
    ProvideCredentials,
}