namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ProductionRazorCompilerReferenceTests
{
    [TestMethod]
    public void RazorVueProductionProjects_DoNotReferenceRazorCompiler()
    {
        var root = FindRepositoryRoot();
        var productionProjectPaths = new[]
        {
            "src/Jazor.Analyzer/Jazor.Analyzer.csproj",
            "src/Jazor.RazorVue/Jazor.RazorVue.csproj",
            "src/Jazor/Jazor.csproj"
        };

        foreach (var relativePath in productionProjectPaths)
        {
            var projectPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Assert.IsTrue(File.Exists(projectPath), "Expected production project was not found: " + relativePath);

            var projectText = File.ReadAllText(projectPath);
            Assert.IsFalse(
                projectText.Contains("Microsoft.CodeAnalysis.Razor.Compiler", StringComparison.Ordinal),
                relativePath + " must not reference or package Microsoft.CodeAnalysis.Razor.Compiler.");
            Assert.IsFalse(
                projectText.Contains("Microsoft.AspNetCore.Razor.Utilities.Shared", StringComparison.Ordinal),
                relativePath + " must not reference or package Microsoft.AspNetCore.Razor.Utilities.Shared.");
        }
    }

    [TestMethod]
    public void RazorVueHook_IsPackagedOnlyByJazorVue()
    {
        var root = FindRepositoryRoot();
        var jazorProject = File.ReadAllText(Path.Combine(root, "src", "Jazor", "Jazor.csproj"));
        var jazorVueProjectPath = Path.Combine(root, "src", "Jazor.Vue", "Jazor.Vue.csproj");
        var jazorVueProject = File.ReadAllText(jazorVueProjectPath);

        Assert.IsFalse(
            jazorProject.Contains("Jazor.RazorVue\\bin", StringComparison.Ordinal),
            "Jazor must not package or install the Razor-to-Vue implementation and generator hook.");
        StringAssert.Contains(jazorVueProject, "<PackageId>Jazor.Vue</PackageId>");
        StringAssert.Contains(jazorVueProject, "Jazor.RazorVue.dll");
        Assert.IsFalse(
            jazorVueProject.Contains("Jazor.RazorVue.Generator", StringComparison.Ordinal),
            "Jazor.Vue must package one RazorVue analyzer assembly, not the retired generator assembly.");

        var packagedAnalyzers = System.Xml.Linq.XDocument.Load(jazorVueProjectPath)
            .Descendants("None")
            .Where(static item => string.Equals((string?)item.Attribute("Pack"), "true", StringComparison.OrdinalIgnoreCase))
            .Where(static item => ((string?)item.Attribute("PackagePath"))?.Replace('\\', '/').StartsWith("analyzers/dotnet/cs/", StringComparison.OrdinalIgnoreCase) == true)
            .Select(static item => Path.GetFileName(((string?)item.Attribute("Include"))?.Replace('\\', '/')))
            .OrderBy(static name => name, StringComparer.Ordinal)
            .ToArray();

        CollectionAssert.AreEqual(
            new[] { "Jazor.RazorVue.dll", "Jazor.RazorVue.pdb" },
            packagedAnalyzers,
            "Jazor.Vue must rely on Jazor for shared analyzer dependencies so catalog generators are loaded only once.");
    }

    [TestMethod]
    public void DeprecatedRazorExtensionProject_IsNotPresent()
    {
        var root = FindRepositoryRoot();
        var deprecatedProjectPath = Path.Combine(
            root,
            "src",
            "Jazor.RazorVue.RazorExtension",
            "Jazor.RazorVue.RazorExtension.csproj");

        Assert.IsFalse(
            File.Exists(deprecatedProjectPath),
            "The deprecated RazorExtension project must not remain in the RazorVue production source tree.");
    }

    [TestMethod]
    public void RazorVueProductionAssemblies_DoNotReferenceRazorCompiler()
    {
        var razorVueAssembly = typeof(Jazor.RazorVue.RazorSdk.GeneratedCSharpBinder).Assembly;
        Assert.AreSame(
            razorVueAssembly,
            typeof(Jazor.RazorVue.Generation.RazorVueGenerator).Assembly,
            "RazorVue lowering and generator entry point must share one assembly.");

        var productionAssemblies = new[] { razorVueAssembly };

        foreach (var assembly in productionAssemblies)
        {
            var referencedAssemblyNames = assembly.GetReferencedAssemblies()
                .Select(static item => item.Name)
                .ToArray();

            CollectionAssert.DoesNotContain(
                referencedAssemblyNames,
                "Microsoft.CodeAnalysis.Razor.Compiler",
                assembly.GetName().Name + " must not reference Microsoft.CodeAnalysis.Razor.Compiler.");
            CollectionAssert.DoesNotContain(
                referencedAssemblyNames,
                "Microsoft.AspNetCore.Razor.Utilities.Shared",
                assembly.GetName().Name + " must not reference Microsoft.AspNetCore.Razor.Utilities.Shared.");
        }
    }

    [TestMethod]
    public void LoweringSources_DoNotRoundTripGeneratedJavaScriptThroughTextOrParser()
    {
        var root = FindRepositoryRoot();
        var compilerRoot = Path.Combine(root, "src", "Jazor.Compiler");
        var razorVueRoot = Path.Combine(root, "src", "Jazor.RazorVue");

        var parserCallSites = Directory
            .EnumerateFiles(compilerRoot, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadLines(path).Select((line, index) => new SourceLine(path, index + 1, line)))
            .Where(static sourceLine =>
                sourceLine.Text.Contains("new Parser(", StringComparison.Ordinal) ||
                sourceLine.Text.Contains(".ParseExpression(", StringComparison.Ordinal) ||
                sourceLine.Text.Contains(".ParseModule(", StringComparison.Ordinal))
            .ToArray();

        Assert.AreEqual(2, parserCallSites.Length, DescribeSourceLines(parserCallSites));
        Assert.IsTrue(
            parserCallSites.All(static sourceLine =>
                sourceLine.Path.EndsWith("SemanticWalker.cs.InlineTemplate.cs", StringComparison.OrdinalIgnoreCase)),
            "Only the explicit authored Inline template boundary may parse JavaScript.\n" + DescribeSourceLines(parserCallSites));

        var astSourceLines = Directory
            .EnumerateFiles(compilerRoot, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(razorVueRoot, "*.cs", SearchOption.AllDirectories))
            .SelectMany(path => File.ReadLines(path).Select((line, index) => new SourceLine(path, index + 1, line)))
            .ToArray();
        var directStringLiteralConstruction = astSourceLines
            .Where(static sourceLine =>
                sourceLine.Text.Contains("new StringLiteral(", StringComparison.Ordinal) ||
                sourceLine.Text.Contains("EscapeJavaScriptString", StringComparison.Ordinal))
            .ToArray();
        Assert.AreEqual(1, directStringLiteralConstruction.Length, DescribeSourceLines(directStringLiteralConstruction));
        Assert.IsTrue(
            directStringLiteralConstruction[0].Path.EndsWith("JavaScriptAstFactory.cs", StringComparison.OrdinalIgnoreCase),
            "Production lowering must create escaped string literal AST nodes through JavaScriptAstFactory.\n" +
            DescribeSourceLines(directStringLiteralConstruction));

        var compilerSemanticSerialization = Directory
            .EnumerateFiles(compilerRoot, "*.cs", SearchOption.AllDirectories)
            .Where(static path => !path.EndsWith("Util.cs", StringComparison.OrdinalIgnoreCase))
            .Where(static path => !path.EndsWith("ESGenerator.cs", StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path).Select((line, index) => new SourceLine(path, index + 1, line)))
            .Where(static sourceLine =>
                sourceLine.Text.Contains(".ToECMAScript(", StringComparison.Ordinal) ||
                sourceLine.Text.Contains(".ToKnRECMAScript(", StringComparison.Ordinal))
            .ToArray();
        Assert.AreEqual(0, compilerSemanticSerialization.Length, DescribeSourceLines(compilerSemanticSerialization));

        var razorVueSources = Directory
            .EnumerateFiles(razorVueRoot, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadLines(path).Select((line, index) => new SourceLine(path, index + 1, line)))
            .ToArray();
        var retiredTextLoweringTokens = new[]
        {
            "compiledScript",
            "rebasedImportLine",
            "SplitCompiledScript",
            "TryParseImportDeclaration",
            "RebaseRootRelativeImportLine",
            "CompilerScriptParts",
            "CompiledSourceLine",
            "DirectRazorSourceMap",
            "ImportLines",
            "PreludeLines",
            "SetupBodyLines",
            "_preludeLines",
            "BuildImportLines",
            "BuildVueImportLine",
            "BuildSetupFactoryParameterList",
            "FormatJavaScriptPropertyAccess",
            "importScript",
            "defaultImportScript",
            "AppendLine(\"import ",
            "new Parser(",
            ".ParseModule(",
            ".ParseExpression(",
            ".ParseScript("
        };
        var razorVueTextLowering = razorVueSources
            .Where(sourceLine => retiredTextLoweringTokens.Any(token =>
                sourceLine.Text.Contains(token, StringComparison.Ordinal)))
            .ToArray();
        Assert.AreEqual(0, razorVueTextLowering.Length, DescribeSourceLines(razorVueTextLowering));

        var razorVueSerialization = razorVueSources
            .Where(static sourceLine =>
                sourceLine.Text.Contains(".ToECMAScript", StringComparison.Ordinal) ||
                sourceLine.Text.Contains(".ToKnRECMAScript", StringComparison.Ordinal))
            .ToArray();
        Assert.AreEqual(2, razorVueSerialization.Length, DescribeSourceLines(razorVueSerialization));
        Assert.IsTrue(
            razorVueSerialization.All(static sourceLine =>
                sourceLine.Path.EndsWith("VueModuleBuilder.cs", StringComparison.OrdinalIgnoreCase)),
            "Only the audited compiler-layout and final Vue-module boundaries may serialize RazorVue AST nodes.\n" +
            DescribeSourceLines(razorVueSerialization));

        var moduleBuilderText = File.ReadAllText(Path.Combine(
            razorVueRoot,
            "RazorSdk",
            "VueModuleBuilder.cs"));
        Assert.IsFalse(
            moduleBuilderText.Contains("AppendLine(", StringComparison.Ordinal),
            "The Vue module builder must compose JavaScript as Acornima AST and serialize the completed Module once.");

        var razorSdkRegexSites = razorVueSources
            .Where(static sourceLine =>
                sourceLine.Path.Contains(
                    Path.Combine("Jazor.RazorVue", "RazorSdk"),
                    StringComparison.OrdinalIgnoreCase) &&
                (sourceLine.Text.Contains("new Regex(", StringComparison.Ordinal) ||
                 sourceLine.Text.Contains("Regex.", StringComparison.Ordinal)))
            .ToArray();
        Assert.AreEqual(0, razorSdkRegexSites.Length, DescribeSourceLines(razorSdkRegexSites));
    }

    [TestMethod]
    public void RazorVueModuleBuilder_DoesNotRetainBuilderProtocolFallback()
    {
        var root = FindRepositoryRoot();
        var moduleBuilderPath = Path.Combine(
            root,
            "src",
            "Jazor.RazorVue",
            "RazorSdk",
            "VueModuleBuilder.cs");
        var moduleBuilderText = File.ReadAllText(moduleBuilderPath);
        var retiredFallbackTokens = new[]
        {
            "createRenderContext",
            "scope.buildRenderTree(builder)",
            "builder.finish()",
            "componentProps",
            "syncSlotParameters",
            "SlotParameterBridge"
        };

        foreach (var token in retiredFallbackTokens)
        {
            Assert.IsFalse(
                moduleBuilderText.Contains(token, StringComparison.Ordinal),
                "Direct RazorVue lowering must not restore the retired RenderTreeBuilder fallback: " + token);
        }
    }

    private static string DescribeSourceLines(IEnumerable<SourceLine> sourceLines)
        => string.Join(
            Environment.NewLine,
            sourceLines.Select(static sourceLine =>
                sourceLine.Path + ":" + sourceLine.Line.ToString(System.Globalization.CultureInfo.InvariantCulture) + ": " + sourceLine.Text.Trim()));

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Jazor.slnx")))
                return current;

            var parent = Directory.GetParent(current);
            if (parent is null)
                break;

            current = parent.FullName;
        }

        throw new InvalidOperationException("Repository root containing Jazor.slnx could not be located.");
    }

    private readonly record struct SourceLine(string Path, int Line, string Text);
}
