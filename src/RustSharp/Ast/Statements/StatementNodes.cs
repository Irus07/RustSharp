using RustSharp.Core.Ast.Expressions;
using RustSharp.Core.Ast.Types;
using RustSharp.Core.Ast.Visitors;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Ast.Statements;

/// <summary>
/// Базовый класс для инструкций (statements).
/// </summary>
public abstract class StatementNode : AstNode
{
    protected StatementNode(SourceSpan span = default) : base(span) { }
}

/// <summary>
/// Блок инструкций в фигурных скобках: { statement* }.
/// </summary>
public sealed class BlockStatementNode : StatementNode
{
    public IReadOnlyList<StatementNode> Statements { get; }

    public BlockStatementNode(IReadOnlyList<StatementNode> statements, SourceSpan span = default)
        : base(span)
    {
        Statements = statements;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Ветвь if или else if: условие + тело.
/// </summary>
public sealed class IfBranchNode : AstNode
{
    public ExpressionNode Condition { get; }
    public BlockStatementNode Body { get; }

    public IfBranchNode(ExpressionNode condition, BlockStatementNode body, SourceSpan span = default)
        : base(span)
    {
        Condition = condition;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Условный оператор: if ... else if ... else ...
/// </summary>
public sealed class IfStatementNode : StatementNode
{
    public IReadOnlyList<IfBranchNode> Branches { get; }
    public BlockStatementNode? ElseBlock { get; }

    public IfStatementNode(
        IReadOnlyList<IfBranchNode> branches,
        BlockStatementNode? elseBlock = null,
        SourceSpan span = default) : base(span)
    {
        Branches = branches;
        ElseBlock = elseBlock;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Цикл while: while (condition) { body }.
/// </summary>
public sealed class WhileStatementNode : StatementNode
{
    public ExpressionNode Condition { get; }
    public BlockStatementNode Body { get; }

    public WhileStatementNode(ExpressionNode condition, BlockStatementNode body, SourceSpan span = default)
        : base(span)
    {
        Condition = condition;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Цикл for: for item in collection { } или for i; item in collection { }.
/// </summary>
public sealed class ForStatementNode : StatementNode
{
    public string? IndexVariable { get; }
    public string ItemVariable { get; }
    public ExpressionNode Iterable { get; }
    public BlockStatementNode Body { get; }

    public ForStatementNode(
        string? indexVariable,
        string itemVariable,
        ExpressionNode iterable,
        BlockStatementNode body,
        SourceSpan span = default) : base(span)
    {
        IndexVariable = indexVariable;
        ItemVariable = itemVariable;
        Iterable = iterable;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Инструкция возврата: return expr?;
/// </summary>
public sealed class ReturnStatementNode : StatementNode
{
    public ExpressionNode? Expression { get; }

    public ReturnStatementNode(ExpressionNode? expression = null, SourceSpan span = default)
        : base(span)
    {
        Expression = expression;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Выброс типизированного исключения: throw expr;
/// </summary>
public sealed class ThrowStatementNode : StatementNode
{
    public ExpressionNode Expression { get; }

    public ThrowStatementNode(ExpressionNode expression, SourceSpan span = default)
        : base(span)
    {
        Expression = expression;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Ветка catch (err: Type) { body }.
/// </summary>
public sealed class CatchClauseNode : AstNode
{
    public string VariableName { get; }
    public TypeNode ErrorType { get; }
    public BlockStatementNode Body { get; }

    public CatchClauseNode(
        string variableName,
        TypeNode errorType,
        BlockStatementNode body,
        SourceSpan span = default) : base(span)
    {
        VariableName = variableName;
        ErrorType = errorType;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Ветка finally { body }.
/// </summary>
public sealed class FinallyClauseNode : AstNode
{
    public BlockStatementNode Body { get; }

    public FinallyClauseNode(BlockStatementNode body, SourceSpan span = default)
        : base(span)
    {
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Блок обработки исключений: try { } catch (...) { } finally { }.
/// </summary>
public sealed class TryStatementNode : StatementNode
{
    public BlockStatementNode TryBlock { get; }
    public IReadOnlyList<CatchClauseNode> CatchClauses { get; }
    public FinallyClauseNode? FinallyClause { get; }

    public TryStatementNode(
        BlockStatementNode tryBlock,
        IReadOnlyList<CatchClauseNode> catchClauses,
        FinallyClauseNode? finallyClause = null,
        SourceSpan span = default) : base(span)
    {
        TryBlock = tryBlock;
        CatchClauses = catchClauses;
        FinallyClause = finallyClause;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Паника (необрабатываемый критический сбой): panic(expr);
/// </summary>
public sealed class PanicStatementNode : StatementNode
{
    public ExpressionNode Expression { get; }

    public PanicStatementNode(ExpressionNode expression, SourceSpan span = default)
        : base(span)
    {
        Expression = expression;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Закрепление адреса в памяти: pin (var ptr = expr) { body }.
/// </summary>
public sealed class PinStatementNode : StatementNode
{
    public bool IsVar { get; }
    public string VariableName { get; }
    public ExpressionNode Expression { get; }
    public BlockStatementNode Body { get; }

    public PinStatementNode(
        bool isVar,
        string variableName,
        ExpressionNode expression,
        BlockStatementNode body,
        SourceSpan span = default) : base(span)
    {
        IsVar = isVar;
        VariableName = variableName;
        Expression = expression;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Изолированный блок смены контекста выполнения: native { } или runtime { }.
/// </summary>
public sealed class ScopedBlockStatementNode : StatementNode
{
    public ExecutionMode Mode { get; }
    public BlockStatementNode Body { get; }

    public ScopedBlockStatementNode(ExecutionMode mode, BlockStatementNode body, SourceSpan span = default)
        : base(span)
    {
        Mode = mode;
        Body = body;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Присваивание или изменение значения: target op value; или target++;
/// </summary>
public sealed class AssignmentStatementNode : StatementNode
{
    public ExpressionNode Target { get; }
    public AssignmentOperator Operator { get; }
    public ExpressionNode? Value { get; }

    public AssignmentStatementNode(
        ExpressionNode target,
        AssignmentOperator op,
        ExpressionNode? value = null,
        SourceSpan span = default) : base(span)
    {
        Target = target;
        Operator = op;
        Value = value;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Выражение как отдельная инструкция: expr;
/// </summary>
public sealed class ExpressionStatementNode : StatementNode
{
    public ExpressionNode Expression { get; }

    public ExpressionStatementNode(ExpressionNode expression, SourceSpan span = default)
        : base(span)
    {
        Expression = expression;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Прерывание цикла: break;
/// </summary>
public sealed class BreakStatementNode : StatementNode
{
    public BreakStatementNode(SourceSpan span = default) : base(span) { }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Переход к следующей итерации: continue;
/// </summary>
public sealed class ContinueStatementNode : StatementNode
{
    public ContinueStatementNode(SourceSpan span = default) : base(span) { }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Инструкция объявления локальной переменной: var x: int = 10; или let x = 20;
/// </summary>
public sealed class VariableDeclarationStatementNode : StatementNode
{
    public RustSharp.Core.Ast.Declarations.VariableDeclarationNode Declaration { get; }

    public VariableDeclarationStatementNode(
        RustSharp.Core.Ast.Declarations.VariableDeclarationNode declaration,
        SourceSpan span = default) : base(span)
    {
        Declaration = declaration;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

/// <summary>
/// Инструкция объявления локальной константы: const X: int = 42;
/// </summary>
public sealed class ConstantDeclarationStatementNode : StatementNode
{
    public RustSharp.Core.Ast.Declarations.ConstantDeclarationNode Declaration { get; }

    public ConstantDeclarationStatementNode(
        RustSharp.Core.Ast.Declarations.ConstantDeclarationNode declaration,
        SourceSpan span = default) : base(span)
    {
        Declaration = declaration;
    }

    public override void Accept(IAstVisitor visitor) => visitor.Visit(this);
    public override TResult Accept<TResult>(IAstVisitor<TResult> visitor) => visitor.Visit(this);
    public override TResult Accept<TResult, TContext>(IAstVisitor<TResult, TContext> visitor, TContext context) => visitor.Visit(this, context);
}
