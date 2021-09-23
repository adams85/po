using System;
using System.Collections.Concurrent;
using System.Globalization;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Logging;

namespace WebApp.Infrastructure.Localization
{
    public static class LocalizationHelper
    {
        private static readonly ConcurrentDictionary<(string, string?), object?> s_unavailableTranslations = new ConcurrentDictionary<(string, string?), object?>();

        internal static void TranslationNotAvailable(this ILogger logger, string name, CultureInfo culture, string? searchedLocation)
        {
            if (s_unavailableTranslations.TryAdd((name, searchedLocation), default))
                logger.LogWarning("Translation for '{NAME}' in culture '{CULTURE}' was not found at the following location(s): {LOCATION}.", name, culture.Name, searchedLocation ?? "(n/a)");
        }

        public static (Plural, TextContext) GetSpecialArgs(object[] args)
        {
            var plural = (Plural?)Array.Find(args, arg => arg is Plural);
            var context = args.Length > 0 ? args[^1] as TextContext? : null;

            return (plural ?? default, context ?? default);
        }
    }
}
