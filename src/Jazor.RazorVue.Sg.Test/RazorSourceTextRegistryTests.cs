using System.Collections.Immutable;
using System.Text;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSourceTextRegistryTests
{
    [TestMethod]
    public void TryCreate_AcceptsRazorTextAndRejectsOtherOrUnavailableAdditionalText()
    {
        Assert.IsNull(RazorSourceTextRegistry.TryCreate(
            new InMemoryAdditionalText("Pages/Counter.cs", "public sealed class Counter;"),
            CancellationToken.None));
        Assert.IsNull(RazorSourceTextRegistry.TryCreate(
            new InMemoryAdditionalText("Pages/Unavailable.razor", null),
            CancellationToken.None));

        var source = RazorSourceTextRegistry.TryCreate(
            new InMemoryAdditionalText("Pages/Counter.RAZOR", "<button>Counter</button>"),
            CancellationToken.None);

        Assert.IsNotNull(source);
        Assert.AreEqual("Pages/Counter.RAZOR", source.Value.Path);
        Assert.AreEqual("<button>Counter</button>", source.Value.Text);
    }

    [TestMethod]
    public void PushGeneratedTrees_UsesOnlyValidCarrierEntriesAndRestoresThePreviousScope()
    {
        var directCarrier = CSharpSyntaxTree.ParseText(
            RazorSourceTextRegistry.BuildCarrierSource(
            [
                new RazorSourceTextRegistry.RazorSourceText("Pages/Counter.razor", "<button>Counter</button>"),
                new RazorSourceTextRegistry.RazorSourceText(string.Empty, "ignored")
            ]),
            path: RazorSourceTextRegistry.CarrierHintName);
        var nestedCarrier = CSharpSyntaxTree.ParseText(
            RazorSourceTextRegistry.BuildCarrierSource(
            [new RazorSourceTextRegistry.RazorSourceText("Pages/Other.razor", "<p>Other</p>")]),
            path: "obj/Razor/" + RazorSourceTextRegistry.CarrierHintName);
        var ordinaryTree = CSharpSyntaxTree.ParseText("internal sealed class Ordinary;", path: "Generated.g.cs");

        Assert.IsTrue(RazorSourceTextRegistry.IsCarrierTree(directCarrier));
        Assert.IsTrue(RazorSourceTextRegistry.IsCarrierTree(nestedCarrier));
        Assert.IsFalse(RazorSourceTextRegistry.IsCarrierTree(ordinaryTree));

        using (RazorSourceTextRegistry.PushGeneratedTrees(
                   ImmutableArray.Create<SyntaxTree>(ordinaryTree, directCarrier, nestedCarrier),
                   CancellationToken.None))
        {
            Assert.AreEqual("<button>Counter</button>", RazorSourceTextRegistry.TryGet("pages/counter.razor"));
            Assert.AreEqual("<p>Other</p>", RazorSourceTextRegistry.TryGet("Pages/Other.razor"));
            Assert.IsNull(RazorSourceTextRegistry.TryGet(null));
        }

        Assert.IsNull(RazorSourceTextRegistry.TryGet("Pages/Counter.razor"));
    }

    [TestMethod]
    public void PushGeneratedTrees_IgnoresMalformedCarrierPayloads()
    {
        var malformedTrees = ImmutableArray.Create<SyntaxTree>(
            CreateCarrierTree("not-base64"),
            CreateCarrierTree(Convert.ToBase64String(Encoding.UTF8.GetBytes("unexpected-header\n"))),
            CreateCarrierTree(Convert.ToBase64String(Encoding.UTF8.GetBytes(
                "Jazor.RazorVue.RazorSourceTextCatalog/v1\nmissing-separator\n"))),
            CSharpSyntaxTree.ParseText(
                """
                namespace Jazor.RazorVue.Generation
                {
                    internal static class RazorSourceTextCatalog
                    {
                        internal static string Payload;
                    }
                }
                """,
                path: RazorSourceTextRegistry.CarrierHintName),
            CSharpSyntaxTree.ParseText("namespace Jazor.RazorVue.Generation { internal sealed class Other; }", path: RazorSourceTextRegistry.CarrierHintName));

        using (RazorSourceTextRegistry.PushGeneratedTrees(malformedTrees, CancellationToken.None))
        {
            Assert.IsNull(RazorSourceTextRegistry.TryGet("Pages/Counter.razor"));
        }
    }

    [TestMethod]
    public void PushGeneratedTrees_SkipsNonPayloadCarrierFields()
    {
        var carrierWithExtraField = CSharpSyntaxTree.ParseText(
            """
            namespace Jazor.RazorVue.Generation
            {
                internal static class RazorSourceTextCatalog
                {
                    internal const string Other = "ignored";
                    internal const string Payload = "SmF6b3IuUmF6b3JWdWUuUmF6b3JTb3VyY2VUZXh0Q2F0YWxvZy92MQpVR0ZuWlhNdlEyOTFiblJsY2k1eVlYcHZjZz09OlBHMWhhVzQrUTI5MWJuUmxjand2YldGcGJqND0K";
                }
            }
            """,
            path: RazorSourceTextRegistry.CarrierHintName);

        using (RazorSourceTextRegistry.PushGeneratedTrees(
                   ImmutableArray.Create<SyntaxTree>(carrierWithExtraField),
                   CancellationToken.None))
        {
            Assert.AreEqual("<main>Counter</main>", RazorSourceTextRegistry.TryGet("Pages/Counter.razor"));
        }
    }

    [TestMethod]
    public void SourceTextScope_UsesUniqueMatchesFallsBackToParentsAndRejectsAmbiguity()
    {
        using var parent = RazorSourceTextRegistry.Push(@"D:\repo\Pages\Parent.razor", "parent");
        using (RazorSourceTextRegistry.PushGeneratedTrees(
                   ImmutableArray.Create<SyntaxTree>(CSharpSyntaxTree.ParseText(
                       RazorSourceTextRegistry.BuildCarrierSource(
                       [
                           new RazorSourceTextRegistry.RazorSourceText("Features/Counter.razor", "first"),
                           new RazorSourceTextRegistry.RazorSourceText("Admin/Counter.razor", "second"),
                           new RazorSourceTextRegistry.RazorSourceText("Pages/Duplicate.razor", "first duplicate"),
                           new RazorSourceTextRegistry.RazorSourceText("Pages/Duplicate.razor", "second duplicate")
                        ]),
                       path: RazorSourceTextRegistry.CarrierHintName)),
                   CancellationToken.None))
        {
            Assert.AreEqual("parent", RazorSourceTextRegistry.TryGet("Pages/Parent.razor"));
            Assert.AreEqual("first", RazorSourceTextRegistry.TryGet(@"D:\repo\Features\Counter.razor"));
            Assert.IsNull(RazorSourceTextRegistry.TryGet("Counter.razor"));
            Assert.IsNull(RazorSourceTextRegistry.TryGet("Pages/Duplicate.razor"));
        }

        var lease = RazorSourceTextRegistry.Push("Pages/Detached.razor", "detached");
        lease.Dispose();
        lease.Dispose();

        Assert.IsNull(RazorSourceTextRegistry.TryGet("Pages/Detached.razor"));
    }

    private static SyntaxTree CreateCarrierTree(string payload)
        => CSharpSyntaxTree.ParseText(
            $$"""
            namespace Jazor.RazorVue.Generation
            {
                internal static class RazorSourceTextCatalog
                {
                    internal const string Payload = "{{payload}}";
                }
            }
            """,
            path: RazorSourceTextRegistry.CarrierHintName);

    private sealed class InMemoryAdditionalText(string path, string? text) : AdditionalText
    {
        private readonly SourceText? _text = text is null ? null : SourceText.From(text);

        public override string Path { get; } = path;

        public override SourceText? GetText(CancellationToken cancellationToken = default)
            => _text;
    }
}
