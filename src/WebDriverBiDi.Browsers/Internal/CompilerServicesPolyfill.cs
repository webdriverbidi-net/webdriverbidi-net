// <copyright file="CompilerServicesPolyfill.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1649 // File name should match first type name

namespace System.Runtime.CompilerServices;

#if !NET5_0_OR_GREATER
using System.ComponentModel;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}
#endif // !NET5_0_OR_GREATER
