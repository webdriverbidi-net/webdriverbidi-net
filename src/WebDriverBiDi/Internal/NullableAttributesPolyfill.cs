// <copyright file="NullableAttributesPolyfill.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1623 // Property summary documentation should match BCL style
#pragma warning disable SA1649 // File name should match first type name

#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotNullWhenAttribute"/> class with the specified return value condition.
        /// </summary>
        /// <param name="returnValue">
        /// The return value condition. If the method returns this value, the associated parameter will not be null.
        /// </param>
        public NotNullWhenAttribute(bool returnValue) => this.ReturnValue = returnValue;

        /// <summary>
        /// Gets the return value condition.
        /// </summary>
        public bool ReturnValue { get; }
    }

    /// <summary>
    /// Specifies that the associated parameter or return value is not null even if the corresponding type allows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class NotNullAttribute : Attribute
    {
    }

    /// <summary>
    /// Specifies that when a method or property returns the specified return value, the specified member will not be null even if the corresponding type allows it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    internal sealed class MemberNotNullWhenAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberNotNullWhenAttribute"/> class with the specified return value condition and member name.
        /// </summary>
        /// <param name="returnValue">
        /// The return value condition. If the method or property returns this value, the associated member will not be null.
        /// </param>
        /// <param name="member">
        /// The name of the member that is guaranteed to be non-null.
        /// </param>
        public MemberNotNullWhenAttribute(bool returnValue, string member)
        {
            this.ReturnValue = returnValue;
            this.Members = new[] { member };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberNotNullWhenAttribute"/> class with the specified return value condition and member names.
        /// </summary>
        /// <param name="returnValue">
        /// The return value condition. If the method or property returns this value, the associated members will not be null.
        /// </param>
        /// <param name="members">
        /// The names of the members that are guaranteed to be non-null.
        /// </param>
        public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
        {
            this.ReturnValue = returnValue;
            this.Members = members;
        }

        /// <summary>
        /// Gets the return value condition.
        /// </summary>
        public bool ReturnValue { get; }

        /// <summary>
        /// Gets the member names.
        /// </summary>
        public string[] Members { get; }
    }
}
#endif
