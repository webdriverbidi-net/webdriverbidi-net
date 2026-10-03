// <copyright file="GroupChoice.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Testing;

using System.Reflection;

/// <summary>
/// Chooses between the assembly's shared browser group and a test class's own.
/// </summary>
internal static class GroupChoice
{
    /// <summary>
    /// The name of the base classes' method that configures the launcher.
    /// </summary>
    public const string ConfigureLauncherMethodName = "ConfigureLauncher";

    /// <summary>
    /// The name of the base classes' property that gives the group's options.
    /// </summary>
    public const string GroupOptionsPropertyName = "GroupOptions";

    /// <summary>
    /// Gets a value indicating whether a test class needs its own group, because it overrides how the group is launched.
    /// </summary>
    /// <param name="testType">The test class.</param>
    /// <param name="baseType">The base class declaring the members.</param>
    /// <returns><see langword="true"/> if the class overrides the launcher or the group's options.</returns>
    public static bool UsesOwnGroup(Type testType, Type baseType)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo configureLauncher = testType.GetMethod(ConfigureLauncherMethodName, flags)!;
        MethodInfo groupOptions = testType.GetProperty(GroupOptionsPropertyName, flags)!.GetMethod!;
        return configureLauncher.DeclaringType != baseType || groupOptions.DeclaringType != baseType;
    }
}
