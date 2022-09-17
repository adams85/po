using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Karambolo.Common.Localization;
using Karambolo.PO;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace WebApp.Infrastructure.Localization;

public sealed class POStringLocalizer : IExtendedStringLocalizer
{
    private readonly ITranslationsProvider _translationsProvider;
    private readonly string _location;
    private readonly CultureInfo? _culture;
    private readonly ILogger _logger;

    public POStringLocalizer(ITranslationsProvider translationsProvider, string location, CultureInfo? culture = null, ILogger<POStringLocalizer>? logger = null)
    {
        _translationsProvider = translationsProvider;
        _location = location;
        _culture = culture;
        _logger = logger ?? (ILogger)NullLogger.Instance;
    }

    private CultureInfo CurrentCulture => _culture ?? CultureInfo.CurrentUICulture;

    public LocalizedString this[string name]
    {
        get
        {
            var resourceNotFound = !TryLocalize(name, out var searchedLocation, out var value);
            if (resourceNotFound)
            {
                _logger.TranslationNotAvailable(name, CurrentCulture, searchedLocation);
                NullStringLocalizer.Instance.TryLocalize(name, out _, out value);
            }
            return new LocalizedString(name, value!, resourceNotFound, searchedLocation);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var resourceNotFound = !TryLocalize(name, arguments, out var searchedLocation, out var value);
            if (resourceNotFound)
            {
                _logger.TranslationNotAvailable(name, CurrentCulture, searchedLocation);
                NullStringLocalizer.Instance.TryLocalize(name, arguments, out _, out value);
            }
            return new LocalizedString(name, value!, resourceNotFound, searchedLocation);
        }
    }

    private POCatalog? GetCatalog()
    {
        var catalogs = _translationsProvider.GetCatalogs();
        var culture = CurrentCulture;
        for (; ; )
        {
            if (catalogs.TryGetValue((_location, culture.Name), out var catalog))
                return catalog;

            var parentCulture = culture.Parent;
            if (culture == parentCulture)
                return null;

            culture = parentCulture;
        }
    }

    public string GetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, out bool resourceNotFound)
    {
        resourceNotFound = !TryGetTranslation(name, plural, context, out searchedLocation, out var value);
        if (resourceNotFound)
        {
            _logger.TranslationNotAvailable(name, CurrentCulture, searchedLocation);
            value = NullStringLocalizer.Instance.GetTranslation(name, plural, context, out _, out _);
        }

        return value!;
    }

    public bool TryGetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, [MaybeNullWhen(false)] out string value)
    {
        var catalog = GetCatalog();
        if (catalog != null)
        {
            var key = new POKey(name, plural.Id, context.Id);
            value = plural.Id == null ? catalog.GetTranslation(key) : catalog.GetTranslation(key, plural.Count);
            if (value != null)
            {
                searchedLocation = _location;
                return true;
            }
        }

        searchedLocation = _location;
        value = default;
        return false;
    }

    public bool TryLocalize(string name, out string? searchedLocation, [MaybeNullWhen(false)] out string value) =>
        TryGetTranslation(name, default, default, out searchedLocation, out value);

    public bool TryLocalize(string name, object[] arguments, out string? searchedLocation, [MaybeNullWhen(false)] out string value)
    {
        var (plural, context) = LocalizationHelper.GetSpecialArgs(arguments);
        if (!TryGetTranslation(name, plural, context, out searchedLocation, out value))
            return false;

        value = string.Format(value, arguments);
        return true;
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var catalogs = _translationsProvider.GetCatalogs();
        var culture = CurrentCulture;
        do
        {
            if (catalogs.TryGetValue((_location, culture.Name), out var catalog))
                foreach (var entry in catalog)
                    if (entry.Count > 0)
                        yield return new LocalizedString(entry.Key.Id, entry[0], resourceNotFound: false, _location);

            var parentCulture = culture.Parent;
            if (culture == parentCulture)
                break;

            culture = parentCulture;
        }
        while (includeParentCultures);
    }

#if !NET5_0_OR_GREATER
    [Obsolete("This method is obsolete. Use `CurrentCulture` and `CurrentUICulture` instead.")]
    public IStringLocalizer WithCulture(CultureInfo culture) =>
        new POStringLocalizer(_translationsProvider, _location, culture, _logger as ILogger<POStringLocalizer>);
#endif
}
