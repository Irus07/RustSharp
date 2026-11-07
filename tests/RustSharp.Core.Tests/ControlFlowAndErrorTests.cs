using Xunit;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Tests;

public class ControlFlowAndErrorTests
{
    [Fact]
    public void Parse_IfElse_AndLoops_Success()
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

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);
    }

    [Fact]
    public void Parse_ErrorHandling_ThrowTryCatchFinally_Success()
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

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);
    }

    [Fact]
    public void Parse_ScopedBlocks_NativeRuntimePinPanic_Success()
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

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);
    }
}
