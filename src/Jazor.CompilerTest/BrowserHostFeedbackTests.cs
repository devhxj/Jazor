using Acornima;
using DenoHost.Core;
using ECMAScript;
using Jazor.Compiler;
using Jazor.ComplierTest;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class BrowserHostFeedbackTests
{
    [TestMethod]
    public async Task Convert_LiveClassListLocationAndNumberCasts_PreserveBrowserOperations()
    {
        const string source = """
            using ECMAScript;
            public static class TestModule
            {
                public static bool Update(Element element)
                {
                    var tokens = element.ClassList;
                    tokens.Add("ready");
                    tokens.Remove("loading");
                    return tokens.Toggle("selected");
                }
                public static void ReloadView(IWindow window) => window.LiveLocation.Reload();
                public static void ReloadBrowser(WindowRef window) => window.Location.Reload();
                public static long Signed(Number value) => (long)value;
                public static ulong Unsigned(Number value) => (ulong)value;
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(source, TestMetadataReferences.PreviewParseOptions);
        var compilation = CSharpCompilation.Create("BrowserHostFeedback", [tree],
            [.. TestMetadataReferences.Net11, MetadataReference.CreateFromFile(typeof(Global).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = compilation.GetDiagnostics().Where(static item => item.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.IsEmpty(errors, string.Join("\n", errors.Select(static item => item.ToString())));
        var model = compilation.GetSemanticModel(tree);
        var symbol = (INamedTypeSymbol)model.GetDeclaredSymbol(tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().Single())!;
        var script = (await new AstConverter(symbol, model).Convert())!.ToKnRECMAScript();
        _ = new Parser().ParseModule(script);
        StringAssert.Contains(script, ".classList");
        StringAssert.Contains(script, ".location.reload()");
        StringAssert.Contains(script, "BigInt(value)");

        var root = Path.Combine(Path.GetTempPath(), "jazor-browser-host-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "app.mjs"), script);
            var runner = Path.Combine(root, "app.test.mjs");
            await File.WriteAllTextAsync(runner, """
                import { Window } from "npm:happy-dom@20.10.6";
                import { Update, ReloadView, ReloadBrowser, Signed, Unsigned } from "./app.mjs";
                Deno.test("live classList and navigation methods", () => {
                  const document = new Window().document;
                  const element = document.createElement('div');
                  element.className = 'loading base';
                  if (!Update(element) || element.className !== 'base ready selected') throw new Error(element.className);
                  if (Update(element) || element.className !== 'base ready') throw new Error(element.className);
                  let reloads = 0;
                  const window = { location: { reload() { reloads++; } } };
                  ReloadView(window);
                  ReloadBrowser(window);
                  if (reloads !== 2) throw new Error('location method was not called');
                  if (Signed(-42) !== -42n || Unsigned(42) !== 42n) throw new Error('numeric domain changed');
                });
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            await Deno.Execute(new DenoExecuteBaseOptions { WorkingDirectory = root },
                ["test", "--quiet", "--allow-all", runner], timeout.Token);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
