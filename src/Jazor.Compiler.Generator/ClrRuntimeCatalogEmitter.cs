using Jazor.Common;
using Jazor.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// 扫描并编译 Jazor.CLR runtime module，生成 ECMAScript runtime catalog 源码。
/// </summary>
/// <remarks>
/// 该发射器使用 Roslyn 源码编译而不是反射加载 CLR 项目，因此能直接处理受限 helper 源码。
/// 生成的 catalog 只携带模块文本、路径和 hash；模块语义仍由 AstConverter/SemanticWalker 负责。
/// </remarks>
internal static class ClrRuntimeCatalogEmitter
{
    public static void Generate(string repoRoot, IEnumerable<MetadataReference> references)
    {
        var clrSourceFiles = SharedGeneration.GetClrCompilationSourceFiles(repoRoot)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (clrSourceFiles.Length == 0)
            return;

        var syntaxTrees = clrSourceFiles
            .Select(SharedGeneration.CreateSyntaxTree)
            .ToArray();
        var compilation = CSharpCompilation.Create(
            "Jazor.Compiler.Generator",
            syntaxTrees,
            [.. references],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var moduleCandidates = DiscoverClrRuntimeModules(compilation, syntaxTrees);
        Console.WriteLine($"clr-runtime candidates={moduleCandidates.Count}");
        if (moduleCandidates.Count == 0)
            return;

        var generatedModules = new List<GeneratedClrRuntimeModule>();
        var failedModules = new List<string>();
        foreach (var candidate in moduleCandidates.OrderBy(static x => x.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var options = new AstConverterOptions(
                    AstConverterProfile.ClrRuntime,
                    symbol => ClrRuntimeSelection.ShouldInclude(candidate.RootType, symbol));

                var converter = new AstConverter(candidate.RootType, candidate.SemanticModel, options);
                var module = converter.Convert().GetAwaiter().GetResult();
                if (module is null)
                    continue;

                var content = module.ToKnRECMAScript();
                generatedModules.Add(new GeneratedClrRuntimeModule(
                    candidate.RootType.ToDisplayString(Format.NameFormat),
                    candidate.RelativePath.Replace('\\', '/'),
                    content,
                    ComputeSha256Hex(content)));
            }
            catch (Exception ex)
            {
                var failure = $"{candidate.RootType.ToDisplayString(Format.NameFormat)} -> {candidate.RelativePath} :: {ex.GetType().Name}: {ex.Message}";
                failedModules.Add(failure);
                Console.WriteLine($"clr-runtime emit fail: {failure}");
                foreach (System.Collections.DictionaryEntry entry in ex.Data)
                {
                    Console.WriteLine($"clr-runtime emit fail data: {entry.Key}={entry.Value}");
                }
            }
        }

        if (generatedModules.Count == 0)
        {
            Console.WriteLine("clr-runtime generated modules=0");
            return;
        }

        Console.WriteLine($"clr-runtime generated modules={generatedModules.Count}");
        if (failedModules.Count > 0)
        {
            Console.WriteLine($"clr-runtime failed modules={failedModules.Count}");
        }

        var outputPath = Path.Combine(repoRoot, "src", "ECMAScript", "Catalog.g.cs");
        File.WriteAllText(outputPath, BuildCatalogSource(generatedModules));
    }

    private static List<ClrRuntimeModuleCandidate> DiscoverClrRuntimeModules(Compilation compilation, IEnumerable<SyntaxTree> syntaxTrees)
    {
        var results = new List<ClrRuntimeModuleCandidate>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var syntaxTree in syntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
            var root = syntaxTree.GetRoot();
            foreach (var typeDeclaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
                    continue;

                if (!IsClrRuntimeModuleRoot(typeSymbol, typeDeclaration, semanticModel))
                    continue;

                var relativePath = SharedGeneration.ReadModulePath(typeDeclaration.AttributeLists, semanticModel);
                if (string.IsNullOrWhiteSpace(relativePath))
                    continue;

                var key = typeSymbol.ToDisplayString(Format.NameFormat);
                if (!seen.Add(key))
                    continue;

                results.Add(new ClrRuntimeModuleCandidate(typeSymbol, semanticModel, relativePath!));
            }
        }

        return results;
    }

    private static bool IsClrRuntimeModuleRoot(
        INamedTypeSymbol typeSymbol,
        TypeDeclarationSyntax typeDeclaration,
        SemanticModel semanticModel)
    {
        var modulePath = SharedGeneration.ReadModulePath(typeDeclaration.AttributeLists, semanticModel);
        var jazorAttr = SharedGeneration.FindAttribute(typeDeclaration.AttributeLists, "Jazor");
        var hasRuntimeContent = ClrRuntimeSelection.HasRuntimeContent(typeDeclaration);

        if (!typeSymbol.IsStatic)
            return false;

        if (typeSymbol.ContainingType is not null)
            return false;

        var isRuntimeCarrier = string.Equals(typeSymbol.Name, "RuntimeModule", StringComparison.Ordinal);
        var isInternalHelperModule = jazorAttr is null && !string.IsNullOrWhiteSpace(modulePath);
        if ((!isRuntimeCarrier && !isInternalHelperModule && jazorAttr is null) || string.IsNullOrWhiteSpace(modulePath))
            return false;

        if (!string.Equals(typeSymbol.ContainingNamespace?.ToDisplayString(), "Jazor.CLR", StringComparison.Ordinal))
            return false;

        return hasRuntimeContent;
    }

    private static string BuildCatalogSource(IReadOnlyList<GeneratedClrRuntimeModule> modules)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("// <auto-generated/>");
        builder.AppendLine("#nullable enable");
        builder.AppendLine("namespace ECMAScript");
        builder.AppendLine("{");
        builder.AppendLine("    [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("    internal static partial class Catalog");
        builder.AppendLine("    {");
        builder.AppendLine("        internal static string AssemblyName { get; } = \"ECMAScript\";");
        builder.AppendLine();
        builder.AppendLine("        internal static global::System.Collections.IEnumerable GetModules()");
        builder.AppendLine("        {");
        builder.AppendLine("            return _modules;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        [global::System.Runtime.CompilerServices.CompilerGenerated]");
        builder.AppendLine("        private sealed class GeneratedModule");
        builder.AppendLine("        {");
        builder.AppendLine("            public GeneratedModule(string assemblyName, string typeName, string id, string relativePath, string content, string hash)");
        builder.AppendLine("            {");
        builder.AppendLine("                AssemblyName = assemblyName;");
        builder.AppendLine("                TypeName = typeName;");
        builder.AppendLine("                Id = id;");
        builder.AppendLine("                RelativePath = relativePath;");
        builder.AppendLine("                Content = content;");
        builder.AppendLine("                Hash = hash;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            public string AssemblyName { get; }");
        builder.AppendLine("            public string TypeName { get; }");
        builder.AppendLine("            public string Id { get; }");
        builder.AppendLine("            public string RelativePath { get; }");
        builder.AppendLine("            public string Content { get; }");
        builder.AppendLine("            public string Hash { get; }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        private static readonly GeneratedModule[] _modules = new GeneratedModule[]");
        builder.AppendLine("        {");

        foreach (var module in modules)
        {
            builder.AppendLine("            new GeneratedModule(");
            builder.AppendLine("                assemblyName: \"ECMAScript\",");
            builder.Append("                typeName: \"").Append(SharedGeneration.EscapeForCSharpStringLiteral(module.TypeName)).AppendLine("\",");
            builder.Append("                id: \"").Append(SharedGeneration.EscapeForCSharpStringLiteral(module.TypeName)).AppendLine("\",");
            builder.Append("                relativePath: \"").Append(SharedGeneration.EscapeForCSharpStringLiteral(module.RelativePath)).AppendLine("\",");
            builder.Append("                content: \"").Append(SharedGeneration.EscapeForCSharpStringLiteral(module.Content)).AppendLine("\",");
            builder.Append("                hash: \"").Append(SharedGeneration.EscapeForCSharpStringLiteral(module.Hash)).AppendLine("\"),");
        }

        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string ComputeSha256Hex(string content)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>待转换的 CLR runtime 模块及其语义模型。</summary>
    private sealed record ClrRuntimeModuleCandidate(
        INamedTypeSymbol RootType,
        SemanticModel SemanticModel,
        string RelativePath);

    /// <summary>已转换的 runtime 模块文本及其稳定内容摘要。</summary>
    private sealed record GeneratedClrRuntimeModule(
        string TypeName,
        string RelativePath,
        string Content,
        string Hash);
}
