lexer grammar Lexer;

// ==========================================
// Keywords - Modules & Visibility
// ==========================================
MODULE      : 'module' ;
IMPORT      : 'import' ;
PUB         : 'pub' ;

// ==========================================
// Keywords - OOP & Types
// ==========================================
OPEN        : 'open' ;
CLASS       : 'class' ;
STRUCT      : 'struct' ;
STATIC      : 'static' ;
VIRTUAL     : 'virtual' ;
OVERRIDE    : 'override' ;

// ==========================================
// Keywords - Functions & Execution Modes
// ==========================================
FN          : 'fn' ;
NATIVE      : 'native' ;
RUNTIME     : 'runtime' ;

// ==========================================
// Keywords - Variables & Bindings
// ==========================================
VAR         : 'var' ;
LET         : 'let' ;
CONST       : 'const' ;

// ==========================================
// Keywords - Control Flow
// ==========================================
IF          : 'if' ;
ELSE        : 'else' ;
WHILE       : 'while' ;
FOR         : 'for' ;
IN          : 'in' ;
BREAK       : 'break' ;
CONTINUE    : 'continue' ;
RETURN      : 'return' ;

// ==========================================
// Keywords - Error Handling
// ==========================================
THROW       : 'throw' ;
TRY         : 'try' ;
CATCH       : 'catch' ;
FINALLY     : 'finally' ;
PANIC       : 'panic' ;

// ==========================================
// Keywords - Memory & References
// ==========================================
PIN         : 'pin' ;
REF         : 'ref' ;
OWN         : 'own' ;
STACKALLOC  : 'stackalloc' ;

// ==========================================
// Keywords - Primitive Types
// ==========================================
TYPE_VOID   : 'void' ;
TYPE_BOOL   : 'bool' ;
TYPE_CHAR   : 'char' ;
TYPE_STR    : 'str' ;
TYPE_STRING : 'string' ;
TYPE_INT    : 'int' ;
TYPE_I16    : 'i16' ;
TYPE_I32    : 'i32' ;
TYPE_I64    : 'i64' ;
TYPE_LONG   : 'long' ;
TYPE_BYTE   : 'byte' ;
TYPE_U8     : 'u8' ;
TYPE_U16    : 'u16' ;
TYPE_U32    : 'u32' ;
TYPE_U64    : 'u64' ;
TYPE_FLOAT  : 'float' ;
TYPE_DOUBLE : 'double' ;
TYPE_F16    : 'f16' ;
TYPE_F32    : 'f32' ;
TYPE_F64    : 'f64' ;

// ==========================================
// Literals
// ==========================================
BOOL_LITERAL
    : 'true'
    | 'false'
    ;

INTEGER_LITERAL
    : [0-9] ('_'? [0-9])*
    | '0' [xX] [0-9a-fA-F] ('_'? [0-9a-fA-F])*
    | '0' [bB] [01] ('_'? [01])*
    ;

FLOAT_LITERAL
    : [0-9] ('_'? [0-9])* '.' [0-9] ('_'? [0-9])* [fF]?
    | [0-9] ('_'? [0-9])* [fF]
    ;

CHAR_LITERAL
    : '\'' (~['\\\r\n] | '\\' .) '\''
    ;

STRING_LITERAL
    : '"' (~["\\\r\n] | '\\' .)* '"'
    ;

MULTILINE_STRING_LITERAL
    : '`' (~[`\\] | '\\' .)* '`'
    ;

// ==========================================
// Punctuation & Operators
// ==========================================
COLONCOLON  : '::' ;
ARROW       : '->' ;
DOTDOT      : '..' ;
PLUSPLUS    : '++' ;
MINUSMINUS  : '--' ;
PLUSEQ      : '+=' ;
MINUSEQ     : '-=' ;
STAREQ      : '*=' ;
SLASHEQ     : '/=' ;
PERCENTEQ   : '%=' ;
EQEQ        : '==' ;
NOTEQ       : '!=' ;
LTEQ        : '<=' ;
GTEQ        : '>=' ;
ANDAND      : '&&' ;
OROR        : '||' ;

ASSIGN      : '=' ;
LT          : '<' ;
GT          : '>' ;
PLUS        : '+' ;
MINUS       : '-' ;
STAR        : '*' ;
SLASH       : '/' ;
PERCENT     : '%' ;
BANG        : '!' ;
AMP         : '&' ;

COLON       : ':' ;
SEMI        : ';' ;
COMMA       : ',' ;
DOT         : '.' ;

LPAREN      : '(' ;
RPAREN      : ')' ;
LBRACE      : '{' ;
RBRACE      : '}' ;
LBRACK      : '[' ;
RBRACK      : ']' ;

// ==========================================
// Identifiers
// ==========================================
IDENTIFIER
    : [a-zA-Z_] [a-zA-Z0-9_]*
    ;

// ==========================================
// Comments & Whitespace
// ==========================================
LINE_COMMENT
    : '//' ~[\r\n]* -> skip
    ;

BLOCK_COMMENT
    : '/*' .*? '*/' -> skip
    ;

WS
    : [ \t\r\n]+ -> skip
    ;
