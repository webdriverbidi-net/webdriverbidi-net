// <copyright file="Keys.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

/// <summary>
/// A helper class for providing character values for non-character keys on a keyboard.
/// </summary>
public static class Keys
{
        /// <summary>
        /// Represents the NUL keystroke.
        /// </summary>
        public static readonly char Null = Convert.ToChar(0xE000);

        /// <summary>
        /// Represents the Cancel keystroke.
        /// </summary>
        public static readonly char Cancel = Convert.ToChar(0xE001);

        /// <summary>
        /// Represents the Help keystroke.
        /// </summary>
        public static readonly char Help = Convert.ToChar(0xE002);

        /// <summary>
        /// Represents the Backspace key.
        /// </summary>
        public static readonly char Backspace = Convert.ToChar(0xE003);

        /// <summary>
        /// Represents the Tab key.
        /// </summary>
        public static readonly char Tab = Convert.ToChar(0xE004);

        /// <summary>
        /// Represents the Clear keystroke.
        /// </summary>
        public static readonly char Clear = Convert.ToChar(0xE005);

        /// <summary>
        /// Represents the Return key.
        /// </summary>
        public static readonly char Return = Convert.ToChar(0xE006);

        /// <summary>
        /// Represents the Enter key.
        /// </summary>
        public static readonly char Enter = Convert.ToChar(0xE007);

        /// <summary>
        /// Represents the Shift key.
        /// </summary>
        public static readonly char Shift = Convert.ToChar(0xE008);

        /// <summary>
        /// Represents the Shift key.
        /// </summary>
        public static readonly char LeftShift = Convert.ToChar(0xE008); // alias

        /// <summary>
        /// Represents the Control key.
        /// </summary>
        public static readonly char Control = Convert.ToChar(0xE009);

        /// <summary>
        /// Represents the Control key.
        /// </summary>
        public static readonly char LeftControl = Convert.ToChar(0xE009); // alias

        /// <summary>
        /// Represents the Alt key.
        /// </summary>
        public static readonly char Alt = Convert.ToChar(0xE00A);

        /// <summary>
        /// Represents the Alt key.
        /// </summary>
        public static readonly char LeftAlt = Convert.ToChar(0xE00A); // alias

        /// <summary>
        /// Represents the Pause key.
        /// </summary>
        public static readonly char Pause = Convert.ToChar(0xE00B);

        /// <summary>
        /// Represents the Escape key.
        /// </summary>
        public static readonly char Escape = Convert.ToChar(0xE00C);

        /// <summary>
        /// Represents the space bar key.
        /// </summary>
        public static readonly char Space = Convert.ToChar(0xE00D);

        /// <summary>
        /// Represents the Page Up key.
        /// </summary>
        public static readonly char PageUp = Convert.ToChar(0xE00E);

        /// <summary>
        /// Represents the Page Down key.
        /// </summary>
        public static readonly char PageDown = Convert.ToChar(0xE00F);

        /// <summary>
        /// Represents the End key.
        /// </summary>
        public static readonly char End = Convert.ToChar(0xE010);

        /// <summary>
        /// Represents the Home key.
        /// </summary>
        public static readonly char Home = Convert.ToChar(0xE011);

        /// <summary>
        /// Represents the left arrow key.
        /// </summary>
        public static readonly char Left = Convert.ToChar(0xE012);

        /// <summary>
        /// Represents the left arrow key.
        /// </summary>
        public static readonly char ArrowLeft = Convert.ToChar(0xE012); // alias

        /// <summary>
        /// Represents the up arrow key.
        /// </summary>
        public static readonly char Up = Convert.ToChar(0xE013);

        /// <summary>
        /// Represents the up arrow key.
        /// </summary>
        public static readonly char ArrowUp = Convert.ToChar(0xE013); // alias

        /// <summary>
        /// Represents the right arrow key.
        /// </summary>
        public static readonly char Right = Convert.ToChar(0xE014);

        /// <summary>
        /// Represents the right arrow key.
        /// </summary>
        public static readonly char ArrowRight = Convert.ToChar(0xE014); // alias

        /// <summary>
        /// Represents the down arrow key.
        /// </summary>
        public static readonly char Down = Convert.ToChar(0xE015);

        /// <summary>
        /// Represents the down arrow key.
        /// </summary>
        public static readonly char ArrowDown = Convert.ToChar(0xE015); // alias

        /// <summary>
        /// Represents the Insert key.
        /// </summary>
        public static readonly char Insert = Convert.ToChar(0xE016);

        /// <summary>
        /// Represents the Delete key.
        /// </summary>
        public static readonly char Delete = Convert.ToChar(0xE017);

        /// <summary>
        /// Represents the semi-colon key.
        /// </summary>
        public static readonly char Semicolon = Convert.ToChar(0xE018);

        /// <summary>
        /// Represents the equal sign key.
        /// </summary>
        public static readonly char Equal = Convert.ToChar(0xE019);

        // Number pad keys

        /// <summary>
        /// Represents the number pad 0 key.
        /// </summary>
        public static readonly char NumberPad0 = Convert.ToChar(0xE01A);

        /// <summary>
        /// Represents the number pad 1 key.
        /// </summary>
        public static readonly char NumberPad1 = Convert.ToChar(0xE01B);

        /// <summary>
        /// Represents the number pad 2 key.
        /// </summary>
        public static readonly char NumberPad2 = Convert.ToChar(0xE01C);

        /// <summary>
        /// Represents the number pad 3 key.
        /// </summary>
        public static readonly char NumberPad3 = Convert.ToChar(0xE01D);

        /// <summary>
        /// Represents the number pad 4 key.
        /// </summary>
        public static readonly char NumberPad4 = Convert.ToChar(0xE01E);

        /// <summary>
        /// Represents the number pad 5 key.
        /// </summary>
        public static readonly char NumberPad5 = Convert.ToChar(0xE01F);

        /// <summary>
        /// Represents the number pad 6 key.
        /// </summary>
        public static readonly char NumberPad6 = Convert.ToChar(0xE020);

        /// <summary>
        /// Represents the number pad 7 key.
        /// </summary>
        public static readonly char NumberPad7 = Convert.ToChar(0xE021);

        /// <summary>
        /// Represents the number pad 8 key.
        /// </summary>
        public static readonly char NumberPad8 = Convert.ToChar(0xE022);

        /// <summary>
        /// Represents the number pad 9 key.
        /// </summary>
        public static readonly char NumberPad9 = Convert.ToChar(0xE023);

        /// <summary>
        /// Represents the number pad multiplication key.
        /// </summary>
        public static readonly char Multiply = Convert.ToChar(0xE024);

        /// <summary>
        /// Represents the number pad addition key.
        /// </summary>
        public static readonly char Add = Convert.ToChar(0xE025);

        /// <summary>
        /// Represents the number pad thousands separator key.
        /// </summary>
        public static readonly char Separator = Convert.ToChar(0xE026);

        /// <summary>
        /// Represents the number pad subtraction key.
        /// </summary>
        public static readonly char Subtract = Convert.ToChar(0xE027);

        /// <summary>
        /// Represents the number pad decimal separator key.
        /// </summary>
        public static readonly char Decimal = Convert.ToChar(0xE028);

        /// <summary>
        /// Represents the number pad division key.
        /// </summary>
        public static readonly char Divide = Convert.ToChar(0xE029);

        // Function keys

        /// <summary>
        /// Represents the function key F1.
        /// </summary>
        public static readonly char F1 = Convert.ToChar(0xE031);

        /// <summary>
        /// Represents the function key F2.
        /// </summary>
        public static readonly char F2 = Convert.ToChar(0xE032);

        /// <summary>
        /// Represents the function key F3.
        /// </summary>
        public static readonly char F3 = Convert.ToChar(0xE033);

        /// <summary>
        /// Represents the function key F4.
        /// </summary>
        public static readonly char F4 = Convert.ToChar(0xE034);

        /// <summary>
        /// Represents the function key F5.
        /// </summary>
        public static readonly char F5 = Convert.ToChar(0xE035);

        /// <summary>
        /// Represents the function key F6.
        /// </summary>
        public static readonly char F6 = Convert.ToChar(0xE036);

        /// <summary>
        /// Represents the function key F7.
        /// </summary>
        public static readonly char F7 = Convert.ToChar(0xE037);

        /// <summary>
        /// Represents the function key F8.
        /// </summary>
        public static readonly char F8 = Convert.ToChar(0xE038);

        /// <summary>
        /// Represents the function key F9.
        /// </summary>
        public static readonly char F9 = Convert.ToChar(0xE039);

        /// <summary>
        /// Represents the function key F10.
        /// </summary>
        public static readonly char F10 = Convert.ToChar(0xE03A);

        /// <summary>
        /// Represents the function key F11.
        /// </summary>
        public static readonly char F11 = Convert.ToChar(0xE03B);

        /// <summary>
        /// Represents the function key F12.
        /// </summary>
        public static readonly char F12 = Convert.ToChar(0xE03C);

        /// <summary>
        /// Represents the function key META.
        /// </summary>
        public static readonly char Meta = Convert.ToChar(0xE03D);

        /// <summary>
        /// Represents the function key COMMAND.
        /// </summary>
        public static readonly char Command = Convert.ToChar(0xE03D);

        /// <summary>
        /// Represents the Zenkaku/Hankaku key.
        /// </summary>
        public static readonly char ZenkakuHankaku = Convert.ToChar(0xE040);
}
