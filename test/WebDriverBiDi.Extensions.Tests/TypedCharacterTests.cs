// <copyright file="TypedCharacterTests.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi;

using WebDriverBiDi.Input;

// Each case gives the characters as .NET Framework's StringInfo splits them, and as one user would count them. The
// .NET Framework tests compile this file too, where the netstandard2.0 build joins what StringInfo splits.
public class TypedCharacterTests
{
    private const string Joiner = "‍";
    private const string Man = "\U0001F468";
    private const string Woman = "\U0001F469";
    private const string Girl = "\U0001F467";
    private const string ThumbsUp = "\U0001F44D";
    private const string MediumSkinTone = "\U0001F3FD";
    private const string BlackFlag = "\U0001F3F4";
    private const string WhiteFlagWithSelector = "\U0001F3F3️";
    private const string Rainbow = "\U0001F308";
    private const string KeycapOne = "1️⃣";

    // England's flag: a black flag followed by the tags "gbeng" and a cancel tag.
    private static readonly string[] EnglandTags = ["\U000E0067", "\U000E0062", "\U000E0065", "\U000E006E", "\U000E0067", "\U000E007F"];

    public static TheoryData<string[], string[]> FrameworkSplits => new()
    {
        { [Man, Joiner, Woman, Joiner, Girl], [Man + Joiner + Woman + Joiner + Girl] },
        { [ThumbsUp, MediumSkinTone, "!"], [ThumbsUp + MediumSkinTone, "!"] },
        { [WhiteFlagWithSelector, Joiner, Rainbow], [WhiteFlagWithSelector + Joiner + Rainbow] },
        { ["\U0001F1FA", "\U0001F1F8", "\U0001F1EC", "\U0001F1E7", "\U0001F1EB"], ["\U0001F1FA\U0001F1F8", "\U0001F1EC\U0001F1E7", "\U0001F1EB"] },
        { [BlackFlag, .. EnglandTags], [BlackFlag + string.Concat(EnglandTags)] },
        { ["क", Joiner, "ष"], ["क" + Joiner, "ष"] },
        { [Man, Joiner, "a"], [Man + Joiner, "a"] },
        { [KeycapOne, "a", "\uD83D"], [KeycapOne, "a", "\uD83D"] },
    };

    [Theory]
    [MemberData(nameof(FrameworkSplits))]
    public void EachUserPerceivedCharacterIsTypedAsOneKey(string[] frameworkElements, string[] expected)
    {
        InputBuilder builder = new();

        builder.AddSendKeysToActiveElementAction(string.Concat(frameworkElements));

        KeySourceActions keyboard = Assert.IsType<KeySourceActions>(Assert.Single(builder.Build()));
        Assert.Equal(expected, keyboard.Actions.OfType<KeyDownAction>().Select(action => action.Value));
    }
}
