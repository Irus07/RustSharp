# RustSharp Language Specification

This section is under construction. It will contain the formal language specification and reference manual.

## Planned Structure

1. **[Introduction & Philosophy](01-introduction.md)**
   - Language goals and core concepts
   - Balancing ergonomics and efficiency
2. **[Lexical Structure & Syntax](02-syntax.md)**
   - Keywords, identifiers, and operators
   - ANTLR grammar specification
3. **[Type System](03-types.md)**
   - Primitive types
   - Compound types (structs, tuples, enums)
4. **[Memory Model & Ownership](04-memory-model.md)**
   - Static ownership & borrowing
   - Runtime fallback mechanism for unresolved lifetimes
5. **[Compiler Pipeline & Codegen](05-compiler-pipeline.md)**
   - LLVM IR generation
   - `llc` and `lld` integration
