using System;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace WebApp.Infrastructure.Localization;

public sealed class ExtendedHtmlLocalizerFactory : IHtmlLocalizerFactory
{
    private readonly IStringLocalizerFactory _stringLocalizerFactory;

    public ExtendedHtmlLocalizerFactory(IStringLocalizerFactory stringLocalizerFactory)
    {
        _stringLocalizerFactory = stringLocalizerFactory ?? throw new ArgumentNullException(nameof(stringLocalizerFactory));
    }

    private static IHtmlLocalizer CreateHtmlLocalizer(IStringLocalizer stringLocalizer) =>
        stringLocalizer is IExtendedStringLocalizer extendedStringLocalizer ?
        new ExtendedHtmlLocalizer(extendedStringLocalizer) :
        new HtmlLocalizer(stringLocalizer);

    public IHtmlLocalizer Create(string baseName, string location) => CreateHtmlLocalizer(_stringLocalizerFactory.Create(baseName, location));

    public IHtmlLocalizer Create(Type resourceSource) => CreateHtmlLocalizer(_stringLocalizerFactory.Create(resourceSource));
}
