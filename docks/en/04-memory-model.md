# RustSharp Memory Model: Hybrid Management (Ownership + Runtime Fallback)

This document describes the memory management architecture of the **RustSharp** programming language, memory segmentation rules, function qualifiers `native` and `runtime`, elimination of raw pointers in favor of safe references (`ref`), transparent auto-dereferencing, and the philosophy of optional ownership ("Use it — great, don't — no problem").

---

## 🧭 Table of Contents

1. [Philosophy: Why a Hybrid Model?](#1-philosophy-why-a-hybrid-model)
2. [Memory Segmentation Architecture](#2-memory-segmentation-architecture)
   - 2.1 [Stack](#21-stack)
   - 2.2 [Native Heap (RAII)](#22-native-heap-raii)
   - 2.3 [Managed Heap (GC)](#23-managed-heap-gc)
3. [Function Keywords: `native`, `runtime`, and Adaptive `fn`](#3-function-keywords-native-runtime-and-adaptive-fn)
   - 3.1 [Default Adaptive Mode (`fn`)](#31-default-adaptive-mode-fn)
   - 3.2 [Strict Native Mode (`native fn`)](#32-strict-native-mode-native-fn)
   - 3.3 [Managed Mode (`runtime fn`)](#33-managed-mode-runtime-fn)
   - 3.4 [Scoped Blocks: `native { ... }` and `runtime { ... }`](#34-scoped-blocks-native----and-runtime---)
4. [Safe References (`ref`) and Optional Ownership (`own`)](#4-safe-references-ref-and-optional-ownership-own)
   - 4.1 [No Raw Pointers](#41-no-raw-pointers)
   - 4.2 [Transparent Auto-Dereferencing](#42-transparent-auto-dereferencing)
   - 4.3 [Safe Reference Types (`ref` and `ref var`)](#43-safe-reference-types-ref-and-ref-var)
   - 4.4 [Optional Ownership ("Use it — great, don't — no problem")](#44-optional-ownership-use-it--great-dont--no-problem)
5. [Compiler Pipeline: Runtime Fallback Algorithm](#5-compiler-pipeline-runtime-fallback-algorithm)
6. [Cross-Domain Interoperability (Interop & Boundaries)](#6-cross-domain-interoperability-interop--boundaries)
7. [Function Modes Comparison Matrix](#7-function-modes-comparison-matrix)

---

## 1. Philosophy: Why a Hybrid Model?

Modern programming languages often force developers into a trade-off:
- **Strict Systems Languages (Rust, C++):** Maximize execution speed and determinism via static lifetimes, but require extensive manual effort to satisfy the borrow checker when implementing complex graph-like structures or rapid prototyping.
- **Managed Languages (C#, Go, Java):** Maximize developer productivity via a garbage collector, but introduce runtime pauses, tracing overhead, and unpredictable memory footprints.

**RustSharp bridges this gap with an adaptive hybrid model:**
- By default, the compiler attempts to prove lifetimes statically (like Rust).
- If proven, resources are released **deterministically with zero runtime overhead (Zero-Cost RAII)**.
- If reference graphs are ambiguous or cyclic, the compiler **does not halt compilation with an error**; it transparently hands the object over to a lightweight garbage collector (**Runtime Fallback**).
- When absolute determinism or managed semantics are explicitly required, developers use `native` or `runtime` keywords.

---

## 2. Memory Segmentation Architecture

```
+-----------------------------------------------------------------+
|                       RUSTSHARP MEMORY                          |
+-------------------+--------------------+------------------------+
|    1. STACK       |  2. NATIVE HEAP    |   3. MANAGED HEAP      |
|                   |    (RAII / LLVM)   |      (Runtime GC)      |
+-------------------+--------------------+------------------------+
| - Local primitives| - native struct /  | - class (complex graphs)|
|   (int, bool)     |   buffers          | - Cyclic references    |
| - Value structs   | - Deterministic    | - Fallback objects     |
| - References &    |   linear ownership | - Tracing GC           |
|   call frames     | - Zero GC pauses   | - Full reference       |
| - O(1) push / pop | - Destructor free  |   ergonomics           |
+-------------------+--------------------+------------------------+
```

---

## 3. Function Keywords: `native`, `runtime`, and Adaptive `fn`

```rustsharp
// 1. Adaptive (non-strict) default function
fn calculateMetrics(id: int) -> ResultData { ... }

// 2. Strict native function (Zero-Cost, no GC)
native fn fastMatrixMul(a: ref Matrix, b: ref Matrix) -> Matrix { ... }

// 3. Managed runtime function (GC-enabled)
runtime fn renderUiWindow(win: Window) -> void { ... }
```

### 3.1 Default Adaptive Mode (`fn`)
Non-strict by design. The compiler performs escape analysis and lifetime inference:
- Deterministic lifespans compile directly to stack or native heap allocations.
- Complex or unresolved cyclic ownership falls back to the tracing GC automatically without compiler errors.

### 3.2 Strict Native Mode (`native fn`)
Designed for performance-critical kernels, drivers, mathematics, and FFI:
- No implicit GC allocations are allowed.
- Lifetime violations trigger compile-time errors instead of falling back to the GC.
- Direct LLVM IR compilation with standard C-ABI calling conventions and no GC safepoint polling.

### 3.3 Managed Mode (`runtime fn`)
Designed for high-level business logic, UI hierarchies, and dynamic scripting:
- Reference types (`class`) reside in the Managed Heap managed by the Tracing GC.
- Stack frames are automatically registered in the GC root map.

### 3.4 Scoped Blocks: `native { ... }` and `runtime { ... }`
Allows granular domain switching inside any function body:
```rustsharp
fn processPayload(data: Packet) -> void {
    native {
        // Guaranteed zero-cost block; managed heap allocations are forbidden
        var rawBuf: byte[] = stackalloc byte[512];
        encryptNative(rawBuf);
    }

    runtime {
        // Access managed subsystems
        EventBus.publish("Processed");
    }
}
```

---

## 4. Safe References (`ref`) and Optional Ownership (`own`)

### 4.1 No Raw Pointers
RustSharp completely omits raw pointers (`*T`, pointer arithmetic, null-pointer dereferencing). All references are safe and verified either by the compiler or by the managed runtime.

### 4.2 Transparent Auto-Dereferencing
Developers **never need to write `*ptr`**. Reading values, updating values, and accessing fields/methods happens directly:
```rustsharp
fn updateScore(scoreRef: ref var int) -> void {
    scoreRef = scoreRef + 10; // Auto-dereference: no * needed
}

fn printPoint(p: ref Point) -> void {
    Console.println(p.x);     // Direct member access via dot
}
```

### 4.3 Safe Reference Types (`ref` and `ref var`)
- `ref T` — immutable safe reference (read-only borrow).
- `ref var T` — mutable safe reference (mutation allowed).
- The caller writes `ref` at the call-site when passing a mutable reference for clarity: `updateScore(ref myScore);`.

### 4.4 Optional Ownership ("Use it — great, don't — no problem")
Ownership is not an imposed dogma in RustSharp:
1. **Using `own` ("Great!"):**
   - Explicitly signals single ownership transfer.
   - The compiler guarantees deterministic destructor cleanup without GC overhead (Zero-Cost RAII).
   - Ideal for files, sockets, native buffers, and performance-critical loops.
2. **Not using `own` ("No problem!"):**
   - Standard, ergonomic object passing.
   - If the object escapes or forms complex graphs, **Runtime Fallback (GC)** handles it seamlessly without any compile-time borrow checker friction.

```rustsharp
native fn processPacket(own packet: Packet) -> void {
    // packet freed deterministically at the end of the function
}

fn handleData() -> void {
    // 1. Explicit ownership (Zero-Cost):
    var p1 = Packet();
    processPacket(own p1);

    // 2. Standard usage without own:
    var p2 = Packet();
    EventBus.publish(p2); // Runtime takes care of everything
}
```

---

## 5. Cross-Domain Interoperability and Pinning

When passing managed objects into native functions, the `pin` statement prevents the GC from moving the object in memory:
```rustsharp
runtime fn sendToHardware(buffer: ManagedBuffer) -> void {
    pin (var ptr = ref buffer[0]) {
        hardwareWriteNative(ptr, buffer.Length);
    }
}
```

---

## 6. Function Modes Comparison Matrix

| Criterion | `native fn` | Default `fn` (Adaptive) | `runtime fn` |
| :--- | :--- | :--- | :--- |
| **Philosophy** | Strict zero-cost systems code | Non-strict, adaptive, ergonomic | High-level managed code |
| **Memory Target** | Stack + Native Heap (RAII) | Hybrid (RAII $\rightarrow$ GC Fallback) | Managed Heap (GC) |
| **Borrow Checker Errors** | Strict compile-time error | No: fallback to GC on complexity | Not applied to classes |
| **Garbage Collector** | Disabled | Activated on-demand per object | Active |
| **Primary Use Cases** | Hot paths, FFI, math, engines | 80% application code, business logic | UI, plugins, complex graphs |
