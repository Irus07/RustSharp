using RustSharp.Core.Ast.Declarations;
using RustSharp.Core.Ast.Expressions;
using RustSharp.Core.Ast.Statements;
using RustSharp.Core.Ast.Types;

namespace RustSharp.Core.Ast.Visitors;

/// <summary>
/// Базовая реализация посетителя AST с обходом дочерних узлов по умолчанию.
/// Для создания кастомного анализатора достаточно переопределить интересующие методы.
/// </summary>
public abstract class AstVisitorBase : IAstVisitor
{
    public virtual void Visit(ProgramNode node)
    {
        node.Module?.Accept(this);
        foreach (var import in node.Imports)
            import.Accept(this);
        foreach (var decl in node.Declarations)
            decl.Accept(this);
    }

    public virtual void Visit(ModuleDeclarationNode node) => node.Name.Accept(this);
    public virtual void Visit(ImportDeclarationNode node) => node.Name.Accept(this);

    public virtual void Visit(ParameterNode node) => node.Type.Accept(this);

    public virtual void Visit(FunctionDeclarationNode node)
    {
        foreach (var param in node.Parameters)
            param.Accept(this);
        node.ReturnType?.Accept(this);
        node.Body.Accept(this);
    }

    public virtual void Visit(ClassDeclarationNode node)
    {
        node.BaseClass?.Accept(this);
        foreach (var member in node.Members)
            member.Accept(this);
    }

    public virtual void Visit(StructDeclarationNode node)
    {
        foreach (var member in node.Members)
            member.Accept(this);
    }

    public virtual void Visit(FieldDeclarationNode node)
    {
        node.Type?.Accept(this);
        node.Initializer?.Accept(this);
    }

    public virtual void Visit(MethodDeclarationNode node)
    {
        foreach (var param in node.Parameters)
            param.Accept(this);
        node.ReturnType?.Accept(this);
        node.Body.Accept(this);
    }

    public virtual void Visit(ConstructorDeclarationNode node)
    {
        foreach (var param in node.Parameters)
            param.Accept(this);
        node.Body.Accept(this);
    }

    public virtual void Visit(VariableDeclarationNode node)
    {
        node.Type?.Accept(this);
        node.Initializer?.Accept(this);
    }

    public virtual void Visit(ConstantDeclarationNode node)
    {
        node.Type?.Accept(this);
        node.Initializer.Accept(this);
    }

    public virtual void Visit(BlockStatementNode node)
    {
        foreach (var stmt in node.Statements)
            stmt.Accept(this);
    }

    public virtual void Visit(IfStatementNode node)
    {
        foreach (var branch in node.Branches)
            branch.Accept(this);
        node.ElseBlock?.Accept(this);
    }

    public virtual void Visit(IfBranchNode node)
    {
        node.Condition.Accept(this);
        node.Body.Accept(this);
    }

    public virtual void Visit(WhileStatementNode node)
    {
        node.Condition.Accept(this);
        node.Body.Accept(this);
    }

    public virtual void Visit(ForStatementNode node)
    {
        node.Iterable.Accept(this);
        node.Body.Accept(this);
    }

    public virtual void Visit(ReturnStatementNode node) => node.Expression?.Accept(this);
    public virtual void Visit(ThrowStatementNode node) => node.Expression.Accept(this);

    public virtual void Visit(TryStatementNode node)
    {
        node.TryBlock.Accept(this);
        foreach (var c in node.CatchClauses)
            c.Accept(this);
        node.FinallyClause?.Accept(this);
    }

    public virtual void Visit(CatchClauseNode node)
    {
        node.ErrorType.Accept(this);
        node.Body.Accept(this);
    }

    public virtual void Visit(FinallyClauseNode node) => node.Body.Accept(this);

    public virtual void Visit(PanicStatementNode node) => node.Expression.Accept(this);

    public virtual void Visit(PinStatementNode node)
    {
        node.Expression.Accept(this);
        node.Body.Accept(this);
    }

    public virtual void Visit(ScopedBlockStatementNode node) => node.Body.Accept(this);

    public virtual void Visit(AssignmentStatementNode node)
    {
        node.Target.Accept(this);
        node.Value?.Accept(this);
    }

    public virtual void Visit(ExpressionStatementNode node) => node.Expression.Accept(this);
    public virtual void Visit(VariableDeclarationStatementNode node) => node.Declaration.Accept(this);
    public virtual void Visit(ConstantDeclarationStatementNode node) => node.Declaration.Accept(this);
    public virtual void Visit(BreakStatementNode node) { }
    public virtual void Visit(ContinueStatementNode node) { }

    public virtual void Visit(LiteralExpressionNode node) { }
    public virtual void Visit(IdentifierExpressionNode node) { }
    public virtual void Visit(QualifiedNameNode node) { }

    public virtual void Visit(BinaryExpressionNode node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
    }

    public virtual void Visit(UnaryExpressionNode node) => node.Operand.Accept(this);
    public virtual void Visit(ArgumentNode node) => node.Expression.Accept(this);

    public virtual void Visit(InvocationExpressionNode node)
    {
        node.Callee.Accept(this);
        foreach (var arg in node.Arguments)
            arg.Accept(this);
    }

    public virtual void Visit(MemberAccessExpressionNode node) => node.Target.Accept(this);

    public virtual void Visit(IndexExpressionNode node)
    {
        node.Target.Accept(this);
        node.Index.Accept(this);
    }

    public virtual void Visit(StackAllocExpressionNode node)
    {
        node.ElementType.Accept(this);
        node.Size.Accept(this);
    }

    public virtual void Visit(ArrayLiteralExpressionNode node)
    {
        foreach (var element in node.Elements)
            element.Accept(this);
    }

    public virtual void Visit(PrimitiveTypeNode node) { }
    public virtual void Visit(CustomTypeNode node) => node.Name.Accept(this);
    public virtual void Visit(ArrayTypeNode node) => node.ElementType.Accept(this);
    public virtual void Visit(ReferenceTypeNode node) => node.InnerType.Accept(this);
}

/// <summary>
/// Базовая реализация типизированного посетителя AST.
/// По умолчанию возвращает <c>default(TResult)!</c>.
/// </summary>
public abstract class AstVisitorBase<TResult> : IAstVisitor<TResult>
{
    public virtual TResult Visit(ProgramNode node) => default!;
    public virtual TResult Visit(ModuleDeclarationNode node) => default!;
    public virtual TResult Visit(ImportDeclarationNode node) => default!;
    public virtual TResult Visit(ParameterNode node) => default!;
    public virtual TResult Visit(FunctionDeclarationNode node) => default!;
    public virtual TResult Visit(ClassDeclarationNode node) => default!;
    public virtual TResult Visit(StructDeclarationNode node) => default!;
    public virtual TResult Visit(FieldDeclarationNode node) => default!;
    public virtual TResult Visit(MethodDeclarationNode node) => default!;
    public virtual TResult Visit(ConstructorDeclarationNode node) => default!;
    public virtual TResult Visit(VariableDeclarationNode node) => default!;
    public virtual TResult Visit(ConstantDeclarationNode node) => default!;

    public virtual TResult Visit(BlockStatementNode node) => default!;
    public virtual TResult Visit(IfStatementNode node) => default!;
    public virtual TResult Visit(IfBranchNode node) => default!;
    public virtual TResult Visit(WhileStatementNode node) => default!;
    public virtual TResult Visit(ForStatementNode node) => default!;
    public virtual TResult Visit(ReturnStatementNode node) => default!;
    public virtual TResult Visit(ThrowStatementNode node) => default!;
    public virtual TResult Visit(TryStatementNode node) => default!;
    public virtual TResult Visit(CatchClauseNode node) => default!;
    public virtual TResult Visit(FinallyClauseNode node) => default!;
    public virtual TResult Visit(PanicStatementNode node) => default!;
    public virtual TResult Visit(PinStatementNode node) => default!;
    public virtual TResult Visit(ScopedBlockStatementNode node) => default!;
    public virtual TResult Visit(AssignmentStatementNode node) => default!;
    public virtual TResult Visit(ExpressionStatementNode node) => default!;
    public virtual TResult Visit(VariableDeclarationStatementNode node) => default!;
    public virtual TResult Visit(ConstantDeclarationStatementNode node) => default!;
    public virtual TResult Visit(BreakStatementNode node) => default!;
    public virtual TResult Visit(ContinueStatementNode node) => default!;

    public virtual TResult Visit(LiteralExpressionNode node) => default!;
    public virtual TResult Visit(IdentifierExpressionNode node) => default!;
    public virtual TResult Visit(QualifiedNameNode node) => default!;
    public virtual TResult Visit(BinaryExpressionNode node) => default!;
    public virtual TResult Visit(UnaryExpressionNode node) => default!;
    public virtual TResult Visit(ArgumentNode node) => default!;
    public virtual TResult Visit(InvocationExpressionNode node) => default!;
    public virtual TResult Visit(MemberAccessExpressionNode node) => default!;
    public virtual TResult Visit(IndexExpressionNode node) => default!;
    public virtual TResult Visit(StackAllocExpressionNode node) => default!;
    public virtual TResult Visit(ArrayLiteralExpressionNode node) => default!;

    public virtual TResult Visit(PrimitiveTypeNode node) => default!;
    public virtual TResult Visit(CustomTypeNode node) => default!;
    public virtual TResult Visit(ArrayTypeNode node) => default!;
    public virtual TResult Visit(ReferenceTypeNode node) => default!;
}
