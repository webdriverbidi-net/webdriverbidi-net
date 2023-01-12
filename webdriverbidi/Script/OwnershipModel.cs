// <copyright file="OwnershipModel.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

/// <summary>
/// Value of the ownership model of values returned from script execution.
/// </summary>
public enum OwnershipModel
{
    /// <summary>
    /// Use no ownership model.
    /// </summary>
    None,

    /// <summary>
    /// Values are owned by root.
    /// </summary>
    Root,
}