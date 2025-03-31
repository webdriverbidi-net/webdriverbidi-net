// <copyright file="ArgumentValue.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Abstract base class for arguments used in scripts.
/// </summary>
[JsonDerivedType(typeof(LocalValue))]
[JsonDerivedType(typeof(RemoteReference))]
[JsonDerivedType(typeof(RemoteObjectReference))]
[JsonDerivedType(typeof(SharedReference))]
public abstract record ArgumentValue
{
}
