using System.Collections.Generic;

namespace Karambolo.PO
{
    internal sealed class POPreviousValueCommentDefaultOrderComparer : IComparer<POComment>
    {
        public static readonly POPreviousValueCommentDefaultOrderComparer Instance = new POPreviousValueCommentDefaultOrderComparer();

        // TODO: in the next major version, reorder POIdKind members to their natural order to
        // make this enum value -> order translation unecessary.
        private static readonly int[] s_idKindToOrder = new int[POIdKind.ContextId - POIdKind.Unknown + 1] { int.MaxValue, 1, 2, 0 };

        private POPreviousValueCommentDefaultOrderComparer() { }

        private int GetOrder(POComment comment)
        {
            var previousValueComment = (POPreviousValueComment)comment;
            return s_idKindToOrder[previousValueComment.IdKind - POIdKind.Unknown];
        }

        public int Compare(POComment x, POComment y)
        {
            return GetOrder(x).CompareTo(GetOrder(y));
        }
    }
}
