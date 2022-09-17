using System.Diagnostics.CodeAnalysis;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;

namespace WebApp.Infrastructure.Localization;

public interface IExtendedStringLocalizer : IStringLocalizer
{
    string GetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, out bool resourceNotFound);
    bool TryGetTranslation(string name, Plural plural, TextContext context, out string? searchedLocation, [MaybeNullWhen(false)] out string value);

    bool TryLocalize(string name, out string? searchedLocation, [MaybeNullWhen(false)] out string value);
    bool TryLocalize(string name, object[] arguments, out string? searchedLocation, [MaybeNullWhen(false)] out string value);
}
