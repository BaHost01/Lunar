# 🌙 Lunar Engine - Comprehensive Documentation

## 🚀 Introduction
Lunar is a high-performance Lua-based game engine core built on .NET 9.0. It provides a robust bridge between C# high-level architecture and Lua script flexibility, specialized for zone-based game design and persistent storytelling.

---

## 📦 Core Systems

### 1. Custom Bytecode (.lbc)
Lunar uses an encrypted/hashed container for compiled Lua.
- **Magic Header**: `LUNAR`
- **Integrity**: SHA256 hashing verifies bytecode against tampering.
- **Caching**: Bytecode is cached on first load for sub-millisecond initialization.

### 2. Thread Management
Cooperative multitasking via `spawn(fn)`.
- **Tick System**: Scripts are step-executed via `engine.Tick()`.
- **Zone Awareness**: Threads are automatically paused when a zone is unloaded to save CPU/RAM.

---

## 🛡️ Advanced Security Layer

### 1. Mandatory Secure Bootloader
The `LunarAPI.Start()` function is now an **awaitable bootloader**.
- **Update Check**: Upon execution, it performs a mandatory version check and DLL integrity sync.
- **Blocking**: The game will not initialize until the bootloader verifies the environment is secure and up-to-date.

### 2. Isolated Guardian Sandbox
`GameScript` logic is now executed in the **Guardian Sandbox**, a randomized execution context.
- **Randomized Identity**: Every sandbox instance has a unique, randomized identity (e.g., `G_a4b2...`).
- **String Obfuscation**: The sandbox environment is injected with hundreds of randomized strings (`ptr_...`) to mislead memory scanners and string-based dumping tools.
- **TCP Coordination**: Communication between the main DLL and the Guardian Sandbox happens over a secure local TCP bridge (Port 5556).
- **Out-of-Memory Logic**: High-privilege game logic is isolated from the main game process memory, making it immune to standard memory read/write exploits.

### 3. Anti-Tamper Bytecode Size Check
- **Bytecode Enforcement**: Memory-bound scripts (`LocalScript`, `ModuleScript`) must match their original compiled size exactly.
- **Detection**: Any byte-level modification in memory is detected instantly, resulting in an immediate execution halt.

---

## 🛠 Scripting Tiers (Revised)
| Type | Permission | Execution Path | Security |
| :--- | :--- | :--- | :--- |
| **LocalScript** | Level 1 | Local Memory | .byenc + Size Check |
| **ModuleScript**| Level 8 | Local Memory | .bymodenc + Spoofing |
| **GameScript**  | Level 8 | Guardian Sandbox | TCP + Obfuscation |

## 📚 Global API Reference

### C# / Engine API
- `engine.DoString(code, name)`: Executes Lua with behavior parsing.
- `engine.RestoreStoryScript(name)`: Re-runs a persistent script from its original source.
- `engine.UnloadStoryScript(name)`: Removes a script from the persistent registry.
- `engine.SaveCache()`: Serializes persistent script states to disk.

### Lua Global Scope
- `print(val)`: Styled console output.
- `warn(msg)`: Yellow warning indicator.
- `spawn(fn)`: Create a managed engine thread.
- `clamp(v, min, max)`: Math utility.
- `lerp(a, b, t)`: Linear interpolation.

---

## 🛠 Built-in Scripts (Persistent Core)
Lunar includes 20+ built-in scripts embedded in the DLL:
1.  `01_init.lua`: Initial hardware/driver stubs.
2.  `02_logger.lua`: Centralized logging service.
3.  `03_math_ext.lua`: Vector and matrix math extensions.
...
20. `20_boot_complete.lua`: Signals engine readiness to the host.

---

## 📦 Packaging & Deployment
To package the engine for release:
1.  Run `./package.sh`.
2.  The engine will produce a structured `Release` folder.
3.  **Depends**: All necessary DLLs (MoonSharp, Spectre).
4.  **Docs**: Automatically bundled Markdown documentation.
