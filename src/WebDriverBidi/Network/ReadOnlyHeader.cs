// <copyright file="ReadOnlyHeader.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

/// <summary>
/// A read-only header from a request.
/// </summary>
public class ReadOnlyHeader
{
    private readonly Header header;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyHeader"/> class.
    /// </summary>
    /// <param name="header">The header to make read-only.</param>
    internal ReadOnlyHeader(Header header)
    {
        this.header = header;
    }

    /// <summary>
    /// Gets the name of the header.
    /// </summary>
    public string Name => this.header.Name;

    /// <summary>
    /// Gets the value of the header.
    /// </summary>
    public BytesValue Value => this.header.Value;
}