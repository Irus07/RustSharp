using Antlr4.Runtime;
using RustSharp.Core.Frontend.Generated;
using RustSharp.Core.Pipeline;

namespace RustSharp.Core.Frontend.Stages;

/// <summary>
/// Стадия синтаксического анализа на основе уже готового списка токенов.
/// Вход:  <c>IList&lt;IToken&gt;</c> — список токенов (результат <see cref="LexerStage"/>).
/// Выход: <see cref="ParseResult"/> — корень синтаксического дерева.
/// </summary>
public sealed class ParserFromTokensStage : IPipelineStage<IList<IToken>, ParseResult>
{
    public string StageName => "Parser";

    public CompilationUnit<ParseResult>? Execute(CompilationUnit<IList<IToken>> input)
    {
        var ctx  = input.Context;
        var diag = ctx.Diagnostics;

        var errorListener = new AntlrDiagnosticErrorListener(diag, ctx.FilePath);

        // Восстанавливаем поток из готового списка токенов
        var tokenStream = new ListTokenSource(input.Payload);
        var commonStream = new CommonTokenStream(tokenStream);

        var parser = new RustSharpParser(commonStream);
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

        var result = new ParseResult(root, diag, ctx.SourceText, ctx.FilePath);
        diag.ReportInfo("P0", "Синтаксический анализ завершён");

        return input.WithPayload(result);
    }
}
