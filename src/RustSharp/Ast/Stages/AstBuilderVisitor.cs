using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using RustSharp.Core.Ast.Declarations;
using RustSharp.Core.Ast.Expressions;
using RustSharp.Core.Ast.Statements;
using RustSharp.Core.Ast.Types;
using RustSharp.Core.Diagnostics;
using RustSharp.Core.Frontend;
using RustSharp.Core.Frontend.Generated;

namespace RustSharp.Core.Ast.Stages;

/// <summary>
/// Посетитель дерева разбора ANTLR, конструирующий строго типизированное абстрактное синтаксическое дерево (AST).
/// </summary>
public sealed class AstBuilderVisitor : RustSharpParserBaseVisitor<IAstNode?>
{
    private readonly CompilerDiagnostics _diagnostics;
    private readonly string? _filePath;

    public AstBuilderVisitor(CompilerDiagnostics diagnostics, string? filePath = null)
    {
        _diagnostics = diagnostics;
        _filePath = filePath;
    }

    private SourceSpan GetSpan(ParserRuleContext context)
    {
        var start = new SourceLocation(context.Start.Line, context.Start.Column + 1, context.Start.StartIndex);
        var stopToken = context.Stop ?? context.Start;
        int stopIndex = stopToken.StopIndex >= stopToken.StartIndex ? stopToken.StopIndex : stopToken.StartIndex;
        int length = Math.Max(1, stopIndex - stopToken.StartIndex + 1);
        var end = new SourceLocation(stopToken.Line, stopToken.Column + 1 + length, stopIndex);
        return new SourceSpan(start, end, _filePath);
    }

    private SourceSpan GetSpan(IToken token)
    {
        var start = new SourceLocation(token.Line, token.Column + 1, token.StartIndex);
        int stopIndex = token.StopIndex >= token.StartIndex ? token.StopIndex : token.StartIndex;
        int length = Math.Max(1, stopIndex - token.StartIndex + 1);
        var end = new SourceLocation(token.Line, token.Column + 1 + length, stopIndex);
        return new SourceSpan(start, end, _filePath);
    }

    // ==========================================
    // 1. Program & Module Level
    // ==========================================

    public override IAstNode? VisitProgram([NotNull] RustSharpParser.ProgramContext context)
    {
        ModuleDeclarationNode? module = null;
        if (context.moduleDeclaration() is { } moduleCtx)
            module = Visit(moduleCtx) as ModuleDeclarationNode;

        var imports = new List<ImportDeclarationNode>();
        foreach (var importCtx in context.importDeclaration())
        {
            if (Visit(importCtx) is ImportDeclarationNode imp)
                imports.Add(imp);
        }

        var declarations = new List<DeclarationNode>();
        foreach (var itemCtx in context.topLevelItem())
        {
            if (Visit(itemCtx) is DeclarationNode decl)
                declarations.Add(decl);
        }

        return new ProgramNode(module, imports, declarations, GetSpan(context));
    }

    public override IAstNode? VisitModuleDeclaration([NotNull] RustSharpParser.ModuleDeclarationContext context)
    {
        var name = VisitQualifiedName(context.qualifiedName()) as QualifiedNameNode;
        return new ModuleDeclarationNode(name!, GetSpan(context));
    }

    public override IAstNode? VisitImportDeclaration([NotNull] RustSharpParser.ImportDeclarationContext context)
    {
        var name = VisitQualifiedName(context.qualifiedName()) as QualifiedNameNode;
        return new ImportDeclarationNode(name!, GetSpan(context));
    }

    public override IAstNode? VisitQualifiedName([NotNull] RustSharpParser.QualifiedNameContext context)
    {
        var parts = context.IDENTIFIER().Select(id => id.GetText()).ToList();
        return new QualifiedNameNode(parts, GetSpan(context));
    }

    public override IAstNode? VisitTopLevelItem([NotNull] RustSharpParser.TopLevelItemContext context)
    {
        if (context.functionDeclaration() is { } func)
            return Visit(func);
        if (context.classDeclaration() is { } cls)
            return Visit(cls);
        if (context.structDeclaration() is { } st)
            return Visit(st);
        if (context.globalVariableDeclaration() is { } gvar)
            return Visit(gvar);

        return null;
    }

    // ==========================================
    // 2. OOP Declarations (Class & Struct)
    // ==========================================

    public override IAstNode? VisitClassDeclaration([NotNull] RustSharpParser.ClassDeclarationContext context)
    {
        ClassModifiers modifiers = ClassModifiers.None;
        if (context.PUB() != null) modifiers |= ClassModifiers.Public;
        if (context.OPEN() != null) modifiers |= ClassModifiers.Open;

        string name = context.IDENTIFIER().GetText();
        QualifiedNameNode? baseClass = null;
        if (context.qualifiedName() is { } baseCtx)
            baseClass = VisitQualifiedName(baseCtx) as QualifiedNameNode;

        var members = new List<ClassMemberNode>();
        foreach (var memberCtx in context.classMember())
        {
            if (Visit(memberCtx) is ClassMemberNode member)
                members.Add(member);
        }

        return new ClassDeclarationNode(modifiers, name, baseClass, members, GetSpan(context));
    }

    public override IAstNode? VisitClassMember([NotNull] RustSharpParser.ClassMemberContext context)
    {
        if (context.fieldDeclaration() is { } field)
            return Visit(field);
        if (context.methodDeclaration() is { } method)
            return Visit(method);
        if (context.constructorDeclaration() is { } ctor)
            return Visit(ctor);

        return null;
    }

    public override IAstNode? VisitStructDeclaration([NotNull] RustSharpParser.StructDeclarationContext context)
    {
        var access = context.PUB() != null ? AccessModifier.Public : AccessModifier.Default;
        string name = context.IDENTIFIER().GetText();

        var members = new List<ClassMemberNode>();
        foreach (var memberCtx in context.structMember())
        {
            if (Visit(memberCtx) is ClassMemberNode member)
                members.Add(member);
        }

        return new StructDeclarationNode(access, name, members, GetSpan(context));
    }

    public override IAstNode? VisitStructMember([NotNull] RustSharpParser.StructMemberContext context)
    {
        if (context.fieldDeclaration() is { } field)
            return Visit(field);
        if (context.methodDeclaration() is { } method)
            return Visit(method);

        return null;
    }

    public override IAstNode? VisitFieldDeclaration([NotNull] RustSharpParser.FieldDeclarationContext context)
    {
        var access = context.PUB() != null ? AccessModifier.Public : AccessModifier.Default;
        string name = context.IDENTIFIER().GetText();
        TypeNode? type = context.type() != null ? Visit(context.type()) as TypeNode : null;
        ExpressionNode? init = context.expression() != null ? Visit(context.expression()) as ExpressionNode : null;

        return new FieldDeclarationNode(access, name, type, init, GetSpan(context));
    }

    public override IAstNode? VisitConstructorDeclaration([NotNull] RustSharpParser.ConstructorDeclarationContext context)
    {
        var access = context.PUB() != null ? AccessModifier.Public : AccessModifier.Default;
        string name = context.IDENTIFIER().GetText();

        var parameters = new List<ParameterNode>();
        if (context.parameterList() is { } pList)
        {
            foreach (var pCtx in pList.parameter())
            {
                if (Visit(pCtx) is ParameterNode p)
                    parameters.Add(p);
            }
        }

        var body = Visit(context.block()) as BlockStatementNode ?? new BlockStatementNode(Array.Empty<StatementNode>());
        return new ConstructorDeclarationNode(access, name, parameters, body, GetSpan(context));
    }

    public override IAstNode? VisitMethodDeclaration([NotNull] RustSharpParser.MethodDeclarationContext context)
    {
        MemberModifiers modifiers = MemberModifiers.None;
        foreach (var mod in context.memberModifier())
        {
            if (mod.PUB() != null) modifiers |= MemberModifiers.Public;
            if (mod.STATIC() != null) modifiers |= MemberModifiers.Static;
            if (mod.VIRTUAL() != null) modifiers |= MemberModifiers.Virtual;
            if (mod.OVERRIDE() != null) modifiers |= MemberModifiers.Override;
            if (mod.executionMode()?.NATIVE() != null) modifiers |= MemberModifiers.Native;
            if (mod.executionMode()?.RUNTIME() != null) modifiers |= MemberModifiers.Runtime;
        }

        var sig = context.functionSignature();
        string name = sig.IDENTIFIER().GetText();

        var parameters = new List<ParameterNode>();
        if (sig.parameterList() is { } pList)
        {
            foreach (var pCtx in pList.parameter())
            {
                if (Visit(pCtx) is ParameterNode p)
                    parameters.Add(p);
            }
        }

        TypeNode? returnType = sig.type() != null ? Visit(sig.type()) as TypeNode : null;
        var body = Visit(context.block()) as BlockStatementNode ?? new BlockStatementNode(Array.Empty<StatementNode>());

        return new MethodDeclarationNode(modifiers, name, parameters, returnType, body, GetSpan(context));
    }

    // ==========================================
    // 3. Functions
    // ==========================================

    public override IAstNode? VisitFunctionDeclaration([NotNull] RustSharpParser.FunctionDeclarationContext context)
    {
        FunctionModifiers modifiers = FunctionModifiers.None;
        foreach (var mod in context.functionModifier())
        {
            if (mod.PUB() != null) modifiers |= FunctionModifiers.Public;
            if (mod.STATIC() != null) modifiers |= FunctionModifiers.Static;
            if (mod.executionMode()?.NATIVE() != null) modifiers |= FunctionModifiers.Native;
            if (mod.executionMode()?.RUNTIME() != null) modifiers |= FunctionModifiers.Runtime;
        }

        var sig = context.functionSignature();
        string name = sig.IDENTIFIER().GetText();

        var parameters = new List<ParameterNode>();
        if (sig.parameterList() is { } pList)
        {
            foreach (var pCtx in pList.parameter())
            {
                if (Visit(pCtx) is ParameterNode p)
                    parameters.Add(p);
            }
        }

        TypeNode? returnType = sig.type() != null ? Visit(sig.type()) as TypeNode : null;
        var body = Visit(context.block()) as BlockStatementNode ?? new BlockStatementNode(Array.Empty<StatementNode>());

        return new FunctionDeclarationNode(modifiers, name, parameters, returnType, body, GetSpan(context));
    }

    public override IAstNode? VisitParameter([NotNull] RustSharpParser.ParameterContext context)
    {
        ParameterModifier modifier = ParameterModifier.None;
        if (context.parameterModifier() is { } mod)
        {
            if (mod.OWN() != null) modifier = ParameterModifier.Own;
            else if (mod.REF() != null && mod.VAR() != null) modifier = ParameterModifier.RefVar;
            else if (mod.REF() != null) modifier = ParameterModifier.Ref;
            else if (mod.AMP() != null && mod.VAR() != null) modifier = ParameterModifier.AmpVar;
            else if (mod.AMP() != null) modifier = ParameterModifier.Amp;
        }

        string name = context.IDENTIFIER().GetText();
        var type = Visit(context.type()) as TypeNode ?? new PrimitiveTypeNode("void");

        return new ParameterNode(modifier, name, type, GetSpan(context));
    }

    // ==========================================
    // 4. Variables & Constants
    // ==========================================

    public override IAstNode? VisitGlobalVariableDeclaration([NotNull] RustSharpParser.GlobalVariableDeclarationContext context)
    {
        var access = context.PUB() != null ? AccessModifier.Public : AccessModifier.Default;
        if (context.variableDeclaration() is { } varCtx)
        {
            var node = Visit(varCtx) as VariableDeclarationNode;
            return new VariableDeclarationNode(node!.Kind, node.Name, node.Type, node.Initializer, access, GetSpan(context));
        }

        if (context.constantDeclaration() is { } constCtx)
        {
            var node = Visit(constCtx) as ConstantDeclarationNode;
            return new ConstantDeclarationNode(node!.Name, node.Initializer, node.Type, access, GetSpan(context));
        }

        return null;
    }

    public override IAstNode? VisitVariableDeclaration([NotNull] RustSharpParser.VariableDeclarationContext context)
    {
        var kind = context.LET() != null ? VariableKind.Let : VariableKind.Var;
        string name = context.IDENTIFIER().GetText();
        TypeNode? type = context.type() != null ? Visit(context.type()) as TypeNode : null;
        ExpressionNode? init = context.expression() != null ? Visit(context.expression()) as ExpressionNode : null;

        return new VariableDeclarationNode(kind, name, type, init, AccessModifier.Default, GetSpan(context));
    }

    public override IAstNode? VisitConstantDeclaration([NotNull] RustSharpParser.ConstantDeclarationContext context)
    {
        string name = context.IDENTIFIER().GetText();
        TypeNode? type = context.type() != null ? Visit(context.type()) as TypeNode : null;
        var init = Visit(context.expression()) as ExpressionNode;

        return new ConstantDeclarationNode(name, init!, type, AccessModifier.Default, GetSpan(context));
    }

    // ==========================================
    // 5. Types
    // ==========================================

    public override IAstNode? VisitType([NotNull] RustSharpParser.TypeContext context)
    {
        if (context.primitiveType() is { } primCtx)
        {
            TypeNode baseType = new PrimitiveTypeNode(primCtx.GetText(), GetSpan(primCtx));
            int arrays = context.arraySpecifier().Length;
            return arrays > 0 ? new ArrayTypeNode(baseType, arrays, GetSpan(context)) : baseType;
        }

        if (context.referenceType() is { } refCtx)
            return VisitReferenceType(refCtx);

        if (context.qualifiedName() is { } qnameCtx)
        {
            var qname = VisitQualifiedName(qnameCtx) as QualifiedNameNode;
            TypeNode customType = new CustomTypeNode(qname!, GetSpan(qnameCtx));
            int arrays = context.arraySpecifier().Length;
            return arrays > 0 ? new ArrayTypeNode(customType, arrays, GetSpan(context)) : customType;
        }

        return new PrimitiveTypeNode("void", GetSpan(context));
    }

    public override IAstNode? VisitReferenceType([NotNull] RustSharpParser.ReferenceTypeContext context)
    {
        ReferenceKind kind;
        if (context.OWN() != null) kind = ReferenceKind.Own;
        else if (context.REF() != null && context.VAR() != null) kind = ReferenceKind.RefVar;
        else if (context.REF() != null) kind = ReferenceKind.Ref;
        else if (context.AMP() != null && context.VAR() != null) kind = ReferenceKind.AmpVar;
        else kind = ReferenceKind.Amp;

        var innerType = Visit(context.type()) as TypeNode ?? new PrimitiveTypeNode("void");
        return new ReferenceTypeNode(kind, innerType, GetSpan(context));
    }

    // ==========================================
    // 6. Statements
    // ==========================================

    public override IAstNode? VisitStatement([NotNull] RustSharpParser.StatementContext context)
    {
        if (context.block() is { } blk) return Visit(blk);
        if (context.variableDeclaration() is { } vdecl)
        {
            var v = Visit(vdecl) as VariableDeclarationNode;
            return new VariableDeclarationStatementNode(v!, GetSpan(context));
        }
        if (context.constantDeclaration() is { } cdecl)
        {
            var c = Visit(cdecl) as ConstantDeclarationNode;
            return new ConstantDeclarationStatementNode(c!, GetSpan(context));
        }
        if (context.ifStatement() is { } ifStmt) return Visit(ifStmt);
        if (context.whileStatement() is { } whileStmt) return Visit(whileStmt);
        if (context.forStatement() is { } forStmt) return Visit(forStmt);
        if (context.returnStatement() is { } retStmt) return Visit(retStmt);
        if (context.throwStatement() is { } throwStmt) return Visit(throwStmt);
        if (context.tryStatement() is { } tryStmt) return Visit(tryStmt);
        if (context.panicStatement() is { } panicStmt) return Visit(panicStmt);
        if (context.pinStatement() is { } pinStmt) return Visit(pinStmt);
        if (context.nativeBlockStatement() is { } natStmt) return Visit(natStmt);
        if (context.runtimeBlockStatement() is { } runStmt) return Visit(runStmt);
        if (context.assignmentStatement() is { } assignStmt) return Visit(assignStmt);
        if (context.expressionStatement() is { } exprStmt) return Visit(exprStmt);
        if (context.breakStatement() is { } breakStmt) return Visit(breakStmt);
        if (context.continueStatement() is { } contStmt) return Visit(contStmt);

        return null;
    }

    public override IAstNode? VisitBlock([NotNull] RustSharpParser.BlockContext context)
    {
        var statements = new List<StatementNode>();
        foreach (var stmtCtx in context.statement())
        {
            if (Visit(stmtCtx) is StatementNode stmt)
                statements.Add(stmt);
        }
        return new BlockStatementNode(statements, GetSpan(context));
    }

    public override IAstNode? VisitIfStatement([NotNull] RustSharpParser.IfStatementContext context)
    {
        var branches = new List<IfBranchNode>();
        var exprs = context.expression();
        var blocks = context.block();

        // if + else if branches
        for (int i = 0; i < exprs.Length; i++)
        {
            var cond = Visit(exprs[i]) as ExpressionNode;
            var body = Visit(blocks[i]) as BlockStatementNode;
            branches.Add(new IfBranchNode(cond!, body!, GetSpan(blocks[i])));
        }

        // optional else block
        BlockStatementNode? elseBlock = null;
        if (blocks.Length > exprs.Length)
        {
            elseBlock = Visit(blocks[^1]) as BlockStatementNode;
        }

        return new IfStatementNode(branches, elseBlock, GetSpan(context));
    }

    public override IAstNode? VisitWhileStatement([NotNull] RustSharpParser.WhileStatementContext context)
    {
        var cond = Visit(context.expression()) as ExpressionNode;
        var body = Visit(context.block()) as BlockStatementNode;
        return new WhileStatementNode(cond!, body!, GetSpan(context));
    }

    public override IAstNode? VisitForStatement([NotNull] RustSharpParser.ForStatementContext context)
    {
        var idents = context.IDENTIFIER();
        string? indexVar = idents.Length > 1 ? idents[0].GetText() : null;
        string itemVar = idents.Length > 1 ? idents[1].GetText() : idents[0].GetText();
        var iterable = Visit(context.expression()) as ExpressionNode;
        var body = Visit(context.block()) as BlockStatementNode;

        return new ForStatementNode(indexVar, itemVar, iterable!, body!, GetSpan(context));
    }

    public override IAstNode? VisitReturnStatement([NotNull] RustSharpParser.ReturnStatementContext context)
    {
        ExpressionNode? expr = context.expression() != null ? Visit(context.expression()) as ExpressionNode : null;
        return new ReturnStatementNode(expr, GetSpan(context));
    }

    public override IAstNode? VisitThrowStatement([NotNull] RustSharpParser.ThrowStatementContext context)
    {
        var expr = Visit(context.expression()) as ExpressionNode;
        return new ThrowStatementNode(expr!, GetSpan(context));
    }

    public override IAstNode? VisitTryStatement([NotNull] RustSharpParser.TryStatementContext context)
    {
        var tryBlock = Visit(context.block()) as BlockStatementNode;

        var catches = new List<CatchClauseNode>();
        foreach (var cCtx in context.catchClause())
        {
            string varName = cCtx.IDENTIFIER().GetText();
            var errorType = Visit(cCtx.type()) as TypeNode;
            var body = Visit(cCtx.block()) as BlockStatementNode;
            catches.Add(new CatchClauseNode(varName, errorType!, body!, GetSpan(cCtx)));
        }

        FinallyClauseNode? fin = null;
        if (context.finallyClause() is { } finCtx)
        {
            var body = Visit(finCtx.block()) as BlockStatementNode;
            fin = new FinallyClauseNode(body!, GetSpan(finCtx));
        }

        return new TryStatementNode(tryBlock!, catches, fin, GetSpan(context));
    }

    public override IAstNode? VisitPanicStatement([NotNull] RustSharpParser.PanicStatementContext context)
    {
        var expr = Visit(context.expression()) as ExpressionNode;
        return new PanicStatementNode(expr!, GetSpan(context));
    }

    public override IAstNode? VisitPinStatement([NotNull] RustSharpParser.PinStatementContext context)
    {
        bool isVar = context.VAR() != null;
        string varName = context.IDENTIFIER().GetText();
        var expr = Visit(context.expression()) as ExpressionNode;
        var body = Visit(context.block()) as BlockStatementNode;
        return new PinStatementNode(isVar, varName, expr!, body!, GetSpan(context));
    }

    public override IAstNode? VisitNativeBlockStatement([NotNull] RustSharpParser.NativeBlockStatementContext context)
    {
        var body = Visit(context.block()) as BlockStatementNode;
        return new ScopedBlockStatementNode(ExecutionMode.Native, body!, GetSpan(context));
    }

    public override IAstNode? VisitRuntimeBlockStatement([NotNull] RustSharpParser.RuntimeBlockStatementContext context)
    {
        var body = Visit(context.block()) as BlockStatementNode;
        return new ScopedBlockStatementNode(ExecutionMode.Runtime, body!, GetSpan(context));
    }

    public override IAstNode? VisitAssignmentStatement([NotNull] RustSharpParser.AssignmentStatementContext context)
    {
        var target = Visit(context.assignable()) as ExpressionNode;

        if (context.PLUSPLUS() != null)
            return new AssignmentStatementNode(target!, AssignmentOperator.PostfixIncrement, null, GetSpan(context));
        if (context.MINUSMINUS() != null)
            return new AssignmentStatementNode(target!, AssignmentOperator.PostfixDecrement, null, GetSpan(context));

        AssignmentOperator op = AssignmentOperator.Assign;
        if (context.assignmentOperator() is { } opCtx)
        {
            if (opCtx.PLUSEQ() != null) op = AssignmentOperator.AddAssign;
            else if (opCtx.MINUSEQ() != null) op = AssignmentOperator.SubtractAssign;
            else if (opCtx.STAREQ() != null) op = AssignmentOperator.MultiplyAssign;
            else if (opCtx.SLASHEQ() != null) op = AssignmentOperator.DivideAssign;
            else if (opCtx.PERCENTEQ() != null) op = AssignmentOperator.ModuloAssign;
        }

        var val = Visit(context.expression()) as ExpressionNode;
        return new AssignmentStatementNode(target!, op, val, GetSpan(context));
    }

    public override IAstNode? VisitAssignable([NotNull] RustSharpParser.AssignableContext context) =>
        Visit(context.postfixExpression());

    public override IAstNode? VisitExpressionStatement([NotNull] RustSharpParser.ExpressionStatementContext context)
    {
        var expr = Visit(context.expression()) as ExpressionNode;
        return new ExpressionStatementNode(expr!, GetSpan(context));
    }

    public override IAstNode? VisitBreakStatement([NotNull] RustSharpParser.BreakStatementContext context) =>
        new BreakStatementNode(GetSpan(context));

    public override IAstNode? VisitContinueStatement([NotNull] RustSharpParser.ContinueStatementContext context) =>
        new ContinueStatementNode(GetSpan(context));

    // ==========================================
    // 7. Expressions
    // ==========================================

    public override IAstNode? VisitExpression([NotNull] RustSharpParser.ExpressionContext context) =>
        Visit(context.logicalOrExpression());

    public override IAstNode? VisitLogicalOrExpression([NotNull] RustSharpParser.LogicalOrExpressionContext context)
    {
        var subExprs = context.logicalAndExpression();
        var left = Visit(subExprs[0]) as ExpressionNode;

        for (int i = 1; i < subExprs.Length; i++)
        {
            var right = Visit(subExprs[i]) as ExpressionNode;
            left = new BinaryExpressionNode(left!, BinaryOperator.LogicalOr, right!, GetSpan(context));
        }

        return left;
    }

    public override IAstNode? VisitLogicalAndExpression([NotNull] RustSharpParser.LogicalAndExpressionContext context)
    {
        var subExprs = context.equalityExpression();
        var left = Visit(subExprs[0]) as ExpressionNode;

        for (int i = 1; i < subExprs.Length; i++)
        {
            var right = Visit(subExprs[i]) as ExpressionNode;
            left = new BinaryExpressionNode(left!, BinaryOperator.LogicalAnd, right!, GetSpan(context));
        }

        return left;
    }

    public override IAstNode? VisitEqualityExpression([NotNull] RustSharpParser.EqualityExpressionContext context)
    {
        var subExprs = context.relationalExpression();
        var left = Visit(subExprs[0]) as ExpressionNode;

        for (int i = 1; i < subExprs.Length; i++)
        {
            var opToken = context.children[(i * 2) - 1].GetText();
            var op = opToken == "==" ? BinaryOperator.Equal : BinaryOperator.NotEqual;
            var right = Visit(subExprs[i]) as ExpressionNode;
            left = new BinaryExpressionNode(left!, op, right!, GetSpan(context));
        }

        return left;
    }

    public override IAstNode? VisitRelationalExpression([NotNull] RustSharpParser.RelationalExpressionContext context)
    {
        var subExprs = context.rangeExpression();
        var left = Visit(subExprs[0]) as ExpressionNode;

        for (int i = 1; i < subExprs.Length; i++)
        {
            var opToken = context.children[(i * 2) - 1].GetText();
            var op = opToken switch
            {
                "<" => BinaryOperator.LessThan,
                "<=" => BinaryOperator.LessThanOrEqual,
                ">" => BinaryOperator.GreaterThan,
                ">=" => BinaryOperator.GreaterThanOrEqual,
                _ => BinaryOperator.LessThan
            };
            var right = Visit(subExprs[i]) as ExpressionNode;
            left = new BinaryExpressionNode(left!, op, right!, GetSpan(context));
        }

        return left;
    }

    public override IAstNode? VisitRangeExpression([NotNull] RustSharpParser.RangeExpressionContext context)
    {
        var subExprs = context.additiveExpression();
        var left = Visit(subExprs[0]) as ExpressionNode;

        if (subExprs.Length > 1)
        {
            var right = Visit(subExprs[1]) as ExpressionNode;
            return new BinaryExpressionNode(left!, BinaryOperator.Range, right!, GetSpan(context));
        }

        return left;
    }

    public override IAstNode? VisitAdditiveExpression([NotNull] RustSharpParser.AdditiveExpressionContext context)
    {
        var subExprs = context.multiplicativeExpression();
        var left = Visit(subExprs[0]) as ExpressionNode;

        for (int i = 1; i < subExprs.Length; i++)
        {
            var opToken = context.children[(i * 2) - 1].GetText();
            var op = opToken == "+" ? BinaryOperator.Add : BinaryOperator.Subtract;
            var right = Visit(subExprs[i]) as ExpressionNode;
            left = new BinaryExpressionNode(left!, op, right!, GetSpan(context));
        }

        return left;
    }

    public override IAstNode? VisitMultiplicativeExpression([NotNull] RustSharpParser.MultiplicativeExpressionContext context)
    {
        var subExprs = context.unaryExpression();
        var left = Visit(subExprs[0]) as ExpressionNode;

        for (int i = 1; i < subExprs.Length; i++)
        {
            var opToken = context.children[(i * 2) - 1].GetText();
            var op = opToken switch
            {
                "*" => BinaryOperator.Multiply,
                "/" => BinaryOperator.Divide,
                "%" => BinaryOperator.Modulo,
                _ => BinaryOperator.Multiply
            };
            var right = Visit(subExprs[i]) as ExpressionNode;
            left = new BinaryExpressionNode(left!, op, right!, GetSpan(context));
        }

        return left;
    }

    public override IAstNode? VisitUnaryExpression([NotNull] RustSharpParser.UnaryExpressionContext context)
    {
        if (context.PLUS() != null)
        {
            var operand = Visit(context.unaryExpression()) as ExpressionNode;
            return new UnaryExpressionNode(UnaryOperator.Plus, operand!, GetSpan(context));
        }
        if (context.MINUS() != null)
        {
            var operand = Visit(context.unaryExpression()) as ExpressionNode;
            return new UnaryExpressionNode(UnaryOperator.Minus, operand!, GetSpan(context));
        }
        if (context.BANG() != null)
        {
            var operand = Visit(context.unaryExpression()) as ExpressionNode;
            return new UnaryExpressionNode(UnaryOperator.Not, operand!, GetSpan(context));
        }
        if (context.OWN() != null)
        {
            var operand = Visit(context.unaryExpression()) as ExpressionNode;
            return new UnaryExpressionNode(UnaryOperator.Own, operand!, GetSpan(context));
        }
        if (context.REF() != null)
        {
            var op = context.VAR() != null ? UnaryOperator.RefVar : UnaryOperator.Ref;
            var operand = Visit(context.unaryExpression()) as ExpressionNode;
            return new UnaryExpressionNode(op, operand!, GetSpan(context));
        }
        if (context.AMP() != null)
        {
            var op = context.VAR() != null ? UnaryOperator.AmpVar : UnaryOperator.Amp;
            var operand = Visit(context.unaryExpression()) as ExpressionNode;
            return new UnaryExpressionNode(op, operand!, GetSpan(context));
        }
        if (context.STACKALLOC() != null)
        {
            var type = Visit(context.type()) as TypeNode ?? new PrimitiveTypeNode("byte");
            var size = Visit(context.expression()) as ExpressionNode;
            return new StackAllocExpressionNode(type, size!, GetSpan(context));
        }

        return Visit(context.postfixExpression());
    }

    public override IAstNode? VisitPostfixExpression([NotNull] RustSharpParser.PostfixExpressionContext context)
    {
        var expr = Visit(context.primaryExpression()) as ExpressionNode;

        foreach (var opCtx in context.postfixOp())
        {
            if (opCtx is RustSharpParser.InvocationOpContext invCtx)
            {
                var args = new List<ArgumentNode>();
                if (invCtx.argumentList() is { } aList)
                {
                    foreach (var aCtx in aList.argument())
                    {
                        ArgumentModifier mod = ArgumentModifier.None;
                        if (aCtx.REF() != null && aCtx.VAR() != null) mod = ArgumentModifier.RefVar;
                        else if (aCtx.REF() != null) mod = ArgumentModifier.Ref;
                        else if (aCtx.AMP() != null && aCtx.VAR() != null) mod = ArgumentModifier.AmpVar;
                        else if (aCtx.AMP() != null) mod = ArgumentModifier.Amp;
                        else if (aCtx.OWN() != null) mod = ArgumentModifier.Own;

                        var argExpr = Visit(aCtx.expression()) as ExpressionNode;
                        args.Add(new ArgumentNode(mod, argExpr!, GetSpan(aCtx)));
                    }
                }
                expr = new InvocationExpressionNode(expr!, args, GetSpan(invCtx));
            }
            else if (opCtx is RustSharpParser.MemberAccessOpContext memCtx)
            {
                string memberName = memCtx.IDENTIFIER().GetText();
                expr = new MemberAccessExpressionNode(expr!, memberName, GetSpan(memCtx));
            }
            else if (opCtx is RustSharpParser.IndexAccessOpContext idxCtx)
            {
                var idx = Visit(idxCtx.expression()) as ExpressionNode;
                expr = new IndexExpressionNode(expr!, idx!, GetSpan(idxCtx));
            }
            else if (opCtx is RustSharpParser.PostfixIncrementOpContext)
            {
                expr = new UnaryExpressionNode(UnaryOperator.Plus, expr!, GetSpan(opCtx));
            }
            else if (opCtx is RustSharpParser.PostfixDecrementOpContext)
            {
                expr = new UnaryExpressionNode(UnaryOperator.Minus, expr!, GetSpan(opCtx));
            }
        }

        return expr;
    }

    public override IAstNode? VisitPrimaryExpression([NotNull] RustSharpParser.PrimaryExpressionContext context)
    {
        if (context.literal() is { } litCtx)
            return Visit(litCtx);

        if (context.qualifiedName() is { } qnameCtx)
        {
            var qname = VisitQualifiedName(qnameCtx) as QualifiedNameNode;
            if (qname!.Parts.Count == 1)
                return new IdentifierExpressionNode(qname.Parts[0], GetSpan(qnameCtx));
            return qname;
        }

        if (context.expression() is { } parenExpr)
            return Visit(parenExpr);

        if (context.arrayLiteral() is { } arrCtx)
        {
            var elements = arrCtx.expression().Select(e => Visit(e) as ExpressionNode).Where(e => e != null).Select(e => e!).ToList();
            return new ArrayLiteralExpressionNode(elements, GetSpan(arrCtx));
        }

        return null;
    }

    public override IAstNode? VisitLiteral([NotNull] RustSharpParser.LiteralContext context)
    {
        string text = context.GetText();
        var span = GetSpan(context);

        if (context.INTEGER_LITERAL() != null)
        {
            return new LiteralExpressionNode(LiteralKind.Integer, text, text, span);
        }
        if (context.FLOAT_LITERAL() != null)
        {
            return new LiteralExpressionNode(LiteralKind.Float, text, text, span);
        }
        if (context.BOOL_LITERAL() != null)
        {
            bool val = text == "true";
            return new LiteralExpressionNode(LiteralKind.Boolean, val, text, span);
        }
        if (context.CHAR_LITERAL() != null)
        {
            return new LiteralExpressionNode(LiteralKind.Char, text, text, span);
        }
        if (context.STRING_LITERAL() != null)
        {
            return new LiteralExpressionNode(LiteralKind.String, text, text, span);
        }
        if (context.MULTILINE_STRING_LITERAL() != null)
        {
            return new LiteralExpressionNode(LiteralKind.MultilineString, text, text, span);
        }

        return new LiteralExpressionNode(LiteralKind.String, text, text, span);
    }
}
