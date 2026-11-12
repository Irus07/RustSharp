using Antlr4.Runtime;
using RustSharp.Core.Frontend.Generated;
using RustSharp.Core.Pipeline;

namespace RustSharp.Core.Frontend.Stages;

/// <summary>
/// Стадия лексического анализа пайплайна компиляции.
/// Вход:  <c>string</c> — исходный текст.
/// Выход: <c>IList&lt;IToken&gt;</c> — список токенов.
/// </summary>
public sealed class LexerStage : IPipelineStage<string, IList<IToken>>
{
    public string StageName => "Lexer";

    public CompilationUnit<IList<IToken>>? Execute(CompilationUnit<string> input)
    {
        var ctx = input.Context;
        var diag = ctx.Diagnostics;

        var errorListener = new AntlrDiagnosticErrorListener(diag, ctx.FilePath);

        var inputStream = new AntlrInputStream(input.Payload);
        if (!string.IsNullOrEmpty(ctx.FilePath))
            inputStream.name = ctx.FilePath;

        var lexer = new RustSharpLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);

        var tokenStream = new CommonTokenStream(lexer);
        tokenStream.Fill();

        var tokens = tokenStream.GetTokens();

        diag.ReportInfo("L0", $"Лексический анализ завершён: {tokens.Count} токенов");

        return input.WithPayload(tokens);
    }
}
