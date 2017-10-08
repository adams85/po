using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Karambolo.PO.PluralExpression;

namespace Karambolo.PO
{
    public class POCatalog : KeyedCollection<POKey, IPOEntry>, IReadOnlyDictionary<POKey, IPOEntry>
    {
        internal const string IdToken = "msgid";
        internal const string PluralIdToken = "msgid_plural";
        internal const string ContextIdToken = "msgctxt";
        internal const string TranslationToken = "msgstr";

        public const string ProjectIdVersionHeaderName = "Project-Id-Version";
        public const string ReportMsgidBugsToHeaderName = "Report-Msgid-Bugs-To";
        public const string PotCreationDateHeaderName = "POT-Creation-Date";
        public const string PORevisionDateHeaderName = "PO-Revision-Date";
        public const string LastTranslatorHeaderName = "Last-Translator";
        public const string LanguageTeamHeaderName = "Language-Team";
        public const string LanguageHeaderName = "Language";
        public const string ContentTypeHeaderName = "Content-Type";
        public const string ContentTransferEncodingHeaderName = "Content-Transfer-Encoding";
        public const string PluralFormsHeaderName = "Plural-Forms";

        const string defaultPluralFormSelector = "0";
        static readonly Func<int, int> defaultCompiledPluralFormSelector = n => 0;

        Func<int, int> _compiledPluralFormSelector;

        public POCatalog()
        {
            PluralFormCount = 1;
            PluralFormSelector = defaultPluralFormSelector;
            _compiledPluralFormSelector = defaultCompiledPluralFormSelector;
        }

        public POCatalog(POCatalog catalog) : this()
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            foreach (var item in catalog)
                Add(item);
        }

        public POCatalog(IEnumerable<IPOEntry> items) : this()
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
                Add(item);
        }

        IEnumerable<POKey> IReadOnlyDictionary<POKey, IPOEntry>.Keys => Dictionary.Keys;

        IEnumerable<IPOEntry> IReadOnlyDictionary<POKey, IPOEntry>.Values => this;

        public IDictionary<string, string> Headers { get; set; }

        public string Encoding { get; set; }

        public string Language { get; set; }

        int _pluralFormCount;
        public int PluralFormCount
        {
            get { return _pluralFormCount; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _pluralFormCount = value;
            }
        }

        string _pluralFormSelector;
        public string PluralFormSelector
        {
            get { return _pluralFormSelector; }
            set
            {
                if (_pluralFormSelector == value)
                    return;

                if (!TrySetPluralFormSelector(value))
                    throw new ArgumentException(null, nameof(value));
            }
        }

        public bool TrySetPluralFormSelector(string expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var lexer = new PluralExpressionLexer(expression);
            var parser = new PluralExpressionParser(lexer);
            var parseResult = parser.Parse();
            if (!parseResult.IsSuccess)
                return false;

            var compiler = new PluralExpressionCompiler(parseResult.Root);
            Func<int, int> @delegate;
            try { @delegate = compiler.Compile(); }
            catch { return false; }

            _pluralFormSelector = expression;
            _compiledPluralFormSelector = @delegate;

            return true;
        }

        public int GetPluralFormIndex(int n)
        {
            var count = _pluralFormCount;
            return
                --count > 0 ?
                Math.Max(Math.Min(count, _compiledPluralFormSelector(n)), 0) :
                0;
        }

        public string GetTranslation(POKey key)
        {
            if (!TryGetValue(key, out IPOEntry entry))
                return null;

            return
                entry.Count > 0 ?
                entry[0] :
                null;
        }

        public string GetTranslation(POKey key, int n)
        {
            if (!TryGetValue(key, out IPOEntry entry))
                return null;

            var count = entry.Count;
            if (count == 0)
                return null;

            return
                --count > 0 ?
                entry[Math.Max(Math.Min(count, _compiledPluralFormSelector(n)), 0)] :
                entry[0];
        }

        protected override POKey GetKeyForItem(IPOEntry item)
        {
            return item.Key;
        }

        public bool TryGetValue(POKey key, out IPOEntry value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        bool IReadOnlyDictionary<POKey, IPOEntry>.ContainsKey(POKey key)
        {
            return Contains(key);
        }

        IEnumerator<KeyValuePair<POKey, IPOEntry>> IEnumerable<KeyValuePair<POKey, IPOEntry>>.GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }
    }
}
