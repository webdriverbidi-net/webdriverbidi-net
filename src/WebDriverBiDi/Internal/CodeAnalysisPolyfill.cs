// <copyright file="CodeAnalysisPolyfill.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace System.Diagnostics.CodeAnalysis;

// This file provides polyfill attributes for code analysis features that are not
// available in all target frameworks. This enables use of the `required` modifier
// and related features while maintaining compatibility with older frameworks.
#if !NET7_0_OR_GREATER
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
internal sealed class SetsRequiredMembersAttribute
    : Attribute
{
}
#endif // !NET7_0_OR_GREATER
