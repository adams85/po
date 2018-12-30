using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Karambolo.Common.Properties;

namespace Karambolo.PO
{
#if USE_HIME
using Karambolo.PO.PluralExpression;
#endif

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

        static readonly Func<int, int> defaultPluralFormSelector = n => 0;

        Func<int, int> _compiledPluralFormSelector;

        public POCatalog() : base(null, Constants.RecommendedKeyedCollectionThreshold)
        {
            _compiledPluralFormSelector = defaultPluralFormSelector;
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

        public IEnumerable<POKey> Keys => Values.Select(GetKeyForItem);

        public IEnumerable<IPOEntry> Values => this;

        public IDictionary<string, string> Headers { get; set; }
        public IList<POComment> HeaderComments { get; set; }

        public string Encoding { get; set; }

        public string Language { get; set; }

        int _pluralFormCount;
        public int PluralFormCount
        {
            get { return _pluralFormCount; }
            set
            {
                if (value < 0)
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
#if USE_HIME
            if (expression == null)
            {
                _compiledPluralFormSelector = defaultPluralFormSelector;
                _pluralFormSelector = null;
                return true;
            }

            var lexer = new PluralExpressionLexer(expression);
            var parser = new PluralExpressionParser(lexer);
            var parseResult = parser.Parse();
            if (!parseResult.IsSuccess)
                return false;

            var compiler = new PluralExpressionCompiler(parseResult.Root);
            Func<int, int> @delegate;
            try { @delegate = compiler.Compile(); }
            catch { return false; }

            _compiledPluralFormSelector = @delegate;
#endif
            _pluralFormSelector = expression;
            return true;
        }

        public int GetPluralFormIndex(int n)
        {
            var count = _pluralFormCount;
            return
                --count > 0 ?
                Math.Max(Math.Min(count, _compiledPluralFormSelector(n)), 0) :
                count;
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
            return
                count > 0 ?
                (entry[--count > 0 ? Math.Max(Math.Min(count, _compiledPluralFormSelector(n)), 0) : 0]) :
                null;
        }

        protected override POKey GetKeyForItem(IPOEntry item)
        {
            return item.Key;
        }

        public bool TryGetValue(POKey key, out IPOEntry value)
        {
            if (Dictionary != null)
                return Dictionary.TryGetValue(key, out value);

            IPOEntry item;
            var n = Count;
            for (var i = 0; i < n; i++)
                if (GetKeyForItem(item = this[i]) == key)
                {
                    value = item;
                    return true;
                }

            value = null;
            return false;
        }

        bool IReadOnlyDictionary<POKey, IPOEntry>.ContainsKey(POKey key)
        {
            return Contains(key);
        }

        IEnumerator<KeyValuePair<POKey, IPOEntry>> IEnumerable<KeyValuePair<POKey, IPOEntry>>.GetEnumerator()
        {
            foreach (var item in Values)
                yield return new KeyValuePair<POKey, IPOEntry>(GetKeyForItem(item), item);
        }
    }
}
