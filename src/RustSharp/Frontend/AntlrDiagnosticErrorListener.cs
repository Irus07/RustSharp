using Antlr4.Runtime;
using RustSharp.Core.Diagnostics;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Frontend;

/// <summary>
/// Перехватывает синтаксические и лексические ошибки ANTLR
/// и перенаправляет их в <see cref="CompilerDiagnostics"/>.
/// </summary>
public sealed class AntlrDiagnosticErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    private readonly CompilerDiagnostics _diagnostics;
    private readonly string? _filePath;

    public AntlrDiagnosticErrorListener(CompilerDiagnostics diagnostics, string? filePath = null)
    {
        _diagnostics = diagnostics;
        _filePath    = filePath;
    }

    // ── Ошибки парсера ───────────────────────────────────────────────────────

    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        var start = new SourceLocation(line, charPositionInLine + 1, offendingSymbol?.StartIndex ?? 0);
        int stopIdx = offendingSymbol?.StopIndex ?? offendingSymbol?.StartIndex ?? 0;
        var end   = new SourceLocation(line, charPositionInLine + 1 + Math.Max(1, stopIdx - (offendingSymbol?.StartIndex ?? 0) + 1), stopIdx);
        var span  = new SourceSpan(start, end, _filePath);

        _diagnostics.ReportError("P1", msg, span);
    }

    // ── Ошибки лексера ───────────────────────────────────────────────────────

    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        int offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        var loc  = new SourceLocation(line, charPositionInLine + 1, 0);
        var span = new SourceSpan(loc, loc, _filePath);

        _diagnostics.ReportError("L1", $"Лексическая ошибка: {msg}", span);
    }
}
