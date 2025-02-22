﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Karambolo.Common.Properties;
using Karambolo.PO.Properties;

namespace Karambolo.PO
{
#if NET40
    using Karambolo.Common.Collections;
#endif

#if ENABLE_PLURALFORMS
    using Karambolo.PO.PluralExpressions;
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
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);

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
#if ENABLE_PLURALFORMS
            if (expression == null)
            {
                _compiledPluralFormSelector = s_defaultPluralFormSelector;
                _pluralFormSelector = null;
                return true;
            }

            Func<int, int> evaluator;
            try
            {
                Expression parsedExpression = PluralExpressionParser.Parse(expression, out ParameterExpression param);
                evaluator = PluralExpressionEvaluator.From(parsedExpression, param);
            }
            catch { return false; }

            _compiledPluralFormSelector = evaluator;
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

        protected override POKey GetKeyForItem(IPOEntry item)
        {
            return item.Key;
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

        protected virtual bool TryAddItem(IPOEntry item)
        {
            CheckEntry(item);

            int index;

#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            if (Dictionary is Dictionary<POKey, IPOEntry> dictionary)
            {
                index = Items.Count;
                if (dictionary.TryAdd(GetKeyForItem(item), item))
                {
                    Items.Insert(index, item);
                    return true;
                }
                return false;
            }
#endif

            if (!Contains(GetKeyForItem(item)))
            {
                index = Items.Count;
                base.InsertItem(index, item);
                return true;
            }

            return false;
        }

        public bool TryAdd(IPOEntry item)
        {
            return TryAddItem(item);
        }

#if !(NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
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
