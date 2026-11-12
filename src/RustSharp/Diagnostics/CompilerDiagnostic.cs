using RustSharp.Core.Frontend;

namespace RustSharp.Core.Diagnostics;

/// <summary>
/// Иммутабельное диагностическое сообщение компилятора.
/// Формат строки: <c>WARN P1 "Не удалось распарсить ..." at file(1:5..1:12)</c>
/// </summary>
/// <param name="Severity">Уровень важности (Info / Warning / Error).</param>
/// <param name="Code">Код ошибки, например <c>P1</c>, <c>L2</c>, <c>C01</c>.</param>
/// <param name="Message">Описание проблемы.</param>
/// <param name="Span">Позиция в исходном тексте (необязательна).</param>
public sealed record CompilerDiagnostic(
    DiagnosticSeverity Severity,
    string Code,
    string Message,
    SourceSpan Span)
{
    // ── Удобные фабрики ──────────────────────────────────────────────────────

    public static CompilerDiagnostic Info(string code, string message, SourceSpan span = default)
        => new(DiagnosticSeverity.Info, code, message, span);

    public static CompilerDiagnostic Warn(string code, string message, SourceSpan span = default)
        => new(DiagnosticSeverity.Warning, code, message, span);

    public static CompilerDiagnostic Error(string code, string message, SourceSpan span = default)
        => new(DiagnosticSeverity.Error, code, message, span);

    // ── Форматирование ───────────────────────────────────────────────────────

    private string SeverityTag => Severity switch
    {
        DiagnosticSeverity.Info    => "INFO",
        DiagnosticSeverity.Warning => "WARN",
        DiagnosticSeverity.Error   => "ERR",
        _                          => "DIAG"
    };

    /// <summary>
    /// Возвращает строку вида <c>WARN P1 "Не удалось распарсить ..." at 1:5..1:12</c>.
    /// </summary>
    public override string ToString()
    {
        string location = Span == default ? "" : $" at {Span}";
        return $"{SeverityTag} {Code} \"{Message}\"{location}";
    }
}
