using Microsoft.AspNetCore.Mvc.Localization;

namespace WebApp.Infrastructure.Localization;

public sealed class ExtendedHtmlLocalizer : HtmlLocalizer
{
    private readonly IExtendedStringLocalizer _stringLocalizer;

    public ExtendedHtmlLocalizer(IExtendedStringLocalizer stringLocalizer) : base(stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }

    public override LocalizedHtmlString this[string name]
    {
        get
        {
            var translation = _stringLocalizer.GetTranslation(name, default, default, out _, out var resourceNotFound);
            return new LocalizedHtmlString(name, translation, resourceNotFound);
        }
    }

    public override LocalizedHtmlString this[string name, params object[] arguments]
    {
        get
        {
            var (plural, context) = LocalizationHelper.GetSpecialArgs(arguments);
            var translation = _stringLocalizer.GetTranslation(name, plural, context, out _, out var resourceNotFound);
            return new LocalizedHtmlString(name, translation, resourceNotFound, arguments);
        }
    }
}
