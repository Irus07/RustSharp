namespace RustSharp.Core.Frontend;

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Сообщение компилятора (ошибка, предупреждение или подсказка) с привязкой к исходному тексту.
/// </summary>
public sealed record Diagnostic(
    string Code,
    string Message,
    DiagnosticSeverity Severity,
    SourceSpan Span)
{
    public static Diagnostic Error(string code, string message, SourceSpan span) =>
        new(code, message, DiagnosticSeverity.Error, span);

    public static Diagnostic Warning(string code, string message, SourceSpan span) =>
        new(code, message, DiagnosticSeverity.Warning, span);

    public static Diagnostic Info(string code, string message, SourceSpan span) =>
        new(code, message, DiagnosticSeverity.Info, span);

    public override string ToString() =>
        $"[{Severity}] {Code}: {Message} at {Span}";
}

/// <summary>
/// Коллекция диагностических сообщений, накопленных в процессе компиляции.
/// </summary>
public sealed class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics = new();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    public bool HasErrors => _diagnostics.Exists(d => d.Severity == DiagnosticSeverity.Error);

    public void Report(Diagnostic diagnostic) => _diagnostics.Add(diagnostic);

    public void ReportError(string code, string message, SourceSpan span) =>
        Report(Diagnostic.Error(code, message, span));

    public void ReportWarning(string code, string message, SourceSpan span) =>
        Report(Diagnostic.Warning(code, message, span));

    public void Clear() => _diagnostics.Clear();
}
