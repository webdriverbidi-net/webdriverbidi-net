// <copyright file="Target.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using Newtonsoft.Json;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Abstract base class for script targets.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(ScriptTargetJsonConverter))]
public abstract class Target
{
}
