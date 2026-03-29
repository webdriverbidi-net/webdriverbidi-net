// <copyright file="CodeAnalysisPolyfill.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1602 // Enum elements should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace System.Diagnostics.CodeAnalysis;

using System.ComponentModel;

// This file provides polyfill attributes for code analysis features that are not
// available in all target frameworks. This enables use of the `required` modifier
// and related features while maintaining compatibility with older frameworks.
#if !NET7_0_OR_GREATER
[Flags]
internal enum DynamicallyAccessedMemberTypes
{
    None = 0,
    PublicParameterlessConstructor = 0x0001,
    PublicConstructors = 0x0002 | PublicParameterlessConstructor,
    NonPublicConstructors = 0x0004,
    PublicMethods = 0x0008,
    NonPublicMethods = 0x0010,
    PublicFields = 0x0020,
    NonPublicFields = 0x0040,
    PublicNestedTypes = 0x0080,
    NonPublicNestedTypes = 0x0100,
    PublicProperties = 0x0200,
    NonPublicProperties = 0x0400,
    PublicEvents = 0x0800,
    NonPublicEvents = 0x1000,
    Interfaces = 0x2000,
    NonPublicConstructorsWithInherited = NonPublicConstructors | 0x4000,
    NonPublicMethodsWithInherited = NonPublicMethods | 0x8000,
    NonPublicFieldsWithInherited = NonPublicFields | 0x10000,
    NonPublicNestedTypesWithInherited = NonPublicNestedTypes | 0x20000,
    NonPublicPropertiesWithInherited = NonPublicProperties | 0x40000,
    NonPublicEventsWithInherited = NonPublicEvents | 0x80000,
    PublicConstructorsWithInherited = PublicConstructors | 0x100000,
    PublicNestedTypesWithInherited = PublicNestedTypes | 0x200000,
    AllConstructors = PublicConstructorsWithInherited | NonPublicConstructorsWithInherited,
    AllMethods = PublicMethods | NonPublicMethodsWithInherited,
    AllFields = PublicFields | NonPublicFieldsWithInherited,
    AllNestedTypes = PublicNestedTypesWithInherited | NonPublicNestedTypesWithInherited,
    AllProperties = PublicProperties | NonPublicPropertiesWithInherited,
    AllEvents = PublicEvents | NonPublicEventsWithInherited,

    [EditorBrowsable(EditorBrowsableState.Never)]
    All = ~None,
}

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
internal sealed class SetsRequiredMembersAttribute
    : Attribute
{
}

internal sealed class DynamicallyAccessedMembersAttribute : Attribute
{
    public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
    {
        this.MemberTypes = memberTypes;
    }

    public DynamicallyAccessedMemberTypes MemberTypes { get; }
}


#endif // !NET7_0_OR_GREATER
