// <copyright file="NegativeDoubleSentinelChecker.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.JsonConverters;

/// <summary>
/// Sentinel checker for <see cref="double"/> that treats any negative value as the
/// sentinel null value, used to indicate a writing an explicit null value in the
/// JSON payload.
/// </summary>
public class NegativeDoubleSentinelChecker : SentinelValueChecker<double>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NegativeDoubleSentinelChecker"/> class.
    /// </summary>
    public NegativeDoubleSentinelChecker()
    {
    }

    /// <inheritdoc />
    public override bool IsSentinelValue(double value) => value < 0;
}
