using System.Collections;
using System.Collections.Generic;

namespace Karambolo.PO.Test.Helpers
{
    public class CustomPOEntry : IPOEntry
    {
        private readonly List<string> _translations;

        public CustomPOEntry(POKey key, params string[] translations)
        {
            Key = key;
            _translations = new List<string>(translations);
        }

        public string this[int index] => _translations[index];

        public POKey Key { get; }

        public IList<POComment> Comments { get; set; }

        public int Count => _translations.Count;

        public IEnumerator<string> GetEnumerator() => _translations.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
