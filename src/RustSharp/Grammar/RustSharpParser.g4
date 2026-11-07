parser grammar RustSharpParser;

options {
    tokenVocab = RustSharpLexer;
}

// ==========================================
// 1. Program & Module Level
// ==========================================

program
    : moduleDeclaration? importDeclaration* topLevelItem* EOF
    ;

moduleDeclaration
    : MODULE qualifiedName SEMI
    ;

importDeclaration
    : IMPORT qualifiedName SEMI
    ;

qualifiedName
    : IDENTIFIER (COLONCOLON IDENTIFIER)*
    ;

topLevelItem
    : functionDeclaration
    | classDeclaration
    | structDeclaration
    | globalVariableDeclaration
    ;

// ==========================================
// 2. OOP Declarations (Class & Struct)
// ==========================================

classDeclaration
    : PUB? OPEN? CLASS IDENTIFIER (COLON qualifiedName)? LBRACE classMember* RBRACE
    ;

classMember
    : fieldDeclaration
    | methodDeclaration
    | constructorDeclaration
    ;

structDeclaration
    : PUB? STRUCT IDENTIFIER LBRACE structMember* RBRACE
    ;

structMember
    : fieldDeclaration
    | methodDeclaration
    ;

fieldDeclaration
    : PUB? VAR IDENTIFIER (COLON type)? (ASSIGN expression)? SEMI
    ;

constructorDeclaration
    : PUB? FN IDENTIFIER LPAREN parameterList? RPAREN block
    ;

methodDeclaration
    : memberModifier* functionSignature block
    ;

memberModifier
    : PUB
    | STATIC
    | VIRTUAL
    | OVERRIDE
    | executionMode
    ;

// ==========================================
// 3. Functions
// ==========================================

functionDeclaration
    : functionModifier* functionSignature block
    ;

functionModifier
    : PUB
    | STATIC
    | executionMode
    ;

executionMode
    : NATIVE
    | RUNTIME
    ;

functionSignature
    : FN IDENTIFIER LPAREN parameterList? RPAREN (ARROW type)?
    ;

parameterList
    : parameter (COMMA parameter)*
    ;

parameter
    : parameterModifier? IDENTIFIER COLON type
    ;

parameterModifier
    : OWN
    | REF VAR
    | REF
    | AMP VAR
    | AMP
    ;

// ==========================================
// 4. Variables & Constants
// ==========================================

globalVariableDeclaration
    : PUB? (variableDeclaration | constantDeclaration)
    ;

variableDeclaration
    : VAR IDENTIFIER (COLON type)? (ASSIGN expression)? SEMI
    | LET IDENTIFIER (COLON type)? ASSIGN expression SEMI
    ;

constantDeclaration
    : CONST IDENTIFIER (COLON type)? ASSIGN expression SEMI
    ;

// ==========================================
// 5. Types
// ==========================================

type
    : primitiveType arraySpecifier*
    | referenceType
    | qualifiedName arraySpecifier*
    ;

arraySpecifier
    : LBRACK RBRACK
    ;

referenceType
    : (REF VAR | REF | AMP VAR | AMP) type
    | OWN type
    ;

primitiveType
    : TYPE_VOID
    | TYPE_BOOL
    | TYPE_CHAR
    | TYPE_STR
    | TYPE_STRING
    | TYPE_INT
    | TYPE_I16
    | TYPE_I32
    | TYPE_I64
    | TYPE_LONG
    | TYPE_BYTE
    | TYPE_U8
    | TYPE_U16
    | TYPE_U32
    | TYPE_U64
    | TYPE_FLOAT
    | TYPE_DOUBLE
    | TYPE_F16
    | TYPE_F32
    | TYPE_F64
    ;

// ==========================================
// 6. Statements
// ==========================================

statement
    : block
    | variableDeclaration
    | constantDeclaration
    | ifStatement
    | whileStatement
    | forStatement
    | returnStatement
    | throwStatement
    | tryStatement
    | panicStatement
    | pinStatement
    | nativeBlockStatement
    | runtimeBlockStatement
    | assignmentStatement
    | expressionStatement
    | breakStatement
    | continueStatement
    ;

block
    : LBRACE statement* RBRACE
    ;

ifStatement
    : IF expression block (ELSE IF expression block)* (ELSE block)?
    ;

whileStatement
    : WHILE expression block
    ;

forStatement
    : FOR (IDENTIFIER SEMI)? IDENTIFIER IN expression block
    ;

returnStatement
    : RETURN expression? SEMI
    ;

throwStatement
    : THROW expression SEMI
    ;

tryStatement
    : TRY block catchClause+ finallyClause?
    ;

catchClause
    : CATCH LPAREN IDENTIFIER COLON type RPAREN block
    ;

finallyClause
    : FINALLY block
    ;

panicStatement
    : PANIC LPAREN expression RPAREN SEMI
    ;

pinStatement
    : PIN LPAREN (VAR | LET)? IDENTIFIER ASSIGN expression RPAREN block
    ;

nativeBlockStatement
    : NATIVE block
    ;

runtimeBlockStatement
    : RUNTIME block
    ;

assignmentStatement
    : assignable assignmentOperator expression SEMI
    | assignable (PLUSPLUS | MINUSMINUS) SEMI
    ;

assignable
    : postfixExpression
    ;

assignmentOperator
    : ASSIGN
    | PLUSEQ
    | MINUSEQ
    | STAREQ
    | SLASHEQ
    | PERCENTEQ
    ;

expressionStatement
    : expression SEMI
    ;

breakStatement
    : BREAK SEMI
    ;

continueStatement
    : CONTINUE SEMI
    ;

// ==========================================
// 7. Expressions
// ==========================================

expression
    : logicalOrExpression
    ;

logicalOrExpression
    : logicalAndExpression (OROR logicalAndExpression)*
    ;

logicalAndExpression
    : equalityExpression (ANDAND equalityExpression)*
    ;

equalityExpression
    : relationalExpression ((EQEQ | NOTEQ) relationalExpression)*
    ;

relationalExpression
    : rangeExpression ((LT | LTEQ | GT | GTEQ) rangeExpression)*
    ;

rangeExpression
    : additiveExpression (DOTDOT additiveExpression)?
    ;

additiveExpression
    : multiplicativeExpression ((PLUS | MINUS) multiplicativeExpression)*
    ;

multiplicativeExpression
    : unaryExpression ((STAR | SLASH | PERCENT) unaryExpression)*
    ;

unaryExpression
    : PLUS unaryExpression
    | MINUS unaryExpression
    | BANG unaryExpression
    | OWN unaryExpression
    | REF VAR? unaryExpression
    | AMP VAR? unaryExpression
    | STACKALLOC type LBRACK expression RBRACK
    | postfixExpression
    ;

postfixExpression
    : primaryExpression postfixOp*
    ;

postfixOp
    : LPAREN argumentList? RPAREN               # InvocationOp
    | DOT IDENTIFIER                             # MemberAccessOp
    | LBRACK expression RBRACK                   # IndexAccessOp
    | PLUSPLUS                                   # PostfixIncrementOp
    | MINUSMINUS                                 # PostfixDecrementOp
    ;

argumentList
    : argument (COMMA argument)*
    ;

argument
    : (REF VAR? | AMP VAR? | OWN)? expression
    ;

primaryExpression
    : literal
    | qualifiedName
    | LPAREN expression RPAREN
    | arrayLiteral
    ;

literal
    : INTEGER_LITERAL
    | FLOAT_LITERAL
    | BOOL_LITERAL
    | CHAR_LITERAL
    | STRING_LITERAL
    | MULTILINE_STRING_LITERAL
    ;

arrayLiteral
    : LBRACK (expression (COMMA expression)*)? RBRACK
    ;
