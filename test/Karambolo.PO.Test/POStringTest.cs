using System;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace Karambolo.PO.Test
{
    public class POStringTest
    {
        [Theory]

        // no prologue, maxLineLength = 0

        [InlineData("", "", 0, true,
            """
            ""
            """)]

        // no prologue, maxLineLength = 2

        [InlineData("", "", 2, true,
            """
            ""
            """)]
        [InlineData("", " ", 2, true,
            """
            ""
            " "
            """)]
        [InlineData("", "\n", 2, false,
            """
            ""
            "\n"
            """)]
        [InlineData("", "\n", 2, true,
            """
            ""
            "\n"
            """)]
        [InlineData("", "\n-", 2, false,
            """
            ""
            "\n"
            "-"
            """)]
        [InlineData("", "\n-", 2, true,
            """
            ""
            "\n"
            "-"
            """)]
        [InlineData("", "\nx", 2, false,
            """
            ""
            "\n"
            "x"
            """)]
        [InlineData("", "\nx", 2, true,
            """
            ""
            "\n"
            "x"
            """)]
        [InlineData("", "\n💩", 2, false,
            """
            ""
            "\n"
            "💩"
            """)]
        [InlineData("", "\n💩", 2, true,
            """
            ""
            "\n"
            "💩"
            """)]
        [InlineData("", "-", 2, true,
            """
            ""
            "-"
            """)]
        [InlineData("", "-\n", 2, false,
            """
            ""
            "-"
            "\n"
            """)]
        [InlineData("", "-\n", 2, true,
            """
            ""
            "-"
            "\n"
            """)]
        [InlineData("", "-x", 2, true,
            """
            ""
            "-"
            "x"
            """)]
        [InlineData("", "-💩", 2, true,
            """
            ""
            "-"
            "💩"
            """)]
        [InlineData("", "x", 2, true,
            """
            ""
            "x"
            """)]
        [InlineData("", "x\n", 2, false,
            """
            ""
            "x"
            "\n"
            """)]
        [InlineData("", "x\n", 2, true,
            """
            ""
            "x"
            "\n"
            """)]
        [InlineData("", "x-", 2, true,
            """
            ""
            "x"
            "-"
            """)]
        [InlineData("", "x💩", 2, true,
            """
            ""
            "x"
            "💩"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 2, false,
            """
            ""
            "💩"
            "_"
            "💩"
            " "
            "💩"
            "\n"
            "x"
            "-"
            "😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 2, true,
            """
            ""
            "💩"
            "_"
            "💩"
            " "
            "💩"
            "\n"
            "x"
            "-"
            "😀"
            """)]

        // no prologue, maxLineLength = 3

        [InlineData("", " ", 3, true,
            """
            " "
            """)]
        [InlineData("", "\n", 3, false,
            """
            ""
            "\n"
            """)]
        [InlineData("", "\n", 3, true,
            """
            ""
            "\n"
            """)]
        [InlineData("", "\n-", 3, false,
            """
            ""
            "\n"
            "-"
            """)]
        [InlineData("", "\n-", 3, true,
            """
            ""
            "\n"
            "-"
            """)]
        [InlineData("", "\nx", 3, false,
            """
            ""
            "\n"
            "x"
            """)]
        [InlineData("", "\nx", 3, true,
            """
            ""
            "\n"
            "x"
            """)]
        [InlineData("", "\n💩", 3, false,
            """
            ""
            "\n"
            "💩"
            """)]
        [InlineData("", "\n💩", 3, true,
            """
            ""
            "\n"
            "💩"
            """)]
        [InlineData("", "-", 3, true,
            """
            "-"
            """)]
        [InlineData("", "-\n", 3, false,
            """
            ""
            "-"
            "\n"
            """)]
        [InlineData("", "-\n", 3, true,
            """
            ""
            "-"
            "\n"
            """)]
        [InlineData("", "-x", 3, true,
            """
            ""
            "-"
            "x"
            """)]
        [InlineData("", "-💩", 3, true,
            """
            ""
            "-"
            "💩"
            """)]
        [InlineData("", "x", 3, true,
            """
            "x"
            """)]
        [InlineData("", "x\n", 3, false,
            """
            ""
            "x"
            "\n"
            """)]
        [InlineData("", "x\n", 3, true,
            """
            ""
            "x"
            "\n"
            """)]
        [InlineData("", "x-", 3, true,
            """
            ""
            "x"
            "-"
            """)]
        [InlineData("", "x💩", 3, true,
            """
            ""
            "x"
            "💩"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 3, false,
            """
            ""
            "💩"
            "_"
            "💩"
            " "
            "💩"
            "\n"
            "x"
            "-"
            "😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 3, true,
            """
            ""
            "💩"
            "_"
            "💩"
            " "
            "💩"
            "\n"
            "x"
            "-"
            "😀"
            """)]

        // no prologue, maxLineLength = 4

        [InlineData("", "\n", 4, false,
            """
            "\n"
            """)]
        [InlineData("", "\n", 4, true,
            """
            ""
            "\n"
            """)]
        [InlineData("", "\n-", 4, false,
            """
            ""
            "\n"
            "-"
            """)]
        [InlineData("", "\n-", 4, true,
            """
            ""
            "\n"
            "-"
            """)]
        [InlineData("", "\nx", 4, false,
            """
            ""
            "\n"
            "x"
            """)]
        [InlineData("", "\nx", 4, true,
            """
            ""
            "\n"
            "x"
            """)]
        [InlineData("", "\n💩", 4, false,
            """
            ""
            "\n"
            "💩"
            """)]
        [InlineData("", "\n💩", 4, true,
            """
            ""
            "\n"
            "💩"
            """)]
        [InlineData("", "-", 4, true,
            """
            "-"
            """)]
        [InlineData("", "-\n", 4, false,
            """
            ""
            "-"
            "\n"
            """)]
        [InlineData("", "-\n", 4, true,
            """
            ""
            "-"
            "\n"
            """)]
        [InlineData("", "-x", 4, true,
            """
            "-x"
            """)]
        [InlineData("", "-💩", 4, true,
            """
            "-💩"
            """)]
        [InlineData("", "x", 4, true,
            """
            "x"
            """)]
        [InlineData("", "x\n", 4, false,
            """
            ""
            "x"
            "\n"
            """)]
        [InlineData("", "x\n", 4, true,
            """
            ""
            "x"
            "\n"
            """)]
        [InlineData("", "x-", 4, true,
            """
            "x-"
            """)]
        [InlineData("", "x💩", 4, true,
            """
            "x💩"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 4, false,
            """
            ""
            "💩_"
            "💩 "
            "💩"
            "\n"
            "x-"
            "😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 4, true,
            """
            ""
            "💩_"
            "💩 "
            "💩"
            "\n"
            "x-"
            "😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 4, false,
            """
            ""
            "💩­"
            "💩­"
            "💩"
            "\n"
            "x‐"
            "😀-"
            "x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 4, true,
            """
            ""
            "💩­"
            "💩­"
            "💩"
            "\n"
            "x‐"
            "😀-"
            "x"
            """)]

        // no prologue, maxLineLength = 5

        [InlineData("", "\n", 5, false,
            """
            "\n"
            """)]
        [InlineData("", "\n", 5, true,
            """
            ""
            "\n"
            """)]
        [InlineData("", "\n-", 5, false,
            """
            "\n-"
            """)]
        [InlineData("", "\n-", 5, true,
            """
            ""
            "\n"
            "-"
            """)]
        [InlineData("", "\nx", 5, false,
            """
            "\nx"
            """)]
        [InlineData("", "\nx", 5, true,
            """
            ""
            "\n"
            "x"
            """)]
        [InlineData("", "\n💩", 5, false,
            """
            "\n💩"
            """)]
        [InlineData("", "\n💩", 5, true,
            """
            ""
            "\n"
            "💩"
            """)]
        [InlineData("", "-", 5, true,
            """
            "-"
            """)]
        [InlineData("", "-\n", 5, false,
            """
            "-\n"
            """)]
        [InlineData("", "-\n", 5, true,
            """
            ""
            "-\n"
            """)]
        [InlineData("", "-x", 5, true,
            """
            "-x"
            """)]
        [InlineData("", "-💩", 5, true,
            """
            "-💩"
            """)]
        [InlineData("", "x", 5, true,
            """
            "x"
            """)]
        [InlineData("", "x\n", 5, false,
            """
            "x\n"
            """)]
        [InlineData("", "x\n", 5, true,
            """
            ""
            "x\n"
            """)]
        [InlineData("", "x-", 5, true,
            """
            "x-"
            """)]
        [InlineData("", "x💩", 5, true,
            """
            "x💩"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 5, false,
            """
            ""
            "💩_💩"
            " "
            "💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 5, true,
            """
            ""
            "💩_💩"
            " "
            "💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 5, false,
            """
            ""
            "💩­"
            "💩­"
            "💩\n"
            "x‐"
            "😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 5, true,
            """
            ""
            "💩­"
            "💩­"
            "💩\n"
            "x‐"
            "😀-x"
            """)]

        // no prologue, maxLineLength = 6

        [InlineData("", "💩_💩 💩\r\nx-😀", 6, false,
            """
            ""
            "💩_💩 "
            "💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 6, true,
            """
            ""
            "💩_💩 "
            "💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩-💩 💩\r\nx-😀", 6, false,
            """
            ""
            "💩-💩 "
            "💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩-💩 💩\r\nx-😀", 6, true,
            """
            ""
            "💩-💩 "
            "💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 6, false,
            """
            ""
            "💩­💩­"
            "💩\n"
            "x‐😀-"
            "x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 6, true,
            """
            ""
            "💩­💩­"
            "💩\n"
            "x‐😀-"
            "x"
            """)]

        // no prologue, maxLineLength = 7

        [InlineData("", "💩_💩 💩\r\nx-😀", 7, false,
            """
            ""
            "💩_💩 "
            "💩\nx-"
            "😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 7, true,
            """
            ""
            "💩_💩 "
            "💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 7, false,
            """
            ""
            "💩­💩­"
            "💩\nx‐"
            "😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 7, true,
            """
            ""
            "💩­💩­"
            "💩\n"
            "x‐😀-x"
            """)]

        // no prologue, maxLineLength = 11

        [InlineData("", "💩_💩 💩\r\nx-😀", 11, false,
            """
            ""
            "💩_💩 💩\nx-"
            "😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 11, true,
            """
            ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 11, false,
            """
            ""
            "💩­💩­💩\nx‐"
            "😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 11, true,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]

        // no prologue, maxLineLength = 12

        [InlineData("", "💩_💩 💩\r\nx-😀", 12, false,
            """
            "💩_💩 💩\nx-😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", 12, true,
            """
            ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 12, false,
            """
            ""
            "💩­💩­💩\nx‐"
            "😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 12, true,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]

        // no prologue, maxLineLength = -1 (unlimited)

        [InlineData("", "💩_💩 💩\r\nx-😀", -1, false,
            """
            "💩_💩 💩\nx-😀"
            """)]
        [InlineData("", "💩_💩 💩\r\nx-😀", -1, true,
            """
            ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", -1, false,
            """
            "💩­💩­💩\nx‐😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", -1, true,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]

        // with prologue, maxLineLength = 0

        [InlineData("msgid ", "", 0, true,
            """
            msgid ""
            """)]

        // with prologue, maxLineLength = 2

        [InlineData("msgid ", "", 2, true,
            """
            msgid ""
            """)]
        [InlineData("msgid ", " ", 2, true,
            """
            msgid ""
            " "
            """)]
        [InlineData("msgid ", "\n", 2, false,
            """
            msgid ""
            "\n"
            """)]
        [InlineData("msgid ", "\n", 2, true,
            """
            msgid ""
            "\n"
            """)]
        [InlineData("msgid ", "\n-", 2, false,
            """
            msgid ""
            "\n"
            "-"
            """)]
        [InlineData("msgid ", "\n-", 2, true,
            """
            msgid ""
            "\n"
            "-"
            """)]
        [InlineData("msgid ", "\nx", 2, false,
            """
            msgid ""
            "\n"
            "x"
            """)]
        [InlineData("msgid ", "\nx", 2, true,
            """
            msgid ""
            "\n"
            "x"
            """)]
        [InlineData("msgid ", "\n💩", 2, false,
            """
            msgid ""
            "\n"
            "💩"
            """)]
        [InlineData("msgid ", "\n💩", 2, true,
            """
            msgid ""
            "\n"
            "💩"
            """)]
        [InlineData("msgid ", "-", 2, true,
            """
            msgid ""
            "-"
            """)]
        [InlineData("msgid ", "-\n", 2, false,
            """
            msgid ""
            "-"
            "\n"
            """)]
        [InlineData("msgid ", "-\n", 2, true,
            """
            msgid ""
            "-"
            "\n"
            """)]
        [InlineData("msgid ", "-x", 2, true,
            """
            msgid ""
            "-"
            "x"
            """)]
        [InlineData("msgid ", "-💩", 2, true,
            """
            msgid ""
            "-"
            "💩"
            """)]
        [InlineData("msgid ", "x", 2, true,
            """
            msgid ""
            "x"
            """)]
        [InlineData("msgid ", "x\n", 2, false,
            """
            msgid ""
            "x"
            "\n"
            """)]
        [InlineData("msgid ", "x\n", 2, true,
            """
            msgid ""
            "x"
            "\n"
            """)]
        [InlineData("msgid ", "x-", 2, true,
            """
            msgid ""
            "x"
            "-"
            """)]
        [InlineData("msgid ", "x💩", 2, true,
            """
            msgid ""
            "x"
            "💩"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 2, false,
            """
            msgid ""
            "💩"
            "_"
            "💩"
            " "
            "💩"
            "\n"
            "x"
            "-"
            "😀"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 2, true,
            """
            msgid ""
            "💩"
            "_"
            "💩"
            " "
            "💩"
            "\n"
            "x"
            "-"
            "😀"
            """)]

        // with prologue, maxLineLength = 8

        [InlineData("msgid ", " ", 8, true,
            """
            msgid ""
            " "
            """)]
        [InlineData("msgid ", "\n", 8, false,
            """
            msgid ""
            "\n"
            """)]
        [InlineData("msgid ", "\n", 8, true,
            """
            msgid ""
            "\n"
            """)]
        [InlineData("msgid ", "\n-", 8, false,
            """
            msgid ""
            "\n-"
            """)]
        [InlineData("msgid ", "\n-", 8, true,
            """
            msgid ""
            "\n"
            "-"
            """)]
        [InlineData("msgid ", "\nx", 8, false,
            """
            msgid ""
            "\nx"
            """)]
        [InlineData("msgid ", "\nx", 8, true,
            """
            msgid ""
            "\n"
            "x"
            """)]
        [InlineData("msgid ", "\n💩", 8, false,
            """
            msgid ""
            "\n💩"
            """)]
        [InlineData("msgid ", "\n💩", 8, true,
            """
            msgid ""
            "\n"
            "💩"
            """)]
        [InlineData("msgid ", "-", 8, true,
            """
            msgid ""
            "-"
            """)]
        [InlineData("msgid ", "-\n", 8, false,
            """
            msgid ""
            "-\n"
            """)]
        [InlineData("msgid ", "-\n", 8, true,
            """
            msgid ""
            "-\n"
            """)]
        [InlineData("msgid ", "-x", 8, true,
            """
            msgid ""
            "-x"
            """)]
        [InlineData("msgid ", "-💩", 8, true,
            """
            msgid ""
            "-💩"
            """)]
        [InlineData("msgid ", "x", 8, true,
            """
            msgid ""
            "x"
            """)]
        [InlineData("msgid ", "x\n", 8, false,
            """
            msgid ""
            "x\n"
            """)]
        [InlineData("msgid ", "x\n", 8, true,
            """
            msgid ""
            "x\n"
            """)]
        [InlineData("msgid ", "x-", 8, true,
            """
            msgid ""
            "x-"
            """)]
        [InlineData("msgid ", "x💩", 8, true,
            """
            msgid ""
            "x💩"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 8, false,
            """
            msgid ""
            "💩_💩 "
            "💩\nx-😀"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 8, true,
            """
            msgid ""
            "💩_💩 "
            "💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 8, false,
            """
            ""
            "💩­💩­"
            "💩\nx‐"
            "😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 8, true,
            """
            ""
            "💩­💩­"
            "💩\n"
            "x‐😀-x"
            """)]

        // with prologue, maxLineLength = 9

        [InlineData("msgid ", " ", 9, true,
            """
            msgid " "
            """)]
        [InlineData("msgid ", "\n", 9, false,
            """
            msgid ""
            "\n"
            """)]
        [InlineData("msgid ", "\n", 9, true,
            """
            msgid ""
            "\n"
            """)]
        [InlineData("msgid ", "\n-", 9, false,
            """
            msgid ""
            "\n-"
            """)]
        [InlineData("msgid ", "\n-", 9, true,
            """
            msgid ""
            "\n"
            "-"
            """)]
        [InlineData("msgid ", "\nx", 9, false,
            """
            msgid ""
            "\nx"
            """)]
        [InlineData("msgid ", "\nx", 9, true,
            """
            msgid ""
            "\n"
            "x"
            """)]
        [InlineData("msgid ", "\n💩", 9, false,
            """
            msgid ""
            "\n💩"
            """)]
        [InlineData("msgid ", "\n💩", 9, true,
            """
            msgid ""
            "\n"
            "💩"
            """)]
        [InlineData("msgid ", "-", 9, true,
            """
            msgid "-"
            """)]
        [InlineData("msgid ", "-\n", 9, false,
            """
            msgid ""
            "-\n"
            """)]
        [InlineData("msgid ", "-\n", 9, true,
            """
            msgid ""
            "-\n"
            """)]
        [InlineData("msgid ", "-x", 9, true,
            """
            msgid ""
            "-x"
            """)]
        [InlineData("msgid ", "-💩", 9, true,
            """
            msgid ""
            "-💩"
            """)]
        [InlineData("msgid ", "x", 9, true,
            """
            msgid "x"
            """)]
        [InlineData("msgid ", "x\n", 9, false,
            """
            msgid ""
            "x\n"
            """)]
        [InlineData("msgid ", "x\n", 9, true,
            """
            msgid ""
            "x\n"
            """)]
        [InlineData("msgid ", "x-", 9, true,
            """
            msgid ""
            "x-"
            """)]
        [InlineData("msgid ", "x💩", 9, true,
            """
            msgid ""
            "x💩"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 9, false,
            """
            msgid ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 9, true,
            """
            msgid ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 9, false,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 9, true,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]

        // with prologue, maxLineLength = 10

        [InlineData("msgid ", " ", 10, true,
            """
            msgid " "
            """)]
        [InlineData("msgid ", "\n", 10, false,
            """
            msgid "\n"
            """)]
        [InlineData("msgid ", "\n", 10, true,
            """
            msgid ""
            "\n"
            """)]
        [InlineData("msgid ", "\n-", 10, false,
            """
            msgid ""
            "\n-"
            """)]
        [InlineData("msgid ", "\n-", 10, true,
            """
            msgid ""
            "\n"
            "-"
            """)]
        [InlineData("msgid ", "\nx", 10, false,
            """
            msgid ""
            "\nx"
            """)]
        [InlineData("msgid ", "\nx", 10, true,
            """
            msgid ""
            "\n"
            "x"
            """)]
        [InlineData("msgid ", "\n💩", 10, false,
            """
            msgid ""
            "\n💩"
            """)]
        [InlineData("msgid ", "\n💩", 10, true,
            """
            msgid ""
            "\n"
            "💩"
            """)]
        [InlineData("msgid ", "-", 10, true,
            """
            msgid "-"
            """)]
        [InlineData("msgid ", "-\n", 10, false,
            """
            msgid ""
            "-\n"
            """)]
        [InlineData("msgid ", "-\n", 10, true,
            """
            msgid ""
            "-\n"
            """)]
        [InlineData("msgid ", "-x", 10, true,
            """
            msgid "-x"
            """)]
        [InlineData("msgid ", "-💩", 10, true,
            """
            msgid "-💩"
            """)]
        [InlineData("msgid ", "x", 10, true,
            """
            msgid "x"
            """)]
        [InlineData("msgid ", "x\n", 10, false,
            """
            msgid ""
            "x\n"
            """)]
        [InlineData("msgid ", "x\n", 10, true,
            """
            msgid ""
            "x\n"
            """)]
        [InlineData("msgid ", "x-", 10, true,
            """
            msgid "x-"
            """)]
        [InlineData("msgid ", "x💩", 10, true,
            """
            msgid "x💩"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 10, false,
            """
            msgid ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 10, true,
            """
            msgid ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 10, false,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 10, true,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]

        // with prologue, maxLineLength = 11

        [InlineData("msgid ", "\n", 11, false,
            """
            msgid "\n"
            """)]
        [InlineData("msgid ", "\n", 11, true,
            """
            msgid ""
            "\n"
            """)]
        [InlineData("msgid ", "\n-", 11, false,
            """
            msgid "\n-"
            """)]
        [InlineData("msgid ", "\n-", 11, true,
            """
            msgid ""
            "\n"
            "-"
            """)]
        [InlineData("msgid ", "\nx", 11, false,
            """
            msgid "\nx"
            """)]
        [InlineData("msgid ", "\nx", 11, true,
            """
            msgid ""
            "\n"
            "x"
            """)]
        [InlineData("msgid ", "\n💩", 11, false,
            """
            msgid "\n💩"
            """)]
        [InlineData("msgid ", "\n💩", 11, true,
            """
            msgid ""
            "\n"
            "💩"
            """)]
        [InlineData("msgid ", "-", 11, true,
            """
            msgid "-"
            """)]
        [InlineData("msgid ", "-\n", 11, false,
            """
            msgid "-\n"
            """)]
        [InlineData("msgid ", "-\n", 11, true,
            """
            msgid ""
            "-\n"
            """)]
        [InlineData("msgid ", "-x", 11, true,
            """
            msgid "-x"
            """)]
        [InlineData("msgid ", "-💩", 11, true,
            """
            msgid "-💩"
            """)]
        [InlineData("msgid ", "x", 11, true,
            """
            msgid "x"
            """)]
        [InlineData("msgid ", "x\n", 11, false,
            """
            msgid "x\n"
            """)]
        [InlineData("msgid ", "x\n", 11, true,
            """
            msgid ""
            "x\n"
            """)]
        [InlineData("msgid ", "x-", 11, true,
            """
            msgid "x-"
            """)]
        [InlineData("msgid ", "x💩", 11, true,
            """
            msgid "x💩"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 11, false,
            """
            msgid ""
            "💩_💩 💩\nx-"
            "😀"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 11, true,
            """
            msgid ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 11, false,
            """
            ""
            "💩­💩­💩\nx‐"
            "😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 11, true,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]

        // with prologue, maxLineLength = 12

        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 12, false,
            """
            msgid ""
            "💩_💩 💩\nx-😀"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", 12, true,
            """
            msgid ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 12, false,
            """
            ""
            "💩­💩­💩\nx‐"
            "😀-x"
            """)]
        [InlineData("", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", 12, true,
            """
            ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]

        // with prologue, maxLineLength = -1 (unlimited)

        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", -1, false,
            """
            msgid "💩_💩 💩\nx-😀"
            """)]
        [InlineData("msgid ", "💩_💩 💩\r\nx-😀", -1, true,
            """
            msgid ""
            "💩_💩 💩\n"
            "x-😀"
            """)]
        [InlineData("msgid ", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", -1, false,
            """
            msgid "💩­💩­💩\nx‐😀-x"
            """)]
        [InlineData("msgid ", "💩\u00AD💩\u00AD💩\r\nx\u2010😀\u002Dx", -1, true,
            """
            msgid ""
            "💩­💩­💩\n"
            "x‐😀-x"
            """)]

        // break within word

        [InlineData("", "abc", 2, true,
            """
            ""
            "a"
            "b"
            "c"
            """)]
        [InlineData("", "abc", 3, true,
            """
            ""
            "a"
            "b"
            "c"
            """)]
        [InlineData("", "abc", 4, true,
            """
            ""
            "ab"
            "c"
            """)]
        [InlineData("", "12345678901234567890", 11, true,
            """
            ""
            "123456789"
            "012345678"
            "90"
            """)]
        [InlineData("msgstr ", "12345678901234567890", 11, true,
            """
            msgstr ""
            "123456789"
            "012345678"
            "90"
            """)]
        [InlineData("msgstr ", "ten  chars", 12, true,
            """
            msgstr ""
            "ten  chars"
            """)]
        [InlineData("msgstr ", "- 10 chars", 12, true,
            """
            msgstr ""
            "- 10 chars"
            """)]
        [InlineData("msgstr ", "ten  chars", 11, true,
            """
            msgstr ""
            "ten  "
            "chars"
            """)]
        [InlineData("msgstr ", "- 10 chars", 11, true,
            """
            msgstr ""
            "- 10 "
            "chars"
            """)]
        [InlineData("msgstr ", "ten  chars", 5, true,
            """
            msgstr ""
            "ten"
            "  "
            "cha"
            "rs"
            """)]

        // escape sequenc

        [InlineData("msgctxt ", "\"\0\a\b\t\n\v\f\r\\\"", 3, false,
            """
            msgctxt ""
            "\""
            "\0"
            "\a"
            "\b"
            "\t"
            "\n"
            "\v"
            "\f"
            "\n"
            "\\"
            "\""
            """)]
        [InlineData("msgctxt ", "\"\0\a\b\t\n\v\f\r\\\"", 3, true,
            """
            msgctxt ""
            "\""
            "\0"
            "\a"
            "\b"
            "\t"
            "\n"
            "\v"
            "\f"
            "\n"
            "\\"
            "\""
            """)]
        [InlineData("msgctxt ", "\"\0\a\b\t\n\v\f\r\\\"", 5, false,
            """
            msgctxt ""
            "\""
            "\0"
            "\a"
            "\b"
            "\t"
            "\n"
            "\v"
            "\f"
            "\n"
            "\\"
            "\""
            """)]
        [InlineData("msgctxt ", "\"\0\a\b\t\n\v\f\r\\\"", 5, true,
            """
            msgctxt ""
            "\""
            "\0"
            "\a"
            "\b"
            "\t"
            "\n"
            "\v"
            "\f"
            "\n"
            "\\"
            "\""
            """)]
        [InlineData("msgctxt ", "\"\0\a\b\t\n\v\f\r\\\"", 6, false,
            """
            msgctxt ""
            "\"\0"
            "\a\b"
            "\t\n"
            "\v\f"
            "\n"
            "\\\""
            """)]
        [InlineData("msgctxt ", "\"\0\a\b\t\n\v\f\r\\\"", 6, true,
            """
            msgctxt ""
            "\"\0"
            "\a\b"
            "\t\n"
            "\v\f"
            "\n"
            "\\\""
            """)]
        [InlineData("msgctxt ", "\"\0\a\b\t\n\v\f\r\\\"", -1, false,
            """
            msgctxt "\"\0\a\b\t\n\v\f\n\\\""
            """)]
        [InlineData("msgctxt ", "\"\0\a\b\t\n\v\f\r\\\"", -1, true,
            """
            msgctxt ""
            "\"\0\a\b\t\n"
            "\v\f\n"
            "\\\""
            """)]

        // line endings

        [InlineData("", "abc\ndef", 10, false,
            """
            "abc\ndef"
            """)]
        [InlineData("", "abc\ndef", 10, true,
            """
            ""
            "abc\n"
            "def"
            """)]
        [InlineData("", "abc\rdef", 10, false,
            """
            "abc\ndef"
            """)]
        [InlineData("", "abc\rdef", 10, true,
            """
            ""
            "abc\n"
            "def"
            """)]
        [InlineData("", "abc\r\ndef", 10, false,
            """
            "abc\ndef"
            """)]
        [InlineData("", "abc\r\ndef", 10, true,
            """
            ""
            "abc\n"
            "def"
            """)]
        [InlineData("", "abc\r\n\r", 10, false,
            """
            "abc\n\n"
            """)]
        [InlineData("", "abc\r\n\r", 10, true,
            """
            ""
            "abc\n"
            "\n"
            """)]
        [InlineData("", "abc\r\r\n", 10, false,
            """
            "abc\n\n"
            """)]
        [InlineData("", "abc\r\r\n", 10, true,
            """
            ""
            "abc\n"
            "\n"
            """)]
        [InlineData("", "abc\n\r\n", 10, false,
            """
            "abc\n\n"
            """)]
        [InlineData("", "abc\n\r\n", 10, true,
            """
            ""
            "abc\n"
            "\n"
            """)]

        // non-breaking characters

        [InlineData("", "123\u00A056\u202F890\u200723\u2011567890", 10, true,
            """
            ""
            "123 56 8"
            "90 23‑56"
            "7890"
            """)]
        [InlineData("msgstr[0] ", "123\u00A056\u202F890\u200723\u2011567890", 10, true,
            """
            msgstr[0] ""
            "123 56 8"
            "90 23‑56"
            "7890"
            """)]

        // UTF16 edge cases - handling lone surrogates

        [InlineData("msgid ", "123456<U+D83D>8", 16, true,
            "msgid \"123456<U+D83D>8\"")]

        [InlineData("msgid ", "123456<U+DCA9>8", 16, true,
            "msgid \"123456<U+DCA9>8\"")]

        [InlineData("msgid ", "1234567<U+D83D>", 16, true,
            "msgid \"1234567<U+D83D>\"")]

        [InlineData("msgid ", "1234567<U+DCA9>", 16, true,
            "msgid \"1234567<U+DCA9>\"")]

        // UTF16 edge cases - keeping surrogate pairs together

        [InlineData("msgid ", "12345678901234567890123456789012345678901234567890123456789012345678901234567💩90", 80, true,
            """
            msgid ""
            "12345678901234567890123456789012345678901234567890123456789012345678901234567💩"
            "90"
            """)]

        public void EncodeDecode(string prologue, string source, int maxLineLength, bool breakAfterNewLine, string expectedEncoded)
        {
            // Decode custom UTF16 sequences (as lone surrogates are messed up by the test runner).
            source = DecodeUTF16Sequences(source);
            expectedEncoded = DecodeUTF16Sequences(expectedEncoded);

            var builder = new StringBuilder(prologue);
            POString.Encode(builder, source, maxLineLength, breakAfterNewLine, POString.StringBreak("\n"));

            expectedEncoded = Regex.Replace(expectedEncoded, @"""\r\n?""", "\"\n\"", RegexOptions.CultureInvariant); // normalize line endings to '\n'
            var actualEncoded = builder.ToString();
            Assert.Equal(expectedEncoded, actualEncoded);

            builder.Clear();
            foreach (var line in actualEncoded.Substring(prologue.Length).Split('\n'))
            {
                Assert.Equal(-1,  POString.Decode(builder, line, 1, line.Length - 1, "\n"));
            }

            var expectedDecoded = Regex.Replace(source, @"\r\n?", "\n", RegexOptions.CultureInvariant); // normalize line endings to '\n'
            var actualDecoded = builder.ToString();
            Assert.Equal(expectedDecoded, actualDecoded);

            static string DecodeUTF16Sequences(string s)
            {
                return Regex.Replace(s, @"<U\+([\da-fA-F]{4})>", m => ((char)Convert.ToUInt16(m.Groups[1].Value, 16)).ToString(), RegexOptions.CultureInvariant);
            }
        }

        public void Encode_ShouldNormalizeLineEndings(string prologue, string source, int maxLineLength, bool breakAfterNewLine, string expectedEncoded)
        {
            // Decode custom UTF16 sequences (as lone surrogates are messed up by the test runner).
            source = DecodeUTF16Sequences(source);
            expectedEncoded = DecodeUTF16Sequences(expectedEncoded);

            var builder = new StringBuilder(prologue);
            POString.Encode(builder, source, maxLineLength, breakAfterNewLine, POString.StringBreak("\n"));

            expectedEncoded = Regex.Replace(expectedEncoded, @"""\r\n?""", "\"\n\"", RegexOptions.CultureInvariant); // normalize line endings to '\n'
            var actualEncoded = builder.ToString();
            Assert.Equal(expectedEncoded, actualEncoded);

            builder.Clear();
            foreach (var line in actualEncoded.Substring(prologue.Length).Split('\n'))
            {
                Assert.Equal(-1,  POString.Decode(builder, line, 1, line.Length - 1, "\n"));
            }

            var expectedDecoded = Regex.Replace(source, @"\r\n?", "\n", RegexOptions.CultureInvariant); // normalize line endings to '\n'
            var actualDecoded = builder.ToString();
            Assert.Equal(expectedDecoded, actualDecoded);

            static string DecodeUTF16Sequences(string s)
            {
                return Regex.Replace(s, @"<U\+([\da-fA-F]{4})>", m => ((char)Convert.ToUInt16(m.Groups[1].Value, 16)).ToString(), RegexOptions.CultureInvariant);
            }
        }

        [Theory]
        [InlineData("", @"""abc\\""", -1)]
        [InlineData("", @"""abc\""", 4)]
        [InlineData("", @"""abc\def""", 4)]
        [InlineData("msgid ", @"""abc\\""", -1)]
        [InlineData("msgid ", @"""abc\""", 10)]
        [InlineData("msgid ", @"""abc\def""", 10)]
        public void Decode_ShouldReportErrorLocationCorrectly(string prologue, string source, int expectedErrorIndex)
        {
            var builder = new StringBuilder();
            source = prologue + source;
            var actualErrorIndex = POString.Decode(builder, source, prologue.Length + 1, source.Length - 1, POString.StringBreak("\n"));
            Assert.Equal(expectedErrorIndex, actualErrorIndex);
        }
    }
}
