using Jazor.Common.SourceMaps;

namespace Jolt.Compiler.Test;

[TestClass]
public sealed class TypeScriptDeclarationTests
{
    [TestMethod]
    public async Task Compile_VueContract_DeclaresPropsModelSlotsExposedAndStaticExports()
    {
        const string source = """
            <div><slot name="reading" amount="@Value" /></div>
            @code {
                [Parameter, EditorRequired] public int Value { get; set; }
                [Parameter] public Action<int>? ValueChanged { get; set; }
                [Parameter] public string? Caption { get; set; }
                [Expose] public int Multiply(int factor) => Value * factor;
                public static int Meter(int value) => value;
                public const int Initial = 2;
                private static int Hidden() => 3;
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("Meters/Meter.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var module = result.Modules.Single();
        var declaration = module.TypeScriptDeclaration;
        StringAssert.Contains(declaration, "\"Caption\"?: string | null;");
        StringAssert.Contains(declaration, "\"caption\"?: string | null;");
        StringAssert.Contains(declaration, "\"Value\": number;");
        StringAssert.Contains(declaration, "\"value\": number }");
        StringAssert.Contains(declaration, "\"onUpdate:value\"?: (value: number) => void;");
        StringAssert.Contains(declaration, "\"reading\"?(scope: {");
        StringAssert.Contains(declaration, "\"amount\": number;");
        StringAssert.Contains(declaration, "\"Multiply\"(arg0: number): number;");
        StringAssert.Contains(declaration, "export default component;");
        StringAssert.Contains(declaration, "as \"Meter\"");
        StringAssert.Contains(declaration, "as \"Initial\"");
        StringAssert.Contains(module.Content, "export function Meter(");
        StringAssert.Contains(module.Content, "\"caption\"");
        Assert.DoesNotContain("Hidden", declaration);

        var map = new SourceMapReader().Read(module.CreateTypeScriptSourceMap("Meter.d.mts", "../../Meters/Meter.jazor")!);
        Assert.AreEqual("Meter.d.mts", map.File);
        Assert.AreEqual("../../Meters/Meter.jazor", map.Sources.Single().Path);
        Assert.IsNull(map.Sources.Single().Content);
        var declarationLines = declaration.Split('\n');
        var authoredLines = source.ReplaceLineEndings("\n").Split('\n');
        foreach (var symbol in new[] { "amount", "Caption", "Multiply" })
        {
            var position = map.Segments.Single(segment => declarationLines[segment.GeneratedLine][segment.GeneratedColumn..].StartsWith($"\"{symbol}\"", StringComparison.Ordinal));
            StringAssert.StartsWith(authoredLines[position.SourceLine][position.SourceColumn..], symbol);
        }
    }

    [TestMethod]
    public async Task Compile_GenericVueContract_CarriesTypeParameterAcrossPropsSlotsAndRef()
    {
        const string source = """
            @typeparam T
            <div><slot name="item" value="@Items[0]" /></div>
            @code {
                [Parameter, EditorRequired] public T[] Items { get; set; } = [];
                [Expose] public T Read() => Items[0];
            }
            """;
        var result = await JoltProjectCompiler.CompileAsync([new("Generic.jazor", source)]);
        Assert.IsEmpty(result.Diagnostics, string.Join(Environment.NewLine, result.Diagnostics));
        var declaration = result.Modules.Single().TypeScriptDeclaration;
        StringAssert.Contains(declaration, "\"Items\": Array<$T>;");
        StringAssert.Contains(declaration, "\"items\": Array<$T> }");
        StringAssert.Contains(declaration, "\"value\": $T | null;");
        StringAssert.Contains(declaration, "\"Read\"(): $T;");
        StringAssert.Contains(declaration, "declare const component: <$T>(props: Props<$T>, context?: {");
        StringAssert.Contains(declaration, "slots: Slots<$T>;");
        StringAssert.Contains(declaration, "expose: (value: Exposed<$T>) => void;");
    }
}
