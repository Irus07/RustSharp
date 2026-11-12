using RustSharp.Core.Ast.Visitors;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Ast;

/// <summary>
/// Базовый интерфейс любого узла абстрактного синтаксического дерева (AST) RustSharp.
/// </summary>
public interface IAstNode
{
    /// <summary>Позиция узла в исходном коде.</summary>
    SourceSpan Span { get; }

    /// <summary>Принимает базовый посетитель (Visitor) без возвращаемого значения.</summary>
    void Accept(IAstVisitor visitor);

    /// <summary>Принимает типизированный посетитель (Visitor), возвращающий результат.</summary>
    TResult Accept<TResult>(IAstVisitor<TResult> visitor);

    /// <summary>Принимает типизированный посетитель (Visitor) с контекстом вызова.</summary>
    TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context);
}

/// <summary>
/// Базовый класс для узлов AST.
/// </summary>
public abstract class AstNode : IAstNode
{
    public SourceSpan Span { get; set; }

    protected AstNode(SourceSpan span = default)
    {
        Span = span;
    }

    public abstract void Accept(IAstVisitor visitor);
    public abstract TResult Accept<TResult>(IAstVisitor<TResult> visitor);
    public abstract TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context);
}
