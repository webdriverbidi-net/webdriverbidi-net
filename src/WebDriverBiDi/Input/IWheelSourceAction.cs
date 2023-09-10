// <copyright file="IWheelSourceAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Text.Json.Serialization;

namespace WebDriverBiDi.Input;

/// <summary>
/// Interface marking an action as an action used with a wheel input device.
/// </summary>
[JsonDerivedType(typeof(WheelScrollAction))]
public interface IWheelSourceAction
{
}