using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace WebApp;

public class AppLocalizationOptions
{
    private static CultureInfo[]? s_defaultSupportedCultureInfos;
    private static CultureInfo[] DefaultSupportedCultureInfos => LazyInitializer.EnsureInitialized(ref s_defaultSupportedCultureInfos, () => new[] { new CultureInfo("en-US") });

    public string[]? SupportedCultures
    {
        get => _supportedCultureInfos?.Select(cultureInfo => cultureInfo.Name).ToArray();
        set => _supportedCultureInfos = value?.Length > 0 ? value.Select(culture => new CultureInfo(culture)).ToArray() : null;
    }

    private CultureInfo[]? _supportedCultureInfos;
    public IReadOnlyList<CultureInfo> SupportedCultureInfos => _supportedCultureInfos ?? DefaultSupportedCultureInfos;

    public CultureInfo DefaultCultureInfo => SupportedCultureInfos[0];
}
