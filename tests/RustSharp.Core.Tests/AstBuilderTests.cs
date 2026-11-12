using Xunit;
using Xunit.Abstractions;
using RustSharp.Core.Ast;
using RustSharp.Core.Ast.Declarations;
using RustSharp.Core.Ast.Expressions;
using RustSharp.Core.Ast.Statements;
using RustSharp.Core.Ast.Types;
using RustSharp.Core.Ast.Visitors;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Tests;

public class AstBuilderTests
{
    private readonly ITestOutputHelper _output;

    public AstBuilderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void BuildAst_AdaptiveFunction_ProducesValidAst()
    {
        string code = @"
            fn addNumbers(a: int, b: int) -> int {
                return a + b;
            }
        ";

        var result = CompilerFrontend.ParseToAst(code);
        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));
        Assert.NotNull(result.Output);

        var astDump = result.Output.DumpAst();
        _output.WriteLine("=== AST for addNumbers ===");
        _output.WriteLine(astDump);

        var program = result.Output;
        Assert.Single(program.Declarations);

        var func = Assert.IsType<FunctionDeclarationNode>(program.Declarations[0]);
        Assert.Equal("addNumbers", func.Name);
        Assert.Equal(FunctionModifiers.None, func.Modifiers);
        Assert.Equal(2, func.Parameters.Count);

        Assert.Equal("a", func.Parameters[0].Name);
        Assert.Equal(ParameterModifier.None, func.Parameters[0].Modifier);
        Assert.Equal("int", ((PrimitiveTypeNode)func.Parameters[0].Type).Name);

        Assert.Equal("b", func.Parameters[1].Name);
        Assert.Equal("int", ((PrimitiveTypeNode)func.Parameters[1].Type).Name);

        Assert.NotNull(func.ReturnType);
        Assert.Equal("int", ((PrimitiveTypeNode)func.ReturnType).Name);

        Assert.Single(func.Body.Statements);
        var ret = Assert.IsType<ReturnStatementNode>(func.Body.Statements[0]);
        var bin = Assert.IsType<BinaryExpressionNode>(ret.Expression);
        Assert.Equal(BinaryOperator.Add, bin.Operator);
        Assert.Equal("a", ((IdentifierExpressionNode)bin.Left).Name);
        Assert.Equal("b", ((IdentifierExpressionNode)bin.Right).Name);
    }

    [Fact]
    public void BuildAst_NativeFunction_WithRefParameters_ProducesValidAst()
    {
        string code = @"
            pub native fn updateMetrics(ref var score: int, ref factor: float) -> void {
                score = score + 10;
            }
        ";

        var result = CompilerFrontend.ParseToAst(code);
        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));

        var program = result.Output!;
        _output.WriteLine("=== AST for updateMetrics ===");
        _output.WriteLine(program.DumpAst());

        var func = Assert.IsType<FunctionDeclarationNode>(program.Declarations[0]);
        Assert.True(func.IsPublic);
        Assert.True(func.IsNative);
        Assert.False(func.IsRuntime);
        Assert.Equal("updateMetrics", func.Name);

        Assert.Equal(2, func.Parameters.Count);
        Assert.Equal(ParameterModifier.RefVar, func.Parameters[0].Modifier);
        Assert.Equal("score", func.Parameters[0].Name);
        Assert.Equal("int", ((PrimitiveTypeNode)func.Parameters[0].Type).Name);

        Assert.Equal(ParameterModifier.Ref, func.Parameters[1].Modifier);
        Assert.Equal("factor", func.Parameters[1].Name);
        Assert.Equal("float", ((PrimitiveTypeNode)func.Parameters[1].Type).Name);

        Assert.Single(func.Body.Statements);
        var assign = Assert.IsType<AssignmentStatementNode>(func.Body.Statements[0]);
        Assert.Equal(AssignmentOperator.Assign, assign.Operator);
        Assert.Equal("score", ((IdentifierExpressionNode)assign.Target).Name);
    }

    [Fact]
    public void BuildAst_RuntimeFunction_WithOwnParameter_ProducesValidAst()
    {
        string code = @"
            runtime fn handlePacket(own packet: Packet) -> bool {
                return true;
            }
        ";

        var result = CompilerFrontend.ParseToAst(code);
        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));

        var program = result.Output!;
        _output.WriteLine("=== AST for handlePacket ===");
        _output.WriteLine(program.DumpAst());

        var func = Assert.IsType<FunctionDeclarationNode>(program.Declarations[0]);
        Assert.True(func.IsRuntime);
        Assert.False(func.IsNative);
        Assert.Equal("handlePacket", func.Name);

        Assert.Single(func.Parameters);
        Assert.Equal(ParameterModifier.Own, func.Parameters[0].Modifier);
        Assert.Equal("packet", func.Parameters[0].Name);
        var customType = Assert.IsType<CustomTypeNode>(func.Parameters[0].Type);
        Assert.Equal("Packet", customType.Name.FullName);
    }

    [Fact]
    public void BuildAst_ClassAndInheritance_ProducesValidAst()
    {
        string code = @"
            pub open class BaseEntity {
                pub var id: int;
                var secretCode: str;

                pub virtual fn describe() -> void {
                    Console.println(""Base"");
                }
            }

            class Player : BaseEntity {
                pub override fn describe() -> void {
                    Console.println(""Player"");
                }
            }
        ";

        var result = CompilerFrontend.ParseToAst(code);
        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));

        var program = result.Output!;
        _output.WriteLine("=== AST for Classes ===");
        _output.WriteLine(program.DumpAst());

        Assert.Equal(2, program.Declarations.Count);

        var baseCls = Assert.IsType<ClassDeclarationNode>(program.Declarations[0]);
        Assert.True(baseCls.IsPublic);
        Assert.True(baseCls.IsOpen);
        Assert.Equal("BaseEntity", baseCls.Name);
        Assert.Null(baseCls.BaseClass);
        Assert.Equal(3, baseCls.Members.Count);

        var field0 = Assert.IsType<FieldDeclarationNode>(baseCls.Members[0]);
        Assert.Equal(AccessModifier.Public, field0.Access);
        Assert.Equal("id", field0.Name);

        var field1 = Assert.IsType<FieldDeclarationNode>(baseCls.Members[1]);
        Assert.Equal(AccessModifier.Default, field1.Access);
        Assert.Equal("secretCode", field1.Name);

        var virtMethod = Assert.IsType<MethodDeclarationNode>(baseCls.Members[2]);
        Assert.True(virtMethod.IsPublic);
        Assert.True(virtMethod.IsVirtual);
        Assert.False(virtMethod.IsOverride);
        Assert.Equal("describe", virtMethod.Name);

        var derivedCls = Assert.IsType<ClassDeclarationNode>(program.Declarations[1]);
        Assert.False(derivedCls.IsOpen);
        Assert.NotNull(derivedCls.BaseClass);
        Assert.Equal("BaseEntity", derivedCls.BaseClass.FullName);
        Assert.Single(derivedCls.Members);

        var overMethod = Assert.IsType<MethodDeclarationNode>(derivedCls.Members[0]);
        Assert.True(overMethod.IsOverride);
        Assert.False(overMethod.IsVirtual);
    }

    [Fact]
    public void BuildAst_Struct_WithValueSemantics_ProducesValidAst()
    {
        string code = @"
            pub struct Vector3 {
                pub var x: float;
                pub var y: float;
                pub var z: float;

                pub static fn zero() -> Vector3 {
                    return Vector3();
                }
            }
        ";

        var result = CompilerFrontend.ParseToAst(code);
        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));

        var program = result.Output!;
        _output.WriteLine("=== AST for Struct ===");
        _output.WriteLine(program.DumpAst());

        var st = Assert.IsType<StructDeclarationNode>(program.Declarations[0]);
        Assert.True(st.IsPublic);
        Assert.Equal("Vector3", st.Name);
        Assert.Equal(4, st.Members.Count);

        var staticMethod = Assert.IsType<MethodDeclarationNode>(st.Members[3]);
        Assert.True(staticMethod.IsPublic);
        Assert.True(staticMethod.IsStatic);
        Assert.Equal("zero", staticMethod.Name);
    }

    [Fact]
    public void BuildAst_ControlFlowAndErrorHandling_ProducesValidAst()
    {
        string code = @"
            fn complexLogic(items: int[]) -> void {
                if items.length == 0 {
                    panic(""Empty"");
                }

                while true {
                    break;
                }

                for i; item in items {
                    Console.println(i, item);
                }

                try {
                    throw CustomError();
                } catch (err: CustomError) {
                    Console.println(""Caught"");
                } finally {
                    Console.println(""Done"");
                }

                native {
                    var buf: byte[] = stackalloc byte[64];
                }

                pin (var ptr = ref items[0]) {
                    process(ptr);
                }
            }
        ";

        var result = CompilerFrontend.ParseToAst(code);
        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));

        var program = result.Output!;
        _output.WriteLine("=== AST for Control Flow ===");
        _output.WriteLine(program.DumpAst());

        var func = Assert.IsType<FunctionDeclarationNode>(program.Declarations[0]);
        var stmts = func.Body.Statements;
        Assert.Equal(6, stmts.Count);

        // 1. if with panic
        var ifStmt = Assert.IsType<IfStatementNode>(stmts[0]);
        Assert.Single(ifStmt.Branches);
        Assert.IsType<PanicStatementNode>(ifStmt.Branches[0].Body.Statements[0]);

        // 2. while with break
        var whileStmt = Assert.IsType<WhileStatementNode>(stmts[1]);
        Assert.IsType<BreakStatementNode>(whileStmt.Body.Statements[0]);

        // 3. for indexed
        var forStmt = Assert.IsType<ForStatementNode>(stmts[2]);
        Assert.Equal("i", forStmt.IndexVariable);
        Assert.Equal("item", forStmt.ItemVariable);

        // 4. try-catch-finally
        var tryStmt = Assert.IsType<TryStatementNode>(stmts[3]);
        Assert.IsType<ThrowStatementNode>(tryStmt.TryBlock.Statements[0]);
        Assert.Single(tryStmt.CatchClauses);
        Assert.Equal("err", tryStmt.CatchClauses[0].VariableName);
        Assert.NotNull(tryStmt.FinallyClause);

        // 5. native block with stackalloc
        var scopedBlock = Assert.IsType<ScopedBlockStatementNode>(stmts[4]);
        Assert.Equal(ExecutionMode.Native, scopedBlock.Mode);
        var vdeclStmt = Assert.IsType<VariableDeclarationStatementNode>(scopedBlock.Body.Statements[0]);
        Assert.IsType<StackAllocExpressionNode>(vdeclStmt.Declaration.Initializer);

        // 6. pin statement
        var pinStmt = Assert.IsType<PinStatementNode>(stmts[5]);
        Assert.True(pinStmt.IsVar);
        Assert.Equal("ptr", pinStmt.VariableName);
    }

    [Fact]
    public void VisitorPattern_CustomMetricVisitor_CountsAstNodesCorrectly()
    {
        string code = @"
            module Game::Core;
            import System::Console;

            pub open class Entity {
                pub var id: int;
                pub fn run() -> void {}
            }

            fn helper() -> int {
                return 42;
            }
        ";

        var program = CompilerFrontend.BuildAst(code);
        Assert.NotNull(program);

        // Используем собственный посетитель для сбора статистики по AST
        var counter = new AstNodeCounterVisitor();
        program.Accept(counter);

        _output.WriteLine($"Visitor Stats: Functions={counter.FunctionCount}, Classes={counter.ClassCount}, Methods={counter.MethodCount}, Fields={counter.FieldCount}");

        Assert.Equal(1, counter.FunctionCount);
        Assert.Equal(1, counter.ClassCount);
        Assert.Equal(1, counter.MethodCount);
        Assert.Equal(1, counter.FieldCount);
        Assert.Equal(1, counter.ModuleCount);
        Assert.Equal(1, counter.ImportCount);
    }

    /// <summary>
    /// Пример кастомного посетителя на базе <see cref="AstVisitorBase"/>
    /// </summary>
    private sealed class AstNodeCounterVisitor : AstVisitorBase
    {
        public int ModuleCount { get; private set; }
        public int ImportCount { get; private set; }
        public int ClassCount { get; private set; }
        public int FunctionCount { get; private set; }
        public int MethodCount { get; private set; }
        public int FieldCount { get; private set; }

        public override void Visit(ModuleDeclarationNode node)
        {
            ModuleCount++;
            base.Visit(node);
        }

        public override void Visit(ImportDeclarationNode node)
        {
            ImportCount++;
            base.Visit(node);
        }

        public override void Visit(ClassDeclarationNode node)
        {
            ClassCount++;
            base.Visit(node);
        }

        public override void Visit(FunctionDeclarationNode node)
        {
            FunctionCount++;
            base.Visit(node);
        }

        public override void Visit(MethodDeclarationNode node)
        {
            MethodCount++;
            base.Visit(node);
        }

        public override void Visit(FieldDeclarationNode node)
        {
            FieldCount++;
            base.Visit(node);
        }
    }
}
