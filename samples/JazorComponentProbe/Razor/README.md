# Razor language probe

Run from the repository root with SDK 11.0.100.

```sh
dotnet run --project samples/JazorComponentProbe/Razor
dotnet run --project samples/JazorComponentProbe/Razor -- samples/JazorComponentProbe/Counter.jazor
```

The default invocation verifies mixed markup and recovery after three unfinished edits. Passing a file prints the Razor IR, source slices, and diagnostics. `RazorProbe.Parse(source, path)` is the callable API. It explicitly selects `RazorFileKind.Component` for the `.jazor` path, captures the document through a public classifier pass, and runs through optimization. It ends the pipeline before C# generation.

For mixed markup, the public IR provides nested elements, attributes, HTML content, and C# expression tokens. Element spans cover full elements. Attribute spans include leading whitespace. A C# expression token within an attribute provides the expression body without the Razor transition. The using node span starts at `using`, and the class code node covers the members inside `@code` braces. Roslyn can parse those C# ranges for finer-grained spans.

Razor reports `RZ10012` for unfamiliar uppercase tags because the adapter supplies no Razor tag helpers. Such a tag remains markup that a separate component binder can interpret.

Public `RazorSyntaxTree` exposes diagnostics but no accessible root for walking raw syntax. The public optimized IR is lossy. A static element such as `<p>Static</p>` can become a `MarkupBlockIntermediateNode` whose HTML content has no per-element source spans. Earlier IR retains the structure in internal unresolved nodes. The probe uses SDK-shipped assembly references and establishes direct language processing for the mixed Counter case. Lossless static markup ranges for editor tooling need a separate approach.
