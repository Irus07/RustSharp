using Antlr4.Runtime;
using RustSharp.Core.Frontend.Generated;
using RustSharp.Core.Pipeline;

namespace RustSharp.Core.Frontend.Stages;

/// <summary>
/// Стадия синтаксического анализа пайплайна компиляции.
/// Вход:  <c>string</c> — исходный текст (перелексирует сам).
/// Выход: <see cref="ParseResult"/> — корень синтаксического дерева + диагностика.
/// </summary>
/// <remarks>
/// Принимает <c>string</c> напрямую, чтобы парсер мог создать собственный
/// <see cref="CommonTokenStream"/> с токенами лексера.
/// Для цепочки Lexer → Parser используйте <see cref="ParserFromTokensStage"/>.
/// </remarks>
public sealed class ParserStage : IPipelineStage<string, ParseResult>
{
    public string StageName => "Parser";

    public CompilationUnit<ParseResult>? Execute(CompilationUnit<string> input)
    {
        var ctx  = input.Context;
        var diag = ctx.Diagnostics;

        var errorListener = new AntlrDiagnosticErrorListener(diag, ctx.FilePath);

        var inputStream = new AntlrInputStream(input.Payload);
        if (!string.IsNullOrEmpty(ctx.FilePath))
            inputStream.name = ctx.FilePath;

        var lexer = new RustSharpLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);

        var tokenStream = new CommonTokenStream(lexer);

        var parser = new RustSharpParser(tokenStream);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        RustSharpParser.ProgramContext? root = null;
        try
        {
            root = parser.program();
        }
        catch (RecognitionException ex)
        {
            diag.ReportError("P0", $"Критическая ошибка разбора: {ex.Message}");
        }

        if (root is null)
        {
            diag.ReportError("P1", "Синтаксический анализ не вернул дерево разбора");
            return null;
        }

        var result = new ParseResult(root, diag, input.Payload, ctx.FilePath);
        diag.ReportInfo("P0", "Синтаксический анализ завершён");

        return input.WithPayload(result);
    }
}
