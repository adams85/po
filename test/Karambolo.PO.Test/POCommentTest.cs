using System;
using Xunit;

namespace Karambolo.PO.Test
{
    public class POCommentTest
    {
        [Fact]
        public void POPreviousValueComment_ParseWithStringDecodingOptions()
        {
            var comment = POPreviousValueComment.Parse("msgid \"Previous id of\\na long text\"");
            Assert.Equal(POCommentKind.PreviousValue, comment.Kind);
            Assert.Equal(POIdKind.Id, comment.IdKind);
            Assert.Equal($"Previous id of{Environment.NewLine}a long text", comment.Value);

            comment = POPreviousValueComment.Parse("msgid \"Previous id of\\na long text\"", new POStringDecodingOptions { KeepTranslationStringsPlatformIndependent = true });
            Assert.Equal(POCommentKind.PreviousValue, comment.Kind);
            Assert.Equal(POIdKind.Id, comment.IdKind);
            Assert.Equal($"Previous id of{Environment.NewLine}a long text", comment.Value);

            comment = POPreviousValueComment.Parse("msgid \"Previous id of\\na long text\"", new POStringDecodingOptions { KeepKeyStringsPlatformIndependent = true });
            Assert.Equal(POCommentKind.PreviousValue, comment.Kind);
            Assert.Equal(POIdKind.Id, comment.IdKind);
            Assert.Equal($"Previous id of\na long text", comment.Value);
        }
    }
}
