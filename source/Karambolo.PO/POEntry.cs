using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Karambolo.PO.Properties;

namespace Karambolo.PO
{
#if NET40
    using Karambolo.Common.Collections;
#endif

    public interface IPOEntry : IReadOnlyList<string>
    {
        POKey Key { get; }

        IList<POComment> Comments { get; set; }
    }

    [DebuggerDisplay("Count = {" + nameof(Count) + "}"), DebuggerTypeProxy(typeof(DebugView))]
    public class POSingularEntry : IPOEntry
    {
        private sealed class DebugView
        {
            private readonly POSingularEntry _entry;

            public DebugView(POSingularEntry entry)
            {
                if (entry == null)
                    throw new ArgumentNullException(nameof(entry));

                _entry = entry;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public string[] Items => new[] { _entry.Translation };
        }

        public POSingularEntry(POKey key)
        {
            if (!key.IsValid || key.PluralId != null)
                throw new ArgumentException(string.Format(Resources.InvalidSingularEntryKey, nameof(POKey.Id), nameof(POKey.PluralId)), nameof(key));

            Key = key;
        }

        public POKey Key { get; }

        private static string CheckIndex(int index, string value)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            return value;
        }

        public string this[int index]
        {
            get => CheckIndex(index, Translation);
            set => Translation = CheckIndex(index, value);
        }

        public int Count => 1;

        public string Translation { get; set; }

        public IList<POComment> Comments { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            yield return Translation;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class POPluralEntry : Collection<string>, IPOEntry
    {
        private POPluralEntry(POKey key, IList<string> translations)
            : base(translations)
        {
            if (!key.IsValid || key.PluralId == null)
                throw new ArgumentException(string.Format(Resources.InvalidPluralEntryKey, nameof(POKey.Id), nameof(POKey.PluralId)), nameof(key));

            Key = key;
        }

        public POPluralEntry(POKey key)
            : this(key, new List<string>()) { }

        public POPluralEntry(POKey key, IEnumerable<string> translations)
            : this(key, (translations ?? throw new ArgumentNullException(nameof(translations))).ToList()) { }

        public POKey Key { get; }

        public IList<POComment> Comments { get; set; }
    }
}
