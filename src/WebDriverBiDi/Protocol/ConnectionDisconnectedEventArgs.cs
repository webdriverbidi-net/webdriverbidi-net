// <copyright file="ConnectionDisconnectedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing event data for events raised when the remote end gracefully closes a WebDriver Bidi connection.
/// </summary>
public record ConnectionDisconnectedEventArgs : WebDriverBiDiEventArgs
{
}
