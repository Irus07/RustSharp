using System.Text;
using RustSharp.Core.Ast.Declarations;
using RustSharp.Core.Ast.Expressions;
using RustSharp.Core.Ast.Statements;
using RustSharp.Core.Ast.Types;

namespace RustSharp.Core.Ast.Visitors;

/// <summary>
/// Посетитель AST, создающий форматированное древовидное представление с ASCII-ветвями.
/// Демонстрирует использование шаблона Visitor и служит для визуализации AST.
/// </summary>
public sealed class AstPrinter : IAstVisitor
{
    private readonly StringBuilder _builder = new();
    private string _indent = "";
    private string _branch = "";

    /// <summary>Форматирует переданное дерево AST в строку.</summary>
    public string Print(IAstNode root)
    {
        _builder.Clear();
        _indent = "";
        _branch = "";
        root.Accept(this);
        return _builder.ToString().TrimEnd();
    }

    private void WriteNode(string text, IEnumerable<IAstNode?> children)
    {
        _builder.Append(_indent).Append(_branch).AppendLine(text);

        var valid = children.Where(c => c != null).Cast<IAstNode>().ToList();
        for (int i = 0; i < valid.Count; i++)
        {
            bool isLast = (i == valid.Count - 1);
            string oldIndent = _indent;
            string oldBranch = _branch;

            string branchIndent = oldBranch switch
            {
                "└── " => "    ",
                "├── " => "│   ",
                _ => ""
            };

            _indent = oldIndent + branchIndent;
            _branch = isLast ? "└── " : "├── ";

            valid[i].Accept(this);

            _indent = oldIndent;
            _branch = oldBranch;
        }
    }

    private void WriteNode(string text, params IAstNode?[] children) =>
        WriteNode(text, (IEnumerable<IAstNode?>)children);

    public void Visit(ProgramNode node)
    {
        var items = new List<IAstNode?>();
        if (node.Module != null) items.Add(node.Module);
        items.AddRange(node.Imports);
        items.AddRange(node.Declarations);
        WriteNode("Program", items);
    }

    public void Visit(ModuleDeclarationNode node) => WriteNode($"Module: {node.Name.FullName}");
    public void Visit(ImportDeclarationNode node) => WriteNode($"Import: {node.Name.FullName}");
    public void Visit(ParameterNode node) => WriteNode($"Parameter: {node}");

    public void Visit(FunctionDeclarationNode node)
    {
        string mods = node.Modifiers == FunctionModifiers.None ? "" : $"[{node.Modifiers}] ";
        string ret = node.ReturnType != null ? $" -> {node.ReturnType}" : "";
        var paramsText = string.Join(", ", node.Parameters);
        WriteNode($"Function: {mods}{node.Name}({paramsText}){ret}", node.Body);
    }

    public void Visit(ClassDeclarationNode node)
    {
        string mods = node.Modifiers == ClassModifiers.None ? "" : $"[{node.Modifiers}] ";
        string baseCls = node.BaseClass != null ? $" : {node.BaseClass.FullName}" : "";
        WriteNode($"Class: {mods}{node.Name}{baseCls}", node.Members);
    }

    public void Visit(StructDeclarationNode node)
    {
        string access = node.IsPublic ? "[Pub] " : "";
        WriteNode($"Struct: {access}{node.Name}", node.Members);
    }

    public void Visit(FieldDeclarationNode node)
    {
        string access = node.Access == AccessModifier.Public ? "pub " : "";
        string type = node.Type != null ? $": {node.Type}" : "";
        string init = node.Initializer != null ? $" = {node.Initializer}" : "";
        WriteNode($"Field: {access}var {node.Name}{type}{init}");
    }

    public void Visit(MethodDeclarationNode node)
    {
        string mods = node.Modifiers == MemberModifiers.None ? "" : $"[{node.Modifiers}] ";
        string ret = node.ReturnType != null ? $" -> {node.ReturnType}" : "";
        var paramsText = string.Join(", ", node.Parameters);
        WriteNode($"Method: {mods}{node.Name}({paramsText}){ret}", node.Body);
    }

    public void Visit(ConstructorDeclarationNode node)
    {
        string access = node.Access == AccessModifier.Public ? "pub " : "";
        var paramsText = string.Join(", ", node.Parameters);
        WriteNode($"Constructor: {access}fn {node.Name}({paramsText})", node.Body);
    }

    public void Visit(VariableDeclarationNode node)
    {
        string access = node.Access == AccessModifier.Public ? "pub " : "";
        string type = node.Type != null ? $": {node.Type}" : "";
        string init = node.Initializer != null ? $" = {node.Initializer}" : "";
        WriteNode($"VarDecl: {access}{node.Kind.ToString().ToLowerInvariant()} {node.Name}{type}{init}");
    }

    public void Visit(ConstantDeclarationNode node)
    {
        string access = node.Access == AccessModifier.Public ? "pub " : "";
        string type = node.Type != null ? $": {node.Type}" : "";
        WriteNode($"ConstDecl: {access}const {node.Name}{type} = {node.Initializer}");
    }

    public void Visit(BlockStatementNode node) => WriteNode("Block", node.Statements);

    public void Visit(IfStatementNode node)
    {
        var children = new List<IAstNode?>(node.Branches);
        if (node.ElseBlock != null)
            children.Add(node.ElseBlock);
        WriteNode("IfStatement", children);
    }

    public void Visit(IfBranchNode node) =>
        WriteNode($"Branch (cond: {node.Condition})", node.Body);

    public void Visit(WhileStatementNode node) =>
        WriteNode($"While (cond: {node.Condition})", node.Body);

    public void Visit(ForStatementNode node)
    {
        string idx = node.IndexVariable != null ? $"{node.IndexVariable}; " : "";
        WriteNode($"For ({idx}{node.ItemVariable} in {node.Iterable})", node.Body);
    }

    public void Visit(ReturnStatementNode node) =>
        WriteNode(node.Expression != null ? $"Return: {node.Expression}" : "Return");

    public void Visit(ThrowStatementNode node) =>
        WriteNode($"Throw: {node.Expression}");

    public void Visit(TryStatementNode node)
    {
        var children = new List<IAstNode?> { node.TryBlock };
        children.AddRange(node.CatchClauses);
        if (node.FinallyClause != null)
            children.Add(node.FinallyClause);
        WriteNode("Try", children);
    }

    public void Visit(CatchClauseNode node) =>
        WriteNode($"Catch ({node.VariableName}: {node.ErrorType})", node.Body);

    public void Visit(FinallyClauseNode node) =>
        WriteNode("Finally", node.Body);

    public void Visit(PanicStatementNode node) =>
        WriteNode($"Panic: {node.Expression}");

    public void Visit(PinStatementNode node)
    {
        string kind = node.IsVar ? "var " : "";
        WriteNode($"Pin ({kind}{node.VariableName} = {node.Expression})", node.Body);
    }

    public void Visit(ScopedBlockStatementNode node) =>
        WriteNode($"ScopedBlock: {node.Mode.ToString().ToLowerInvariant()}", node.Body);

    public void Visit(AssignmentStatementNode node)
    {
        string val = node.Value != null ? $" {node.Value}" : "";
        WriteNode($"Assign: {node.Target} {node.Operator}{val}");
    }

    public void Visit(ExpressionStatementNode node) =>
        WriteNode($"ExprStmt: {node.Expression}");

    public void Visit(VariableDeclarationStatementNode node) =>
        node.Declaration.Accept(this);

    public void Visit(ConstantDeclarationStatementNode node) =>
        node.Declaration.Accept(this);

    public void Visit(BreakStatementNode node) => WriteNode("Break");
    public void Visit(ContinueStatementNode node) => WriteNode("Continue");

    public void Visit(LiteralExpressionNode node) => WriteNode($"Literal: {node.RawText}");
    public void Visit(IdentifierExpressionNode node) => WriteNode($"Ident: {node.Name}");
    public void Visit(QualifiedNameNode node) => WriteNode($"QualName: {node.FullName}");

    public void Visit(BinaryExpressionNode node) =>
        WriteNode($"Binary: {node.Left} {node.Operator} {node.Right}");

    public void Visit(UnaryExpressionNode node) =>
        WriteNode($"Unary: {node.Operator}{node.Operand}");

    public void Visit(ArgumentNode node) => WriteNode($"Arg: {node}");

    public void Visit(InvocationExpressionNode node) =>
        WriteNode($"Call: {node.Callee}({string.Join(", ", node.Arguments)})");

    public void Visit(MemberAccessExpressionNode node) =>
        WriteNode($"MemberAccess: {node.Target}.{node.MemberName}");

    public void Visit(IndexExpressionNode node) =>
        WriteNode($"IndexAccess: {node.Target}[{node.Index}]");

    public void Visit(StackAllocExpressionNode node) =>
        WriteNode($"StackAlloc: {node.ElementType}[{node.Size}]");

    public void Visit(ArrayLiteralExpressionNode node) =>
        WriteNode($"ArrayLiteral: [{string.Join(", ", node.Elements)}]");

    public void Visit(PrimitiveTypeNode node) => WriteNode($"PrimitiveType: {node.Name}");
    public void Visit(CustomTypeNode node) => WriteNode($"CustomType: {node.Name.FullName}");
    public void Visit(ArrayTypeNode node) => WriteNode($"ArrayType: {node}");
    public void Visit(ReferenceTypeNode node) => WriteNode($"RefType: {node}");
}

/// <summary>
/// Расширения для удобного вывода AST.
/// </summary>
public static class AstNodeExtensions
{
    public static string DumpAst(this IAstNode node) => new AstPrinter().Print(node);
}
