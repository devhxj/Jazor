# Jolt

Jolt builds Vue components authored in C# with `.jazor`. Install the tool from a NuGet feed containing the Jolt packages.

```sh
dotnet tool install Jazor.Jolt --global
jolt build .
jolt lsp
```

The tool requires .NET 11 and publishes packages for Windows and glibc Linux on x64 and ARM64. Component builds restore the project's locked npm dependencies and use the public TypeScript declarations and ESM exports from those dependencies.

To pack and verify the tool from the Jazor repository, run:

```sh
dotnet run --file scripts/csharp/verify-jolt-package.cs
```
