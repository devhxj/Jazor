# C# Diagnostic Scripts

This directory contains single-file C# apps for local repository diagnostics.

Run scripts from the repository root with:

```powershell
dotnet run --file scripts/csharp/<script-name>.cs
```

Use these scripts for reflection, Roslyn, metadata inspection, and other probes where inline PowerShell quoting would be fragile. Keep scripts deterministic and read-only unless their name and header clearly say otherwise.
