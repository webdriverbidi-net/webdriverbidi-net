// <copyright file="Keys.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

/// <summary>
/// A helper class for providing character values for non-character keys on a keyboard.
/// </summary>
public static class Keys
{
    /// <summary>
    /// Represents the NUL keystroke.
    /// </summary>
    public const string Null = "\uE000";

    /// <summary>
    /// Represents the Cancel keystroke.
    /// </summary>
    public const string Cancel = "\uE001";

    /// <summary>
    /// Represents the Help keystroke.
    /// </summary>
    public const string Help = "\uE002";

    /// <summary>
    /// Represents the Backspace key.
    /// </summary>
    public const string Backspace = "\uE003";

    /// <summary>
    /// Represents the Tab key.
    /// </summary>
    public const string Tab = "\uE004";

    /// <summary>
    /// Represents the Clear keystroke.
    /// </summary>
    public const string Clear = "\uE005";

    /// <summary>
    /// Represents the Return key.
    /// </summary>
    public const string Return = "\uE006";

    /// <summary>
    /// Represents the Enter key.
    /// </summary>
    public const string Enter = "\uE007";

    /// <summary>
    /// Represents the Shift key.
    /// </summary>
    public const string Shift = "\uE008";

    /// <summary>
    /// Represents the Shift key.
    /// </summary>
    public const string LeftShift = "\uE008"; // alias

    /// <summary>
    /// Represents the Control key.
    /// </summary>
    public const string Control = "\uE009";

    /// <summary>
    /// Represents the Control key.
    /// </summary>
    public const string LeftControl = "\uE009"; // alias

    /// <summary>
    /// Represents the Alt key.
    /// </summary>
    public const string Alt = "\uE00A";

    /// <summary>
    /// Represents the Alt key.
    /// </summary>
    public const string LeftAlt = "\uE00A"; // alias

    /// <summary>
    /// Represents the Pause key.
    /// </summary>
    public const string Pause = "\uE00B";

    /// <summary>
    /// Represents the Escape key.
    /// </summary>
    public const string Escape = "\uE00C";

    /// <summary>
    /// Represents the space bar key.
    /// </summary>
    public const string Space = "\uE00D";

    /// <summary>
    /// Represents the Page Up key.
    /// </summary>
    public const string PageUp = "\uE00E";

    /// <summary>
    /// Represents the Page Down key.
    /// </summary>
    public const string PageDown = "\uE00F";

    /// <summary>
    /// Represents the End key.
    /// </summary>
    public const string End = "\uE010";

    /// <summary>
    /// Represents the Home key.
    /// </summary>
    public const string Home = "\uE011";

    /// <summary>
    /// Represents the left arrow key.
    /// </summary>
    public const string Left = "\uE012";

    /// <summary>
    /// Represents the left arrow key.
    /// </summary>
    public const string ArrowLeft = "\uE012"; // alias

    /// <summary>
    /// Represents the up arrow key.
    /// </summary>
    public const string Up = "\uE013";

    /// <summary>
    /// Represents the up arrow key.
    /// </summary>
    public const string ArrowUp = "\uE013"; // alias

    /// <summary>
    /// Represents the right arrow key.
    /// </summary>
    public const string Right = "\uE014";

    /// <summary>
    /// Represents the right arrow key.
    /// </summary>
    public const string ArrowRight = "\uE014"; // alias

    /// <summary>
    /// Represents the down arrow key.
    /// </summary>
    public const string Down = "\uE015";

    /// <summary>
    /// Represents the down arrow key.
    /// </summary>
    public const string ArrowDown = "\uE015"; // alias

    /// <summary>
    /// Represents the Insert key.
    /// </summary>
    public const string Insert = "\uE016";

    /// <summary>
    /// Represents the Delete key.
    /// </summary>
    public const string Delete = "\uE017";

    /// <summary>
    /// Represents the semi-colon key.
    /// </summary>
    public const string Semicolon = "\uE018";

    /// <summary>
    /// Represents the equal sign key.
    /// </summary>
    public const string Equal = "\uE019";

    // Number pad keys

    /// <summary>
    /// Represents the number pad 0 key.
    /// </summary>
    public const string NumberPad0 = "\uE01A";

    /// <summary>
    /// Represents the number pad 1 key.
    /// </summary>
    public const string NumberPad1 = "\uE01B";

    /// <summary>
    /// Represents the number pad 2 key.
    /// </summary>
    public const string NumberPad2 = "\uE01C";

    /// <summary>
    /// Represents the number pad 3 key.
    /// </summary>
    public const string NumberPad3 = "\uE01D";

    /// <summary>
    /// Represents the number pad 4 key.
    /// </summary>
    public const string NumberPad4 = "\uE01E";

    /// <summary>
    /// Represents the number pad 5 key.
    /// </summary>
    public const string NumberPad5 = "\uE01F";

    /// <summary>
    /// Represents the number pad 6 key.
    /// </summary>
    public const string NumberPad6 = "\uE020";

    /// <summary>
    /// Represents the number pad 7 key.
    /// </summary>
    public const string NumberPad7 = "\uE021";

    /// <summary>
    /// Represents the number pad 8 key.
    /// </summary>
    public const string NumberPad8 = "\uE022";

    /// <summary>
    /// Represents the number pad 9 key.
    /// </summary>
    public const string NumberPad9 = "\uE023";

    /// <summary>
    /// Represents the number pad multiplication key.
    /// </summary>
    public const string Multiply = "\uE024";

    /// <summary>
    /// Represents the number pad addition key.
    /// </summary>
    public const string Add = "\uE025";

    /// <summary>
    /// Represents the number pad thousands separator key.
    /// </summary>
    public const string Separator = "\uE026";

    /// <summary>
    /// Represents the number pad subtraction key.
    /// </summary>
    public const string Subtract = "\uE027";

    /// <summary>
    /// Represents the number pad decimal separator key.
    /// </summary>
    public const string Decimal = "\uE028";

    /// <summary>
    /// Represents the number pad division key.
    /// </summary>
    public const string Divide = "\uE029";

    // Function keys

    /// <summary>
    /// Represents the function key F1.
    /// </summary>
    public const string F1 = "\uE031";

    /// <summary>
    /// Represents the function key F2.
    /// </summary>
    public const string F2 = "\uE032";

    /// <summary>
    /// Represents the function key F3.
    /// </summary>
    public const string F3 = "\uE033";

    /// <summary>
    /// Represents the function key F4.
    /// </summary>
    public const string F4 = "\uE034";

    /// <summary>
    /// Represents the function key F5.
    /// </summary>
    public const string F5 = "\uE035";

    /// <summary>
    /// Represents the function key F6.
    /// </summary>
    public const string F6 = "\uE036";

    /// <summary>
    /// Represents the function key F7.
    /// </summary>
    public const string F7 = "\uE037";

    /// <summary>
    /// Represents the function key F8.
    /// </summary>
    public const string F8 = "\uE038";

    /// <summary>
    /// Represents the function key F9.
    /// </summary>
    public const string F9 = "\uE039";

    /// <summary>
    /// Represents the function key F10.
    /// </summary>
    public const string F10 = "\uE03A";

    /// <summary>
    /// Represents the function key F11.
    /// </summary>
    public const string F11 = "\uE03B";

    /// <summary>
    /// Represents the function key F12.
    /// </summary>
    public const string F12 = "\uE03C";

    /// <summary>
    /// Represents the function key META.
    /// </summary>
    public const string Meta = "\uE03D";

    /// <summary>
    /// Represents the function key COMMAND.
    /// </summary>
    public const string Command = "\uE03D";

    /// <summary>
    /// Represents the Zenkaku/Hankaku key.
    /// </summary>
    public const string ZenkakuHankaku = "\uE040";

    /// <summary>
    /// Represents the right-hand Shift key.
    /// </summary>
    public const string RightShift = "\uE050";

    /// <summary>
    /// Represents the right-hand Control key.
    /// </summary>
    public const string RightControl = "\uE051";

    /// <summary>
    /// Represents the right-hand Alt key.
    /// </summary>
    public const string RightAlt = "\uE052";

    /// <summary>
    /// Represents the right-hand Meta key.
    /// </summary>
    public const string RightMeta = "\uE053";

    /// <summary>
    /// Represents the Page Up key on the number pad.
    /// </summary>
    public const string NumberPadPageUp = "\uE054";

    /// <summary>
    /// Represents the Page Down key on the number pad.
    /// </summary>
    public const string NumberPadPageDown = "\uE055";

    /// <summary>
    /// Represents the End key on the number pad.
    /// </summary>
    public const string NumberPadEnd = "\uE056";

    /// <summary>
    /// Represents the Home key on the number pad.
    /// </summary>
    public const string NumberPadHome = "\uE057";

    /// <summary>
    /// Represents the left arrow key on the number pad.
    /// </summary>
    public const string NumberPadArrowLeft = "\uE058";

    /// <summary>
    /// Represents the up arrow key on the number pad.
    /// </summary>
    public const string NumberPadArrowUp = "\uE059";

    /// <summary>
    /// Represents the right arrow key on the number pad.
    /// </summary>
    public const string NumberPadArrowRight = "\uE05A";

    /// <summary>
    /// Represents the down arrow key on the number pad.
    /// </summary>
    public const string NumberPadArrowDown = "\uE05B";

    /// <summary>
    /// Represents the Insert key on the number pad.
    /// </summary>
    public const string NumberPadInsert = "\uE05C";

    /// <summary>
    /// Represents the Delete key on the number pad.
    /// </summary>
    public const string NumberPadDelete = "\uE05D";
}
