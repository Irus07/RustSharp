using RustSharp.Core.Diagnostics;

namespace RustSharp.Core.Pipeline;

/// <summary>
/// Результат выполнения пайплайна компиляции.
/// </summary>
/// <typeparam name="TOut">Тип финального результата (например, дерево разбора).</typeparam>
public sealed class PipelineResult<TOut>
{
    /// <summary>Финальный продукт пайплайна или <c>null</c> если выполнение прервалось.</summary>
    public TOut? Output { get; }

    /// <summary>Диагностические сообщения, накопленные всеми стадиями.</summary>
    public CompilerDiagnostics Diagnostics { get; }

    /// <summary>Имя стадии, на которой произошёл сбой (<c>null</c> если успешно).</summary>
    public string? FailedAt { get; }

    /// <summary>Возвращает <c>true</c> если пайплайн завершился без ошибок.</summary>
    public bool IsSuccess => !Diagnostics.HasErrors && Output is not null;

    public PipelineResult(TOut? output, CompilerDiagnostics diagnostics, string? failedAt = null)
    {
        Output      = output;
        Diagnostics = diagnostics;
        FailedAt    = failedAt;
    }
}
