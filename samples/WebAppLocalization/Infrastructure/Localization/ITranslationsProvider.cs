using System.Collections.Generic;
using Karambolo.PO;

namespace WebApp.Infrastructure.Localization
{
    public interface ITranslationsProvider
    {
        IReadOnlyDictionary<(string Location, string Culture), POCatalog> GetCatalogs();
    }
}
