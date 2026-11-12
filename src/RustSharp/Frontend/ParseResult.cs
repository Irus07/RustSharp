using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using RustSharp.Core.Diagnostics;
using RustSharp.Core.Frontend.Generated;

namespace RustSharp.Core.Frontend;

/// <summary>
/// Результат работы фронтенд-парсера: дерево разбора, диагностика и утилиты инспекции.
/// </summary>
public sealed class ParseResult
{
    public RustSharpParser.ProgramContext? Root { get; }

    /// <summary>
    /// Диагностика, накопленная за время парсинга.
    /// Ссылается на <see cref="CompilerDiagnostics"/> контекста компиляции.
    /// </summary>
    public CompilerDiagnostics Diagnostics { get; }

    public string SourceCode { get; }
    public string? FilePath  { get; }

    public bool IsSuccess => !Diagnostics.HasErrors && Root is not null;

    public ParseResult(
        RustSharpParser.ProgramContext? root,
        CompilerDiagnostics diagnostics,
        string sourceCode,
        string? filePath = null)
    {
        Root        = root;
        Diagnostics = diagnostics;
        SourceCode  = sourceCode;
        FilePath    = filePath;
    }

    // ── DumpTree ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Возвращает структурированное визуальное представление дерева разбора (Parse Tree).
    /// </summary>
    public string DumpTree()
    {
        if (Root is null)
            return "<пустое дерево разбора>";

        var sb = new StringBuilder();
        DumpNode(Root, "", true, sb);
        return sb.ToString().TrimEnd();
    }

    /// <summary>Перегрузка для обратной совместимости.</summary>
    public string DumpTree(RustSharpParser? parser) => DumpTree();

    private static void DumpNode(ITree node, string prefix, bool isTail, StringBuilder sb)
    {
        string nodeName;

        if (node is IErrorNode errorNode)
        {
            nodeName = $"<ErrorNode: {errorNode.Symbol?.Text ?? "UNKNOWN"}>";
        }
        else if (node is RuleContext rc)
        {
            int ruleIndex = rc.RuleIndex;
            string ruleName = (ruleIndex >= 0 && ruleIndex < RustSharpParser.ruleNames.Length)
                ? RustSharpParser.ruleNames[ruleIndex]
                : node.GetType().Name;
            nodeName = $"[{ruleName}]";
        }
        else if (node is ITerminalNode tn)
        {
            string text = tn.Symbol.Text.Replace("\r", "\\r").Replace("\n", "\\n");
            nodeName = $"'{text}'";
        }
        else
        {
            nodeName = node.ToString() ?? "";
        }

        sb.Append(prefix).AppendLine(nodeName);

        int count = node.ChildCount;
        for (int i = 0; i < count; i++)
        {
            bool childIsLast = (i == count - 1);
            string newPrefix = prefix;

            if (prefix.EndsWith("└── "))
                newPrefix = prefix[..^4] + "    ";
            else if (prefix.EndsWith("├── "))
                newPrefix = prefix[..^4] + "│   ";

            string branch = childIsLast ? "└── " : "├── ";
            DumpNode(node.GetChild(i), newPrefix + branch, childIsLast, sb);
        }
    }
}
