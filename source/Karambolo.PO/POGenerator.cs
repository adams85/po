using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Karambolo.Common;
using Karambolo.PO.Properties;

namespace Karambolo.PO
{
    public class POGeneratorSettings
    {
        public static readonly POGeneratorSettings Default = new POGeneratorSettings();

        public bool IgnoreEncoding { get; set; }
        public bool SkipInfoHeaders { get; set; }
        public bool SkipComments { get; set; }
        public bool IgnoreLineBreaks { get; set; }
        public bool IgnoreLongLines { get; set; }
    }

    public class POGenerator
    {
        [Flags]
        enum Flags
        {
            None = 0,
            IgnoreEncoding = 0x1,
            SkipInfoHeaders = 0x2,
            SkipComments = 0x4,
            IgnoreLineBreaks = 0x8,
            IgnoreLongLines = 0x10,
        }

        const int maxLineLength = 80;
        static readonly string stringBreak = string.Concat("\"", Environment.NewLine, "\"");

        readonly Flags _flags;

        readonly StringBuilder _builder;
        int _lineStartIndex;

        TextWriter _writer;
        POCatalog _catalog;

        public POGenerator() : this(POGeneratorSettings.Default) { }

        public POGenerator(POGeneratorSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.IgnoreEncoding)
                _flags |= Flags.IgnoreEncoding;

            if (settings.SkipInfoHeaders)
                _flags |= Flags.SkipInfoHeaders;

            if (settings.SkipComments)
                _flags |= Flags.SkipComments;

            if (settings.IgnoreLineBreaks)
                _flags |= Flags.IgnoreLineBreaks;

            if (settings.IgnoreLongLines)
                _flags |= Flags.IgnoreLongLines;

            _builder = new StringBuilder();
        }

        int IndexOfNewLine(int startIndex, int endIndex)
        {
            for (endIndex--; startIndex < endIndex; startIndex++)
                if (_builder[startIndex] == '\\' && _builder[++startIndex] == 'n')
                    if (++startIndex <= endIndex)
                        return startIndex;
                    else
                        break;

            return -1;
        }

        int GetStringBreakIndex()
        {
            var result = -1;

            var endIndex = _builder.Length;
            int index;

            if ((_flags & Flags.IgnoreLongLines) == Flags.None && endIndex - _lineStartIndex > maxLineLength)
            {
                result = _lineStartIndex + maxLineLength - 1;

                char c;
                for (index = result - 1; index > _lineStartIndex; index--)
                    if ((c = _builder[index]) == '-' || char.IsWhiteSpace(c))
                    {
                        result = index + 1;
                        break;
                    }

                // escape sequences are kept together
                if (_builder[result - 1] == '\\')
                    result--;
            }

            if ((_flags & Flags.IgnoreLineBreaks) == Flags.None && (index = IndexOfNewLine(_lineStartIndex + 1, endIndex - 1)) >= 0 &&
                (result < 0 || index < result))
                result = index;

            return result;
        }

        void ResetBuilder()
        {
            _builder.Clear();
            _lineStartIndex = 0;
        }

        void BuildString(string value)
        {
            var startIndex = _builder.Length;
            _builder.Append('"');
            POString.Encode(_builder, value, 0, value.Length);
            _builder.Append('"');
            var endIndex = _builder.Length;

            if (!((_flags & Flags.IgnoreLongLines) == Flags.None && endIndex - _lineStartIndex > maxLineLength ||
                 ((_flags & Flags.IgnoreLineBreaks) == Flags.None && IndexOfNewLine(startIndex + 1, endIndex - 1) >= 0)))
                return;

            startIndex++;
            _builder.Insert(startIndex, stringBreak);
            _lineStartIndex = startIndex + stringBreak.Length;
            _lineStartIndex--;

            while ((startIndex = GetStringBreakIndex()) >= 0)
            {
                _builder.Insert(startIndex, stringBreak);
                _lineStartIndex = startIndex + stringBreak.Length;
                _lineStartIndex--;
            }
        }

        void WriteComments(IList<POComment> comments)
        {
            var commentLookup = comments.ToLookup(c => c.Kind).OrderBy(c => c.Key);

            foreach (var commentGroup in commentLookup)
                foreach (var comment in commentGroup)
                {
                    char commentKindToken;
                    switch (comment.Kind)
                    {
                        case POCommentKind.Translator: commentKindToken = ' '; break;
                        case POCommentKind.Extracted: commentKindToken = '.'; break;
                        case POCommentKind.Reference: commentKindToken = ':'; break;
                        case POCommentKind.Flags: commentKindToken = ','; break;
                        case POCommentKind.PreviousValue: commentKindToken = '|'; break;
                        default: throw new InvalidOperationException();
                    }

                    _writer.WriteLine($"#{commentKindToken} {comment}");
                }
        }

        void WriteEntry(IPOEntry entry)
        {
            if ((_flags & Flags.SkipComments) == Flags.None && entry.Comments != null)
                WriteComments(entry.Comments);

            if (entry.Key.ContextId != null)
            {
                ResetBuilder();
                _builder.Append(POCatalog.ContextIdToken);
                _builder.Append(' ');
                BuildString(entry.Key.ContextId);
                _writer.WriteLine(_builder);
            }

            ResetBuilder();
            _builder.Append(POCatalog.IdToken);
            _builder.Append(' ');
            BuildString(entry.Key.Id);
            _writer.WriteLine(_builder);

            if (entry.Key.PluralId != null)
            {
                ResetBuilder();
                _builder.Append(POCatalog.PluralIdToken);
                _builder.Append(' ');
                BuildString(entry.Key.PluralId);
                _writer.WriteLine(_builder);
            }

            switch (entry)
            {
                case POSingularEntry singularEntry:
                    ResetBuilder();
                    _builder.Append(POCatalog.TranslationToken);
                    _builder.Append(' ');
                    BuildString(singularEntry.Translation);
                    _writer.WriteLine(_builder);
                    break;
                case POPluralEntry pluralEntry:
                    var n = pluralEntry.Count;
                    for (var i = 0; i < n; i++)
                    {
                        ResetBuilder();
                        _builder.Append(POCatalog.TranslationToken);
                        _builder.Append('[');
                        _builder.Append(i);
                        _builder.Append(']');
                        _builder.Append(' ');
                        BuildString(pluralEntry[i]);
                        _writer.WriteLine(_builder);
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        IPOEntry CreateHeaderEntry()
        {
            var headers =
                (_flags & Flags.SkipInfoHeaders) == Flags.None && _catalog.Headers != null ?
                new Dictionary<string, string>(_catalog.Headers, StringComparer.OrdinalIgnoreCase) :
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (_catalog.Encoding != null)
            {
                headers["Content-Type"] = $"text/plain; charset={_catalog.Encoding}";

                if (!headers.ContainsKey("Content-Transfer-Encoding"))
                    headers["Content-Transfer-Encoding"] = "8bit";
            }

            if (_catalog.Language != null)
                headers["Language"] = _catalog.Language;

            if (_catalog.PluralFormCount > 0 && _catalog.PluralFormSelector != null)
                headers["Plural-Forms"] = $"nplurals={_catalog.PluralFormCount}; plural={_catalog.PluralFormSelector};";

            var value =
                headers.Count > 0 ?
                string.Join("\n", headers.OrderBy(kvp => kvp.Key).Select(kvp => string.Concat(kvp.Key, ": ", kvp.Value)).WithTail(string.Empty)) :
                string.Empty;

            return new POSingularEntry(new POKey(string.Empty))
            {
                Translation = value,
                Comments = _catalog.HeaderComments
            };
        }

        public void Generate(TextWriter writer, POCatalog catalog)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            if ((_flags & Flags.IgnoreEncoding) == Flags.None)
            {
                if (writer.Encoding.GetByteCount(" ") > 1)
                    throw new InvalidOperationException(Resources.EncodingNotSingleByte);

                if (catalog.Encoding == null || Encoding.GetEncoding(catalog.Encoding).WebName != writer.Encoding.WebName)
                    throw new InvalidOperationException(Resources.EncodingMismatch);
            }

            _writer = writer;
            _catalog = catalog;

            try
            {
                _lineStartIndex = 0;

                var entry = CreateHeaderEntry();
                if (entry != null)
                    WriteEntry(entry);

                var n = catalog.Count;
                for (var i = 0; i < n; i++)
                {
                    _writer.WriteLine();
                    WriteEntry(catalog[i]);
                }
            }
            finally
            {
                _builder.Clear();
                if (_builder.Capacity > 1024)
                    _builder.Capacity = 1024;
            }
        }
    }

    public static class POGeneratorExtensions
    {
        public static void Generate(this POGenerator @this, StringBuilder output, POCatalog catalog)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            using (var writer = new StringWriter(output))
                @this.Generate(writer, catalog);
        }

        public static void Generate(this POGenerator @this, Stream output, POCatalog catalog)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var writer = new StreamWriter(output);
            @this.Generate(writer, catalog);
            writer.Flush();
        }
    }
}
