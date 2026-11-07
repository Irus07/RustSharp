using Antlr4.Runtime;
using RustSharp.Core.Frontend.Generated;

namespace RustSharp.Core.Frontend;

/// <summary>
/// Главная точка входа во фронтенд компилятора RustSharp.
/// Преобразует исходный код в дерево синтаксического разбора (Parse Tree).
/// </summary>
public static class CompilerFrontend
{
    /// <summary>
    /// Выполняет синтаксический анализ исходного кода RustSharp.
    /// </summary>
    /// <param name="source">Исходный код программы.</param>
    /// <param name="filePath">Опциональный путь к исходному файлу для диагностики.</param>
    /// <returns>Результат парсинга, содержащий корень дерева разбора и диагностику.</returns>
    public static ParseResult Parse(string source, string? filePath = null)
    {
        var diagnostics = new DiagnosticBag();
        var errorListener = new AntlrDiagnosticErrorListener(diagnostics, filePath);

        var inputStream = new AntlrInputStream(source);
        if (!string.IsNullOrEmpty(filePath))
        {
            inputStream.name = filePath;
        }

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
            diagnostics.ReportError("RS0003", $"Критическая ошибка разбора: {ex.Message}", SourceSpan.None);
        }

        return new ParseResult(root, diagnostics, source, filePath);
    }

    /// <summary>
    /// Загружает и анализирует файл с исходным кодом RustSharp.
    /// </summary>
    public static ParseResult ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var bag = new DiagnosticBag();
            bag.ReportError("RS0004", $"Файл не найден: {filePath}", SourceSpan.None);
            return new ParseResult(null, bag, string.Empty, filePath);
        }

        string source = File.ReadAllText(filePath);
        return Parse(source, filePath);
    }

    /// <summary>
    /// Выполняет лексический анализ (токенизацию) исходного кода.
    /// </summary>
    public static IList<IToken> Tokenize(string source, DiagnosticBag? diagnostics = null)
    {
        var bag = diagnostics ?? new DiagnosticBag();
        var errorListener = new AntlrDiagnosticErrorListener(bag);

        var inputStream = new AntlrInputStream(source);
        var lexer = new RustSharpLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);

        var tokenStream = new CommonTokenStream(lexer);
        tokenStream.Fill();

        return tokenStream.GetTokens();
    }
}
