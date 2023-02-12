using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Karambolo.Common.Properties;
using Karambolo.PO.Properties;

namespace Karambolo.PO
{
#if NET40
    using Karambolo.Common.Collections;
#endif

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
        public const string MIMEVersionHeaderName = "MIME-Version";
        public const string ContentTypeHeaderName = "Content-Type";
        public const string ContentTransferEncodingHeaderName = "Content-Transfer-Encoding";
        public const string PluralFormsHeaderName = "Plural-Forms";

        private static readonly Func<int, int> s_defaultPluralFormSelector = n => 0;

        private Func<int, int> _compiledPluralFormSelector;

        public POCatalog() : base(null, Constants.RecommendedKeyedCollectionThreshold)
        {
            _compiledPluralFormSelector = s_defaultPluralFormSelector;
        }

        public POCatalog(IEnumerable<IPOEntry> items)
            : this()
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            foreach (IPOEntry item in items)
                Add(item);
        }

        [Obsolete("This constructor is redundant (as it is not a proper copy constructor), thus it will be removed or reworked in the next major version. Use POCatalog(IEnumerable<IPOEntry>) instead.")]
        public POCatalog(POCatalog catalog)
            : this((catalog ?? throw new ArgumentNullException(nameof(catalog))).AsEnumerable<IPOEntry>()) { }

        public IEnumerable<POKey> Keys => Values.Select(GetKeyForItem);

        public IEnumerable<IPOEntry> Values => this;

        public IDictionary<string, string> Headers { get; set; }
        public IList<POComment> HeaderComments { get; set; }

        public string Encoding { get; set; }

        public string Language { get; set; }

        private int _pluralFormCount;
        public int PluralFormCount
        {
            get => _pluralFormCount;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _pluralFormCount = value;
            }
        }

        private string _pluralFormSelector;
        public string PluralFormSelector
        {
            get => _pluralFormSelector;
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
                _compiledPluralFormSelector = s_defaultPluralFormSelector;
                _pluralFormSelector = null;
                return true;
            }

            var lexer = new PluralExpressionLexer(expression);
            var parser = new PluralExpressionParser(lexer);
            Hime.Redist.ParseResult parseResult = parser.Parse();
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
                entry[--count > 0 ? Math.Max(Math.Min(count, _compiledPluralFormSelector(n)), 0) : 0] :
                null;
        }

        private static void CheckEntry(IPOEntry item)
        {
            POKey key = item.Key;
            if (!key.IsValid || key.IsHeaderEntryKey)
                throw new ArgumentException(string.Format(Resources.InvalidCatalogEntryKey, nameof(POKey.Id),nameof(POKey.PluralId), nameof(POKey.ContextId)), nameof(item));
        }

        protected override void InsertItem(int index, IPOEntry item)
        {
            CheckEntry(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IPOEntry item)
        {
            CheckEntry(item);
            base.SetItem(index, item);
        }

        protected override POKey GetKeyForItem(IPOEntry item)
        {
            return item.Key;
        }

#if NET40 || NET45 || NETSTANDARD1_0 || NETSTANDARD2_0
        public bool TryGetValue(POKey key, out IPOEntry value)
        {
            if (Dictionary != null)
                return Dictionary.TryGetValue(key, out value);

            IPOEntry item;
            for (int i = 0, n = Count; i < n; i++)
                if (GetKeyForItem(item = this[i]) == key)
                {
                    value = item;
                    return true;
                }

            value = null;
            return false;
        }
#endif

        bool IReadOnlyDictionary<POKey, IPOEntry>.ContainsKey(POKey key)
        {
            return Contains(key);
        }

        IEnumerator<KeyValuePair<POKey, IPOEntry>> IEnumerable<KeyValuePair<POKey, IPOEntry>>.GetEnumerator()
        {
            foreach (IPOEntry item in Values)
                yield return new KeyValuePair<POKey, IPOEntry>(GetKeyForItem(item), item);
        }
    }
}
