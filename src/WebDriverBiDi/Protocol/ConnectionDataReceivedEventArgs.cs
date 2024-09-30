// <copyright file="ConnectionDataReceivedEventArgs.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

/// <summary>
/// Object containing event data for events raised when data is received from a WebDriver Bidi connection.
/// </summary>
public class ConnectionDataReceivedEventArgs : WebDriverBiDiEventArgs
{
    private readonly byte[] data;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionDataReceivedEventArgs" /> class.
    /// </summary>
    /// <param name="data">The data received from the connection.</param>
    public ConnectionDataReceivedEventArgs(byte[] data)
    {
        this.data = data;
    }

    /// <summary>
    /// Gets the data received from the connection.
    /// </summary>
    public byte[] Data => this.data;
}
