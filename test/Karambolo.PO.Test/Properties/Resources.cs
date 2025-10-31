using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Karambolo.PO.Test.Properties
{
    public static class Resources
    {
        private static readonly Assembly s_assembly = typeof(Resources).Assembly;

        public static byte[] SamplePO =>
            GetEmbeddedResourceAsByteArray("Resources/sample.po");

        public static byte[] SamplePO_WithCustomHeaderOrder =>
            GetEmbeddedResourceAsByteArray("Resources/sample_withcustomheaderorder.po");

        public static byte[] SamplePO_WithTrailingWhiteSpace =>
            GetEmbeddedResourceAsByteArray("Resources/sample_withtrailingwhitespace.po");

        public static byte[] StringEdgeCasesPO =>
            GetEmbeddedResourceAsByteArray("Resources/stringedgecases.po");

        public static byte[] NewLineTestPO =>
            GetEmbeddedResourceAsByteArray("Resources/newlinetest.po");

        public static byte[] EscapedCharTestPO =>
            GetEmbeddedResourceAsByteArray("Resources/escapedchartest.po");

        public static byte[] InvalidControlCharTestPO =>
            GetEmbeddedResourceAsByteArray("Resources/invalidcontrolchartest.po");

        public static byte[] GetEmbeddedResourceAsByteArray(string resourcePath)
        {
            using (Stream stream = s_assembly.GetManifestResourceStream($"{s_assembly.GetName().Name}.{resourcePath.Replace('/', '.')}"))
            using (var ms = new MemoryStream((int)stream.Length))
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string GetEmbeddedResourceAsString(string resourcePath)
        {
            using (Stream stream = s_assembly.GetManifestResourceStream($"{s_assembly.GetName().Name}.{resourcePath.Replace('/', '.')}"))
            using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                return reader.ReadToEnd();
        }
    }
}
