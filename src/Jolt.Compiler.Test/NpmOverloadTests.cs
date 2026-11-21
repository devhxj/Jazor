using Jolt.Npm;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class NpmOverloadTests
{
    [TestMethod]
    public void Analyze_CachedProjectionPreservesDeclarationIdentityAcrossSourceEdits()
    {
        var project = Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "jolt-projection-" + Guid.NewGuid()));
        try
        {
            const string source = """
                @using Client = Npm.Client.Exports
                @code { private void Forward() { Client.Write(Client.Read()); } }
                """;
            var cache = JoltNpmBindings.CacheDirectory(project.FullName);
            JoltAnalysis Analyze(int start, bool sameDeclaration)
            {
                var definition = new NpmDefinition("node_modules/client/types.d.ts", start, 8);
                var member = new NpmDefinition(definition.Path, start + 10, 5);
                var output = new NpmValueType("object", Name: "Settings", TypeDefinition: definition,
                    Members: [new("label", new("string"), true, member)]);
                var input = sameDeclaration ? output : output with { TypeDefinition = definition with { Start = start + 40 } };
                NpmValueExport Export(string name, NpmValueSignature signature) =>
                    new("client", name, "client", "1.0", "index.js", "types.d.ts", null, [signature], null);
                return JoltProjectCompiler.Analyze([new("App.jazor", source)], new()
                {
                    NpmProjectionCacheDirectory = cache,
                    NpmValues = [Export("read", new([], output)), Export("write", new([new("value", input, false)], new("void")))],
                });
            }

            var initial = Analyze(20, true);
            Assert.IsEmpty(initial.Diagnostics, string.Join(Environment.NewLine, initial.Diagnostics));
            var entry = Directory.GetFiles(cache, "Projection.g.cs", SearchOption.AllDirectories).Single();
            Assert.DoesNotContain('\r', File.ReadAllText(entry));
            File.SetLastWriteTimeUtc(entry, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var written = File.GetLastWriteTimeUtc(entry);
            var moved = Analyze(60, true);
            Assert.IsEmpty(moved.Diagnostics, string.Join(Environment.NewLine, moved.Diagnostics));
            Assert.AreEqual(70, moved.NpmValueDefinitions.First(pair => pair.Key.Name == "Label").Value.Start);
            Assert.AreEqual(entry, Directory.GetFiles(cache, "Projection.g.cs", SearchOption.AllDirectories).Single());
            Assert.AreEqual(written, File.GetLastWriteTimeUtc(entry));

            var separate = Analyze(60, false);
            Assert.IsTrue(separate.Diagnostics.Any(diagnostic => diagnostic.Id == "CS1503"));
            Assert.HasCount(2, Directory.GetFiles(cache, "Projection.g.cs", SearchOption.AllDirectories));
        }
        finally
        {
            project.Delete(recursive: true);
        }
    }

    [TestMethod]
    public async Task Compile_SharesNamedInputsByTypeScriptDeclarationAndShape()
    {
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                [Setup]
                private void Configure() {
                    var duration = new Tool.Duration { Days = 2 };
                    Tool.Add(duration);
                    Tool.Sub(duration);
                    Tool.Alt(new() { Days = 2 });
                    Tool.Wider(new() { Days = 2, Hours = 5 });
                    Tool.Read().Days.ToString();
                }
            }
            """;
        var typeDefinition = new NpmDefinition("package/types.d.ts", 30, 8);
        var memberDefinition = new NpmDefinition("package/types.d.ts", 47, 4);
        var number = new NpmValueType("number");
        var duration = new NpmValueType("object", Members: [new("days", number, true, memberDefinition)],
            Name: "Duration", TypeDefinition: typeDefinition);
        var otherDuration = duration with { TypeDefinition = typeDefinition with { Start = 100 } };
        var widerDuration = duration with { Members = [.. duration.Members!, new("hours", number, true)] };
        NpmValueExport Export(string name, NpmValueType input) => new("tool", name, "tool", "1.0.0", "index.js", "index.d.ts", null,
            [new([new("options", input, false)], number)], null);
        var read = new NpmValueExport("tool", "read", "tool", "1.0.0", "index.js", "index.d.ts", null, [new([], duration)], null);
        var options = new JoltCompileOptions
        {
            NpmValues = [Export("wider", widerDuration), Export("sub", duration), Export("alt", otherDuration), read, Export("add", duration)],
        };
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", source)], options);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "let duration = { days: 2 }");
        StringAssert.Contains(js, "sub as ");
        StringAssert.Contains(js, ".days");

        var analysis = JoltProjectCompiler.Analyze([new("App.jazor", source)], options);
        Assert.IsTrue(analysis.NpmValueDefinitions.Any(pair => pair.Key.Name == "Duration" && pair.Value == typeDefinition));
        Assert.IsTrue(analysis.NpmValueDefinitions.Any(pair => pair.Key.Name == "Days" && pair.Key.ContainingType?.Name == "Duration" &&
            pair.Value == memberDefinition));

        var chained = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("Tool.Sub(duration)", "Tool.Sub(Tool.Read())"))], options);
        Assert.IsEmpty(chained.Diagnostics, string.Join(Environment.NewLine, chained.Diagnostics));
        // Alt's identically shaped parameter originates at a different declaration.
        var invalid = JoltProjectCompiler.Analyze([new("App.jazor", source.Replace("Tool.Sub(duration)", "Tool.Alt(Tool.Read())"))], options);
        Assert.AreEqual("CS1503", invalid.Diagnostics.Single().Id);

        var collidingExport = new NpmValueExport("tool", "duration", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [], number);
        var collisionOptions = options with { NpmValues = [.. options.NpmValues, collidingExport] };
        var name = ProjectedMethods(collisionOptions.NpmValues).Where(method => method.Identifier.ValueText == "Add")
            .Select(method => method.ParameterList.Parameters[0].Type!.ToString())
            .Single(type => type.StartsWith("Duration_", StringComparison.Ordinal));
        StringAssert.StartsWith(name, "Duration_");
        var collidingSource = source.Replace("new Tool.Duration {", $"new Tool.{name} {{", StringComparison.Ordinal);
        var collision = JoltProjectCompiler.Analyze([new("App.jazor", collidingSource)], collisionOptions);
        Assert.IsEmpty(collision.Diagnostics, string.Join(Environment.NewLine, collision.Diagnostics));
    }

    [TestMethod]
    public async Task Compile_OverloadsKeepDistinctOptionsAndResultShapes()
    {
        const string source = """
            @using Tool = Npm.Tool.Exports
            @code {
                [Setup]
                private void Configure() {
                    var first = Tool.Format(new Tool.FormatOptionsInput { Prefix = "demo" });
                    var second = Tool.Format(new Tool.DigitsOptions { Digits = 2 });
                    var detailed = Tool.Format(new Tool.FormatOptionsInput { Prefix = "test" }, true);
                    string text = first.Text + second.Output + detailed.Text;
                }
            }
            """;
        var text = new NpmValueType("string");
        var number = new NpmValueType("number");
        var firstOptions = new NpmValueType("object", Members: [new("prefix", text, false)]);
        var secondOptions = new NpmValueType("object", Members: [new("digits", number, false)]);
        var firstResult = new NpmValueType("object", Members: [new("text", text, true)]);
        var secondResult = new NpmValueType("object", Members: [new("output", text, true)]);
        var export = new NpmValueExport("tool", "format", "tool", "1.0.0", "index.js", "index.d.ts", null,
            [new([new("options", firstOptions, false)], firstResult),
             new([new("options", secondOptions, false)], secondResult),
             new([new("options", firstOptions with { Members = [.. firstOptions.Members!] }, false),
                  new("detailed", new("boolean"), true)], firstResult)], null);

        var secondOptionsName = ProjectedMethods([export]).Where(method => method.Identifier.ValueText == "Format")
            .Select(method => method.ParameterList.Parameters[0].Type!.ToString())
            .Distinct().Single(name => name != "FormatOptionsInput");
        var usage = source.Replace("Tool.DigitsOptions", $"Tool.{secondOptionsName}", StringComparison.Ordinal);
        var result = await JoltProjectCompiler.CompileAsync([new("App.jazor", usage)], new() { NpmValues = [export] });

        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var js = result.Modules.Single().Content;
        StringAssert.Contains(js, "format as ");
        StringAssert.Contains(js, ".text");
        StringAssert.Contains(js, ".output");
    }

    private static IEnumerable<MethodDeclarationSyntax> ProjectedMethods(IReadOnlyList<NpmValueExport> exports) =>
        CSharpSyntaxTree.ParseText(JoltNpmBindings.GenerateSource(exports)).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>();
}
