# GEMINI.md - Lunar Project Context

## Project Overview
**Lunar** is a custom Lua interpreter and game execution environment built with **.NET 9.0**. It provides a visually enhanced CLI for running Lua scripts, managing custom bytecode, and monitoring execution threads.

The project leverages:
- **MoonSharp**: The core Lua interpreter engine.
- **Spectre.Console**: Used for rich terminal UI elements.

## Technical Architecture
The system is divided into:
1.  **Program.cs**: CLI interface and UI loop.
2.  **LunarEngine.cs**: Core engine logic, handling bytecode serialization and thread management.

### Features
- **Auto-Update**: Built-in GitHub-based version checking and binary synchronization.
- **Guardian Sandbox**: Isolated execution for high-privilege scripts via TCP.
- **Debug CLI**: Interactive control panel available in debug builds for real-time engine manipulation.
- **Custom Bytecode (.lbc)**: Encapsulated Lua bytecode with SHA256 integrity checks and magic headers.
- **Thread Monitor**: Real-time tracking of Lua coroutines.

## Building and Running

### Prerequisites
- .NET 9.0 SDK

### Key Commands
- **Run the project**: `dotnet run`
- **Build the project**: `dotnet build`
- **CI/CD Build**: The project automatically outputs a DLL instead of an EXE when `GITHUB_ACTIONS=true` is set.

## Development Conventions

### Bytecode & Security
- Use `LunarEngine.ExportBytecode` to generate raw bytecode.
- Use `LunarEngine.SaveBytecode` to wrap it in the `.lbc` container.

### Lua Threading
- Always use the `spawn(fn)` global in Lua to ensure threads are tracked by the engine's monitor.

### Documentation
- Reference `ENGINE.md` for detailed technical specifications of the engine's core.
