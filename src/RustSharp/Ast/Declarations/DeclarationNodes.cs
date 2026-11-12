using RustSharp.Core.Ast.Expressions;
using RustSharp.Core.Ast.Statements;
using RustSharp.Core.Ast.Types;
using RustSharp.Core.Ast.Visitors;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Ast.Declarations;

/// <summary>
/// Базовый класс для всех объявлений в RustSharp.
/// </summary>
public abstract class DeclarationNode : AstNode
{
    protected DeclarationNode(SourceSpan span = default) : base(span) { }
}

/// <summary>
/// Корневой узел программы: модуль, импорты и объявления верхнего уровня.
/// </summary>
public sealed class ProgramNode : AstNode
{
    public ModuleDeclarationNode? Module { get; }
    public IReadOnlyList<ImportDeclarationNode> Imports { get; }
    public IReadOnlyList<DeclarationNode> Declarations { get; }

    public ProgramNode(
        ModuleDeclarationNode? module,
        IReadOnlyList<ImportDeclarationNode> imports,
        IReadOnlyList<DeclarationNode> declarations,
        SourceSpan span = default) : base(span)
    {
        Module = module;
        Imports = imports;
        Declarations = declarations;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Объявление модуля: module Name::Space;
/// </summary>
public sealed class ModuleDeclarationNode : DeclarationNode
{
    public QualifiedNameNode Name { get; }

    public ModuleDeclarationNode(QualifiedNameNode name, SourceSpan span = default) : base(span)
    {
        Name = name;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Директива импорта: import Name::Space;
/// </summary>
public sealed class ImportDeclarationNode : DeclarationNode
{
    public QualifiedNameNode Name { get; }

    public ImportDeclarationNode(QualifiedNameNode name, SourceSpan span = default) : base(span)
    {
        Name = name;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Параметр функции или метода: modifier? name: type
/// </summary>
public sealed class ParameterNode : AstNode
{
    public ParameterModifier Modifier { get; }
    public string Name { get; }
    public TypeNode Type { get; }

    public ParameterNode(
        ParameterModifier modifier,
        string name,
        TypeNode type,
        SourceSpan span = default) : base(span)
    {
        Modifier = modifier;
        Name = name;
        Type = type;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public override string ToString() => Modifier == ParameterModifier.None
        ? $"{Name}: {Type}"
        : $"{Modifier.ToString().ToLowerInvariant()} {Name}: {Type}";
}

/// <summary>
/// Объявление функции: modifiers fn name(params) -> returnType { body }
/// </summary>
public sealed class FunctionDeclarationNode : DeclarationNode
{
    public FunctionModifiers Modifiers { get; }
    public string Name { get; }
    public IReadOnlyList<ParameterNode> Parameters { get; }
    public TypeNode? ReturnType { get; }
    public BlockStatementNode Body { get; }

    public bool IsPublic => (Modifiers & FunctionModifiers.Public) != 0;
    public bool IsStatic => (Modifiers & FunctionModifiers.Static) != 0;
    public bool IsNative => (Modifiers & FunctionModifiers.Native) != 0;
    public bool IsRuntime => (Modifiers & FunctionModifiers.Runtime) != 0;

    public FunctionDeclarationNode(
        FunctionModifiers modifiers,
        string name,
        IReadOnlyList<ParameterNode> parameters,
        TypeNode? returnType,
        BlockStatementNode body,
        SourceSpan span = default) : base(span)
    {
        Modifiers = modifiers;
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Член класса.
/// </summary>
public abstract class ClassMemberNode : AstNode
{
    protected ClassMemberNode(SourceSpan span = default) : base(span) { }
}

/// <summary>
/// Член структуры.
/// </summary>
public abstract class StructMemberNode : AstNode
{
    protected StructMemberNode(SourceSpan span = default) : base(span) { }
}

/// <summary>
/// Объявление поля: pub? var name: type? = expr?;
/// </summary>
public sealed class FieldDeclarationNode : ClassMemberNode
{
    public AccessModifier Access { get; }
    public string Name { get; }
    public TypeNode? Type { get; }
    public ExpressionNode? Initializer { get; }

    public FieldDeclarationNode(
        AccessModifier access,
        string name,
        TypeNode? type,
        ExpressionNode? initializer = null,
        SourceSpan span = default) : base(span)
    {
        Access = access;
        Name = name;
        Type = type;
        Initializer = initializer;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Объявление метода класса или структуры: modifiers fn name(params) -> returnType { body }
/// </summary>
public sealed class MethodDeclarationNode : ClassMemberNode
{
    public MemberModifiers Modifiers { get; }
    public string Name { get; }
    public IReadOnlyList<ParameterNode> Parameters { get; }
    public TypeNode? ReturnType { get; }
    public BlockStatementNode Body { get; }

    public bool IsPublic => (Modifiers & MemberModifiers.Public) != 0;
    public bool IsStatic => (Modifiers & MemberModifiers.Static) != 0;
    public bool IsVirtual => (Modifiers & MemberModifiers.Virtual) != 0;
    public bool IsOverride => (Modifiers & MemberModifiers.Override) != 0;
    public bool IsNative => (Modifiers & MemberModifiers.Native) != 0;
    public bool IsRuntime => (Modifiers & MemberModifiers.Runtime) != 0;

    public MethodDeclarationNode(
        MemberModifiers modifiers,
        string name,
        IReadOnlyList<ParameterNode> parameters,
        TypeNode? returnType,
        BlockStatementNode body,
        SourceSpan span = default) : base(span)
    {
        Modifiers = modifiers;
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Конструктор класса: pub? fn TypeName(params) { body }
/// </summary>
public sealed class ConstructorDeclarationNode : ClassMemberNode
{
    public AccessModifier Access { get; }
    public string Name { get; }
    public IReadOnlyList<ParameterNode> Parameters { get; }
    public BlockStatementNode Body { get; }

    public ConstructorDeclarationNode(
        AccessModifier access,
        string name,
        IReadOnlyList<ParameterNode> parameters,
        BlockStatementNode body,
        SourceSpan span = default) : base(span)
    {
        Access = access;
        Name = name;
        Parameters = parameters;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Объявление класса: pub? open? class Name : Base? { members }
/// </summary>
public sealed class ClassDeclarationNode : DeclarationNode
{
    public ClassModifiers Modifiers { get; }
    public string Name { get; }
    public QualifiedNameNode? BaseClass { get; }
    public IReadOnlyList<ClassMemberNode> Members { get; }

    public bool IsPublic => (Modifiers & ClassModifiers.Public) != 0;
    public bool IsOpen => (Modifiers & ClassModifiers.Open) != 0;

    public ClassDeclarationNode(
        ClassModifiers modifiers,
        string name,
        QualifiedNameNode? baseClass,
        IReadOnlyList<ClassMemberNode> members,
        SourceSpan span = default) : base(span)
    {
        Modifiers = modifiers;
        Name = name;
        BaseClass = baseClass;
        Members = members;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Объявление структуры: pub? struct Name { members }
/// </summary>
public sealed class StructDeclarationNode : DeclarationNode
{
    public AccessModifier Access { get; }
    public string Name { get; }
    public IReadOnlyList<ClassMemberNode> Members { get; }

    public bool IsPublic => Access == AccessModifier.Public;

    public StructDeclarationNode(
        AccessModifier access,
        string name,
        IReadOnlyList<ClassMemberNode> members,
        SourceSpan span = default) : base(span)
    {
        Access = access;
        Name = name;
        Members = members;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Объявление локальной или глобальной переменной: var x: int = 10; или let x = 20;
/// </summary>
public sealed class VariableDeclarationNode : DeclarationNode
{
    public AccessModifier Access { get; }
    public VariableKind Kind { get; }
    public string Name { get; }
    public TypeNode? Type { get; }
    public ExpressionNode? Initializer { get; }

    public VariableDeclarationNode(
        VariableKind kind,
        string name,
        TypeNode? type = null,
        ExpressionNode? initializer = null,
        AccessModifier access = AccessModifier.Default,
        SourceSpan span = default) : base(span)
    {
        Kind = kind;
        Name = name;
        Type = type;
        Initializer = initializer;
        Access = access;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Объявление константы: const Name: Type = expr;
/// </summary>
public sealed class ConstantDeclarationNode : DeclarationNode
{
    public AccessModifier Access { get; }
    public string Name { get; }
    public TypeNode? Type { get; }
    public ExpressionNode Initializer { get; }

    public ConstantDeclarationNode(
        string name,
        ExpressionNode initializer,
        TypeNode? type = null,
        AccessModifier access = AccessModifier.Default,
        SourceSpan span = default) : base(span)
    {
        Name = name;
        Initializer = initializer;
        Type = type;
        Access = access;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}
