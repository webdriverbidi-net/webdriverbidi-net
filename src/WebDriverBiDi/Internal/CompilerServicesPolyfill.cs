// <copyright file="CompilerServicesPolyfill.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace System.Runtime.CompilerServices;

// This file provides polyfill attributes for compiler features that are not available
// in all target frameworks. This enables use of the `required` modifier and the `init`
// accessor while maintaining compatibility with older frameworks.
#if !NET5_0_OR_GREATER
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}
#endif // !NET5_0_OR_GREATER

#if !NET7_0_OR_GREATER
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class RequiredMemberAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    public const string RefStructs = nameof(RefStructs);

    public const string RequiredMembers = nameof(RequiredMembers);

    public CompilerFeatureRequiredAttribute(string featureName)
    {
        this.FeatureName = featureName;
    }

    public string FeatureName { get; }

    public bool IsOptional { get; init; }
}
#endif // !NET7_0_OR_GREATER
