using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Karambolo.PO
{
    public interface IPOEntry : IReadOnlyList<string>
    {
        POKey Key { get; }

        IList<POComment> Comments { get; set; }
    }

    [DebuggerDisplay("Count = {" + nameof(Count) +"}"), DebuggerTypeProxy(typeof(DebugView))]
    public class POSingularEntry : IPOEntry
    {
        sealed class DebugView
        {
            POSingularEntry _entry;

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
            if (!key.IsValid)
                throw new ArgumentException(null, nameof(key));

            Key = key;
        }

        public POKey Key { get; }

        static string CheckIndex(int index, string value)
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
        public POPluralEntry(POKey key)
        {
            if (!key.IsValid)
                throw new ArgumentException(null, nameof(key));

            Key = key;
        }

        public POKey Key { get; }

        public IList<POComment> Comments { get; set; }
    }
}
