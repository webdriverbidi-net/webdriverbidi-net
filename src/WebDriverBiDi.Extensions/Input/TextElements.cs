// <copyright file="TextElements.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Globalization;

/// <summary>
/// Splits text into user-perceived characters. The runtime's segmentation does this on .NET 5 and later; .NET
/// Framework follows older Unicode rules, which make separate characters of the zero-width joiner and what it joins,
/// emoji modifiers, tag characters, and each regional indicator, so the netstandard2.0 build joins those again.
/// </summary>
internal static class TextElements
{
#if NETSTANDARD2_0
    private const int ZeroWidthJoiner = 0x200D;
#endif

    /// <summary>
    /// Splits text into user-perceived characters.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The characters, each of one or more code points.</returns>
    public static List<string> Split(string text)
    {
        List<string> elements = [];
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            elements.Add(enumerator.GetTextElement());
        }

#if NETSTANDARD2_0
        return Join(elements);
#else
        return elements;
#endif
    }

#if NETSTANDARD2_0
    /// <summary>
    /// Joins the characters that older Unicode rules split out of one user-perceived character.
    /// </summary>
    /// <param name="elements">The characters, as the runtime's segmentation split them.</param>
    /// <returns>The characters, joined where they are one.</returns>
    internal static List<string> Join(IReadOnlyList<string> elements)
    {
        List<string> joined = [];
        foreach (string element in elements)
        {
            if (joined.Count > 0 && Continues(joined[joined.Count - 1], element))
            {
                joined[joined.Count - 1] += element;
            }
            else
            {
                joined.Add(element);
            }
        }

        return joined;
    }

    private static bool Continues(string previous, string next)
    {
        int first = FirstCodePoint(next);

        // A joiner, an emoji modifier, or a tag character extends the character before it.
        if (first == ZeroWidthJoiner || first is >= 0x1F3FB and <= 0x1F3FF || first is >= 0xE0020 and <= 0xE007F)
        {
            return true;
        }

        // A joiner between two pictographs makes them one, as in a family emoji.
        if (previous[previous.Length - 1] == ZeroWidthJoiner && IsPictographic(FirstCodePoint(previous)) && IsPictographic(first))
        {
            return true;
        }

        // Regional indicators pair into flags.
        return IsRegionalIndicator(first) && previous.Length == 2 && IsRegionalIndicator(FirstCodePoint(previous));
    }

    // Text may hold a lone surrogate, which is its own character.
    private static int FirstCodePoint(string text) => char.IsSurrogatePair(text, 0) ? char.ConvertToUtf32(text, 0) : text[0];

    // Extended_Pictographic from Unicode's emoji data, compacted into ranges; it only decides whether a joiner joins.
    private static bool IsPictographic(int codePoint) => codePoint is 0x00A9 or 0x00AE or 0x203C or 0x2049 or 0x2122 or 0x2139
        or (>= 0x2194 and <= 0x21AA) or (>= 0x231A and <= 0x23FF) or 0x24C2 or (>= 0x25AA and <= 0x25FE) or (>= 0x2600 and <= 0x27BF)
        or 0x2934 or 0x2935 or (>= 0x2B05 and <= 0x2B55) or 0x3030 or 0x303D or 0x3297 or 0x3299
        or (>= 0x1F000 and <= 0x1F1E5) or (>= 0x1F200 and <= 0x1F3FA) or (>= 0x1F400 and <= 0x1FAFF) or (>= 0x1FC00 and <= 0x1FFFD);

    private static bool IsRegionalIndicator(int codePoint) => codePoint is >= 0x1F1E6 and <= 0x1F1FF;
#endif
}
