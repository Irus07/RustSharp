using RustSharp.Core.Diagnostics;

namespace RustSharp.Core.Pipeline;

/// <summary>
/// Неизменяемый контекст одного прогона компиляции.
/// Создаётся до запуска пайплайна и передаётся через все стадии.
/// </summary>
public sealed class CompilationContext
{
    /// <summary>Путь к исходному файлу (или <c>null</c> для строкового ввода).</summary>
    public string? FilePath { get; init; }

    /// <summary>Исходный текст программы.</summary>
    public string SourceText { get; init; }

    /// <summary>
    /// Централизированный реестр диагностических сообщений для этого прогона компиляции.
    /// Все стадии пишут в него через <c>context.Diagnostics.ReportError(...)</c>.
    /// </summary>
    public CompilerDiagnostics Diagnostics { get; } = new();

    /// <summary>
    /// Произвольные данные, которые стадии могут разделять между собой.
    /// Ключ — имя параметра, значение — произвольный объект.
    /// </summary>
    public Dictionary<string, object?> Properties { get; } = new();

    public CompilationContext(string sourceText, string? filePath = null)
    {
        SourceText = sourceText;
        FilePath   = filePath;
    }

    // ── Вспомогательные методы ───────────────────────────────────────────────

    /// <summary>Возвращает сохранённое свойство или <c>default</c> если отсутствует.</summary>
    public T? GetProperty<T>(string key) =>
        Properties.TryGetValue(key, out var value) ? (T?)value : default;

    /// <summary>Сохраняет произвольное свойство.</summary>
    public void SetProperty<T>(string key, T value) => Properties[key] = value;
}
