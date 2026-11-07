using Antlr4.Runtime;

namespace RustSharp.Core.Frontend;

/// <summary>
/// Перехватывает синтаксические и лексические ошибки ANTLR и преобразует их в Diagnositic.
/// </summary>
public sealed class AntlrDiagnosticErrorListener : BaseErrorListener, IAntlrErrorListener<int>
{
    private readonly DiagnosticBag _diagnostics;
    private readonly string? _filePath;

    public AntlrDiagnosticErrorListener(DiagnosticBag diagnostics, string? filePath = null)
    {
        _diagnostics = diagnostics;
        _filePath = filePath;
    }

    // Ошибки парсера
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
        var stopIndex = offendingSymbol?.StopIndex ?? offendingSymbol?.StartIndex ?? 0;
        var end = new SourceLocation(line, charPositionInLine + 1 + Math.Max(1, stopIndex - (offendingSymbol?.StartIndex ?? 0) + 1), stopIndex);
        var span = new SourceSpan(start, end, _filePath);

        _diagnostics.ReportError("RS0001", msg, span);
    }

    // Ошибки лексера
    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        int offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        var loc = new SourceLocation(line, charPositionInLine + 1, 0);
        var span = new SourceSpan(loc, loc, _filePath);

        _diagnostics.ReportError("RS0002", $"Лексическая ошибка: {msg}", span);
    }
}
