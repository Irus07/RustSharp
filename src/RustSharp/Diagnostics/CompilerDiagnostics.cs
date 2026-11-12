using RustSharp.Core.Frontend;

namespace RustSharp.Core.Diagnostics;

/// <summary>
/// Централизированный API для сбора диагностических сообщений компилятора.
/// Содержит три независимых потока: <see cref="Info"/>, <see cref="Warnings"/>, <see cref="Errors"/>.
/// </summary>
/// <remarks>
/// Пример использования:
/// <code>
/// var diag = new CompilerDiagnostics();
/// diag.ReportError("P1", "Не удалось распарсить выражение", span);
/// // => ERR P1 "Не удалось распарсить выражение" at 1:5..1:12
///
/// diag.Errors.Subscribe(msg => Console.Error.WriteLine(msg));
/// </code>
/// </remarks>
public sealed class CompilerDiagnostics
{
    // ── Три стандартных потока ───────────────────────────────────────────────

    /// <summary>Поток информационных сообщений (INFO).</summary>
    public DiagnosticStream Info     { get; } = new(DiagnosticSeverity.Info);

    /// <summary>Поток предупреждений (WARN).</summary>
    public DiagnosticStream Warnings { get; } = new(DiagnosticSeverity.Warning);

    /// <summary>Поток ошибок компиляции (ERR).</summary>
    public DiagnosticStream Errors   { get; } = new(DiagnosticSeverity.Error);

    // ── Агрегирующие свойства ────────────────────────────────────────────────

    /// <summary>Возвращает <c>true</c>, если накоплена хотя бы одна ошибка.</summary>
    public bool HasErrors => Errors.HasMessages;

    /// <summary>Возвращает <c>true</c>, если накоплено хотя бы одно предупреждение.</summary>
    public bool HasWarnings => Warnings.HasMessages;

    /// <summary>Суммарное количество сообщений во всех потоках.</summary>
    public int TotalCount => Info.Count + Warnings.Count + Errors.Count;

    /// <summary>Все сообщения из всех потоков в порядке добавления (пересборка при каждом вызове).</summary>
    public IEnumerable<CompilerDiagnostic> All =>
        Info.Messages.Concat(Warnings.Messages).Concat(Errors.Messages);

    // ── Запись через единый API ──────────────────────────────────────────────

    /// <summary>Записывает произвольное диагностическое сообщение в соответствующий поток.</summary>
    public void Report(CompilerDiagnostic diagnostic)
    {
        var stream = diagnostic.Severity switch
        {
            DiagnosticSeverity.Info    => Info,
            DiagnosticSeverity.Warning => Warnings,
            DiagnosticSeverity.Error   => Errors,
            _                          => Errors
        };
        stream.Write(diagnostic);
    }

    /// <summary>Записывает ошибку. Формат: <c>ERR {code} "{message}"</c>.</summary>
    public void ReportError(string code, string message, SourceSpan span = default)
        => Report(CompilerDiagnostic.Error(code, message, span));

    /// <summary>Записывает предупреждение. Формат: <c>WARN {code} "{message}"</c>.</summary>
    public void ReportWarning(string code, string message, SourceSpan span = default)
        => Report(CompilerDiagnostic.Warn(code, message, span));

    /// <summary>Записывает информационное сообщение. Формат: <c>INFO {code} "{message}"</c>.</summary>
    public void ReportInfo(string code, string message, SourceSpan span = default)
        => Report(CompilerDiagnostic.Info(code, message, span));

    // ── Слияние ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Переносит все сообщения из <paramref name="other"/> в текущий экземпляр.
    /// Используется для агрегации диагностики отдельных стадий пайплайна.
    /// </summary>
    public void MergeFrom(CompilerDiagnostics other)
    {
        foreach (var msg in other.Info.Messages)     Info.Write(msg);
        foreach (var msg in other.Warnings.Messages) Warnings.Write(msg);
        foreach (var msg in other.Errors.Messages)   Errors.Write(msg);
    }

    /// <summary>Очищает все потоки.</summary>
    public void Clear()
    {
        Info.Clear();
        Warnings.Clear();
        Errors.Clear();
    }

    // ── Вывод сводки ─────────────────────────────────────────────────────────

    /// <summary>
    /// Возвращает краткую сводку: <c>ERR: 2, WARN: 1, INFO: 0</c>.
    /// </summary>
    public string Summary()
        => $"ERR: {Errors.Count}, WARN: {Warnings.Count}, INFO: {Info.Count}";
}
