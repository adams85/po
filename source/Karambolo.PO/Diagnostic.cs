using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Karambolo.PO
{
#if NET40
    using Karambolo.Common.Collections;
#endif

    public enum DiagnosticSeverity
    {
        Unknown,
        Information,
        Warning,
        Error,
    }

    public abstract class Diagnostic
    {
        public Diagnostic(DiagnosticSeverity severity, string code, params object[] args)
        {
            Severity = severity;
            Code = code;
            Args = args;
        }

        public DiagnosticSeverity Severity { get; }
        public string Code { get; }
        public object[] Args { get; }

        [Obsolete("This property is redundant, thus it will be removed in the next major version. Use the ToString method instead.")]
        public string Message => ToString();

        protected abstract string GetMessageFormat();

        public override string ToString() => string.Format(GetMessageFormat(), Args);
    }

    public interface IDiagnostics : IReadOnlyList<Diagnostic>
    {
        bool HasWarning { get; }
        bool HasError { get; }
    }

    internal class DiagnosticCollection : Collection<Diagnostic>, IDiagnostics
    {
        public bool HasWarning => this.Any(d => d.Severity == DiagnosticSeverity.Warning);
        public bool HasError => this.Any(d => d.Severity == DiagnosticSeverity.Error);
    }
}
