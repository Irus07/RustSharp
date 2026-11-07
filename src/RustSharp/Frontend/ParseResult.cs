using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using RustSharp.Core.Frontend.Generated;

namespace RustSharp.Core.Frontend;

/// <summary>
/// Результат работы фронтенд-парсера: дерево разбора, диагностика и утилиты инспекции.
/// </summary>
public sealed class ParseResult
{
    public RustSharpParser.ProgramContext? Root { get; }
    public DiagnosticBag Diagnostics { get; }
    public string SourceCode { get; }
    public string? FilePath { get; }

    public bool IsSuccess => !Diagnostics.HasErrors && Root != null;

    public ParseResult(
        RustSharpParser.ProgramContext? root,
        DiagnosticBag diagnostics,
        string sourceCode,
        string? filePath = null)
    {
        Root = root;
        Diagnostics = diagnostics;
        SourceCode = sourceCode;
        FilePath = filePath;
    }

    /// <summary>
    /// Возвращает текстовое представление дерева разбора (Parse Tree) в структурированном виде.
    /// </summary>
    public string DumpTree(RustSharpParser parser)
    {
        if (Root == null)
            return "<пустое дерево разбора>";

        var sb = new StringBuilder();
        DumpNode(Root, parser, 0, sb);
        return sb.ToString();
    }

    private static void DumpNode(ITree node, Parser parser, int indent, StringBuilder sb)
    {
        string indentStr = new string(' ', indent * 2);
        string nodeText = Trees.GetNodeText(node, parser);

        sb.Append(indentStr).AppendLine(nodeText);

        for (int i = 0; i < node.ChildCount; i++)
        {
            DumpNode(node.GetChild(i), parser, indent + 1, sb);
        }
    }
}
