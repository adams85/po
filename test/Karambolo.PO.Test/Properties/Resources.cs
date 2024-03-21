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

        public static byte[] SamplePO
        {
            get
            {
                using (Stream stream = s_assembly.GetManifestResourceStream($"{s_assembly.GetName().Name}.Resources.sample.po"))
                using (var ms = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] SamplePO_WithCustomHeaderOrder
        {
            get
            {
                using (Stream stream = s_assembly.GetManifestResourceStream($"{s_assembly.GetName().Name}.Resources.sample_withcustomheaderorder.po"))
                using (var ms = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] NewLineTestPO
        {
            get
            {
                using (Stream stream = s_assembly.GetManifestResourceStream($"{s_assembly.GetName().Name}.Resources.newlinetest.po"))
                using (var ms = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] EscapedCharTestPO
        {
            get
            {
                using (Stream stream = s_assembly.GetManifestResourceStream($"{s_assembly.GetName().Name}.Resources.escapedchartest.po"))
                using (var ms = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] InvalidControlCharTestPO
        {
            get
            {
                using (Stream stream = s_assembly.GetManifestResourceStream($"{s_assembly.GetName().Name}.Resources.invalidcontrolchartest.po"))
                using (var ms = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
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
