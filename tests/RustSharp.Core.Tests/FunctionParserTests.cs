using Xunit;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Tests;

public class FunctionParserTests
{
    [Fact]
    public void Parse_DefaultAdaptiveFunction_Success()
    {
        string code = @"
            fn addNumbers(a: int, b: int) -> int {
                return a + b;
            }
        ";

        var result = CompilerFrontend.Parse(code);

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.NotNull(result.Root);
        Assert.Empty(result.Diagnostics.Diagnostics);
    }

    [Fact]
    public void Parse_NativeFunction_WithRefParameters_Success()
    {
        string code = @"
            pub native fn updateMetrics(scoreRef: ref var int, factor: ref float) -> void {
                scoreRef = scoreRef + 10;
            }
        ";

        var result = CompilerFrontend.Parse(code);

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);
    }

    [Fact]
    public void Parse_RuntimeFunction_WithOwnParameter_Success()
    {
        string code = @"
            runtime fn handlePacket(own packet: Packet) -> bool {
                return true;
            }
        ";

        var result = CompilerFrontend.Parse(code);

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);
    }

    [Fact]
    public void Parse_ProcedureWithoutReturnType_Success()
    {
        string code = @"
            fn printGreeting(name: str) {
                Console.println(""Hello, "" + name);
            }
        ";

        var result = CompilerFrontend.Parse(code);

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
    }
}
