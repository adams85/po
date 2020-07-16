using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Karambolo.Common;
using Karambolo.PO.Properties;

namespace Karambolo.PO
{
#if USE_COMMON
    using Karambolo.Common.Collections;
#endif

    public class POParserSettings
    {
        public static readonly POParserSettings Default = new POParserSettings();

        internal bool ReadContentTypeHeaderOnly { get; set; }
        public bool ReadHeaderOnly { get; set; }
#if USE_COMMON
        public bool PreserveHeadersOrder { get; set; }
#endif
        public bool SkipInfoHeaders { get; set; }
        public bool SkipComments { get; set; }
    }

    public class POParseResult
    {
        private readonly DiagnosticCollection _diagnostics;

        internal POParseResult(POCatalog catalog, DiagnosticCollection diagnostics)
        {
            if (diagnostics == null)
                throw new ArgumentNullException(nameof(diagnostics));

            Catalog = catalog;
            _diagnostics = diagnostics;
        }

        public POCatalog Catalog { get; }
        public IDiagnostics Diagnostics => _diagnostics;
        public bool Success => !_diagnostics.HasError;
    }

    public class POParser
    {
        private class DiagnosticCodes
        {
            public static string DuplicateHeaderKey = nameof(Resources.DuplicateHeaderKey);
            public static string DuplicatePluralForm = nameof(Resources.DuplicatePluralForm);
            public static string ExpectedToken = nameof(Resources.ExpectedToken);
            public static string HeaderNotSingular = nameof(Resources.HeaderNotSingular);
            public static string IncompleteEntry = nameof(Resources.IncompleteEntry);
            public static string InvalidEntryKey = nameof(Resources.InvalidEntryKey);
            public static string InvalidEscapeSequence = nameof(Resources.InvalidEscapeSequence);
            public static string InvalidHeaderComment = nameof(Resources.InvalidHeaderComment);
            public static string InvalidHeaderEntryKey = nameof(Resources.InvalidHeaderEntryKey);
            public static string InvalidHeaderValue = nameof(Resources.InvalidHeaderValue);
            public static string InvalidPluralIndex = nameof(Resources.InvalidPluralIndex);
            public static string MalformedComment = nameof(Resources.MalformedComment);
            public static string MalformedHeaderItem = nameof(Resources.MalformedHeaderItem);
            public static string MissingPluralIndex = nameof(Resources.MissingPluralIndex);
            public static string MissingPluralForm = nameof(Resources.MissingPluralForm);
            public static string UnexpectedToken = nameof(Resources.UnexpectedToken);
            public static string UnnecessaryPluralIndex = nameof(Resources.UnnecessaryPluralIndex);
        }

        private class ParserDiagnostic : Diagnostic
        {
            public ParserDiagnostic(DiagnosticSeverity severity, string code, params object[] args)
                : base(severity, code, args) { }

            protected override string GetMessageFormat()
            {
                return Resources.ResourceManager.GetString(Code);
            }
        }

        [Flags]
        private enum Flags
        {
            None = 0,
            ReadContentTypeHeaderOnly = 0x1,
            ReadHeaderOnly = 0x2,
#if USE_COMMON
            PreserveHeadersOrder = 0x4,
#endif
            SkipInfoHeaders = 0x8,
            SkipComments = 0x10,
        }

        [Flags]
        private enum EntryTokens
        {
            None,
            Id = 0x1,
            PluralId = 0x2,
            ContextId = 0x4,
            Translation = 0x8,
        }

        // caching delegate
        private static readonly Predicate<char> s_matchNonWhiteSpace = c => !char.IsWhiteSpace(c);

        public static Encoding DetectEncoding(Stream stream)
        {
            var reader = new StreamReader(stream, Encoding.GetEncoding("ASCII"), detectEncodingFromByteOrderMarks: true);

            var parser = new POParser(new POParserSettings { ReadHeaderOnly = true, ReadContentTypeHeaderOnly = true });
            POParseResult result = parser.Parse(reader);
            if (!result.Success)
                return null;

            return
                result.Catalog.Encoding != null ?
                Encoding.GetEncoding(result.Catalog.Encoding) :
                reader.CurrentEncoding;
        }

        private readonly Flags _flags;
        private readonly StringBuilder _builder;
        private readonly List<KeyValuePair<TextLocation, string>> _commentBuffer;
        private TextReader _reader;
        private POCatalog _catalog;
        private DiagnosticCollection _diagnostics;
        private string _line;
        private int _lineIndex;
        private int _columnIndex;

        public POParser() : this(POParserSettings.Default) { }

        public POParser(POParserSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.ReadContentTypeHeaderOnly)
                _flags |= Flags.ReadContentTypeHeaderOnly;

            if (settings.ReadHeaderOnly)
                _flags |= Flags.ReadHeaderOnly;

#if USE_COMMON
            if (settings.PreserveHeadersOrder)
                _flags |= Flags.PreserveHeadersOrder;
#endif

            if (settings.SkipInfoHeaders)
                _flags |= Flags.SkipInfoHeaders;

            if (settings.SkipComments)
                _flags |= Flags.SkipComments;
            else
                _commentBuffer = new List<KeyValuePair<TextLocation, string>>();

            _builder = new StringBuilder();
        }

        private bool HasFlags(Flags flags)
        {
            return (_flags & flags) == flags;
        }

        private void AddDiagnostic(DiagnosticSeverity severity, string code, params object[] args)
        {
            _diagnostics.Add(new ParserDiagnostic(severity, code, args));
        }

        private void AddInformation(string code, params object[] args)
        {
            AddDiagnostic(DiagnosticSeverity.Information, code, args);
        }

        private void AddWarning(string code, params object[] args)
        {
            AddDiagnostic(DiagnosticSeverity.Warning, code, args);
        }

        private void AddError(string code, params object[] args)
        {
            AddDiagnostic(DiagnosticSeverity.Error, code, args);
        }

        private int FindNextTokenInLine(bool requireWhiteSpace = false)
        {
            var index = _line.FindIndex(_columnIndex, s_matchNonWhiteSpace);
            if (requireWhiteSpace && index <= _columnIndex)
            {
                AddError(DiagnosticCodes.UnexpectedToken, new TextLocation(_lineIndex, _columnIndex));
                return -1;
            }
            return index;
        }

        // resets _commentBuffer and _columnIndex
        private void SeekNextToken()
        {
            if (_line != null && (_columnIndex = FindNextTokenInLine()) >= 0)
                return;

            _commentBuffer?.Clear();

            while (true)
            {
                _line = _reader.ReadLine();
                _lineIndex++;
                _columnIndex = 0;

                if (_line == null)
                    return;

                _columnIndex = FindNextTokenInLine();

                if (_columnIndex >= 0)
                    if (_line[_columnIndex] != '#')
                        return;
                    else
                        _commentBuffer?.Add(new KeyValuePair<TextLocation, string>(
                            new TextLocation(_lineIndex, _columnIndex),
                            _line.Substring(_columnIndex + 1)));
            }
        }

        private EntryTokens DetectEntryToken(out int length)
        {
            var n = _line.Length - _columnIndex;
            if (n >= 5 && string.Compare(_line, _columnIndex, "msg", 0, 3) == 0)
            {
                var index = _columnIndex + 3;
                switch (_line[index++])
                {
                    case 'i':
                        if (_line[index++] == 'd')
                            if (string.Compare(_line, index, "_plural", 0, 7) == 0)
                            {
                                length = 12;
                                return EntryTokens.PluralId;
                            }
                            else
                            {
                                length = 5;
                                return EntryTokens.Id;
                            }
                        break;
                    case 'c':
                        if (string.Compare(_line, index, "txt", 0, 3) == 0)
                        {
                            length = 7;
                            return EntryTokens.ContextId;
                        }
                        break;
                    case 's':
                        if (string.Compare(_line, index, "tr", 0, 2) == 0)
                        {
                            length = 6;
                            return EntryTokens.Translation;
                        }
                        break;
                }
            }

            length = default(int);
            return EntryTokens.None;
        }

        private bool TryReadPOStringPart(StringBuilder builder)
        {
            var lineLength = _line.Length;

            if (_columnIndex >= lineLength || _line[_columnIndex] != '"')
            {
                AddError(DiagnosticCodes.ExpectedToken, new TextLocation(_lineIndex, _columnIndex >= 0 ? _columnIndex : lineLength), '"');
                return false;
            }

            var startIndex = ++_columnIndex;
            for (; _columnIndex < lineLength; _columnIndex++)
                if (_line[_columnIndex] == '"' && !IsEscaped(startIndex))
                {
                    var endIndex = _columnIndex++;
                    _columnIndex = FindNextTokenInLine();
                    if (_columnIndex >= 0)
                    {
                        AddError(DiagnosticCodes.UnexpectedToken, new TextLocation(_lineIndex, lineLength));
                        return false;
                    }

                    var index = POString.Decode(builder, _line, startIndex, endIndex - startIndex);
                    if (index >= 0)
                    {
                        AddError(DiagnosticCodes.InvalidEscapeSequence, new TextLocation(_lineIndex, index));
                        return false;
                    }

                    _columnIndex = lineLength;
                    return true;
                }

            AddError(DiagnosticCodes.ExpectedToken, new TextLocation(_lineIndex, lineLength), '"');
            return false;

            bool IsEscaped(int si)
            {
                bool result = false;
                for (var i = _columnIndex - 1; i >= si; i--)
                    if (_line[i] == '\\')
                        result = !result;
                    else
                        break;

                return result;
            }
        }

        private bool TryReadPOString(out string result)
        {
            _builder.Clear();

            if (!TryReadPOStringPart(_builder))
            {
                result = null;
                return false;
            }

            do { SeekNextToken(); }
            while (_line != null && _line[_columnIndex] == '"' && TryReadPOStringPart(_builder));

            result = _builder.ToString();
            return true;
        }

        private bool TryReadPluralIndex(out int? result)
        {
            var lineLength = _line.Length;

            if (_columnIndex >= lineLength || _line[_columnIndex] != '[')
            {
                result = null;
                return true;
            }

            var startIndex = ++_columnIndex;
            char c;
            for (; _columnIndex < lineLength; _columnIndex++)
                if ((c = _line[_columnIndex]) == ']')
                {
                    var endIndex = _columnIndex++;
                    if (!int.TryParse(_line.Substring(startIndex, endIndex - startIndex), out int indexValue))
                    {
                        AddError(DiagnosticCodes.InvalidPluralIndex, new TextLocation(_lineIndex, startIndex - 1));
                        result = null;
                        return false;
                    }
                    result = indexValue;
                    return true;
                }
                else if (!char.IsDigit(c))
                {
                    AddError(DiagnosticCodes.InvalidPluralIndex, new TextLocation(_lineIndex, startIndex - 1));
                    result = null;
                    return false;
                }

            AddError(DiagnosticCodes.ExpectedToken, new TextLocation(_lineIndex, _columnIndex), ']');
            result = null;
            return false;
        }

        private List<POComment> ParseComments()
        {
            var result = new List<POComment>();

            KeyValuePair<TextLocation, string> commentKvp;
            string comment;
            int commentLength;
            var n = _commentBuffer.Count;
            for (var i = 0; i < n; i++)
                if ((commentLength = (comment = (commentKvp = _commentBuffer[i]).Value).Length) > 0)
                {
                    var index = 0;
                    var c = comment[index++];
                    POCommentKind kind;
                    switch (c)
                    {
                        case '.': kind = POCommentKind.Extracted; break;
                        case ':': kind = POCommentKind.Reference; break;
                        case ',': kind = POCommentKind.Flags; break;
                        case '|': kind = POCommentKind.PreviousValue; break;
                        default:
                            if (char.IsWhiteSpace(c))
                            {
                                kind = POCommentKind.Translator;
                                break;
                            }
                            else
                                continue;
                    }

                    if (kind != POCommentKind.Translator &&
                        (index >= commentLength || !char.IsWhiteSpace(comment[index++])))
                        continue;

                    comment = comment.Substring(index);
                    switch (kind)
                    {
                        case POCommentKind.Translator:
                            result.Add(new POTranslatorComment { Text = comment.Trim() });
                            break;
                        case POCommentKind.Extracted:
                            result.Add(new POExtractedComment { Text = comment.Trim() });
                            break;
                        case POCommentKind.Reference:
                            if (POReferenceComment.TryParse(comment, out POReferenceComment referenceComment))
                                result.Add(referenceComment);
                            else
                                AddWarning(DiagnosticCodes.MalformedComment, commentKvp.Key);
                            break;
                        case POCommentKind.Flags:
                            result.Add(POFlagsComment.Parse(comment));
                            break;
                        case POCommentKind.PreviousValue:
                            if (POPreviousValueComment.TryParse(comment, out POPreviousValueComment previousValueComment))
                                result.Add(previousValueComment);
                            else
                                AddWarning(DiagnosticCodes.MalformedComment, commentKvp.Key);
                            break;
                    }
                }

            return result;
        }

        private bool TryReadEntry(bool allowEmptyId, out IPOEntry result)
        {
            if (_line == null)
            {
                result = null;
                return false;
            }

            var entryLocation = new TextLocation(_lineIndex, _columnIndex);

            List<POComment> comments = _commentBuffer != null ? ParseComments() : null;
            Dictionary<int, string> translations = null;
            string id = null, pluralId = null, contextId = null;
            IPOEntry entry = null;
            EntryTokens expectedTokens = EntryTokens.Id | EntryTokens.PluralId | EntryTokens.ContextId;
            do
            {
                EntryTokens token = DetectEntryToken(out int tokenLength) & expectedTokens;
                if (token == EntryTokens.None)
                {
                    if (!(expectedTokens == EntryTokens.Translation && entry is POPluralEntry))
                    {
                        AddError(DiagnosticCodes.UnexpectedToken, new TextLocation(_lineIndex, _columnIndex));
                        result = null;
                        return false;
                    }
                    else
                        break;
                }

                _columnIndex += tokenLength;
                switch (token)
                {
                    case EntryTokens.Id:
                        _columnIndex = FindNextTokenInLine(requireWhiteSpace: true);
                        if (_columnIndex < 0 || !TryReadPOString(out id))
                        {
                            result = null;
                            return false;
                        }

                        expectedTokens &= ~EntryTokens.Id;
                        expectedTokens |= EntryTokens.Translation;
                        break;
                    case EntryTokens.PluralId:
                        _columnIndex = FindNextTokenInLine(requireWhiteSpace: true);
                        if (_columnIndex < 0 || !TryReadPOString(out pluralId))
                        {
                            result = null;
                            return false;
                        }
                        expectedTokens &= ~EntryTokens.PluralId;
                        break;
                    case EntryTokens.ContextId:
                        _columnIndex = FindNextTokenInLine(requireWhiteSpace: true);
                        if (_columnIndex < 0 || !TryReadPOString(out contextId))
                        {
                            result = null;
                            return false;
                        }
                        expectedTokens &= ~EntryTokens.ContextId;
                        break;
                    case EntryTokens.Translation:
                        var originalColumnIndex = _columnIndex;
                        TryReadPluralIndex(out int? pluralIndex);

                        _columnIndex = FindNextTokenInLine(requireWhiteSpace: true);
                        if (_columnIndex < 0 || !TryReadPOString(out string value))
                        {
                            result = null;
                            return false;
                        }

                        if (!allowEmptyId && entry == null && id == string.Empty)
                        {
                            AddError(Resources.InvalidEntryKey, entryLocation);
                            result = null;
                            return false;
                        }

                        // plural
                        if (pluralId != null)
                        {
                            if (pluralIndex != null)
                            {
                                if (pluralIndex < 0 || (_catalog.PluralFormCount > 0 && pluralIndex >= _catalog.PluralFormCount))
                                {
                                    AddError(DiagnosticCodes.InvalidPluralIndex, new TextLocation(_lineIndex, originalColumnIndex));
                                    break;
                                }
                            }
                            else
                            {
                                AddWarning(DiagnosticCodes.MissingPluralIndex, new TextLocation(_lineIndex, originalColumnIndex));
                                pluralIndex = 0;
                            }

                            if (entry == null)
                            {
                                entry = new POPluralEntry(new POKey(id, pluralId, contextId))
                                {
                                    Comments = comments,
                                };

                                translations = new Dictionary<int, string>();
                            }

                            if (translations.ContainsKey(pluralIndex.Value))
                                AddWarning(DiagnosticCodes.DuplicatePluralForm, entryLocation, pluralIndex.Value);

                            translations[pluralIndex.Value] = value;

                            expectedTokens = EntryTokens.Translation;
                        }
                        // singular
                        else
                        {
                            if (pluralIndex != null)
                                if (pluralIndex != 0)
                                {
                                    AddError(DiagnosticCodes.InvalidPluralIndex, new TextLocation(_lineIndex, originalColumnIndex));
                                    break;
                                }
                                else
                                    AddWarning(DiagnosticCodes.UnnecessaryPluralIndex, new TextLocation(_lineIndex, originalColumnIndex));

                            entry = new POSingularEntry(new POKey(id, null, contextId))
                            {
                                Comments = comments,
                                Translation = value
                            };

                            expectedTokens = EntryTokens.None;
                        }

                        break;
                }

                SeekNextToken();
            }
            while (_line != null && expectedTokens != EntryTokens.None);

            if (entry is POPluralEntry pluralEntry)
            {
                var n =
                    _catalog.PluralFormCount > 0 ? _catalog.PluralFormCount :
                    translations.Count > 0 ? translations.Keys.Max() + 1 :
                    0;

                for (var i = 0; i < n; i++)
                    if (translations.TryGetValue(i, out string value))
                        pluralEntry.Add(value);
                    else
                    {
                        pluralEntry.Add(null);
                        AddWarning(DiagnosticCodes.MissingPluralForm, entryLocation, i);
                    }
            }

            if (entry == null)
            {
                AddError(DiagnosticCodes.IncompleteEntry, entryLocation);
                result = null;
                return false;
            }

            result = entry;
            return true;
        }

        private bool CheckHeader(IPOEntry entry)
        {
            if (entry == null || entry.Key.Id != string.Empty)
                return false;

            if (!(entry is POSingularEntry))
            {
                AddError(DiagnosticCodes.HeaderNotSingular);
                return false;
            }

            if (entry.Key.PluralId != null || entry.Key.ContextId != null)
                AddWarning(DiagnosticCodes.InvalidHeaderEntryKey);

            if (entry.Comments != null && entry.Comments.Any(c => c.Kind == POCommentKind.PreviousValue || c.Kind == POCommentKind.Reference))
                AddWarning(DiagnosticCodes.InvalidHeaderComment);

            return true;
        }

        private void ParseEncoding(string line, string value)
        {
            Match match;
            if (value.Length > 0 &&
                (match = Regex.Match(value, @"^text/plain\s*;\s*charset\s*=\s*(\S+)$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)).Success)
                _catalog.Encoding = match.Groups[1].Value;
            else
                AddWarning(DiagnosticCodes.InvalidHeaderValue, line);
        }

        private void ParseLanguage(string line, string value)
        {
            if (value.Length > 0)
                _catalog.Language = value;
            else
                AddWarning(DiagnosticCodes.InvalidHeaderValue, line);
        }

        private void ParsePluralForms(string line, string value)
        {
            Match match;
            if (value.Length > 0 &&
                (match = Regex.Match(value, @"^nplurals\s*=\s*(\d+);\s*plural\s*=\s*([^;]+);$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)).Success &&
                int.TryParse(match.Groups[1].Value, out int pluralFormCount) &&
                pluralFormCount > 0 &&
                _catalog.TrySetPluralFormSelector(match.Groups[2].Value))
                _catalog.PluralFormCount = pluralFormCount;
            else
                AddWarning(DiagnosticCodes.InvalidHeaderValue, line);
        }

        private void ParseHeader(POSingularEntry entry)
        {
            if (!HasFlags(Flags.SkipInfoHeaders))
            {
#if USE_COMMON
                if (HasFlags(Flags.PreserveHeadersOrder))
                    _catalog.Headers = new OrderedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                else
#endif
                _catalog.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            _catalog.HeaderComments = entry.Comments;

            if (string.IsNullOrEmpty(entry.Translation))
                return;

            foreach (var line in entry
                .Translation.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()))
            {
                var index = line.IndexOf(':');
                if (index <= 0)
                {
                    AddWarning(DiagnosticCodes.MalformedHeaderItem, line);
                    continue;
                }

                var key = line.Substring(0, index).TrimEnd();
                var value = line.Remove(0, index + 1).TrimStart();

                if (HasFlags(Flags.ReadContentTypeHeaderOnly) &&
                    !string.Equals(key, "content-type", StringComparison.OrdinalIgnoreCase))
                    continue;

                switch (key.ToLowerInvariant())
                {
                    case "content-type":
                        ParseEncoding(line, value);
                        break;
                    case "language":
                        ParseLanguage(line, value);
                        break;
                    case "plural-forms":
                        ParsePluralForms(line, value);
                        break;
                }

                if (!HasFlags(Flags.SkipInfoHeaders))
                {
                    if (_catalog.Headers.TryGetValue(key, out string existingValue))
                        AddWarning(DiagnosticCodes.DuplicateHeaderKey, key);

                    _catalog.Headers[key] = value;
                }
            }
        }

        public POParseResult Parse(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            _reader = reader;

            _diagnostics = new DiagnosticCollection();
            _catalog = new POCatalog();

            _line = null;
            _lineIndex = -1;

            SeekNextToken();

            try
            {
                if (!TryReadEntry(allowEmptyId: true, result: out IPOEntry entry))
                    return new POParseResult(_catalog, _diagnostics);

                var isHeader = CheckHeader(entry);
                if (isHeader)
                    ParseHeader((POSingularEntry)entry);

                if (!HasFlags(Flags.ReadHeaderOnly))
                {
                    if (!isHeader)
                        _catalog.Add(entry);

                    while (TryReadEntry(allowEmptyId: false, result: out entry))
                        _catalog.Add(entry);
                }

                return new POParseResult(_catalog, _diagnostics);
            }
            finally
            {
                _builder.Clear();
                if (_builder.Capacity > 1024)
                    _builder.Capacity = 1024;
            }
        }
    }

    public static class POParserExtensions
    {
        public static POParseResult Parse(this POParser parser, string input)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            using (var reader = new StringReader(input))
                return parser.Parse(reader);
        }

        public static POParseResult Parse(this POParser parser, Stream input)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var reader = new StreamReader(input);
            return parser.Parse(reader);
        }

        public static POParseResult Parse(this POParser parser, Stream input, Encoding encoding)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var reader = new StreamReader(input, encoding);
            return parser.Parse(reader);
        }
    }
}
