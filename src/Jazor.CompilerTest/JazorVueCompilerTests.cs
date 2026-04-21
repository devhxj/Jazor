using System.Text.Json;
using Jazor.Vue;
using static Jazor.CompilerTest.SourceMapTestHelpers;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueCompilerTests
{
    [TestMethod]
    public void JazorVue_Parser_ParsesJsImportsAndInfersVueComponentsFromRazorMarkup()
    {
        var source = """
            @module { debounce } from "lodash-es"
            @module dayjs from "dayjs"

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
        Assert.AreEqual(JazorImportKind.VueImport, document.Imports[2].Kind);
        Assert.AreEqual("./UserCard.vue", document.Imports[2].Source);
        Assert.AreEqual("UserCard", document.Imports[2].Bindings[0].LocalName);
        StringAssert.Contains(document.Template, "<UserCard :title=\"title\" />");
        StringAssert.Contains(document.Code, "[Prop] public string Title");
    }

    [TestMethod]
    public void JazorVue_Parser_SupportsRazorImportDirectiveAndInfersImportKindFromSource()
    {
        var source = """
            @module UserCard from "./UserCard.vue"
            @module { debounce } from "lodash-es"

            <template>
              <div />
            </template>
            """;

        var parser = new JazorVueParser();
        var document = parser.Parse("Counter.jazor", source);

        Assert.AreEqual(2, document.Imports.Count);
        Assert.AreEqual(JazorImportKind.VueImport, document.Imports[0].Kind);
        Assert.AreEqual("./UserCard.vue", document.Imports[0].Source);
        Assert.AreEqual("UserCard", document.Imports[0].Bindings[0].LocalName);
        Assert.AreEqual(JazorImportKind.JSImport, document.Imports[1].Kind);
        Assert.AreEqual("lodash-es", document.Imports[1].Source);
        Assert.AreEqual("debounce", document.Imports[1].Bindings[0].LocalName);
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
    public void JazorVue_Parser_SkipsDirectiveWithoutBlockBodyBeforeRealCodeBlock()
    {
        var source = """
            @code

            @code {
                private int Count = 1;
            }
            """;

        var parser = new JazorVueParser();
        var document = parser.Parse("Counter.jazor", source);

        StringAssert.Contains(document.Code, "private int Count = 1;");
        Assert.IsTrue(document.CodeStartIndex > source.IndexOf("@code", StringComparison.Ordinal));
    }

    [TestMethod]
    public void JazorVue_Parser_IgnoresCommentedCodeDirectiveMarkersInRazorComments()
    {
        var source = """
            @*
            @code {
            *@
            @using System

            <template>
              <div>Hello</div>
            </template>
            """;

        var parser = new JazorVueParser();
        var document = parser.Parse("Counter.jazor", source);

        Assert.AreEqual(string.Empty, document.Code);
        Assert.AreEqual(-1, document.CodeStartIndex);
        StringAssert.Contains(document.Template, "<div>Hello</div>");
    }

    [TestMethod]
    public void JazorVue_Parser_IgnoresBracesInsideStringsAndCommentsWithinCodeBlock()
    {
        var source = """"
            @code {
                private string Json => "}";
                /* } */
                private string Raw => """
                {
                }
                """;
                private int Count => 1;
            }
            """";

        var parser = new JazorVueParser();
        var document = parser.Parse("Counter.jazor", source);

        StringAssert.Contains(document.Code, "private string Json => \"}\";");
        StringAssert.Contains(document.Code, "private string Raw =>");
        StringAssert.Contains(document.Code, "private int Count => 1;");
    }

    [TestMethod]
    public void JazorVue_Parser_IgnoresModuleDirectivesInsideCommentsAndCodeBlocks()
    {
        var source = """
            @module dayjs from "dayjs"
            @*
            @module FakeComment from "./FakeComment.vue"
            *@

            @code {
                @module FakeCode from "./fake-code.ts"
            }

            <template>
              <div />
            </template>
            """;

        var parser = new JazorVueParser();
        var document = parser.Parse("Counter.jazor", source);

        Assert.AreEqual(1, document.Imports.Count);
        Assert.AreEqual("dayjs", document.Imports[0].Source);
        Assert.AreEqual("dayjs", document.Imports[0].Bindings[0].LocalName);
    }

    [TestMethod]
    public void JazorVue_Compiler_EmitsBridgeVueArtifactFromDocument()
    {
        var source = """
            @module { debounce } from "lodash-es"
            @module dayjs from "dayjs"

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
    public void JazorVue_Compiler_EmitsGeneratedVueSourceMapForGeneratedArtifact()
    {
        var source = """
            @module { debounce } from "lodash-es"

            <template>
              <button @click="increment()">@Count</button>
            </template>

            @code {
                [State] private int Count = 1;

                public void Increment()
                {
                    Count++;
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        Assert.IsNotNull(result.GeneratedVueSourceMap);
        using var sourceMap = JsonDocument.Parse(result.GeneratedVueSourceMap);
        Assert.AreEqual(3, sourceMap.RootElement.GetProperty("version").GetInt32());
        Assert.AreEqual("Counter.jazor", sourceMap.RootElement.GetProperty("file").GetString());
        Assert.AreEqual("Counter.jazor", sourceMap.RootElement.GetProperty("sources")[0].GetString());
        Assert.AreEqual(source, sourceMap.RootElement.GetProperty("sourcesContent")[0].GetString());

        var mappedLines = DecodeGeneratedLineToSourceLine(sourceMap.RootElement);
        var segments = DecodeSegments(sourceMap.RootElement);
        AssertGeneratedLineMapsToSourceLine(
            result.GeneratedVueText,
            "const count = ref(1);",
            source,
            "[State] private int Count = 1;",
            mappedLines);
        AssertGeneratedLineMapsToSourceLine(
            result.GeneratedVueText,
            "function increment()",
            source,
            "public void Increment()",
            mappedLines);
        AssertGeneratedLineMapsToSourceLine(
            result.GeneratedVueText,
            "count.value++;",
            source,
            "Count++;",
            mappedLines);
        AssertGeneratedLineMapsToSourceLine(
            result.GeneratedVueText,
            "<button @click=\"increment()\">@Count</button>",
            source,
            "<button @click=\"increment()\">@Count</button>",
            mappedLines);

        AssertGeneratedLineHasStableColumnZeroAnchor(
            result.GeneratedVueText,
            "count.value++;",
            source,
            "Count++;",
            segments);
        AssertGeneratedLineHasNonZeroColumnMapping(
            result.GeneratedVueText,
            "count.value++;",
            source,
            "Count++;",
            segments);
        AssertGeneratedLineHasNonZeroColumnMapping(
            result.GeneratedVueText,
            "[State] private int Count = 1;",
            source,
            "[State] private int Count = 1;",
            segments);
        AssertGeneratedLineHasNonZeroColumnMapping(
            result.GeneratedVueText,
            "<button @click=\"increment()\">@Count</button>",
            source,
            "<button @click=\"increment()\">@Count</button>",
            segments);
    }

    [TestMethod]
    public void JazorVue_Compiler_EmitsColumnShiftSegmentForGeneratedPunctuationLine()
    {
        var source = """
            <template>
              <div>@Title</div>
            </template>

            @code {
                [Prop] public string Title { get; set; } = "";
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        Assert.IsNotNull(result.GeneratedVueSourceMap);
        using var sourceMap = JsonDocument.Parse(result.GeneratedVueSourceMap);
        var segments = DecodeSegments(sourceMap.RootElement);

        AssertGeneratedLineHasSourceColumnShiftMapping(
            result.GeneratedVueText,
            "});",
            source,
            "[Prop] public string Title { get; set; } = \"\";",
            segments);
    }

    [TestMethod]
    public void JazorVue_Compiler_EmitsMultipleColumnAnchorsForLoweredExpressionLine()
    {
        var source = """
            <template>
              <div>@Count</div>
            </template>

            @code {
                [Prop] public int Step { get; set; } = 2;
                [State] private int Count = 1;

                public void Sync()
                {
                    Count = Count + Step;
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        Assert.IsNotNull(result.GeneratedVueSourceMap);
        using var sourceMap = JsonDocument.Parse(result.GeneratedVueSourceMap);
        var segments = DecodeSegments(sourceMap.RootElement);

        AssertGeneratedLineHasMultipleNonZeroColumnMappings(
            result.GeneratedVueText,
            "count.value = count.value + step.value;",
            source,
            "Count = Count + Step;",
            segments,
            minimumDistinctMappings: 2);
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
    public void JazorVue_Compiler_ExtractsMethodBodiesWithoutTruncatingOnBracesInsideStringsAndComments()
    {
        var source = """
            <template>
              <div>{{ count }}</div>
            </template>

            @code {
                [State] private int count = 0;

                public void Refresh()
                {
                    var json = "{";
                    // }
                    /* { } */
                    if (count == 0)
                    {
                        count++;
                    }

                    count++;
                }

                public void Reset()
                {
                    count = 0;
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "function refresh()");
        StringAssert.Contains(result.GeneratedVueText, "let json = \"{\";");
        StringAssert.Contains(result.GeneratedVueText, "if (count.value == 0)");
        Assert.AreEqual(2, CountOccurrences(result.GeneratedVueText, "count.value++;"));
        StringAssert.Contains(result.GeneratedVueText, "function reset()");
        StringAssert.Contains(result.GeneratedVueText, "count.value = 0;");
    }

    [TestMethod]
    public void JazorVue_Compiler_DoesNotRewriteSpecialTokensInsideStringLiterals()
    {
        var source = """
            <template>
              <button @click="refreshAsync()">{{ title }}</button>
            </template>

            @code {
                [Prop] public string? Title { get; set; }

                public async Task RefreshAsync()
                {
                    var literal = "this.Title|string.Empty|Task.CompletedTask|new InvalidOperationException(";
                    var actual = this.Title ?? string.Empty;
                    await Task.CompletedTask;
                    throw new InvalidOperationException("boom");
                }
            }
            """;

        var parser = new JazorVueParser();
        var compiler = new JazorVueCompiler();
        var document = parser.Parse("Counter.jazor", source);
        var result = compiler.Compile(document);

        StringAssert.Contains(result.GeneratedVueText, "let literal = \"this.Title|string.Empty|Task.CompletedTask|new InvalidOperationException(\";");
        StringAssert.Contains(result.GeneratedVueText, "let actual = title.value ?? \"\";");
        StringAssert.Contains(result.GeneratedVueText, "await Promise.resolve();");
        StringAssert.Contains(result.GeneratedVueText, "throw new Error(\"boom\");");
        Assert.IsFalse(result.GeneratedVueText.Contains("\"title.value|\"\"|Promise.resolve()|new Error(\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void JazorVue_Parser_ParsesDefaultNamespaceAndNamedAliasBindings()
    {
        var source = """
            @module dayjs from "dayjs"
            @module * as math from "./math"
            @module { format as formatDate, debounce } from "date-kit"
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
            @module Vue, { ref as vueRef, computed } from "vue"
            @module React, * as ReactRuntime from "react"
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
                "@module dayjs from \"dayjs\""),
            new JazorImportDirective(
                JazorImportKind.JSImport,
                "./math",
                [new JazorImportBinding("math", null, JazorImportBindingKind.Namespace)],
                "@module * as math from \"./math\""),
            new JazorImportDirective(
                JazorImportKind.JSImport,
                "vueuse",
                [
                    new JazorImportBinding("useMouse", "useMouse", JazorImportBindingKind.Named),
                    new JazorImportBinding("debounce", "debounce", JazorImportBindingKind.Named)
                ],
                "@module { useMouse, debounce } from \"vueuse\""),
            new JazorImportDirective(
                JazorImportKind.VueImport,
                "./UserCard.vue",
                [new JazorImportBinding("UserCard", null, JazorImportBindingKind.Default)],
                "@module UserCard from \"./UserCard.vue\"")
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
            @module dayjs from "dayjs"
            @module * as math from "./math"
            @module { format as formatDate, debounce } from "date-kit"

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
            @module Vue, { ref as vueRef, computed } from "vue"
            @module React, * as ReactRuntime from "react"

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

    [TestMethod]
    public void JazorVue_Parser_InfersNearbyVueImportsFromRazorMarkup()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var componentsDirectory = Path.Combine(tempDirectory, "Components");
            Directory.CreateDirectory(componentsDirectory);
            File.WriteAllText(
                Path.Combine(componentsDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");

            var parser = new JazorVueParser();
            var document = parser.Parse(documentPath, """
                <UserCard />

                @code {
                    [Prop] public string Title { get; set; } = "";
                }
                """);

            Assert.AreEqual(1, document.Imports.Count);
            Assert.AreEqual(JazorImportKind.VueImport, document.Imports[0].Kind);
            Assert.AreEqual("./Components/UserCard.vue", document.Imports[0].Source);
            Assert.AreEqual("UserCard", document.Imports[0].Bindings[0].LocalName);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [TestMethod]
    public void JazorVue_Compiler_EmitsInferredNearbyVueImportsFromRazorMarkup()
    {
        var tempDirectory = CreateTemporaryDirectory();

        try
        {
            var documentPath = Path.Combine(tempDirectory, "Counter.jazor");
            var componentsDirectory = Path.Combine(tempDirectory, "Components");
            Directory.CreateDirectory(componentsDirectory);
            File.WriteAllText(
                Path.Combine(componentsDirectory, "UserCard.vue"),
                "<template><div>UserCard</div></template>");

            var parser = new JazorVueParser();
            var compiler = new JazorVueCompiler();
            var document = parser.Parse(documentPath, """
                <UserCard Title="@Title" />

                @code {
                    [Prop] public string Title { get; set; } = "";
                }
                """);
            var result = compiler.Compile(document);

            StringAssert.Contains(result.GeneratedVueText, "import UserCard from \"./Components/UserCard.vue\";");
            StringAssert.Contains(result.GeneratedVueText, "<UserCard Title=\"@Title\" />");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JazorVueCompilerTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var searchIndex = 0;
        while ((searchIndex = text.IndexOf(value, searchIndex, StringComparison.Ordinal)) >= 0)
        {
            count++;
            searchIndex += value.Length;
        }

        return count;
    }

    private static void AssertGeneratedLineHasStableColumnZeroAnchor(
        string generatedText,
        string generatedNeedle,
        string sourceText,
        string sourceNeedle,
        IReadOnlyList<SourceMapSegment> segments)
    {
        var generatedLine = GetLineIndexContaining(generatedText, generatedNeedle);
        var sourceLine = GetLineIndexContaining(sourceText, sourceNeedle);
        var lineSegments = segments
            .Where(segment => segment.GeneratedLine == generatedLine)
            .OrderBy(segment => segment.GeneratedColumn)
            .ToArray();

        Assert.IsTrue(lineSegments.Length > 0, $"Expected source-map segments for generated line containing '{generatedNeedle}'.");
        Assert.AreEqual(0, lineSegments[0].GeneratedColumn);
        Assert.AreEqual(sourceLine, lineSegments[0].SourceLine);
        Assert.AreEqual(0, lineSegments[0].SourceColumn);
    }

    private static void AssertGeneratedLineHasNonZeroColumnMapping(
        string generatedText,
        string generatedNeedle,
        string sourceText,
        string sourceNeedle,
        IReadOnlyList<SourceMapSegment> segments)
    {
        var generatedLine = GetLineIndexContaining(generatedText, generatedNeedle);
        var sourceLine = GetLineIndexContaining(sourceText, sourceNeedle);

        Assert.IsTrue(
            segments.Any(segment =>
                segment.GeneratedLine == generatedLine
                && segment.SourceLine == sourceLine
                && segment.GeneratedColumn > 0
                && segment.SourceColumn > 0),
            $"Expected a non-zero column source-map segment for generated line containing '{generatedNeedle}'.");
    }

    private static void AssertGeneratedLineHasSourceColumnShiftMapping(
        string generatedText,
        string generatedNeedle,
        string sourceText,
        string sourceNeedle,
        IReadOnlyList<SourceMapSegment> segments)
    {
        var generatedLine = GetLineIndexContaining(generatedText, generatedNeedle);
        var sourceLine = GetLineIndexContaining(sourceText, sourceNeedle);

        Assert.IsTrue(
            segments.Any(segment =>
                segment.GeneratedLine == generatedLine
                && segment.SourceLine == sourceLine
                && segment.GeneratedColumn == 0
                && segment.SourceColumn > 0),
            $"Expected a source-column-shift segment for generated line containing '{generatedNeedle}'.");
    }

    private static void AssertGeneratedLineHasMultipleNonZeroColumnMappings(
        string generatedText,
        string generatedNeedle,
        string sourceText,
        string sourceNeedle,
        IReadOnlyList<SourceMapSegment> segments,
        int minimumDistinctMappings)
    {
        var generatedLine = GetLineIndexContaining(generatedText, generatedNeedle);
        var sourceLine = GetLineIndexContaining(sourceText, sourceNeedle);

        var mappedColumns = segments
            .Where(segment =>
                segment.GeneratedLine == generatedLine
                && segment.SourceLine == sourceLine
                && segment.GeneratedColumn > 0
                && segment.SourceColumn > 0)
            .Select(segment => (segment.GeneratedColumn, segment.SourceColumn))
            .Distinct()
            .ToArray();

        Assert.IsTrue(
            mappedColumns.Length >= minimumDistinctMappings,
            $"Expected at least {minimumDistinctMappings} non-zero column source-map anchors for generated line containing '{generatedNeedle}', actual {mappedColumns.Length}.");
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
