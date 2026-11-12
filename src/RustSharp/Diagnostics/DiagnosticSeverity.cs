namespace RustSharp.Core.Diagnostics;

/// <summary>
/// Уровень важности диагностического сообщения.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>Информационное сообщение (INFO).</summary>
    Info,

    /// <summary>Предупреждение (WARN).</summary>
    Warning,

    /// <summary>Ошибка компиляции (ERR).</summary>
    Error
}
