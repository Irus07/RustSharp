using Xunit;
using Xunit.Abstractions;
using RustSharp.Core.Frontend;
using RustSharp.Core.Frontend.Generated;

namespace RustSharp.Core.Tests;

public class FunctionParserTests
{
    private readonly ITestOutputHelper _output;

    public FunctionParserTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Parse_DefaultAdaptiveFunction_SuccessAndValidTree()
    {
        string code = @"
            fn addNumbers(a: int, b: int) -> int {
                return a + b;
            }
        ";

        var result = CompilerFrontend.Parse(code);
        _output.WriteLine("=== Parse Tree for addNumbers ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));
        Assert.NotNull(result.Root);
        Assert.Empty(result.Diagnostics.Errors.Messages);

        var topLevelItems = result.Root.topLevelItem();
        Assert.Single(topLevelItems);

        var func = topLevelItems[0].functionDeclaration();
        Assert.NotNull(func);

        var sig = func.functionSignature();
        Assert.NotNull(sig);
        Assert.Equal("addNumbers", sig.IDENTIFIER().GetText());
        Assert.Equal("int", sig.type().GetText());

        var paramList = sig.parameterList();
        Assert.NotNull(paramList);
        var parameters = paramList.parameter();
        Assert.Equal(2, parameters.Length);

        Assert.Null(parameters[0].parameterModifier());
        Assert.Equal("a", parameters[0].IDENTIFIER().GetText());
        Assert.Equal("int", parameters[0].type().GetText());

        Assert.Null(parameters[1].parameterModifier());
        Assert.Equal("b", parameters[1].IDENTIFIER().GetText());
        Assert.Equal("int", parameters[1].type().GetText());

        var block = func.block();
        Assert.NotNull(block);
        var statements = block.statement();
        Assert.Single(statements);
        Assert.NotNull(statements[0].returnStatement());
    }

    [Fact]
    public void Parse_NativeFunction_WithRefParameters_SuccessAndValidTree()
    {
        string code = @"
            pub native fn updateMetrics(scoreRef: ref var int, factor: ref float) -> void {
                scoreRef = scoreRef + 10;
            }
        ";

        var result = CompilerFrontend.Parse(code);
        _output.WriteLine("=== Parse Tree for updateMetrics ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));
        Assert.Empty(result.Diagnostics.Errors.Messages);

        var func = result.Root!.topLevelItem(0).functionDeclaration();
        Assert.NotNull(func);

        var modifiers = func.functionModifier();
        Assert.Equal(2, modifiers.Length);
        Assert.NotNull(modifiers[0].PUB());
        Assert.NotNull(modifiers[1].executionMode()?.NATIVE());

        var sig = func.functionSignature();
        Assert.Equal("updateMetrics", sig.IDENTIFIER().GetText());
        Assert.Equal("void", sig.type().GetText());

        var parameters = sig.parameterList().parameter();
        Assert.Equal(2, parameters.Length);

        // scoreRef: ref var int
        Assert.Equal("scoreRef", parameters[0].IDENTIFIER().GetText());
        var refType0 = parameters[0].type().referenceType();
        Assert.NotNull(refType0);
        Assert.NotNull(refType0.REF());
        Assert.NotNull(refType0.VAR());
        Assert.Equal("int", refType0.type().GetText());

        // factor: ref float
        Assert.Equal("factor", parameters[1].IDENTIFIER().GetText());
        var refType1 = parameters[1].type().referenceType();
        Assert.NotNull(refType1);
        Assert.NotNull(refType1.REF());
        Assert.Null(refType1.VAR());
        Assert.Equal("float", refType1.type().GetText());
    }

    [Fact]
    public void Parse_ParameterModifiers_BeforeIdentifier_SuccessAndValidTree()
    {
        string code = @"
            fn mutateData(ref var buffer: byte[], ref factor: float) {
                buffer[0] = 1;
            }
        ";

        var result = CompilerFrontend.Parse(code);
        _output.WriteLine("=== Parse Tree for mutateData ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));
        var func = result.Root!.topLevelItem(0).functionDeclaration();
        var parameters = func.functionSignature().parameterList().parameter();
        Assert.Equal(2, parameters.Length);

        // ref var buffer: byte[]
        var mod0 = parameters[0].parameterModifier();
        Assert.NotNull(mod0?.REF());
        Assert.NotNull(mod0?.VAR());
        Assert.Equal("buffer", parameters[0].IDENTIFIER().GetText());

        // ref factor: float
        var mod1 = parameters[1].parameterModifier();
        Assert.NotNull(mod1?.REF());
        Assert.Null(mod1?.VAR());
        Assert.Equal("factor", parameters[1].IDENTIFIER().GetText());
    }

    [Fact]
    public void Parse_RuntimeFunction_WithOwnParameter_SuccessAndValidTree()
    {
        string code = @"
            runtime fn handlePacket(own packet: Packet) -> bool {
                return true;
            }
        ";

        var result = CompilerFrontend.Parse(code);
        _output.WriteLine("=== Parse Tree for handlePacket ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));
        Assert.Empty(result.Diagnostics.Errors.Messages);

        var func = result.Root!.topLevelItem(0).functionDeclaration();
        Assert.NotNull(func);

        var modifiers = func.functionModifier();
        Assert.Single(modifiers);
        Assert.NotNull(modifiers[0].executionMode()?.RUNTIME());

        var sig = func.functionSignature();
        Assert.Equal("handlePacket", sig.IDENTIFIER().GetText());
        Assert.Equal("bool", sig.type().GetText());

        var parameters = sig.parameterList().parameter();
        Assert.Single(parameters);
        Assert.Equal("packet", parameters[0].IDENTIFIER().GetText());
        Assert.NotNull(parameters[0].parameterModifier()?.OWN());
        Assert.Equal("Packet", parameters[0].type().GetText());
    }

    [Fact]
    public void Parse_ProcedureWithoutReturnType_SuccessAndValidTree()
    {
        string code = @"
            fn printGreeting(name: str) {
                Console.println(""Hello, "" + name);
            }
        ";

        var result = CompilerFrontend.Parse(code);
        _output.WriteLine("=== Parse Tree for printGreeting ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Errors.Messages));
        var func = result.Root!.topLevelItem(0).functionDeclaration();
        Assert.NotNull(func);

        var sig = func.functionSignature();
        Assert.Equal("printGreeting", sig.IDENTIFIER().GetText());
        Assert.Null(sig.type()); // процедура без явного типа возврата
    }

    [Fact]
    public void Parse_InvalidFunctionSyntax_ReportsDiagnostics()
    {
        string brokenCode = @"
            fn brokenFunc(a: int b: int) -> {
                return 42;
            }
        ";

        var result = CompilerFrontend.Parse(brokenCode);
        _output.WriteLine("=== Parse Diagnostics for broken code ===");
        foreach (var diag in result.Diagnostics.Errors.Messages)
        {
            _output.WriteLine(diag.ToString());
        }

        Assert.False(result.IsSuccess);
        Assert.True(result.Diagnostics.HasErrors);
        Assert.NotEmpty(result.Diagnostics.Errors.Messages);
    }
}
