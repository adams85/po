using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;

namespace WebApp.Infrastructure.Localization
{
    public sealed class NullStringLocalizer : IExtendedStringLocalizer
    {
        public static readonly NullStringLocalizer Instance = new NullStringLocalizer();

        private NullStringLocalizer() { }

        public LocalizedString this[string name]
        {
            get
            {
                TryLocalize(name, out var searchedLocation, out var value);
                return new LocalizedString(name, value, resourceNotFound: false, searchedLocation);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                TryLocalize(name, arguments, out var searchedLocation, out var value);
                return new LocalizedString(name, value, resourceNotFound: false, searchedLocation);
            }
        }

        public string GetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, out bool resourceNotFound)
        {
            TryGetTranslation(name, plural, context, out searchedLocation, out var value);
            resourceNotFound = false;
            return value!;
        }

        public bool TryGetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, out string value)
        {
            searchedLocation = null;
            value = plural.Id != null && plural.Count != 1 ? plural.Id : name;
            return true;
        }

        public bool TryLocalize(string name, out string? searchedLocation, out string value)
        {
            TryGetTranslation(name, default, default, out searchedLocation, out value!);
            return true;
        }

        public bool TryLocalize(string name, object[] arguments, out string? searchedLocation, out string value)
        {
            var (plural, context) = LocalizationHelper.GetSpecialArgs(arguments);
            TryGetTranslation(name, plural, context, out searchedLocation, out value);
            value = string.Format(value, arguments);
            return true;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotSupportedException();

#if !NET5_0_OR_GREATER
        [Obsolete("This method is obsolete. Use `CurrentCulture` and `CurrentUICulture` instead.")]
        public IStringLocalizer WithCulture(CultureInfo culture) => this;
#endif
    }
}
