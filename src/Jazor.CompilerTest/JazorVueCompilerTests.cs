using Jazor.Vue;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueCompilerTests
{
    [TestMethod]
    public void JazorVue_Parser_ParsesImportsTemplateAndCode()
    {
        var source = """
            @jsimport { debounce } from "lodash-es"
            @jsimport dayjs from "dayjs"
            @vueimport UserCard from "./UserCard.vue"

            <template>
              <UserCard :title="title" />
            </template>

            @code {
                [Prop] public string Title { get; set; } = "";
                [State] private int count = 1;
            }
            """;

        var parser = new JazorVueParser();
        var document = parser.Parse("Counter.jazor", source);

        Assert.AreEqual("Counter.jazor", document.FilePath);
        Assert.AreEqual(3, document.Imports.Count);
        Assert.AreEqual(JazorImportKind.JSImport, document.Imports[0].Kind);
        Assert.AreEqual("lodash-es", document.Imports[0].Source);
        Assert.AreEqual("UserCard", document.Imports[2].Bindings[0].LocalName);
        StringAssert.Contains(document.Template, "<UserCard :title=\"title\" />");
        StringAssert.Contains(document.Code, "[Prop] public string Title");
    }

    [TestMethod]
    public void JazorVue_Parser_UsesRazorMarkupWhenTemplateBlockIsAbsent()
    {
        var source = """
            <UserCard Title="@Title" />

            @code {
                [Prop] public string Title { get; set; } = "";
            }
            """;

        var parser = new JazorVueParser();
        var document = parser.Parse("Counter.jazor", source);

        StringAssert.Contains(document.Template, "<UserCard Title=\"@Title\" />");
        StringAssert.Contains(document.Code, "[Prop] public string Title");
    }

    [TestMethod]
    public void JazorVue_Compiler_EmitsBridgeVueArtifactFromDocument()
    {
        var source = """
            @jsimport { debounce } from "lodash-es"
            @jsimport dayjs from "dayjs"
            @vueimport UserCard from "./UserCard.vue"

            <template>
              <UserCard :title="title" />
            </template>

            @code {
                [Prop] public string Title { get; set; } = "";
                [State] private int count = 1;
                [Computed] public string Label => $"Count: {count}";

                public void Increment()
                {
                    count++;
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        Assert.AreEqual(3, result.ExternalSymbols.Symbols.Count);
        StringAssert.Contains(result.GeneratedVueText, "import { debounce } from \"lodash-es\";");
        StringAssert.Contains(result.GeneratedVueText, "import dayjs from \"dayjs\";");
        StringAssert.Contains(result.GeneratedVueText, "import UserCard from \"./UserCard.vue\";");
        StringAssert.Contains(result.GeneratedVueText, "import { computed, ref, toRef } from \"vue\";");
        StringAssert.Contains(result.GeneratedVueText, "const props = defineProps({");
        StringAssert.Contains(result.GeneratedVueText, "title: String");
        StringAssert.Contains(result.GeneratedVueText, "const title = toRef(props, \"title\");");
        StringAssert.Contains(result.GeneratedVueText, "const count = ref(1);");
        StringAssert.Contains(result.GeneratedVueText, "const label = computed(() => `Count: ${count.value}`);");
        StringAssert.Contains(result.GeneratedVueText, "function increment()");
        StringAssert.Contains(result.GeneratedVueText, "count.value++;");
        StringAssert.Contains(result.GeneratedVueText, "<UserCard :title=\"title\" />");
    }

    [TestMethod]
    public void JazorVue_Compiler_LowersSimpleComputedExpressionsAndMethodBodies()
    {
        var source = """
            <template>
              <button @click="increment(step)">{{ summary }}</button>
            </template>

            @code {
                [Prop] public int Step { get; set; } = 2;
                [State] private int count = 1;
                [Computed] public string Summary => $"Count: {count + Step}";

                public string Increment(int delta)
                {
                    var next = count + delta + Step;
                    count = next;
                    return Summary;
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "import { computed, ref, toRef } from \"vue\";");
        StringAssert.Contains(result.GeneratedVueText, "const step = toRef(props, \"step\");");
        StringAssert.Contains(result.GeneratedVueText, "const count = ref(1);");
        StringAssert.Contains(result.GeneratedVueText, "const summary = computed(() => `Count: ${count.value + step.value}`);");
        StringAssert.Contains(result.GeneratedVueText, "function increment(delta) {");
        StringAssert.Contains(result.GeneratedVueText, "let next = count.value + delta + step.value;");
        StringAssert.Contains(result.GeneratedVueText, "count.value = next;");
        StringAssert.Contains(result.GeneratedVueText, "return summary.value;");
        Assert.IsFalse(result.GeneratedVueText.Contains("TODO: lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public void JazorVue_Compiler_LowersTypedLocalsAndSimpleIfElseControlFlow()
    {
        var source = """
            <template>
              <div>{{ normalizedLabel }}</div>
            </template>

            @code {
                [Prop] public string? Title { get; set; }
                [State] private int count = 0;
                [Computed] public string NormalizedLabel => Title ?? $"Count: {count}";

                public string GetLabel()
                {
                    string label = Title ?? "Untitled";
                    if (count > 0)
                    {
                        return label;
                    }
                    else
                    {
                        return $"Count: {count}";
                    }
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "const normalizedLabel = computed(() => title.value ?? `Count: ${count.value}`);");
        StringAssert.Contains(result.GeneratedVueText, "function getLabel() {");
        StringAssert.Contains(result.GeneratedVueText, "let label = title.value ?? \"Untitled\";");
        StringAssert.Contains(result.GeneratedVueText, "if (count.value > 0)");
        StringAssert.Contains(result.GeneratedVueText, "return label;");
        StringAssert.Contains(result.GeneratedVueText, "return `Count: ${count.value}`;");
        Assert.IsFalse(result.GeneratedVueText.Contains("Fallback compiler could not lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public void JazorVue_Compiler_LowersForWhileAwaitAndStableMemberAccess()
    {
        var source = """
            <template>
              <button @click="refreshAsync()">{{ title }}</button>
            </template>

            @code {
                [Prop] public int Step { get; set; } = 3;
                [Prop] public string? Title { get; set; }
                [State] private int count = 0;

                public async Task LogAsync(string value)
                {
                    await Task.CompletedTask;
                }

                public async Task RefreshAsync(CardModel model)
                {
                    for (int i = 0; i < Step; i++)
                    {
                        if (i == 1)
                        {
                            continue;
                        }

                        count += i;
                    }

                    while (count < Step)
                    {
                        count++;
                        if (count > 10)
                        {
                            break;
                        }
                    }

                    if (model.Title != null)
                    {
                        await LogAsync(model.Title);
                    }
                    else if (Title != null)
                    {
                        await LogAsync(Title);
                        return;
                    }
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "import { ref, toRef } from \"vue\";");
        StringAssert.Contains(result.GeneratedVueText, "const step = toRef(props, \"step\");");
        StringAssert.Contains(result.GeneratedVueText, "const title = toRef(props, \"title\");");
        StringAssert.Contains(result.GeneratedVueText, "async function refreshAsync(model) {");
        StringAssert.Contains(result.GeneratedVueText, "for (let i = 0; i < step.value; i++)");
        StringAssert.Contains(result.GeneratedVueText, "continue;");
        StringAssert.Contains(result.GeneratedVueText, "count.value += i;");
        StringAssert.Contains(result.GeneratedVueText, "while (count.value < step.value)");
        StringAssert.Contains(result.GeneratedVueText, "break;");
        StringAssert.Contains(result.GeneratedVueText, "if (model.Title != null)");
        StringAssert.Contains(result.GeneratedVueText, "await logAsync(model.Title);");
        StringAssert.Contains(result.GeneratedVueText, "else if (title.value != null)");
        StringAssert.Contains(result.GeneratedVueText, "await logAsync(title.value);");
        StringAssert.Contains(result.GeneratedVueText, "return;");
        Assert.IsFalse(result.GeneratedVueText.Contains("model.title.value", StringComparison.Ordinal));
        Assert.IsFalse(result.GeneratedVueText.Contains("Fallback compiler could not lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public void JazorVue_Compiler_LowersForeachTypedCatchAndExceptionConstruction()
    {
        var source = """
            <template>
              <button @click="refreshAsync()">{{ title }}</button>
            </template>

            @code {
                [Prop] public IEnumerable<int> Numbers { get; set; } = Array.Empty<int>();
                [Prop] public string? Title { get; set; }
                [State] private int count = 0;

                public async Task LogAsync(string value)
                {
                    await Task.CompletedTask;
                }

                public void ThrowBoom()
                {
                    throw new InvalidOperationException(Title ?? "boom");
                }

                public async Task RefreshAsync()
                {
                    foreach (var number in Numbers)
                    {
                        count += number;
                    }

                    try
                    {
                        ThrowBoom();
                    }
                    catch (InvalidOperationException ex)
                    {
                        await LogAsync(ex.Message);
                    }
                    finally
                    {
                        count++;
                    }
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "const numbers = toRef(props, \"numbers\");");
        StringAssert.Contains(result.GeneratedVueText, "function throwBoom()");
        StringAssert.Contains(result.GeneratedVueText, "throw new Error(title.value ?? \"boom\");");
        StringAssert.Contains(result.GeneratedVueText, "async function refreshAsync() {");
        StringAssert.Contains(result.GeneratedVueText, "for (const number of numbers.value)");
        StringAssert.Contains(result.GeneratedVueText, "count.value += number;");
        StringAssert.Contains(result.GeneratedVueText, "try");
        StringAssert.Contains(result.GeneratedVueText, "catch (ex)");
        StringAssert.Contains(result.GeneratedVueText, "await logAsync(ex.Message);");
        StringAssert.Contains(result.GeneratedVueText, "finally");
        StringAssert.Contains(result.GeneratedVueText, "count.value++;");
        Assert.IsFalse(result.GeneratedVueText.Contains("Fallback compiler could not lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public void JazorVue_Compiler_RestoresMemberRewritesAfterBlockScopedShadowing()
    {
        var source = """
            <template>
              <button @click="refreshAsync()">{{ title }}</button>
            </template>

            @code {
                [Prop] public string? Title { get; set; }

                public async Task LogAsync(string value)
                {
                    await Task.CompletedTask;
                }

                public async Task RefreshAsync()
                {
                    if (Title != null)
                    {
                        string Title = "local";
                        await LogAsync(Title);
                    }

                    await LogAsync(Title ?? "fallback");
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "let Title = \"local\";");
        StringAssert.Contains(result.GeneratedVueText, "await logAsync(Title);");
        StringAssert.Contains(result.GeneratedVueText, "await logAsync(title.value ?? \"fallback\");");
        Assert.IsFalse(result.GeneratedVueText.Contains("await logAsync(Title ?? \"fallback\")", StringComparison.Ordinal));
        Assert.IsFalse(result.GeneratedVueText.Contains("Fallback compiler could not lower", StringComparison.Ordinal));
    }

    [TestMethod]
    public void JazorVue_Parser_ParsesDefaultNamespaceAndNamedAliasBindings()
    {
        var source = """
            @jsimport dayjs from "dayjs"
            @jsimport * as math from "./math"
            @jsimport { format as formatDate, debounce } from "date-kit"
            """;

        var parser = new JazorVueParser();
        var document = parser.Parse("ImportsOnly.jazor", source);

        Assert.AreEqual(3, document.Imports.Count);

        var defaultImport = document.Imports[0];
        Assert.AreEqual(JazorImportBindingKind.Default, defaultImport.Bindings[0].BindingKind);
        Assert.AreEqual("dayjs", defaultImport.Bindings[0].LocalName);
        Assert.IsNull(defaultImport.Bindings[0].ImportedName);

        var namespaceImport = document.Imports[1];
        Assert.AreEqual(JazorImportBindingKind.Namespace, namespaceImport.Bindings[0].BindingKind);
        Assert.AreEqual("math", namespaceImport.Bindings[0].LocalName);
        Assert.IsNull(namespaceImport.Bindings[0].ImportedName);

        var namedImport = document.Imports[2];
        Assert.AreEqual(2, namedImport.Bindings.Count);
        Assert.AreEqual(JazorImportBindingKind.Named, namedImport.Bindings[0].BindingKind);
        Assert.AreEqual("formatDate", namedImport.Bindings[0].LocalName);
        Assert.AreEqual("format", namedImport.Bindings[0].ImportedName);
        Assert.AreEqual("debounce", namedImport.Bindings[1].LocalName);
        Assert.AreEqual("debounce", namedImport.Bindings[1].ImportedName);
    }

    [TestMethod]
    public void JazorVue_Parser_ParsesDefaultPlusNamedAndDefaultPlusNamespaceBindings()
    {
        var source = """
            @jsimport Vue, { ref as vueRef, computed } from "vue"
            @jsimport React, * as ReactRuntime from "react"
            """;

        var parser = new JazorVueParser();
        var document = parser.Parse("CompositeImports.jazor", source);

        Assert.AreEqual(2, document.Imports.Count);

        var defaultPlusNamedImport = document.Imports[0];
        Assert.AreEqual(3, defaultPlusNamedImport.Bindings.Count);
        Assert.AreEqual(JazorImportBindingKind.Default, defaultPlusNamedImport.Bindings[0].BindingKind);
        Assert.AreEqual("Vue", defaultPlusNamedImport.Bindings[0].LocalName);
        Assert.AreEqual(JazorImportBindingKind.Named, defaultPlusNamedImport.Bindings[1].BindingKind);
        Assert.AreEqual("vueRef", defaultPlusNamedImport.Bindings[1].LocalName);
        Assert.AreEqual("ref", defaultPlusNamedImport.Bindings[1].ImportedName);
        Assert.AreEqual(JazorImportBindingKind.Named, defaultPlusNamedImport.Bindings[2].BindingKind);
        Assert.AreEqual("computed", defaultPlusNamedImport.Bindings[2].LocalName);

        var defaultPlusNamespaceImport = document.Imports[1];
        Assert.AreEqual(2, defaultPlusNamespaceImport.Bindings.Count);
        Assert.AreEqual(JazorImportBindingKind.Default, defaultPlusNamespaceImport.Bindings[0].BindingKind);
        Assert.AreEqual("React", defaultPlusNamespaceImport.Bindings[0].LocalName);
        Assert.AreEqual(JazorImportBindingKind.Namespace, defaultPlusNamespaceImport.Bindings[1].BindingKind);
        Assert.AreEqual("ReactRuntime", defaultPlusNamespaceImport.Bindings[1].LocalName);
    }

    [TestMethod]
    public void JazorVue_VirtualExternalSymbolProjection_ClassifiesImportsByExportAndSymbolKind()
    {
        var imports = new[]
        {
            new JazorImportDirective(
                JazorImportKind.JSImport,
                "dayjs",
                [new JazorImportBinding("dayjs", null, JazorImportBindingKind.Default)],
                "@jsimport dayjs from \"dayjs\""),
            new JazorImportDirective(
                JazorImportKind.JSImport,
                "./math",
                [new JazorImportBinding("math", null, JazorImportBindingKind.Namespace)],
                "@jsimport * as math from \"./math\""),
            new JazorImportDirective(
                JazorImportKind.JSImport,
                "vueuse",
                [
                    new JazorImportBinding("useMouse", "useMouse", JazorImportBindingKind.Named),
                    new JazorImportBinding("debounce", "debounce", JazorImportBindingKind.Named)
                ],
                "@jsimport { useMouse, debounce } from \"vueuse\""),
            new JazorImportDirective(
                JazorImportKind.VueImport,
                "./UserCard.vue",
                [new JazorImportBinding("UserCard", null, JazorImportBindingKind.Default)],
                "@vueimport UserCard from \"./UserCard.vue\"")
        };

        var symbols = VirtualExternalSymbolTable.FromImports(imports).Symbols.ToDictionary(static symbol => symbol.PublicName);

        Assert.AreEqual(5, symbols.Count);
        AssertSymbol(symbols["dayjs"], ExternalExportKind.Default, ExternalSymbolKind.Value, false);
        AssertSymbol(symbols["math"], ExternalExportKind.Namespace, ExternalSymbolKind.Namespace, false);
        AssertSymbol(symbols["useMouse"], ExternalExportKind.Named, ExternalSymbolKind.Composable, false);
        AssertSymbol(symbols["debounce"], ExternalExportKind.Named, ExternalSymbolKind.Function, false);
        AssertSymbol(symbols["UserCard"], ExternalExportKind.Default, ExternalSymbolKind.Component, true);
    }

    [TestMethod]
    public void JazorVue_Compiler_EmitsDefaultNamespaceAndNamedAliasImportSyntax()
    {
        var source = """
            @jsimport dayjs from "dayjs"
            @jsimport * as math from "./math"
            @jsimport { format as formatDate, debounce } from "date-kit"

            <template>
              <div>{{ formatDate(dayjs()) }} {{ math.PI }} {{ debounce }}</div>
            </template>

            @code {
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("ImportsOnly.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "import dayjs from \"dayjs\";");
        StringAssert.Contains(result.GeneratedVueText, "import * as math from \"./math\";");
        StringAssert.Contains(result.GeneratedVueText, "import { format as formatDate, debounce } from \"date-kit\";");
    }

    [TestMethod]
    public void JazorVue_Compiler_EmitsCompositeImportSyntax()
    {
        var source = """
            @jsimport Vue, { ref as vueRef, computed } from "vue"
            @jsimport React, * as ReactRuntime from "react"

            <template>
              <div>{{ vueRef }} {{ computed }} {{ ReactRuntime.Fragment }}</div>
            </template>

            @code {
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("CompositeImports.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "import Vue, { ref as vueRef, computed } from \"vue\";");
        StringAssert.Contains(result.GeneratedVueText, "import React, * as ReactRuntime from \"react\";");
    }

    private static void AssertSymbol(
        ExternalSymbolDescriptor symbol,
        ExternalExportKind expectedExportKind,
        ExternalSymbolKind expectedSymbolKind,
        bool expectedTemplateVisible)
    {
        Assert.AreEqual(expectedExportKind, symbol.ExportKind);
        Assert.AreEqual(expectedSymbolKind, symbol.SymbolKind);
        Assert.AreEqual(expectedTemplateVisible, symbol.TemplateVisible);
        Assert.AreEqual(symbol.PublicName, symbol.RuntimeImportName);
        Assert.AreEqual(ExternalTypeQuality.Opaque, symbol.TypeQuality);
    }
}
