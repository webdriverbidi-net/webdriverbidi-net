// <copyright file="CodeAnalysisPolyfill.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1602 // Enumeration items should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace System.Diagnostics.CodeAnalysis;

#if !NET5_0_OR_GREATER
[Flags]
internal enum DynamicallyAccessedMemberTypes
{
    None = 0,
    PublicParameterlessConstructor = 0x0001,
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.GenericParameter | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Struct, Inherited = false)]
internal sealed class DynamicallyAccessedMembersAttribute : Attribute
{
    public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
    {
        this.MemberTypes = memberTypes;
    }

    public DynamicallyAccessedMemberTypes MemberTypes { get; }
}
#endif // !NET5_0_OR_GREATER
