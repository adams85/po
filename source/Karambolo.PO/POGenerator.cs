using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Karambolo.Common;
using Karambolo.PO.Properties;

namespace Karambolo.PO
{
#if USE_COMMON
    using Karambolo.Common.Collections;
#endif

    public class POGeneratorSettings
    {
        public static readonly POGeneratorSettings Default = new POGeneratorSettings();

        public bool IgnoreEncoding { get; set; }
#if USE_COMMON
        public bool PreserveHeadersOrder { get; set; }
#endif
        public bool SkipInfoHeaders { get; set; }
        public bool SkipComments { get; set; }
        public bool IgnoreLineBreaks { get; set; }
        public bool IgnoreLongLines { get; set; }
    }

    public class POGenerator
    {
        [Flags]
        private enum Flags
        {
            None = 0,
            IgnoreEncoding = 0x1,
#if USE_COMMON
            PreserveHeadersOrder = 0x2,
#endif
            SkipInfoHeaders = 0x4,
            SkipComments = 0x8,
            IgnoreLineBreaks = 0x10,
            IgnoreLongLines = 0x20,
        }

        private const int MaxLineLength = 80;
        private readonly Flags _flags;
        private readonly StringBuilder _builder;
        private TextWriter _writer;
        private POCatalog _catalog;
        private string _stringBreak;
        private int _lineStartIndex;

        public POGenerator() : this(POGeneratorSettings.Default) { }

        public POGenerator(POGeneratorSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.IgnoreEncoding)
                _flags |= Flags.IgnoreEncoding;

#if USE_COMMON
            if (settings.PreserveHeadersOrder)
                _flags |= Flags.PreserveHeadersOrder;
#endif

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

        private bool HasFlags(Flags flags)
        {
            return (_flags & flags) == flags;
        }

        private int IndexOfNewLine(int startIndex, int endIndex)
        {
            for (endIndex--; startIndex < endIndex; startIndex++)
                if (_builder[startIndex] == '\\' && _builder[++startIndex] == 'n')
                    if (++startIndex <= endIndex)
                        return startIndex;
                    else
                        break;

            return -1;
        }

        private int GetStringBreakIndex()
        {
            var result = -1;

            var endIndex = _builder.Length;
            int index;

            if (!HasFlags(Flags.IgnoreLongLines) && endIndex - _lineStartIndex > MaxLineLength)
            {
                result = _lineStartIndex + MaxLineLength - 1;

                char c;
                for (index = result - 1; index > _lineStartIndex; index--)
                    if ((c = _builder[index]) == '-' || char.IsWhiteSpace(c))
                    {
                        result = index + 1;
                        break;
                    }

                // escape sequences are kept together
                if (IsEscaped(_builder, result, _lineStartIndex))
                    result--;
            }

            if (!HasFlags(Flags.IgnoreLineBreaks) && (index = IndexOfNewLine(_lineStartIndex + 1, endIndex - 1)) >= 0 &&
                (result < 0 || index < result))
                result = index;

            return result;

            bool IsEscaped(StringBuilder sb, int currentIndex, int startIndex)
            {
                bool isEscaped = false;

                for (currentIndex--; currentIndex > startIndex; currentIndex--)
                    if (sb[currentIndex] == '\\')
                        isEscaped = !isEscaped;
                    else
                        break;

                return isEscaped;
            }
        }

        private void ResetBuilder()
        {
            _builder.Clear();
            _lineStartIndex = 0;
        }

        private void AppendPOString(string value)
        {
            if (value == null)
                value = string.Empty;

            var startIndex = _builder.Length;
            _builder.Append('"');
            POString.Encode(_builder, value, 0, value.Length);
            _builder.Append('"');
            var endIndex = _builder.Length;

            if (!(!HasFlags(Flags.IgnoreLongLines) && endIndex - _lineStartIndex > MaxLineLength ||
                  !HasFlags(Flags.IgnoreLineBreaks) && IndexOfNewLine(startIndex + 1, endIndex - 1) >= 0))
                return;

            startIndex++;
            _builder.Insert(startIndex, _stringBreak);
            _lineStartIndex = startIndex + _stringBreak.Length;
            _lineStartIndex--;

            while ((startIndex = GetStringBreakIndex()) >= 0)
            {
                _builder.Insert(startIndex, _stringBreak);
                _lineStartIndex = startIndex + _stringBreak.Length;
                _lineStartIndex--;
            }
        }

        private void WriteComments(IList<POComment> comments)
        {
            IOrderedEnumerable<IGrouping<POCommentKind, POComment>> commentLookup = comments.ToLookup(c => c.Kind).OrderBy(c => c.Key);

            foreach (IGrouping<POCommentKind, POComment> commentGroup in commentLookup)
                foreach (POComment comment in commentGroup)
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

                    var commentContent = comment.ToString(); 
                    var separator = !string.IsNullOrEmpty(commentContent) ? " " : string.Empty;
                    _writer.WriteLine($"#{commentKindToken}{separator}{commentContent}");
                }
        }

        private void WriteEntryCommentsAndKey(IPOEntry entry)
        {
            if (!HasFlags(Flags.SkipComments) && entry.Comments != null)
                WriteComments(entry.Comments);

            if (entry.Key.ContextId != null)
            {
                ResetBuilder();
                _builder.Append(POCatalog.ContextIdToken);
                _builder.Append(' ');
                AppendPOString(entry.Key.ContextId);
                _writer.WriteLine(_builder);
            }

            ResetBuilder();
            _builder.Append(POCatalog.IdToken);
            _builder.Append(' ');
            AppendPOString(entry.Key.Id);
            _writer.WriteLine(_builder);

            if (entry.Key.PluralId != null)
            {
                ResetBuilder();
                _builder.Append(POCatalog.PluralIdToken);
                _builder.Append(' ');
                AppendPOString(entry.Key.PluralId);
                _writer.WriteLine(_builder);
            }
        }

        private void WriteSingularEntryTranslation(string translation)
        {
            ResetBuilder();
            _builder.Append(POCatalog.TranslationToken);
            _builder.Append(' ');
            AppendPOString(translation);
            _writer.WriteLine(_builder);
        }

        private void WritePluralEntryTranslation(int index, string translation)
        {
            ResetBuilder();
            _builder.Append(POCatalog.TranslationToken);
            _builder.Append('[');
            _builder.Append(index, CultureInfo.InvariantCulture);
            _builder.Append(']');
            _builder.Append(' ');
            AppendPOString(translation);
            _writer.WriteLine(_builder);
        }
        private void WriteEntry(IPOEntry entry, Action<TextWriter> writeBeginning)
        {
            switch (entry)
            {
                case POSingularEntry singularEntry:
                    writeBeginning(_writer);
                    WriteEntryCommentsAndKey(singularEntry);
                    WriteSingularEntryTranslation(singularEntry.Translation);
                    break;
                case POPluralEntry pluralEntry:
                    var n = entry.Count;
                    if (n > 0)
                    {
                        writeBeginning(_writer);
                        WriteEntryCommentsAndKey(pluralEntry);
                        for (var i = 0; i < n; i++)
                            WritePluralEntryTranslation(i, entry[i]);
                    }
                    break;
                default:
                    n = entry.Count;
                    if (n > 0)
                    {
                        writeBeginning(_writer);
                        WriteEntryCommentsAndKey(entry);
                        if (entry.Key.PluralId != null)
                        {
                            for (var i = 0; i < n; i++)
                                WritePluralEntryTranslation(i, entry[i]);
                        }
                        else
                            WriteSingularEntryTranslation(entry[0]);
                    }
                    break;
            }
        }

        private IPOEntry CreateHeaderEntry()
        {
            IDictionary<string, string> headers;
            if (!HasFlags(Flags.SkipInfoHeaders) && _catalog.Headers != null)
            {
#if USE_COMMON
                if (HasFlags(Flags.PreserveHeadersOrder))
                    headers = new OrderedDictionary<string, string>(_catalog.Headers, StringComparer.OrdinalIgnoreCase);
                else
#endif
                headers = new Dictionary<string, string>(_catalog.Headers, StringComparer.OrdinalIgnoreCase);
            }
            else
                headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (_catalog.Encoding != null)
            {
                if (!headers.ContainsKey("Content-Transfer-Encoding"))
                    headers["Content-Transfer-Encoding"] = "8bit";

                headers["Content-Type"] = $"text/plain; charset={_catalog.Encoding}";
            }

            if (_catalog.Language != null)
                headers["Language"] = _catalog.Language;

            if (_catalog.PluralFormCount > 0 && _catalog.PluralFormSelector != null)
                headers["Plural-Forms"] = $"nplurals={_catalog.PluralFormCount.ToString(CultureInfo.InvariantCulture)}; plural={_catalog.PluralFormSelector};";

            IEnumerable<KeyValuePair<string, string>> orderedHeaders;
#if USE_COMMON
            if (headers is IOrderedDictionary<string, string>)
                orderedHeaders = headers.AsEnumerable();
            else
#endif
            orderedHeaders = headers.OrderBy(kvp => kvp.Key);

            var value =
                headers.Count > 0 ?
                string.Join("\n", orderedHeaders.Select(kvp => kvp.Key + ": " + kvp.Value).Append(string.Empty)) :
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

            if (!HasFlags(Flags.IgnoreEncoding))
            {
                if (writer.Encoding.GetByteCount(" ") > 1)
                    throw new ArgumentException(Resources.EncodingNotSingleByte, nameof(writer));

                if (catalog.Encoding == null || Encoding.GetEncoding(catalog.Encoding).WebName != writer.Encoding.WebName)
                    throw new ArgumentException(Resources.EncodingMismatch, nameof(writer));
            }

            _writer = writer;
            _catalog = catalog;

            _stringBreak = "\"" + _writer.NewLine + "\"";
            _lineStartIndex = 0;

            try
            {
                WriteEntry(CreateHeaderEntry(), _ => { });

                for (int i = 0, n = catalog.Count; i < n; i++)
                    WriteEntry(catalog[i], w => w.WriteLine());
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
        public static void Generate(this POGenerator generator, StringBuilder output, POCatalog catalog)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            using (var writer = new StringWriter(output))
                generator.Generate(writer, catalog);
        }

        public static void Generate(this POGenerator generator, Stream output, POCatalog catalog)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var writer = new StreamWriter(output);
            generator.Generate(writer, catalog);
            writer.Flush();
        }

        public static void Generate(this POGenerator generator, Stream output, POCatalog catalog, Encoding encoding)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var writer = new StreamWriter(output, encoding);
            generator.Generate(writer, catalog);
            writer.Flush();
        }
    }
}
