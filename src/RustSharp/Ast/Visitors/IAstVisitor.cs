using RustSharp.Core.Ast.Declarations;
using RustSharp.Core.Ast.Expressions;
using RustSharp.Core.Ast.Statements;
using RustSharp.Core.Ast.Types;

namespace RustSharp.Core.Ast.Visitors;

/// <summary>
/// Интерфейс базового посетителя узлов AST (без возвращаемого значения).
/// </summary>
public interface IAstVisitor
{
    void Visit(ProgramNode node);
    void Visit(ModuleDeclarationNode node);
    void Visit(ImportDeclarationNode node);
    void Visit(ParameterNode node);
    void Visit(FunctionDeclarationNode node);
    void Visit(ClassDeclarationNode node);
    void Visit(StructDeclarationNode node);
    void Visit(FieldDeclarationNode node);
    void Visit(MethodDeclarationNode node);
    void Visit(ConstructorDeclarationNode node);
    void Visit(VariableDeclarationNode node);
    void Visit(ConstantDeclarationNode node);

    // Statements
    void Visit(BlockStatementNode node);
    void Visit(IfStatementNode node);
    void Visit(IfBranchNode node);
    void Visit(WhileStatementNode node);
    void Visit(ForStatementNode node);
    void Visit(ReturnStatementNode node);
    void Visit(ThrowStatementNode node);
    void Visit(TryStatementNode node);
    void Visit(CatchClauseNode node);
    void Visit(FinallyClauseNode node);
    void Visit(PanicStatementNode node);
    void Visit(PinStatementNode node);
    void Visit(ScopedBlockStatementNode node);
    void Visit(AssignmentStatementNode node);
    void Visit(ExpressionStatementNode node);
    void Visit(VariableDeclarationStatementNode node);
    void Visit(ConstantDeclarationStatementNode node);
    void Visit(BreakStatementNode node);
    void Visit(ContinueStatementNode node);

    // Expressions
    void Visit(LiteralExpressionNode node);
    void Visit(IdentifierExpressionNode node);
    void Visit(QualifiedNameNode node);
    void Visit(BinaryExpressionNode node);
    void Visit(UnaryExpressionNode node);
    void Visit(ArgumentNode node);
    void Visit(InvocationExpressionNode node);
    void Visit(MemberAccessExpressionNode node);
    void Visit(IndexExpressionNode node);
    void Visit(StackAllocExpressionNode node);
    void Visit(ArrayLiteralExpressionNode node);

    // Types
    void Visit(PrimitiveTypeNode node);
    void Visit(CustomTypeNode node);
    void Visit(ArrayTypeNode node);
    void Visit(ReferenceTypeNode node);
}

/// <summary>
/// Интерфейс типизированного посетителя узлов AST, возвращающего результат типа <typeparamref name="TResult"/>.
/// </summary>
public interface IAstVisitor<out TResult>
{
    TResult Visit(ProgramNode node);
    TResult Visit(ModuleDeclarationNode node);
    TResult Visit(ImportDeclarationNode node);
    TResult Visit(ParameterNode node);
    TResult Visit(FunctionDeclarationNode node);
    TResult Visit(ClassDeclarationNode node);
    TResult Visit(StructDeclarationNode node);
    TResult Visit(FieldDeclarationNode node);
    TResult Visit(MethodDeclarationNode node);
    TResult Visit(ConstructorDeclarationNode node);
    TResult Visit(VariableDeclarationNode node);
    TResult Visit(ConstantDeclarationNode node);

    // Statements
    TResult Visit(BlockStatementNode node);
    TResult Visit(IfStatementNode node);
    TResult Visit(IfBranchNode node);
    TResult Visit(WhileStatementNode node);
    TResult Visit(ForStatementNode node);
    TResult Visit(ReturnStatementNode node);
    TResult Visit(ThrowStatementNode node);
    TResult Visit(TryStatementNode node);
    TResult Visit(CatchClauseNode node);
    TResult Visit(FinallyClauseNode node);
    TResult Visit(PanicStatementNode node);
    TResult Visit(PinStatementNode node);
    TResult Visit(ScopedBlockStatementNode node);
    TResult Visit(AssignmentStatementNode node);
    TResult Visit(ExpressionStatementNode node);
    TResult Visit(VariableDeclarationStatementNode node);
    TResult Visit(ConstantDeclarationStatementNode node);
    TResult Visit(BreakStatementNode node);
    TResult Visit(ContinueStatementNode node);

    // Expressions
    TResult Visit(LiteralExpressionNode node);
    TResult Visit(IdentifierExpressionNode node);
    TResult Visit(QualifiedNameNode node);
    TResult Visit(BinaryExpressionNode node);
    TResult Visit(UnaryExpressionNode node);
    TResult Visit(ArgumentNode node);
    TResult Visit(InvocationExpressionNode node);
    TResult Visit(MemberAccessExpressionNode node);
    TResult Visit(IndexExpressionNode node);
    TResult Visit(StackAllocExpressionNode node);
    TResult Visit(ArrayLiteralExpressionNode node);

    // Types
    TResult Visit(PrimitiveTypeNode node);
    TResult Visit(CustomTypeNode node);
    TResult Visit(ArrayTypeNode node);
    TResult Visit(ReferenceTypeNode node);
}

/// <summary>
/// Интерфейс контекстного посетителя узлов AST.
/// </summary>
public interface IAstVisitor<out TResult, in TContext>
{
    TResult Visit(ProgramNode node, TContext context);
    TResult Visit(ModuleDeclarationNode node, TContext context);
    TResult Visit(ImportDeclarationNode node, TContext context);
    TResult Visit(ParameterNode node, TContext context);
    TResult Visit(FunctionDeclarationNode node, TContext context);
    TResult Visit(ClassDeclarationNode node, TContext context);
    TResult Visit(StructDeclarationNode node, TContext context);
    TResult Visit(FieldDeclarationNode node, TContext context);
    TResult Visit(MethodDeclarationNode node, TContext context);
    TResult Visit(ConstructorDeclarationNode node, TContext context);
    TResult Visit(VariableDeclarationNode node, TContext context);
    TResult Visit(ConstantDeclarationNode node, TContext context);

    // Statements
    TResult Visit(BlockStatementNode node, TContext context);
    TResult Visit(IfStatementNode node, TContext context);
    TResult Visit(IfBranchNode node, TContext context);
    TResult Visit(WhileStatementNode node, TContext context);
    TResult Visit(ForStatementNode node, TContext context);
    TResult Visit(ReturnStatementNode node, TContext context);
    TResult Visit(ThrowStatementNode node, TContext context);
    TResult Visit(TryStatementNode node, TContext context);
    TResult Visit(CatchClauseNode node, TContext context);
    TResult Visit(FinallyClauseNode node, TContext context);
    TResult Visit(PanicStatementNode node, TContext context);
    TResult Visit(PinStatementNode node, TContext context);
    TResult Visit(ScopedBlockStatementNode node, TContext context);
    TResult Visit(AssignmentStatementNode node, TContext context);
    TResult Visit(ExpressionStatementNode node, TContext context);
    TResult Visit(VariableDeclarationStatementNode node, TContext context);
    TResult Visit(ConstantDeclarationStatementNode node, TContext context);
    TResult Visit(BreakStatementNode node, TContext context);
    TResult Visit(ContinueStatementNode node, TContext context);

    // Expressions
    TResult Visit(LiteralExpressionNode node, TContext context);
    TResult Visit(IdentifierExpressionNode node, TContext context);
    TResult Visit(QualifiedNameNode node, TContext context);
    TResult Visit(BinaryExpressionNode node, TContext context);
    TResult Visit(UnaryExpressionNode node, TContext context);
    TResult Visit(ArgumentNode node, TContext context);
    TResult Visit(InvocationExpressionNode node, TContext context);
    TResult Visit(MemberAccessExpressionNode node, TContext context);
    TResult Visit(IndexExpressionNode node, TContext context);
    TResult Visit(StackAllocExpressionNode node, TContext context);
    TResult Visit(ArrayLiteralExpressionNode node, TContext context);

    // Types
    TResult Visit(PrimitiveTypeNode node, TContext context);
    TResult Visit(CustomTypeNode node, TContext context);
    TResult Visit(ArrayTypeNode node, TContext context);
    TResult Visit(ReferenceTypeNode node, TContext context);
}
