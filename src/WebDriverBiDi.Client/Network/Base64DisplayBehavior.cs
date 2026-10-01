// <copyright file="Base64DisplayBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

/// <summary>
/// Values describing how to display base64 encoded binary data in requests and responses.
/// </summary>
public enum Base64DisplayBehavior
{
    /// <summary>
    /// Do not display the data; instead display a message showing binary data was captured, and its length.
    /// </summary>
    NoDisplay,

    /// <summary>
    /// Display the base64 encoded data.
    /// </summary>
    Display,

    /// <summary>
    /// Decode the base64 data into a byte array and display the decoded data. This may include unprintable characters.
    /// </summary>
    Decode,
}
