# Jolt

Jolt builds Vue components authored in C# with `.jazor`. Install the published tool and run it from a Jolt project folder. [Authoring guide](../../docs/03-guides/jolt-authoring.md)

```sh
dotnet tool install --global Jazor.Jolt --version 2.2.0
jolt dev .
```

The tool requires .NET 11 and publishes packages for Windows and glibc Linux on x64 and ARM64. Component builds restore the project's locked npm dependencies and use the public TypeScript declarations and ESM exports from those dependencies.

To pack and verify the tool from the Jazor repository, run:

```sh
dotnet run --file scripts/csharp/verify-jolt-package.cs
```
