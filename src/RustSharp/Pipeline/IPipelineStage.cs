namespace RustSharp.Core.Pipeline;

/// <summary>
/// Контракт одной стадии пайплайна компиляции.
/// Принимает <see cref="CompilationUnit{TIn}"/> и возвращает <see cref="CompilationUnit{TOut}"/>.
/// </summary>
/// <typeparam name="TIn">Входной тип нагрузки (например, <c>string</c> для исходного кода).</typeparam>
/// <typeparam name="TOut">Выходной тип нагрузки (например, <c>IList&lt;IToken&gt;</c>).</typeparam>
public interface IPipelineStage<TIn, TOut>
{
    /// <summary>Имя стадии для логирования и отображения.</summary>
    string StageName { get; }

    /// <summary>
    /// Выполняет стадию.
    /// При фатальной ошибке возвращает <c>null</c> — пайплайн прерывается.
    /// </summary>
    CompilationUnit<TOut>? Execute(CompilationUnit<TIn> input);
}
