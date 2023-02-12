using System;
using System.Collections.Generic;

namespace Karambolo.PO
{
    internal sealed class POHeaderDefaultOrderComparer : IComparer<string>
    {
        public static readonly POHeaderDefaultOrderComparer Instance = new POHeaderDefaultOrderComparer();

        private POHeaderDefaultOrderComparer() { }

        private static readonly
#if NET40
            Dictionary<string, int>
#else
            IReadOnlyDictionary<string, int>
#endif
            s_wellKnownHeaderKeyToOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                [POCatalog.ProjectIdVersionHeaderName] = 1,
                [POCatalog.ReportMsgidBugsToHeaderName] = 2,
                [POCatalog.PotCreationDateHeaderName] = 3,
                [POCatalog.PORevisionDateHeaderName] = 4,
                [POCatalog.LastTranslatorHeaderName] = 5,
                [POCatalog.LanguageTeamHeaderName] = 6,
                [POCatalog.LanguageHeaderName] = 7,
                [POCatalog.MIMEVersionHeaderName] = 8,
                [POCatalog.ContentTypeHeaderName] = 9,
                [POCatalog.ContentTransferEncodingHeaderName] = 10,
                [POCatalog.PluralFormsHeaderName] = 11,
            };

        public int Compare(string x, string y)
        {
            if (s_wellKnownHeaderKeyToOrder.TryGetValue(x, out var order1))
            {
                return s_wellKnownHeaderKeyToOrder.TryGetValue(y, out var order2) ? order1.CompareTo(order2) : -1;
            }
            else
            {
                return s_wellKnownHeaderKeyToOrder.ContainsKey(y) ? 1 : StringComparer.OrdinalIgnoreCase.Compare(x, y);
            }
        }
    }
}
