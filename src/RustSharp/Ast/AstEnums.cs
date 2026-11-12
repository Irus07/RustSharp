namespace RustSharp.Core.Ast;

/// <summary>
/// Модификатор доступа (по умолчанию private/package-internal, либо pub).
/// </summary>
public enum AccessModifier
{
    Default,
    Public
}

/// <summary>
/// Режим выполнения функции или блока кода.
/// </summary>
public enum ExecutionMode
{
    /// <summary>Адаптивный режим по умолчанию (JIT/AOT).</summary>
    Adaptive,

    /// <summary>Нативный режим без GC и оверхеда среды исполнения.</summary>
    Native,

    /// <summary>Управляемый режим с поддержкой GC и высокоуровневых сервисов.</summary>
    Runtime
}

/// <summary>
/// Модификатор передачи параметра функции.
/// </summary>
public enum ParameterModifier
{
    None,
    Own,
    Ref,
    RefVar,
    Amp,
    AmpVar
}

/// <summary>
/// Модификаторы для объявления классов.
/// </summary>
[Flags]
public enum ClassModifiers
{
    None = 0,
    Public = 1 << 0,
    Open = 1 << 1
}

/// <summary>
/// Модификаторы для методов классов и структур.
/// </summary>
[Flags]
public enum MemberModifiers
{
    None = 0,
    Public = 1 << 0,
    Static = 1 << 1,
    Virtual = 1 << 2,
    Override = 1 << 3,
    Native = 1 << 4,
    Runtime = 1 << 5
}

/// <summary>
/// Модификаторы функций верхнего уровня.
/// </summary>
[Flags]
public enum FunctionModifiers
{
    None = 0,
    Public = 1 << 0,
    Static = 1 << 1,
    Native = 1 << 2,
    Runtime = 1 << 3
}

/// <summary>
/// Вид переменной: изменяемая (var) или неизменяемая привязка (let).
/// </summary>
public enum VariableKind
{
    Var,
    Let
}

/// <summary>
/// Виды бинарных операторов.
/// </summary>
public enum BinaryOperator
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LogicalAnd,
    LogicalOr,
    Range
}

/// <summary>
/// Виды унарных операторов.
/// </summary>
public enum UnaryOperator
{
    Plus,
    Minus,
    Not,
    Own,
    Ref,
    RefVar,
    Amp,
    AmpVar
}

/// <summary>
/// Операторы присваивания и изменения значений.
/// </summary>
public enum AssignmentOperator
{
    Assign,
    AddAssign,
    SubtractAssign,
    MultiplyAssign,
    DivideAssign,
    ModuloAssign,
    PostfixIncrement,
    PostfixDecrement
}

/// <summary>
/// Вид литерального значения.
/// </summary>
public enum LiteralKind
{
    Integer,
    Float,
    Boolean,
    Char,
    String,
    MultilineString
}

/// <summary>
/// Вид модификатора аргумента при вызове (например, func(ref var x)).
/// </summary>
public enum ArgumentModifier
{
    None,
    Ref,
    RefVar,
    Amp,
    AmpVar,
    Own
}

/// <summary>
/// Вид ссылочного типа данных.
/// </summary>
public enum ReferenceKind
{
    Ref,
    RefVar,
    Amp,
    AmpVar,
    Own
}
