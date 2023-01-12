// <copyright file="ScriptTarget.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using WebDriverBidi.JsonConverters;

/// <summary>
/// Abstract base class for script targets.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(ScriptTargetJsonConverter))]
public abstract class ScriptTarget
{
}