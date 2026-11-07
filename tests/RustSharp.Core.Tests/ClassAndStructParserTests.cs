using Xunit;
using Xunit.Abstractions;
using RustSharp.Core.Frontend;
using RustSharp.Core.Frontend.Generated;

namespace RustSharp.Core.Tests;

public class ClassAndStructParserTests
{
    private readonly ITestOutputHelper _output;

    public ClassAndStructParserTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Parse_OpenClass_WithVirtualMethod_SuccessAndValidTree()
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
        _output.WriteLine("=== Parse Tree for BaseEntity ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        Assert.Empty(result.Diagnostics.Diagnostics);

        var cls = result.Root!.topLevelItem(0).classDeclaration();
        Assert.NotNull(cls);
        Assert.NotNull(cls.PUB());
        Assert.NotNull(cls.OPEN());
        Assert.Equal("BaseEntity", cls.IDENTIFIER().GetText());
        Assert.Null(cls.qualifiedName()); // нет базового класса

        var members = cls.classMember();
        Assert.Equal(3, members.Length);

        // member 0: pub var id: int;
        var field0 = members[0].fieldDeclaration();
        Assert.NotNull(field0);
        Assert.NotNull(field0.PUB());
        Assert.Equal("id", field0.IDENTIFIER().GetText());
        Assert.Equal("int", field0.type().GetText());

        // member 1: var secretCode: str;
        var field1 = members[1].fieldDeclaration();
        Assert.NotNull(field1);
        Assert.Null(field1.PUB());
        Assert.Equal("secretCode", field1.IDENTIFIER().GetText());
        Assert.Equal("str", field1.type().GetText());

        // member 2: pub virtual fn describe() -> void
        var method = members[2].methodDeclaration();
        Assert.NotNull(method);
        var modifiers = method.memberModifier();
        Assert.Equal(2, modifiers.Length);
        Assert.NotNull(modifiers[0].PUB());
        Assert.NotNull(modifiers[1].VIRTUAL());

        var sig = method.functionSignature();
        Assert.Equal("describe", sig.IDENTIFIER().GetText());
        Assert.Equal("void", sig.type().GetText());
    }

    [Fact]
    public void Parse_DerivedClass_WithOverride_SuccessAndValidTree()
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
        _output.WriteLine("=== Parse Tree for UserPlayer ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        var cls = result.Root!.topLevelItem(0).classDeclaration();
        Assert.NotNull(cls);
        Assert.Null(cls.OPEN());
        Assert.Equal("UserPlayer", cls.IDENTIFIER().GetText());
        Assert.Equal("BaseEntity", cls.qualifiedName().GetText());

        var members = cls.classMember();
        Assert.Equal(2, members.Length);

        var field = members[0].fieldDeclaration();
        Assert.NotNull(field);
        Assert.Equal("nickname", field.IDENTIFIER().GetText());

        var method = members[1].methodDeclaration();
        Assert.NotNull(method);
        var modifiers = method.memberModifier();
        Assert.Equal(2, modifiers.Length);
        Assert.NotNull(modifiers[0].PUB());
        Assert.NotNull(modifiers[1].OVERRIDE());
        Assert.Equal("describe", method.functionSignature().IDENTIFIER().GetText());
    }

    [Fact]
    public void Parse_Struct_WithValueSemantics_SuccessAndValidTree()
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
        _output.WriteLine("=== Parse Tree for Vector3 ===");
        _output.WriteLine(result.DumpTree());

        Assert.True(result.IsSuccess, string.Join("; ", result.Diagnostics.Diagnostics));
        var st = result.Root!.topLevelItem(0).structDeclaration();
        Assert.NotNull(st);
        Assert.NotNull(st.PUB());
        Assert.Equal("Vector3", st.IDENTIFIER().GetText());

        var members = st.structMember();
        Assert.Equal(4, members.Length);

        // Поля x, y, z
        Assert.Equal("x", members[0].fieldDeclaration().IDENTIFIER().GetText());
        Assert.Equal("y", members[1].fieldDeclaration().IDENTIFIER().GetText());
        Assert.Equal("z", members[2].fieldDeclaration().IDENTIFIER().GetText());

        // Метод static fn zero() -> Vector3
        var method = members[3].methodDeclaration();
        Assert.NotNull(method);
        Assert.NotNull(method.memberModifier()[0].PUB());
        Assert.NotNull(method.memberModifier()[1].STATIC());
        Assert.Equal("zero", method.functionSignature().IDENTIFIER().GetText());
        Assert.Equal("Vector3", method.functionSignature().type().GetText());
    }

    [Fact]
    public void Parse_BrokenClassSyntax_ReportsDiagnostics()
    {
        string broken = @"
            class IncompleteClass {
                pub var x: int
            }
        ";

        var result = CompilerFrontend.Parse(broken);
        _output.WriteLine("=== Parse Diagnostics for broken class ===");
        foreach (var diag in result.Diagnostics.Diagnostics)
        {
            _output.WriteLine(diag.ToString());
        }

        Assert.False(result.IsSuccess);
        Assert.True(result.Diagnostics.HasErrors);
    }
}
