using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Karambolo.PO
{
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
        public string Message => string.Format(GetMessageFormat(), Args);

        protected abstract string GetMessageFormat();
    }

    public interface IDiagnostics : IReadOnlyList<Diagnostic>
    {
        bool HasWarning { get; }
        bool HasError { get; }
    }

    class DiagnosticCollection : Collection<Diagnostic>, IDiagnostics
    {
        public bool HasWarning => this.Any(d => d.Severity == DiagnosticSeverity.Warning);
        public bool HasError => this.Any(d => d.Severity == DiagnosticSeverity.Error);
    }
}
