using Antlr4.Runtime;
using RustSharp.Core.Ast.Declarations;
using RustSharp.Core.Ast.Stages;
using RustSharp.Core.Diagnostics;
using RustSharp.Core.Frontend.Generated;
using RustSharp.Core.Frontend.Stages;
using RustSharp.Core.Pipeline;

namespace RustSharp.Core.Frontend;

/// <summary>
/// Удобный фасад над модульным пайплайном компиляции.
/// Предоставляет статические методы для быстрого разбора без ручной сборки пайплайна.
/// </summary>
/// <remarks>
/// Для гибкой настройки используйте <see cref="PipelineBuilder"/> напрямую:
/// <code>
/// var context = new CompilationContext(source, "file.rs");
/// var result  = PipelineBuilder
///                 .Start(context)
///                 .Then(new LexerStage())
///                 .Then(new ParserFromTokensStage())
///                 .Then(new AstGenerationStage())
///                 .Run();
/// </code>
/// </remarks>
public static class CompilerFrontend
{
    // ── Быстрый разбор в дерево разбора (Parse Tree) ─────────────────────────

    /// <summary>
    /// Выполняет лексический и синтаксический анализ исходного кода.
    /// Использует стандартный пайплайн: Lexer → Parser.
    /// </summary>
    public static ParseResult Parse(string source, string? filePath = null)
    {
        var context = new CompilationContext(source, filePath);

        var pipelineResult = PipelineBuilder
            .Start(context)
            .Then(new LexerStage())
            .Then(new ParserFromTokensStage())
            .Run();

        // Если пайплайн вернул результат — возвращаем его
        if (pipelineResult.Output is { } parseResult)
            return parseResult;

        // Пайплайн прервался — возвращаем пустой результат с диагностикой
        return new ParseResult(null, context.Diagnostics, source, filePath);
    }

    /// <summary>
    /// Загружает и анализирует файл с исходным кодом.
    /// </summary>
    public static ParseResult ParseFile(string filePath)
    {
        var diag = new CompilerDiagnostics();

        if (!File.Exists(filePath))
        {
            diag.ReportError("IO1", $"Файл не найден: {filePath}");
            return new ParseResult(null, diag, string.Empty, filePath);
        }

        string source = File.ReadAllText(filePath);
        return Parse(source, filePath);
    }

    // ── Разбор в абстрактное синтаксическое дерево (AST) ─────────────────────

    /// <summary>
    /// Выполняет полный разбор до абстрактного синтаксического дерева (AST).
    /// Цепочка стадий: Lexer → Parser → AstGeneration.
    /// </summary>
    public static PipelineResult<ProgramNode> ParseToAst(string source, string? filePath = null)
    {
        var context = new CompilationContext(source, filePath);

        return PipelineBuilder
            .Start(context)
            .Then(new LexerStage())
            .Then(new ParserFromTokensStage())
            .Then(new AstGenerationStage())
            .Run();
    }

    /// <summary>
    /// Выполняет разбор до AST и возвращает корневой узел ProgramNode (или null при ошибках).
    /// </summary>
    public static ProgramNode? BuildAst(string source, string? filePath = null, CompilerDiagnostics? diagnostics = null)
    {
        var result = ParseToAst(source, filePath);
        if (diagnostics is not null)
            diagnostics.MergeFrom(result.Diagnostics);
        return result.Output;
    }

    // ── Только лексический анализ ────────────────────────────────────────────

    /// <summary>
    /// Выполняет только лексический анализ и возвращает список токенов.
    /// </summary>
    public static IList<IToken> Tokenize(string source, CompilerDiagnostics? diagnostics = null)
    {
        var context = new CompilationContext(source);

        var pipelineResult = PipelineBuilder
            .Start(context)
            .Then(new LexerStage())
            .Run();

        // Переносим диагностику, если передан внешний приёмник
        if (diagnostics is not null)
            diagnostics.MergeFrom(context.Diagnostics);

        return pipelineResult.Output ?? Array.Empty<IToken>();
    }
}
