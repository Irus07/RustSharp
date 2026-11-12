using RustSharp.Core.Ast.Declarations;
using RustSharp.Core.Frontend;
using RustSharp.Core.Pipeline;

namespace RustSharp.Core.Ast.Stages;

/// <summary>
/// Стадия генерации абстрактного синтаксического дерева (AST) из дерева синтаксического разбора (Parse Tree).
/// Вход:  <see cref="ParseResult"/> — результат синтаксического анализа.
/// Выход: <see cref="ProgramNode"/> — типизированный корень AST.
/// </summary>
public sealed class AstGenerationStage : IPipelineStage<ParseResult, ProgramNode>
{
    public string StageName => "AstGeneration";

    public CompilationUnit<ProgramNode>? Execute(CompilationUnit<ParseResult> input)
    {
        var parseResult = input.Payload;
        var ctx = input.Context;
        var diag = ctx.Diagnostics;

        if (parseResult.Root is null)
        {
            diag.ReportError("A1", "Не удалось построить AST: отсутствует дерево синтаксического разбора (Parse Tree)");
            return null;
        }

        var visitor = new AstBuilderVisitor(diag, ctx.FilePath);
        var programNode = visitor.VisitProgram(parseResult.Root) as ProgramNode;

        if (programNode is null)
        {
            diag.ReportError("A2", "Не удалось построить корневой узел AST программы");
            return null;
        }

        diag.ReportInfo("A0", $"Генерация AST завершена: {programNode.Declarations.Count} объявлений верхнего уровня");

        return input.WithPayload(programNode);
    }
}
