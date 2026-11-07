namespace RustSharp.Core.Frontend;

/// <summary>
/// Представляет позицию символа в исходном коде.
/// </summary>
public readonly record struct SourceLocation(int Line, int Column, int Offset)
{
    public static SourceLocation None => new(0, 0, 0);

    public override string ToString() => $"{Line}:{Column}";
}

/// <summary>
/// Представляет диапазон символов в исходном коде.
/// </summary>
public readonly record struct SourceSpan(SourceLocation Start, SourceLocation End, string? FilePath = null)
{
    public static SourceSpan None => new(SourceLocation.None, SourceLocation.None);

    public override string ToString() =>
        FilePath != null ? $"{FilePath}({Start}..{End})" : $"{Start}..{End}";
}
