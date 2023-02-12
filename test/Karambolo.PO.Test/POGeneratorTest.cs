using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Karambolo.PO.Test.Helpers;
using Karambolo.PO.Test.Properties;
using Xunit;

namespace Karambolo.PO.Test
{
#if USE_COMMON
    using Karambolo.Common;
    using HeaderDictionary = Common.Collections.OrderedDictionary<string, string>;
#else
    using HeaderDictionary = Dictionary<string, string>;
#endif
    public class POGeneratorTest
    {
        private static readonly POCatalog s_catalog = CreateCatalog();

        public static POCatalog CreateCatalog()
        {
            var result = new POCatalog();

            result.HeaderComments = new POComment[]
            {
                new POTranslatorComment { Text = "header comment" }
            };

            result.Headers = new HeaderDictionary(StringComparer.OrdinalIgnoreCase)
            {
                { "Language-Team", "" },
                { "PO-Revision-Date", "" },
                { "POT-Creation-Date", "2018-06-22 07:01+0200" },
                { "Project-Id-Version", "" },
                { "Report-Msgid-Bugs-To", "" },
                { "MIME-Version", "1.0" },
                { "X-Generator", "Poedit 2.0.8" },
                { "Last-Translator", "" },
            };

            result.Encoding = "UTF-8";
            result.PluralFormCount = 2;
            result.PluralFormSelector = "(n != 1)";
            result.Language = "en_US";

            var key = new POKey("{0} hour to midnight", "{0} hours to midnight", "Home");
            IPOEntry entry = new POPluralEntry(key)
            {
                "Translation of {0} hour to midnight",
                "Translation of {0} hours to midnight",
            };
            entry.Comments = new POComment[]
            {
                new POTranslatorComment { Text = "some translator comment" },
                new POExtractedComment { Text = "some extracted comment" },
                new POReferenceComment { References = new POSourceReference[] { new POSourceReference("/Views/Home/Index.cshtml", 8) } },
                new POFlagsComment { Flags = new SortedSet<string> { "fuzzy", "csharp-format" } },
                new POPreviousValueComment { IdKind = POIdKind.Id, Value = "{0} hour to midnite" },
                new POPreviousValueComment { IdKind = POIdKind.PluralId, Value = "{0} hours to midnite" },
            };
            result.Add(entry);

            key = new POKey($"Here is an example of how one might continue a very long string{Environment.NewLine}" +
                $"for the common case the string represents multi-line output.{Environment.NewLine}");
            entry = new POSingularEntry(key)
            {
                Translation = "Some translation of long text"
            };
            result.Add(entry);

            return result;
        }

        [Fact]
        public void GenerateFull()
        {
            var generator = new POGenerator();

            string result;
            using (var ms = new MemoryStream())
            {
                generator.Generate(ms, s_catalog);
                result = Encoding.UTF8.GetString(ms.ToArray());
            }

            // Encoding.GetString keeps BOM
            var expected = new StreamReader(new MemoryStream(Resources.SamplePO)).ReadToEnd()
                .Replace("# should be skipped\r\n", "")
                .Replace("# should be skipped too\r\n", "");

            Assert.Equal(expected, result);
        }

#if USE_COMMON
        [Fact]
        public void GeneratePreserveHeadersOrder()
        {
            var generator = new POGenerator(new POGeneratorSettings { PreserveHeadersOrder = true, IgnoreEncoding = true });

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
                generator.Generate(writer, s_catalog);

            var lines = sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            
            // sorting lines manually to match expected
            GeneralUtils.Swap(ref lines[11], ref lines[3]);
            GeneralUtils.Swap(ref lines[12], ref lines[4]);
            GeneralUtils.Swap(ref lines[13], ref lines[5]);
            GeneralUtils.Swap(ref lines[11], ref lines[6]);
            GeneralUtils.Swap(ref lines[10], ref lines[7]);
            GeneralUtils.Swap(ref lines[14], ref lines[9]);
            GeneralUtils.Swap(ref lines[12], ref lines[10]);
            GeneralUtils.Swap(ref lines[13], ref lines[11]);
            GeneralUtils.Swap(ref lines[13], ref lines[12]);

            var result = string.Join(Environment.NewLine, lines);

            // Encoding.GetString keeps BOM
            var expected = new StreamReader(new MemoryStream(Resources.SamplePO)).ReadToEnd()
                .Replace("# should be skipped\r\n", "")
                .Replace("# should be skipped too\r\n", "");

            Assert.Equal(expected, result);
        }
#endif

        [Fact]
        public void GenerateSkipComments()
        {
            var generator = new POGenerator(new POGeneratorSettings { SkipComments = true });

            string result;
            using (var ms = new MemoryStream())
            {
                generator.Generate(ms, s_catalog);
                result = Encoding.UTF8.GetString(ms.ToArray());
            }

            // Encoding.GetString keeps BOM
            var expected = new StreamReader(new MemoryStream(Resources.SamplePO)).ReadToEnd();
            expected = Regex.Replace(expected, @"(^|\r\n)(#[^\r]*\r\n)+", "$1");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GenerateSkipInfoHeaders()
        {
            var generator = new POGenerator(new POGeneratorSettings { SkipInfoHeaders = true });

            string result;
            using (var ms = new MemoryStream())
            {
                generator.Generate(ms, s_catalog);
                result = Encoding.UTF8.GetString(ms.ToArray());
            }

            // Encoding.GetString keeps BOM
            var expected = new StreamReader(new MemoryStream(Resources.SamplePO)).ReadToEnd()
                .Replace("\"Project-Id-Version: \\n\"\r\n", "")
                .Replace("\"Report-Msgid-Bugs-To: \\n\"\r\n", "")
                .Replace("\"POT-Creation-Date: 2018-06-22 07:01+0200\\n\"\r\n", "")
                .Replace("\"PO-Revision-Date: \\n\"\r\n", "")
                .Replace("\"Last-Translator: \\n\"\r\n", "")
                .Replace("\"Language-Team: \\n\"\r\n", "")
                .Replace("\"MIME-Version: 1.0\\n\"\r\n", "")
                .Replace("\"X-Generator: Poedit 2.0.8\\n\"\r\n", "")
                .Replace("# should be skipped\r\n", "")
                .Replace("# should be skipped too\r\n", "");

            Assert.Equal(expected, result);
        }

        private void Generate_LineBreak_Core(string id, params string[] lines)
        {
            var generator = new POGenerator(new POGeneratorSettings
            {
                IgnoreEncoding = true,
                SkipInfoHeaders = true,
            });

            var catalog = new POCatalog { Encoding = "UTF-8" };
            var entry = new POSingularEntry(new POKey(id));
            catalog.Add(entry);

            var sb = new StringBuilder();
            generator.Generate(sb, catalog);

            var expected = new List<string>();
            expected.Add(@"msgid """"");
            expected.AddRange(lines);
            expected.Add(@"msgstr """"");

            IEnumerable<string> actual = sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(5).Take(lines.Length + 2);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Generate_LineBreak()
        {
            Generate_LineBreak_Core(@"01234567891123456789212345678931234567894123456789512345678961234567897123456789812345678991234567890123456789112345678921234567893123456789412345678951234567896123456789712345678981234567899123456789",
                @"""012345678911234567892123456789312345678941234567895123456789612345678971234567""",
                @"""898123456789912345678901234567891123456789212345678931234567894123456789512345""",
                @"""67896123456789712345678981234567899123456789""");
        }

        [Fact]
        public void Generate_LineBreak_EscapeSequences()
        {
            // non-escape char

            Generate_LineBreak_Core(@"012345678911234567892123456789312345678941234567895123456789612345678971234567""98123456789",
                @"""012345678911234567892123456789312345678941234567895123456789612345678971234567""",
                @"""\""98123456789""");

            Generate_LineBreak_Core(@"01234567891123456789212345678931234567894123456789512345678961234567897123456\	98123456789",
                @"""01234567891123456789212345678931234567894123456789512345678961234567897123456""",
                @"""\\\t98123456789""");

            Generate_LineBreak_Core(@"0123456789112345678921234567893123456789412345678951234567896123456789712345\\""98123456789",
                @"""0123456789112345678921234567893123456789412345678951234567896123456789712345\\""",
                @"""\\\""98123456789""");

            Generate_LineBreak_Core(@"01234567891123456789212345678931234567894123456789512345678961234567897123456""898123456789",
                @"""01234567891123456789212345678931234567894123456789512345678961234567897123456""",
                @"""\""898123456789""");

            Generate_LineBreak_Core(@"0123456789112345678921234567893123456789412345678951234567896123456789712345\	898123456789",
                @"""0123456789112345678921234567893123456789412345678951234567896123456789712345\\""",
                @"""\t898123456789""");

            Generate_LineBreak_Core(@"012345678911234567892123456789312345678941234567895123456789612345678971234\\""898123456789",
                @"""012345678911234567892123456789312345678941234567895123456789612345678971234\\""",
                @"""\\\""898123456789""");

            // escape char 

            Generate_LineBreak_Core(@"012345678911234567892123456789312345678941234567895123456789612345678971234567\98123456789",
                @"""012345678911234567892123456789312345678941234567895123456789612345678971234567""",
                @"""\\98123456789""");

            Generate_LineBreak_Core(@"01234567891123456789212345678931234567894123456789512345678961234567897123456\\98123456789",
                @"""01234567891123456789212345678931234567894123456789512345678961234567897123456""",
                @"""\\\\98123456789""");

            Generate_LineBreak_Core(@"0123456789112345678921234567893123456789412345678951234567896123456789712345\\\98123456789",
                @"""0123456789112345678921234567893123456789412345678951234567896123456789712345\\""",
                @"""\\\\98123456789""");

            Generate_LineBreak_Core(@"01234567891123456789212345678931234567894123456789512345678961234567897123456\898123456789",
                @"""01234567891123456789212345678931234567894123456789512345678961234567897123456""",
                @"""\\898123456789""");

            Generate_LineBreak_Core(@"0123456789112345678921234567893123456789412345678951234567896123456789712345\\898123456789",
                @"""0123456789112345678921234567893123456789412345678951234567896123456789712345\\""",
                @"""\\898123456789""");

            Generate_LineBreak_Core(@"012345678911234567892123456789312345678941234567895123456789612345678971234\\\898123456789",
                @"""012345678911234567892123456789312345678941234567895123456789612345678971234\\""",
                @"""\\\\898123456789""");

            // new line (\r\n)

            Generate_LineBreak_Core("012345678911234567892123456789312345678941234567895123456789612345678971234567\r\n8123456789",
                @"""012345678911234567892123456789312345678941234567895123456789612345678971234567""",
                @"""\n""",
                @"""8123456789""");

            Generate_LineBreak_Core("01234567891123456789212345678931234567894123456789512345678961234567897123456\r\n98123456789",
                @"""01234567891123456789212345678931234567894123456789512345678961234567897123456""",
                @"""\n""",
                @"""98123456789""");

            // new line (\n)

            Generate_LineBreak_Core("012345678911234567892123456789312345678941234567895123456789612345678971234567\n98123456789",
                @"""012345678911234567892123456789312345678941234567895123456789612345678971234567""",
                @"""\n""",
                @"""98123456789""");

            Generate_LineBreak_Core("01234567891123456789212345678931234567894123456789512345678961234567897123456\n898123456789",
                @"""01234567891123456789212345678931234567894123456789512345678961234567897123456""",
                @"""\n""",
                @"""898123456789""");
        }

        [Fact]
        public void CustomEntryType()
        {
            var generator = new POGenerator(new POGeneratorSettings
            {
                IgnoreEncoding = true,
                SkipInfoHeaders = true,
            });

            var catalog = new POCatalog { Encoding = "UTF-8" };

            var entry = new CustomPOEntry(new POKey());
            Assert.Throws<ArgumentException>("item", () => catalog.Add(entry));

            entry = new CustomPOEntry(new POKey(""));
            Assert.Throws<ArgumentException>("item", () => catalog.Add(entry));

            entry = new CustomPOEntry(new POKey("", null, ""), "");
            catalog.Add(entry);

            entry = new CustomPOEntry(new POKey("", null, ""), "");
            Assert.Throws<ArgumentException>(() => catalog.Add(entry));

            entry = new CustomPOEntry(new POKey("", null, "X"), "");
            catalog.Add(entry);

            entry = new CustomPOEntry(new POKey("", "", ""), "", "");
            catalog.Add(entry);

            var sb = new StringBuilder();
            generator.Generate(sb, catalog);

            var expected =
@"msgid """"
msgstr """"
""Content-Type: text/plain; charset=UTF-8\n""
""Content-Transfer-Encoding: 8bit\n""

msgctxt """"
msgid """"
msgstr """"

msgctxt ""X""
msgid """"
msgstr """"

msgctxt """"
msgid """"
msgid_plural """"
msgstr[0] """"
msgstr[1] """"
";

            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void Pr20_EmptyCommentContent()
        {
            var generator = new POGenerator(new POGeneratorSettings
            {
                IgnoreEncoding = true,
                SkipInfoHeaders = true,
            });

            var catalog = new POCatalog { Encoding = "UTF-8" };

            catalog.Add(new POSingularEntry(new POKey("x"))
            {
                Comments = new POComment[]
                {
                    new POTranslatorComment { }
                }
            });

            var writer = new StringWriter();
            generator.Generate(writer, catalog);

            var expected =
@"msgid """"
msgstr """"
""Content-Type: text/plain; charset=UTF-8\n""
""Content-Transfer-Encoding: 8bit\n""

# 
msgid ""x""
msgstr """"
";

            Assert.Equal(expected, writer.ToString());
        }
    }
}
