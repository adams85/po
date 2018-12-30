using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Karambolo.PO.Test.Properties;
using Xunit;

namespace Karambolo.PO.Test
{
    public class POParserTest
    {
        void CheckHeader(POCatalog catalog, bool expectComments, bool expectInfoHeaders, bool expectOrderedHeaders)
        {
            if (expectInfoHeaders)
            {
                Assert.Equal(12, catalog.Headers.Count);

                Assert.Equal("8bit", catalog.Headers["Content-Transfer-Encoding"]);
                Assert.Equal("text/plain; charset=UTF-8", catalog.Headers["Content-Type"]);
                Assert.Equal("", catalog.Headers["Language-Team"]);
                Assert.Equal("nplurals=2; plural=(n != 1);", catalog.Headers["Plural-Forms"]);
                Assert.Equal("", catalog.Headers["PO-Revision-Date"]);
                Assert.Equal("2018-06-22 07:01+0200", catalog.Headers["POT-Creation-Date"]);
                Assert.Equal("", catalog.Headers["Project-Id-Version"]);
                Assert.Equal("", catalog.Headers["Report-Msgid-Bugs-To"]);
                Assert.Equal("1.0", catalog.Headers["MIME-Version"]);
                Assert.Equal("Poedit 2.0.8", catalog.Headers["X-Generator"]);
                Assert.Equal("", catalog.Headers["Last-Translator"]);
                Assert.Equal("en_US", catalog.Headers["Language"]);
            }
            else
                Assert.Null(catalog.Headers);

            if (expectOrderedHeaders)
            {
#if USE_COMMON
                Assert.IsAssignableFrom<Karambolo.Common.Collections.IOrderedDictionary<string, string>>(catalog.Headers);

                Assert.Equal(new[] { "Content-Transfer-Encoding", "Content-Type", "Language", "Language-Team", "Last-Translator", "MIME-Version",
                    "Plural-Forms", "PO-Revision-Date", "POT-Creation-Date", "Project-Id-Version", "Report-Msgid-Bugs-To", "X-Generator" },
                    catalog.Headers.Keys);
#else
                Assert.True(false, "Compact version doesn't include PreserveHeadersOrder.");
#endif
            }

            Assert.Equal("UTF-8", catalog.Encoding);
            Assert.Equal("en_US", catalog.Language);
            Assert.Equal(2, catalog.PluralFormCount);
            Assert.Equal("(n != 1)", catalog.PluralFormSelector);

#if USE_HIME
            Assert.Equal(0, catalog.GetPluralFormIndex(1));
            Assert.Equal(1, catalog.GetPluralFormIndex(2));
            Assert.Equal(1, catalog.GetPluralFormIndex(5));
#else
            Assert.Equal(0, catalog.GetPluralFormIndex(1));
            Assert.Equal(0, catalog.GetPluralFormIndex(2));
            Assert.Equal(0, catalog.GetPluralFormIndex(5));
#endif

            if (expectComments)
            {
                Assert.Equal(1, catalog.HeaderComments.Count);

                var comments = catalog.HeaderComments;
                Assert.Equal(POCommentKind.Translator, comments[0].Kind);
                Assert.Equal("header comment", ((POTranslatorComment)comments[0]).Text);
            }
            else
                Assert.Null(catalog.HeaderComments);
        }

        void CheckItems(POCatalog catalog, bool expectComments)
        {
            var key1 = new POKey("{0} hour to midnight", "{0} hours to midnight", "Home");
            var key2 = new POKey($"Here is an example of how one might continue a very long string{Environment.NewLine}" +
                $"for the common case the string represents multi-line output.{Environment.NewLine}");

            Assert.Equal(2, catalog.Count);
            Assert.Contains(key1, catalog.Keys);
            Assert.Contains(key2, catalog.Keys);

            Assert.Equal(2, catalog[key1].Count);
            Assert.Equal("Translation of {0} hour to midnight", catalog.GetTranslation(key1, 1));
            Assert.Equal("Translation of {0} hours to midnight", catalog[key1][1]);
            Assert.Equal("Translation of {0} hour to midnight", catalog.GetTranslation(key1, 1));
#if USE_HIME
            Assert.Equal("Translation of {0} hours to midnight", catalog.GetTranslation(key1, 2));
#else
            Assert.Equal("Translation of {0} hour to midnight", catalog.GetTranslation(key1, 2));
#endif

            Assert.Equal(1, catalog[key2].Count);
            Assert.Equal("Some translation of long text", catalog[key2][0]);

            if (expectComments)
            {
                Assert.Equal(6, catalog[key1].Comments.Count);
                Assert.Equal(0, catalog[key2].Comments.Count);

                var comments = catalog[key1].Comments;

                Assert.Equal(POCommentKind.Translator, comments[0].Kind);
                Assert.Equal("some translator comment", ((POTranslatorComment)comments[0]).Text);

                Assert.Equal(POCommentKind.Extracted, comments[1].Kind);
                Assert.Equal("some extracted comment", ((POExtractedComment)comments[1]).Text);

                Assert.Equal(POCommentKind.Reference, comments[2].Kind);
                var references = ((POReferenceComment)comments[2]).References;
                Assert.Equal(1, references.Count);
                Assert.Equal("/Views/Home/Index.cshtml", references[0].FilePath);
                Assert.Equal(8, references[0].Line);

                Assert.Equal(POCommentKind.Flags, comments[3].Kind);
                var flags = ((POFlagsComment)comments[3]).Flags;
                Assert.Equal(2, flags.Count);
                Assert.Contains("fuzzy", flags);
                Assert.Contains("csharp-format", flags);

                Assert.Equal(POCommentKind.PreviousValue, comments[4].Kind);
                Assert.Equal(POIdKind.Id, ((POPreviousValueComment)comments[4]).IdKind);
                Assert.Equal("{0} hour to midnite", ((POPreviousValueComment)comments[4]).Value);

                Assert.Equal(POCommentKind.PreviousValue, comments[5].Kind);
                Assert.Equal(POIdKind.PluralId, ((POPreviousValueComment)comments[5]).IdKind);
                Assert.Equal("{0} hours to midnite", ((POPreviousValueComment)comments[5]).Value);
            }
            else
            {
                Assert.Null(catalog[key1].Comments);
                Assert.Null(catalog[key2].Comments);
            }
        }

        [Fact]
        public void ParseFull()
        {
            var parser = new POParser();

            POParseResult result;
            using (var ms = new MemoryStream(Resources.SamplePO))
                result = parser.Parse(ms);

            Assert.True(result.Success);

            var catalog = result.Catalog;
            CheckHeader(catalog, expectComments: true, expectInfoHeaders: true, expectOrderedHeaders: false);
            CheckItems(catalog, expectComments: true);
        }

        [Fact]
        public void ParseHeaderOnly()
        {
            var parser = new POParser(new POParserSettings
            {
                ReadHeaderOnly = true
            });

            POParseResult result;
            using (var ms = new MemoryStream(Resources.SamplePO))
                result = parser.Parse(ms);

            Assert.True(result.Success);

            var catalog = result.Catalog;
            CheckHeader(catalog, expectComments: true, expectInfoHeaders: true, expectOrderedHeaders: false);
            Assert.Empty(catalog);
        }

#if USE_COMMON
        [Fact]
        public void ParsePreserveHeadersOrder()
        {
            var parser = new POParser(new POParserSettings
            {
                PreserveHeadersOrder = true
            });

            POParseResult result;
            using (var ms = new MemoryStream(Resources.SamplePO))
                result = parser.Parse(ms);

            Assert.True(result.Success);

            var catalog = result.Catalog;
            CheckHeader(catalog, expectComments: true, expectInfoHeaders: true, expectOrderedHeaders: true);
            CheckItems(catalog, expectComments: true);
        }
#endif

        [Fact]
        public void ParseSkipComments()
        {
            var parser = new POParser(new POParserSettings
            {
                SkipComments = true
            });

            // Encoding.GetString keeps BOM
            var input = new StreamReader(new MemoryStream(Resources.SamplePO)).ReadToEnd();
            var result = parser.Parse(input);

            Assert.True(result.Success);

            var catalog = result.Catalog;
            CheckHeader(catalog, expectComments: false, expectInfoHeaders: true, expectOrderedHeaders: false);
            CheckItems(catalog, expectComments: false);
        }

        [Fact]
        public void ParseSkipInfoHeaders()
        {
            var parser = new POParser(new POParserSettings
            {
                SkipInfoHeaders = true
            });

            var input = new StreamReader(new MemoryStream(Resources.SamplePO));
            var result = parser.Parse(input);

            Assert.True(result.Success);

            var catalog = result.Catalog;
            CheckHeader(catalog, expectComments: true, expectInfoHeaders: false, expectOrderedHeaders: false);
            CheckItems(catalog, expectComments: true);
        }
    }
}
