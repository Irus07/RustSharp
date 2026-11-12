using RustSharp.Core.Diagnostics;

namespace RustSharp.Core.Pipeline;

/// <summary>
/// Строитель типизированного пайплайна компиляции.
/// Позволяет соединять стадии в цепочку через fluent API.
/// </summary>
/// <remarks>
/// Пример сборки пайплайна:
/// <code>
/// var result = PipelineBuilder
///     .Start(context)
///     .Then(new LexerStage())
///     .Then(new MacroProcessorStage())
///     .Then(new ParserStage())
///     .Run();
/// </code>
/// </remarks>
public static class PipelineBuilder
{
    /// <summary>Начинает построение пайплайна из исходного текста программы.</summary>
    public static Pipeline<string> Start(CompilationContext context)
    {
        var unit = new CompilationUnit<string>(context.SourceText, context);
        return new Pipeline<string>(unit);
    }
}

/// <summary>
/// Исполнитель пайплайна, несущий текущее состояние (единицу компиляции).
/// </summary>
/// <typeparam name="TCurrent">Тип текущей нагрузки.</typeparam>
public sealed class Pipeline<TCurrent>
{
    private readonly CompilationUnit<TCurrent>? _current;
    private readonly string? _failedStageName;

    internal Pipeline(CompilationUnit<TCurrent>? current, string? failedStageName = null)
    {
        _current         = current;
        _failedStageName = failedStageName;
    }

    /// <summary>
    /// Присоединяет следующую стадию пайплайна.
    /// Если предыдущая стадия вернула <c>null</c>, эта стадия пропускается.
    /// </summary>
    public Pipeline<TNext> Then<TNext>(IPipelineStage<TCurrent, TNext> stage)
    {
        // Пайплайн уже прерван — пропускаем стадию
        if (_current is null)
        {
            var ctx = new CompilationContext("", null);
            return new Pipeline<TNext>(null, _failedStageName);
        }

        // Если уже есть ошибки и стадия не помечена как "продолжать при ошибках" — прерываем
        if (_current.Context.Diagnostics.HasErrors)
        {
            return new Pipeline<TNext>(null, stage.StageName);
        }

        var result = stage.Execute(_current);
        return result is null
            ? new Pipeline<TNext>(null, stage.StageName)
            : new Pipeline<TNext>(result);
    }

    /// <summary>
    /// Присоединяет стадию, которая выполняется даже при наличии ошибок.
    /// Используется для стадий анализа и сбора диагностики.
    /// </summary>
    public Pipeline<TNext> ThenAlways<TNext>(IPipelineStage<TCurrent, TNext> stage)
    {
        if (_current is null)
            return new Pipeline<TNext>(null, _failedStageName);

        var result = stage.Execute(_current);
        return result is null
            ? new Pipeline<TNext>(null, stage.StageName)
            : new Pipeline<TNext>(result);
    }

    /// <summary>Завершает пайплайн и возвращает итоговый результат.</summary>
    public PipelineResult<TCurrent> Run()
    {
        var diagnostics = _current?.Context.Diagnostics ?? new CompilerDiagnostics();
        TCurrent? payload = _current is not null ? _current.Payload : default;
        return new PipelineResult<TCurrent>(payload, diagnostics, _failedStageName);
    }
}
