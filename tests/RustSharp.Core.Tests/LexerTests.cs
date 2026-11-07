using Xunit;
using RustSharp.Core.Frontend;
using RustSharp.Core.Frontend.Generated;

namespace RustSharp.Core.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenize_Keywords_RecognizedCorrectly()
    {
        string source = "module import pub open class struct static virtual override fn native runtime var let const if else while for in break continue return throw try catch finally panic pin ref own stackalloc void";

        var tokens = CompilerFrontend.Tokenize(source);

        Assert.Contains(tokens, t => t.Type == RustSharpLexer.MODULE);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.IMPORT);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.PUB);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.OPEN);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.CLASS);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.STRUCT);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.STATIC);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.VIRTUAL);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.OVERRIDE);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.FN);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.NATIVE);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.RUNTIME);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.VAR);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.LET);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.CONST);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.THROW);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.TRY);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.CATCH);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.FINALLY);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.PANIC);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.PIN);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.REF);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.OWN);
    }

    [Theory]
    [InlineData("42", RustSharpLexer.INTEGER_LITERAL)]
    [InlineData("1_000_000", RustSharpLexer.INTEGER_LITERAL)]
    [InlineData("0xFF", RustSharpLexer.INTEGER_LITERAL)]
    [InlineData("0b1010", RustSharpLexer.INTEGER_LITERAL)]
    [InlineData("3.14", RustSharpLexer.FLOAT_LITERAL)]
    [InlineData("10.5f", RustSharpLexer.FLOAT_LITERAL)]
    [InlineData("true", RustSharpLexer.BOOL_LITERAL)]
    [InlineData("false", RustSharpLexer.BOOL_LITERAL)]
    [InlineData("'R'", RustSharpLexer.CHAR_LITERAL)]
    [InlineData("\"Hello\"", RustSharpLexer.STRING_LITERAL)]
    [InlineData("`Multiline\nString`", RustSharpLexer.MULTILINE_STRING_LITERAL)]
    public void Tokenize_Literals_Recognized(string literal, int expectedTokenType)
    {
        var tokens = CompilerFrontend.Tokenize(literal);

        Assert.NotEmpty(tokens);
        Assert.Equal(expectedTokenType, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_Comments_AreSkipped()
    {
        string source = @"
            // Однострочный комментарий
            var x = 10;
            /* Многострочный
               комментарий */
            var y = 20;
        ";

        var tokens = CompilerFrontend.Tokenize(source);

        // Должны остаться только значащие токены: var, x, =, 10, ;, var, y, =, 20, ;, EOF
        Assert.DoesNotContain(tokens, t => t.Type == RustSharpLexer.LINE_COMMENT);
        Assert.DoesNotContain(tokens, t => t.Type == RustSharpLexer.BLOCK_COMMENT);
        Assert.Contains(tokens, t => t.Type == RustSharpLexer.VAR);
    }
}
