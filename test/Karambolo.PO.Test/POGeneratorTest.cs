using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Karambolo.Common.Collections;
using Karambolo.PO.Test.Properties;
using Xunit;

namespace Karambolo.PO.Test
{
    public class POGeneratorTest
    {
        static readonly POCatalog catalog = CreateCatalog();

        public static POCatalog CreateCatalog()
        {
            var result = new POCatalog();

            result.HeaderComments = new POComment[]
            {
                new POTranslatorComment { Text = "header comment" }
            };

            result.Headers = new OrderedDictionary<string, string>
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
                generator.Generate(ms, catalog);
                result = Encoding.UTF8.GetString(ms.ToArray());
            }

            // Encoding.GetString keeps BOM
            var expected = new StreamReader(new MemoryStream(Resources.SamplePO)).ReadToEnd()
                .Replace("# should be skipped\r\n", "")
                .Replace("# should be skipped too\r\n", "");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GenerateSkipComments()
        {
            var generator = new POGenerator(new POGeneratorSettings { SkipComments = true });

            string result;
            using (var ms = new MemoryStream())
            {
                generator.Generate(ms, catalog);
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
                generator.Generate(ms, catalog);
                result = Encoding.UTF8.GetString(ms.ToArray());
            }

            // Encoding.GetString keeps BOM
            var expected = new StreamReader(new MemoryStream(Resources.SamplePO)).ReadToEnd()
                .Replace("\"Language-Team: \\n\"\r\n", "")
                .Replace("\"Last-Translator: \\n\"\r\n", "")
                .Replace("\"MIME-Version: 1.0\\n\"\r\n", "")
                .Replace("\"PO-Revision-Date: \\n\"\r\n", "")
                .Replace("\"POT-Creation-Date: 2018-06-22 07:01+0200\\n\"\r\n", "")
                .Replace("\"Project-Id-Version: \\n\"\r\n", "")
                .Replace("\"Report-Msgid-Bugs-To: \\n\"\r\n", "")
                .Replace("\"X-Generator: Poedit 2.0.8\\n\"\r\n", "")
                .Replace("# should be skipped\r\n", "")
                .Replace("# should be skipped too\r\n", "");

            Assert.Equal(expected, result);
        }
    }
}
