using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Karambolo.Common;
using Karambolo.PO.Properties;

namespace Karambolo.PO
{
    [Flags]
    public enum POCommentKind
    {
        None = 0,
        Translator = 0x1,
        Extracted = 0x2,
        Reference = 0x4,
        Flags = 0x8,
        PreviousValue = 0x10,
    }

    public abstract class POComment
    {
        public POComment(POCommentKind kind)
        {
            var kindValue = (int)kind;
            // checking if a single value is set only
            if (kindValue == 0 ||
                (kindValue & (kindValue - 1)) != 0)
                throw new ArgumentException(null, nameof(kind));

            Kind = kind;
        }

        public POCommentKind Kind { get; }
    }

    public class POTranslatorComment : POComment
    {
        public POTranslatorComment() : base(POCommentKind.Translator) { }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class POExtractedComment : POComment
    {
        public POExtractedComment() : base(POCommentKind.Extracted) { }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public readonly struct POSourceReference
    {
        public static bool TryParse(string value, out POSourceReference result)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var index = value.LastIndexOf(':');
            if (index >= 0 && int.TryParse(value.Substring(index + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int line))
                value = value.Remove(index);
            else
                line = default(int);

            var length = value.Length;
            if (length > 0 && value[0] == '"')
                if (length > 1 && value[length - 1] == '"')
                    value = value.Substring(1, length - 2);
                else
                {
                    result = default(POSourceReference);
                    return false;
                }

            result = new POSourceReference(value, line);
            return true;
        }

        public static POSourceReference Parse(string value)
        {
            return
                TryParse(value, out POSourceReference result) ?
                result :
                throw new FormatException(Resources.IncorrectFormat);
        }

        public POSourceReference(string filePath, int line)
        {
            FilePath = filePath;
            Line = line;
        }

        public string FilePath { get; }
        public int Line { get; }

        public override string ToString()
        {
            var filePath =
                !string.IsNullOrEmpty(FilePath) ?
                (FilePath.FindIndex(char.IsWhiteSpace) >= 0 ? "\"" + FilePath + "\"" : FilePath) :
                "(unknown)";

            return
                Line > 0 ?
                filePath + ":" + Line.ToString(CultureInfo.InvariantCulture) :
                filePath;
        }
    }

    public class POReferenceComment : POComment
    {
        public static bool TryParse(string value, out POReferenceComment result)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var withinQuotes = false;
            IEnumerable<string> parts = value
                .Split(c =>
                {
                    if (c == '"')
                        withinQuotes = !withinQuotes;

                    return !withinQuotes && char.IsWhiteSpace(c);
                }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());

            var references = new List<POSourceReference>();
            foreach (var part in parts)
                if (POSourceReference.TryParse(part, out POSourceReference reference))
                    references.Add(reference);
                else
                {
                    result = null;
                    return false;
                }

            result = new POReferenceComment { References = references };
            return true;
        }

        public static POReferenceComment Parse(string value)
        {
            return
                TryParse(value, out POReferenceComment result) ?
                result :
                throw new FormatException(Resources.IncorrectFormat);
        }

        public POReferenceComment() : base(POCommentKind.Reference) { }

        public IList<POSourceReference> References { get; set; }

        public override string ToString()
        {
            return
                References != null ?
                string.Join(" ", References) :
                string.Empty;
        }
    }

    public class POFlagsComment : POComment
    {
        public static POFlagsComment Parse(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var flags = value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToHashSet();

            return new POFlagsComment { Flags = flags };
        }

        public POFlagsComment() : base(POCommentKind.Flags) { }

        public ISet<string> Flags { get; set; }

        public override string ToString()
        {
            return
                Flags != null ?
                string.Join(", ", Flags) :
                string.Empty;
        }
    }

    public class POPreviousValueComment : POComment
    {
        internal static bool TryParse(string value, string keyStringNewLine, out POPreviousValueComment result)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string idKindToken = null;
            value = value.Trim();
            var index = value.FindIndex(char.IsWhiteSpace);
            if (index >= 0)
            {
                idKindToken = value.Remove(index);
                value = value.Substring(index + 1).TrimStart();
            }

            int length;
            POIdKind idKind;
            StringBuilder sb;
            if (index < 0 ||
                (length = value.Length) < 2 || value[0] != '"' || value[length - 1] != '"' ||
                (idKind = POKey.GetIdKind(idKindToken)) == POIdKind.Unknown ||
                POString.Decode(sb = new StringBuilder(), value, 1, length - 2, keyStringNewLine) >= 0)
            {
                result = null;
                return false;
            }

            result = new POPreviousValueComment { IdKind = idKind, Value = sb.ToString() };
            return true;
        }

        public static bool TryParse(string value, out POPreviousValueComment result)
        {
            return TryParse(value, (POStringDecodingOptions)null, out result);
        }

        public static bool TryParse(string value, POStringDecodingOptions options, out POPreviousValueComment result)
        {
            return TryParse(value, POString.NewLine(options?.KeepKeyStringsPlatformIndependent ?? false), out result);
        }

        public static POPreviousValueComment Parse(string value)
        {
            return Parse(value, (POStringDecodingOptions)null);
        }

        public static POPreviousValueComment Parse(string value, POStringDecodingOptions options)
        {
            return
                TryParse(value, options, out POPreviousValueComment result) ?
                result :
                throw new FormatException(Resources.IncorrectFormat);
        }

        public POPreviousValueComment() : base(POCommentKind.PreviousValue) { }

        public POIdKind IdKind { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            var idKindToken = POKey.GetIdKindToken(IdKind) ?? "?";
            var value = Value ?? string.Empty;

            var sb = new StringBuilder();
            sb.Append(idKindToken);
            sb.Append(' ');
            sb.Append('"');
            POString.Encode(sb, value, 0, value.Length);
            sb.Append('"');

            return sb.ToString();
        }
    }
}
