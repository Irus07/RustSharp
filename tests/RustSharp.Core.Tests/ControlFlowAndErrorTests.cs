using Xunit;
using Xunit.Abstractions;
using RustSharp.Core.Frontend;
using RustSharp.Core.Frontend.Generated;

namespace RustSharp.Core.Tests;

public class ControlFlowAndErrorTests
{
    private readonly ITestOutputHelper _output;

    public ControlFlowAndErrorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Parse_IfElse_AndLoops_SuccessAndValidTree()
    {
        string code = @"
            fn testControlFlow() -> void {
                var score = 85;

                if score >= 90 {
                    Console.println(""A"");
                } else if score >= 60 {
                    Console.println(""B"");
                } else {
                    Console.println(""C"");
                }

                while score > 0 {
                    score--;
                }

                for item in items {
                    Console.println(item);
                }

                for i; item in items {
                    Console.println(i, item);
                }
            }
        ";

        var result = CompilerFrontend.Parse(code);
        _output.WriteLine("=== Parse Tree for testControlFlow ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);

        var func = result.Root!.topLevelItem(0).functionDeclaration();
        var stmts = func.block().statement();
        Assert.Equal(5, stmts.Length);

        // 1. var score = 85;
        var varDecl = stmts[0].variableDeclaration();
        Assert.NotNull(varDecl);
        Assert.Equal("score", varDecl.IDENTIFIER().GetText());

        // 2. if-else if-else
        var ifStmt = stmts[1].ifStatement();
        Assert.NotNull(ifStmt);
        // Должно быть 2 условия (score >= 90 и score >= 60) и 3 блока (then, else if, else)
        Assert.Equal(2, ifStmt.expression().Length);
        Assert.Equal(3, ifStmt.block().Length);

        // 3. while score > 0
        var whileStmt = stmts[2].whileStatement();
        Assert.NotNull(whileStmt);
        Assert.Equal("score>0", whileStmt.expression().GetText().Replace(" ", ""));

        // 4. for item in items
        var forSingle = stmts[3].forStatement();
        Assert.NotNull(forSingle);
        Assert.Single(forSingle.IDENTIFIER());
        Assert.Equal("item", forSingle.IDENTIFIER(0).GetText());
        Assert.Equal("items", forSingle.expression().GetText());

        // 5. for i; item in items
        var forIndexed = stmts[4].forStatement();
        Assert.NotNull(forIndexed);
        Assert.Equal(2, forIndexed.IDENTIFIER().Length);
        Assert.Equal("i", forIndexed.IDENTIFIER(0).GetText());
        Assert.Equal("item", forIndexed.IDENTIFIER(1).GetText());
        Assert.Equal("items", forIndexed.expression().GetText());
    }

    [Fact]
    public void Parse_ErrorHandling_ThrowTryCatchFinally_SuccessAndValidTree()
    {
        string code = @"
            fn safeDivide(a: int, b: int) -> int {
                try {
                    if b == 0 {
                        throw DivideByZeroError();
                    }
                    return a / b;
                } catch (err: DivideByZeroError) {
                    Console.println(""Divide error"");
                    return -1;
                } catch (err: Error) {
                    Console.println(""General error"");
                    return 0;
                } finally {
                    Console.println(""Cleanup done"");
                }
            }
        ";

        var result = CompilerFrontend.Parse(code);
        _output.WriteLine("=== Parse Tree for safeDivide ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);

        var func = result.Root!.topLevelItem(0).functionDeclaration();
        var stmts = func.block().statement();
        Assert.Single(stmts);

        var tryStmt = stmts[0].tryStatement();
        Assert.NotNull(tryStmt);

        // Try block: contains if with throw
        var tryBodyStmts = tryStmt.block().statement();
        Assert.Equal(2, tryBodyStmts.Length);
        var ifStmtInTry = tryBodyStmts[0].ifStatement();
        var throwStmt = ifStmtInTry.block(0).statement(0).throwStatement();
        Assert.NotNull(throwStmt);
        Assert.Equal("DivideByZeroError()", throwStmt.expression().GetText());

        // Catches
        var catches = tryStmt.catchClause();
        Assert.Equal(2, catches.Length);

        // catch 1: DivideByZeroError
        Assert.Equal("err", catches[0].IDENTIFIER().GetText());
        Assert.Equal("DivideByZeroError", catches[0].type().GetText());

        // catch 2: Error
        Assert.Equal("err", catches[1].IDENTIFIER().GetText());
        Assert.Equal("Error", catches[1].type().GetText());

        // Finally
        var finallyClause = tryStmt.finallyClause();
        Assert.NotNull(finallyClause);
        Assert.NotEmpty(finallyClause.block().statement());
    }

    [Fact]
    public void Parse_ScopedBlocks_NativeRuntimePinPanic_SuccessAndValidTree()
    {
        string code = @"
            fn advancedFeatures(buf: ManagedBuffer) -> void {
                native {
                    var rawBuf: byte[] = stackalloc byte[128];
                    rawBuf[0] = 0xFF;
                }

                runtime {
                    Console.println(""In runtime"");
                }

                pin (var ptr = ref buf[0]) {
                    writeHardware(ptr);
                }

                if buf.Length == 0 {
                    panic(""Buffer cannot be empty"");
                }
            }
        ";

        var result = CompilerFrontend.Parse(code);
        _output.WriteLine("=== Parse Tree for advancedFeatures ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);

        var func = result.Root!.topLevelItem(0).functionDeclaration();
        var stmts = func.block().statement();
        Assert.Equal(4, stmts.Length);

        // 1. native { ... }
        var nativeBlock = stmts[0].nativeBlockStatement();
        Assert.NotNull(nativeBlock);
        Assert.Equal(2, nativeBlock.block().statement().Length);

        // 2. runtime { ... }
        var runtimeBlock = stmts[1].runtimeBlockStatement();
        Assert.NotNull(runtimeBlock);
        Assert.Single(runtimeBlock.block().statement());

        // 3. pin (var ptr = ref buf[0]) { ... }
        var pinStmt = stmts[2].pinStatement();
        Assert.NotNull(pinStmt);
        Assert.Equal("ptr", pinStmt.IDENTIFIER().GetText());
        Assert.NotNull(pinStmt.VAR());
        Assert.Equal("refbuf[0]", pinStmt.expression().GetText().Replace(" ", ""));

        // 4. if ... { panic(...) }
        var ifStmt = stmts[3].ifStatement();
        var panicStmt = ifStmt.block(0).statement(0).panicStatement();
        Assert.NotNull(panicStmt);
        Assert.Equal("\"Buffer cannot be empty\"", panicStmt.expression().GetText());
    }
}
