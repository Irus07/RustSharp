using RustSharp.Core.Ast.Expressions;
using RustSharp.Core.Ast.Visitors;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Ast.Types;

/// <summary>
/// Базовый узел для типов данных.
/// </summary>
public abstract class TypeNode : AstNode
{
    protected TypeNode(SourceSpan span = default) : base(span) { }
}

/// <summary>
/// Примитивный тип (int, float, bool, str, void, byte, etc.).
/// </summary>
public sealed class PrimitiveTypeNode : TypeNode
{
    public string Name { get; }

    public PrimitiveTypeNode(string name, SourceSpan span = default) : base(span)
    {
        Name = name;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => Name;
}

/// <summary>
/// Пользовательский тип по квалифицированному имени (например, System::Console или MyClass).
/// </summary>
public sealed class CustomTypeNode : TypeNode
{
    public QualifiedNameNode Name { get; }

    public CustomTypeNode(QualifiedNameNode name, SourceSpan span = default) : base(span)
    {
        Name = name;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => Name.FullName;
}

/// <summary>
/// Массивный тип (например, int[], byte[][]).
/// </summary>
public sealed class ArrayTypeNode : TypeNode
{
    public TypeNode ElementType { get; }
    public int Dimensions { get; }

    public ArrayTypeNode(TypeNode elementType, int dimensions = 1, SourceSpan span = default) : base(span)
    {
        ElementType = elementType;
        Dimensions = dimensions;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"{ElementType}{string.Concat(Enumerable.Repeat("[]", Dimensions))}";
}

/// <summary>
/// Ссылочный тип (ref T, ref var T, &T, &var T, own T).
/// </summary>
public sealed class ReferenceTypeNode : TypeNode
{
    public ReferenceKind Kind { get; }
    public TypeNode InnerType { get; }

    public ReferenceTypeNode(ReferenceKind kind, TypeNode innerType, SourceSpan span = default) : base(span)
    {
        Kind = kind;
        InnerType = innerType;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => $"{Kind.ToString().ToLowerInvariant()} {InnerType}";
}
