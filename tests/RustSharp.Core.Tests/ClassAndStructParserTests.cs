using Xunit;
using RustSharp.Core.Frontend;

namespace RustSharp.Core.Tests;

public class ClassAndStructParserTests
{
    [Fact]
    public void Parse_OpenClass_WithVirtualMethod_Success()
    {
        string code = @"
            pub open class BaseEntity {
                pub var id: int;
                var secretCode: str;

                pub virtual fn describe() -> void {
                    Console.println(""Base Entity"");
                }
            }
        ";

        var result = CompilerFrontend.Parse(code);

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);
    }

    [Fact]
    public void Parse_DerivedClass_WithOverride_Success()
    {
        string code = @"
            class UserPlayer : BaseEntity {
                pub var nickname: str;

                pub override fn describe() -> void {
                    Console.println(""Player: "" + nickname);
                }
            }
        ";

        var result = CompilerFrontend.Parse(code);

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);
    }

    [Fact]
    public void Parse_Struct_WithValueSemantics_Success()
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

        var result = CompilerFrontend.Parse(code);

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);
    }
}
