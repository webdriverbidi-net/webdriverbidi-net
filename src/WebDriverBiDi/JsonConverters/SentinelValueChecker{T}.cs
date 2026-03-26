// <copyright file="SentinelValueChecker{T}.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Determines whether a given value of type <typeparamref name="T"/> is a sentinel
/// that should be serialized as JSON <c>null</c> by <see cref="SentinelNullJsonConverter{T,TSentinelChecker}"/>.
/// </summary>
/// <typeparam name="T">The value type to check.</typeparam>
public abstract class SentinelValueChecker<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SentinelValueChecker{T}"/> class.
    /// </summary>
    protected SentinelValueChecker()
    {
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="value"/> is the sentinel
    /// that should be written as JSON <c>null</c>.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns><see langword="true"/> if the value should be serialized as JSON <c>null</c>; otherwise, <see langword="false"/>.</returns>
    public abstract bool IsSentinelValue(T value);
}
