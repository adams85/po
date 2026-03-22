using System;

namespace Karambolo.Common.Localization
{
    public readonly struct TextContext
    {
        public static TextContext From(string id)
        {
            return new TextContext(id);
        }

        private TextContext(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            Id = id;
        }

        public string Id { get; }
    }
}
