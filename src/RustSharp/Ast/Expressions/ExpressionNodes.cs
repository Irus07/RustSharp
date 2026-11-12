using RustSharp.Core.Ast.Types;
using RustSharp.Core.Ast.Visitors;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Ast.Expressions;

/// <summary>
/// Базовый класс для всех выражений в RustSharp.
/// </summary>
public abstract class ExpressionNode : AstNode
{
    protected ExpressionNode(SourceSpan span = default) : base(span) { }
}

/// <summary>
/// Литерал (числовой, строковый, булевый, символьный).
/// </summary>
public sealed class LiteralExpressionNode : ExpressionNode
{
    public LiteralKind Kind { get; }
    public object? Value { get; }
    public string RawText { get; }

    public LiteralExpressionNode(LiteralKind kind, object? value, string rawText, SourceSpan span = default)
        : base(span)
    {
        Kind = kind;
        Value = value;
        RawText = rawText;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => RawText;
}

/// <summary>
/// Идентификатор (имя переменной, поля, параметра).
/// </summary>
public sealed class IdentifierExpressionNode : ExpressionNode
{
    public string Name { get; }

    public IdentifierExpressionNode(string name, SourceSpan span = default) : base(span)
    {
        Name = name;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => Name;
}

/// <summary>
/// Квалифицированное имя (например, std::io::println или MyModule::Item).
/// </summary>
public sealed class QualifiedNameNode : ExpressionNode
{
    public IReadOnlyList<string> Parts { get; }
    public string FullName => string.Join("::", Parts);

    public QualifiedNameNode(IReadOnlyList<string> parts, SourceSpan span = default) : base(span)
    {
        Parts = parts;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => FullName;
}

/// <summary>
/// Бинарная операция (left op right).
/// </summary>
public sealed class BinaryExpressionNode : ExpressionNode
{
    public ExpressionNode Left { get; }
    public BinaryOperator Operator { get; }
    public ExpressionNode Right { get; }

    public BinaryExpressionNode(
        ExpressionNode left,
        BinaryOperator op,
        ExpressionNode right,
        SourceSpan span = default) : base(span)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"({Left} {Operator} {Right})";
}

/// <summary>
/// Унарная операция (+x, -x, !x, ref x, ref var x, own x).
/// </summary>
public sealed class UnaryExpressionNode : ExpressionNode
{
    public UnaryOperator Operator { get; }
    public ExpressionNode Operand { get; }

    public UnaryExpressionNode(UnaryOperator op, ExpressionNode operand, SourceSpan span = default)
        : base(span)
    {
        Operator = op;
        Operand = operand;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"({Operator}{Operand})";
}

/// <summary>
/// Аргумент при вызове функции или метода.
/// </summary>
public sealed class ArgumentNode : AstNode
{
    public ArgumentModifier Modifier { get; }
    public ExpressionNode Expression { get; }

    public ArgumentNode(ArgumentModifier modifier, ExpressionNode expression, SourceSpan span = default)
        : base(span)
    {
        Modifier = modifier;
        Expression = expression;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => Modifier == ArgumentModifier.None
        ? Expression.ToString()!
        : $"{Modifier.ToString().ToLowerInvariant()} {Expression}";
}

/// <summary>
/// Вызов функции или метода: callee(args).
/// </summary>
public sealed class InvocationExpressionNode : ExpressionNode
{
    public ExpressionNode Callee { get; }
    public IReadOnlyList<ArgumentNode> Arguments { get; }

    public InvocationExpressionNode(
        ExpressionNode callee,
        IReadOnlyList<ArgumentNode> arguments,
        SourceSpan span = default) : base(span)
    {
        Callee = callee;
        Arguments = arguments;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"{Callee}({string.Join(", ", Arguments)})";
}

/// <summary>
/// Обращение к члену: target.memberName.
/// </summary>
public sealed class MemberAccessExpressionNode : ExpressionNode
{
    public ExpressionNode Target { get; }
    public string MemberName { get; }

    public MemberAccessExpressionNode(ExpressionNode target, string memberName, SourceSpan span = default)
        : base(span)
    {
        Target = target;
        MemberName = memberName;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"{Target}.{MemberName}";
}

/// <summary>
/// Доступ по индексу: target[index].
/// </summary>
public sealed class IndexExpressionNode : ExpressionNode
{
    public ExpressionNode Target { get; }
    public ExpressionNode Index { get; }

    public IndexExpressionNode(ExpressionNode target, ExpressionNode index, SourceSpan span = default)
        : base(span)
    {
        Target = target;
        Index = index;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"{Target}[{Index}]";
}

/// <summary>
/// Выделение памяти на стеке: stackalloc Type[size].
/// </summary>
public sealed class StackAllocExpressionNode : ExpressionNode
{
    public TypeNode ElementType { get; }
    public ExpressionNode Size { get; }

    public StackAllocExpressionNode(TypeNode elementType, ExpressionNode size, SourceSpan span = default)
        : base(span)
    {
        ElementType = elementType;
        Size = size;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"stackalloc {ElementType}[{Size}]";
}

/// <summary>
/// Литерал массива: [elem1, elem2, ...].
/// </summary>
public sealed class ArrayLiteralExpressionNode : ExpressionNode
{
    public IReadOnlyList<ExpressionNode> Elements { get; }

    public ArrayLiteralExpressionNode(IReadOnlyList<ExpressionNode> elements, SourceSpan span = default)
        : base(span)
    {
        Elements = elements;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"[{string.Join(", ", Elements)}]";
}
