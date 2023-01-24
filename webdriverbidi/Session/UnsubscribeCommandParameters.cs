// <copyright file="UnsubscribeCommandParameters.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Session;

using Newtonsoft.Json;

/// <summary>
/// Provides parameters for the session.unsubscribe command.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class UnsubscribeCommandParameters : SubscribeCommandParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnsubscribeCommandParameters"/> class.
    /// </summary>
    public UnsubscribeCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    public override string MethodName => "session.unsubscribe";
}