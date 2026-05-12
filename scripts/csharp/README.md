# C# Diagnostic Scripts

This directory contains single-file C# apps for local repository diagnostics.

Run scripts from the repository root with:

```powershell
dotnet run --file scripts/csharp/<script-name>.cs
```

Use these scripts for reflection, Roslyn, metadata inspection, and other probes where inline PowerShell quoting would be fragile. Keep scripts deterministic and read-only unless their name and header clearly say otherwise.

## Playground

`playground-verify-smoke.cs` verifies the real `Playground` host without any `.ps1` wrapper:

```powershell
dotnet run --file scripts/csharp/playground-verify-smoke.cs -- --build
dotnet run --file scripts/csharp/playground-verify-smoke.cs -- --publish
```

The publish mode asserts that `/jazor/*` is served from `wwwroot/jazor` and that no publish-root `jazor/` shadow directory exists.
