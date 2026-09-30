// <copyright file="ModifierKeyValues.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Input;

/// <summary>
/// The key values of modifier keys, in the order they are pressed.
/// </summary>
internal static class ModifierKeyValues
{
    /// <summary>
    /// Gets the key values of modifier keys.
    /// </summary>
    /// <param name="modifiers">The modifier keys.</param>
    /// <returns>Their key values.</returns>
    public static string[] For(KeyModifiers modifiers)
    {
        List<string> keys = [];
        if (modifiers.HasFlag(KeyModifiers.Alt))
        {
            keys.Add(Keys.Alt);
        }

        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            keys.Add(Keys.Control);
        }

        if (modifiers.HasFlag(KeyModifiers.Meta))
        {
            keys.Add(Keys.Meta);
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            keys.Add(Keys.Shift);
        }

        return [.. keys];
    }
}
