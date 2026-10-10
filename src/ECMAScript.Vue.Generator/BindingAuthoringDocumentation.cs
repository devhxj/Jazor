using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using File = System.IO.File;

/// <summary>Maintains type-derived Razor hints and scalar projections, without a component-name registry.</summary>
internal sealed class BindingAuthoringDocumentation
{
    private const string ProjectionRegion = "Generated union authoring projections";
    private const string NumericRegion = "Generated union numeric conversions";
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Preview);
    private readonly Dictionary<string, UnionShape> _unions = new(StringComparer.Ordinal);
    private readonly SortedSet<string> _numberInputs = new(StringComparer.Ordinal);

    internal BindingAuthoringDocumentation(string repository) : this(BindingFiles(repository).Select(File.ReadAllText)) { }

    internal BindingAuthoringDocumentation(IEnumerable<string> sources)
    {
        // This is generation tooling. The authored declarations, not a runtime registry,
        // remain the source of truth for names, branches and legal assignment inputs.
        foreach (var source in sources) ReadShapes(StripMaintainedContent(source));
    }

    internal static IEnumerable<string> BindingFiles(string repository)
        => Directory.EnumerateDirectories(Path.Combine(repository, "src"), "ECMAScript*")
            .Where(static directory => !Path.GetFileName(directory).Contains("Test", StringComparison.Ordinal) &&
                !Path.GetFileName(directory).Contains("Generator", StringComparison.Ordinal))
            .SelectMany(SourceFiles).Order(StringComparer.Ordinal);

    private static IEnumerable<string> SourceFiles(string directory)
        => Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
            .Where(static path => !path.Replace('\\', '/').Contains("/obj/", StringComparison.Ordinal) &&
                !path.Replace('\\', '/').Contains("/bin/", StringComparison.Ordinal));

    internal static void Run(string[] args)
    {
        var check = args is ["--check"];
        if (!check && args.Length != 0) throw new ArgumentException("Supported arguments: --check.");
        var repository = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(repository, "Jazor.slnx")))
            repository = Directory.GetParent(repository)?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        var documentation = new BindingAuthoringDocumentation(repository);
        var changed = new List<string>();
        var hints = 0;
        var projections = 0;
        foreach (var path in BindingFiles(repository))
        {
            var original = File.ReadAllText(path);
            var updated = documentation.Annotate(original);
            hints += Regex.Matches(updated, "data-authoring=\"types\"").Count;
            projections += Regex.Matches(updated, "#region " + ProjectionRegion).Count;
            if (original == updated) continue;
            changed.Add(Path.GetRelativePath(repository, path));
            if (!check) File.WriteAllText(path, updated, new UTF8Encoding(false));
        }
        Console.WriteLine($"Binding authoring: {hints} property hints, {projections} unions with direct scalar projections; {changed.Count} files {(check ? "need updates" : "updated")}.");
        if (check && changed.Count > 0)
            throw new InvalidOperationException("Type-derived authoring output is stale. Run `dotnet run --project src/ECMAScript.Vue.Generator -- authoring`.\n" + string.Join("\n", changed));
    }

    private static string StripMaintainedContent(string source)
    {
        source = Regex.Replace(source, @"(?m)^[ \t]*/// <remarks data-authoring=""types"">[^\r\n]*\r?\n", "");
        return Regex.Replace(source, @"(?ms)^[ \t]*#region (?:" + ProjectionRegion + "|" + NumericRegion + @")\r?\n.*?^[ \t]*#endregion\r?\n", "");
    }

    private void ReadShapes(string source)
    {
        foreach (var declaration in CSharpSyntaxTree.ParseText(source, ParseOptions).GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            if (QualifiedKey(Context(declaration), declaration.Identifier.ValueText, 0) == "ECMAScript.Number")
                foreach (var conversion in declaration.Members.OfType<ConversionOperatorDeclarationSyntax>()
                    .Where(conversion => conversion.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword) &&
                        conversion.Type.ToString() == "Number" && conversion.ParameterList.Parameters.Count == 1))
                    _numberInputs.Add(conversion.ParameterList.Parameters[0].Type!.ToString());
            var native = declaration as UnionDeclarationSyntax;
            if (native?.ParameterList is null && !declaration.AttributeLists.SelectMany(list => list.Attributes)
                .Any(attribute => Name(attribute.Name.ToString()) is "Union" or "UnionAttribute")) continue;
            var branches = native?.ParameterList?.Parameters.Select(parameter => parameter.Type!.ToString()).ToArray()
                ?? declaration.Members.OfType<ConstructorDeclarationSyntax>().Where(constructor => constructor.ParameterList.Parameters.Count == 1)
                    .Select(constructor => constructor.ParameterList.Parameters[0].Type!.ToString()).Distinct(StringComparer.Ordinal).ToArray();
            var context = Context(declaration);
            var parameters = declaration.TypeParameterList?.Parameters.Select(parameter => parameter.Identifier.ValueText).ToArray() ?? [];
            var key = QualifiedKey(context, declaration.Identifier.ValueText, parameters.Length);
            var inputs = branches.Concat(declaration.Members.OfType<ConversionOperatorDeclarationSyntax>()
                .Where(conversion => conversion.ImplicitOrExplicitKeyword.IsKind(SyntaxKind.ImplicitKeyword) && conversion.ParameterList.Parameters.Count == 1)
                .Select(conversion => conversion.ParameterList.Parameters[0].Type!.ToString())).ToArray();
            var projections = declaration.Members.OfType<PropertyDeclarationSyntax>()
                .Where(property => property.Identifier.ValueText.StartsWith("As", StringComparison.Ordinal))
                .Select(property => new Projection(property.Identifier.ValueText, property.Type.ToString(), property.Identifier.ValueText)).ToArray();
            _unions[key] = new UnionShape(key, context, parameters, branches, inputs, projections, native is not null);
        }
    }

    internal string Annotate(string original)
    {
        var source = StripMaintainedContent(original);
        var newline = source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var root = CSharpSyntaxTree.ParseText(source, ParseOptions).GetRoot();
        var edits = new List<(int Position, string Text)>();
        foreach (var union in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            if (union.OpenBraceToken.IsMissing) continue;
            var shape = Resolve(union.Identifier.ValueText + (union.TypeParameterList?.ToString() ?? ""), Context(union));
            if (shape is null) continue;
            var projections = Projections(shape, new HashSet<string>(StringComparer.Ordinal))
                .Where(projection => !shape.Projections.Any(existing => existing.Name == projection.Name)).ToArray();
            var numericInputs = NumericInputs(shape).ToArray();
            if (projections.Length == 0 && numericInputs.Length == 0) continue;
            var indent = union.Members.Count > 0 ? Indent(source, union.Members[0].SpanStart) : Indent(source, union.SpanStart) + "    ";
            var text = new StringBuilder();
            if (projections.Length > 0)
            {
                text.Append(indent).Append("#region ").Append(ProjectionRegion).Append(newline);
                foreach (var projection in projections)
                {
                    text.Append(indent).Append("/// <summary>Reads the scalar branch directly; other branches return null. 直接读取标量分支，不匹配时返回 null。</summary>").Append(newline);
                    text.Append(indent).Append("public ").Append(projection.Type).Append(' ').Append(projection.Name)
                        .Append(" => ").Append(projection.Path).Append(';').Append(newline);
                }
                text.Append(indent).Append("#endregion").Append(newline);
            }
            if (numericInputs.Length > 0)
            {
                text.Append(indent).Append("#region ").Append(NumericRegion).Append(newline);
                var typeName = union.Identifier.ValueText + (union.TypeParameterList?.ToString() ?? "");
                foreach (var input in numericInputs)
                {
                    text.Append(indent).Append("/// <summary>Accepts a numeric value through the declared Number branch; avoids two chained C# user conversions. 经现有 Number 分支接受数值。</summary>").Append(newline);
                    text.Append(indent).Append("public static implicit operator ").Append(typeName).Append('(').Append(input)
                        .Append(" value) => (").Append(typeName).Append(")(ECMAScript.Number)value;").Append(newline);
                }
                text.Append(indent).Append("#endregion").Append(newline);
            }
            edits.Add((LineStart(source, union.CloseBraceToken.SpanStart), text.ToString()));
        }
        foreach (var property in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            if (!property.AttributeLists.SelectMany(list => list.Attributes)
                .Any(attribute => Name(attribute.Name.ToString()) is "Parameter" or "ParameterAttribute")) continue;
            var remarks = Describe(property.Identifier.ValueText, property.Type.ToString(), Context(property));
            if (remarks is null) continue;
            edits.Add((LineStart(source, property.SpanStart), Indent(source, property.SpanStart) +
                "/// <remarks data-authoring=\"types\">" + EscapeXml(remarks) + "</remarks>" + newline));
        }
        var builder = new StringBuilder(source);
        foreach (var edit in edits.OrderByDescending(edit => edit.Position)) builder.Insert(edit.Position, edit.Text);
        return builder.ToString();
    }

    private IEnumerable<string> NumericInputs(UnionShape shape)
    {
        // C# cannot chain int -> Number -> union user conversions. Derive the exact
        // strongly typed bridges from Number's existing implicit input declarations.
        // A primitive numeric branch already accepts normal literals, so keep it as-is.
        if (!shape.Inputs.Any(input => Name(input) == "Number") ||
            shape.Branches.Any(input => IsNumber(input) && Name(input) != "Number")) return [];
        return _numberInputs.Where(input => !shape.Inputs.Contains(input, StringComparer.Ordinal));
    }

    private IEnumerable<Projection> Projections(UnionShape shape, HashSet<string> active)
    {
        if (!active.Add(shape.Key)) return [];
        var result = shape.Projections.ToList();
        if (shape.Native)
        {
            var candidates = new List<Projection>();
            foreach (var outer in shape.Projections)
            {
                if (!shape.Branches.Contains(outer.Type.TrimEnd('?'), StringComparer.Ordinal)) continue;
                var nested = Resolve(outer.Type.TrimEnd('?'), shape.Context);
                if (nested is null || !outer.Type.EndsWith('?')) continue;
                foreach (var inner in Projections(nested, active))
                {
                    if (!IsScalar(inner.Type)) continue;
                    candidates.Add(inner with { Path = outer.Name + "?." + inner.Path });
                }
            }
            // Ambiguous names are not invented or widened. Authors keep their explicit
            // branch paths when two branches expose different projections with one name.
            result.AddRange(candidates.GroupBy(projection => projection.Name, StringComparer.Ordinal)
                .Where(group => group.Count() == 1 && !result.Any(existing => existing.Name == group.Key)).Select(group => group.Single()));
        }
        active.Remove(shape.Key);
        return result;
    }

    internal string? Describe(string member, string sourceType, string context = "")
    {
        var type = sourceType.TrimEnd('?');
        var syntax = SyntaxFactory.ParseTypeName(type);
        var generic = RightmostName(syntax) as GenericNameSyntax;
        var callback = generic is { Identifier.ValueText: "EventCallback", TypeArgumentList.Arguments.Count: 1 };
        if (callback) type = generic!.TypeArgumentList.Arguments[0].ToString().TrimEnd('?');
        var shape = Resolve(type, context);
        if (!IsNumber(type) && shape is null) return null;
        if (callback)
            return "C# 回调参数为 " + generic!.TypeArgumentList.Arguments[0] + "；保持与模型相同的强类型。" + ProjectionNotes(shape, generic.TypeArgumentList.Arguments[0].ToString().EndsWith('?'));
        if (shape is null)
            return "C# 类型为 " + sourceType + "；Razor 数值写 " + member + "=\"@(32)\" 或 " + member + "=\"@value\"，由 C#/Razor 检查转换。Number/double 遵循 JavaScript 双精度语义，大整数 ID 使用字符串。";
        var leaves = Leaves(shape, new HashSet<string>(StringComparer.Ordinal)).Distinct(StringComparer.Ordinal).ToArray();
        var text = "C# union " + sourceType + "；值域为 " + string.Join(" | ", leaves) + "。";
        if (shape.Inputs.Any(IsNumber))
            text += "数值用 " + member + "=\"@(32)\"；变量用 " + member + "=\"@value\"，无需 double 后缀。";
        else
            text += "变量用 " + member + "=\"@value\"，并保持声明的分支类型。";
        if (shape.Inputs.Any(input => Name(input) == "string"))
            text += "字符串用 " + member + "=\"text\"；数字字符串保持 string。";
        if (leaves.Any(leaf => leaf.EndsWith("[]", StringComparison.Ordinal)))
            text += "数组使用强类型数组/已支持的集合表达式；空数组与 null 分开，运行时不检查数组元素类型。";
        return text + ProjectionNotes(shape, sourceType.EndsWith('?')) +
            (sourceType.EndsWith('?') ? "清空值由宿主组件的公开参数决定。" : "");
    }

    private string ProjectionNotes(UnionShape? shape, bool nullable)
    {
        if (shape is null) return "";
        var names = Projections(shape, new HashSet<string>(StringComparer.Ordinal)).Select(projection => projection.Name).ToArray();
        return (names.Length == 0 ? "" : "投影：" + string.Join("、", names.Select(name => "value" + (nullable ? "?." : ".") + name)) + "；不匹配返回 null，0/false 保留。") +
            (shape.Native ? "" : "带标签的重叠分支不能从擦除后的 JS 值恢复精确标签；这类投影需要明确的宿主映射。");
    }

    private IEnumerable<string> Leaves(UnionShape shape, HashSet<string> active)
    {
        if (!active.Add(shape.Key)) yield break;
        foreach (var branch in shape.Branches)
        {
            // Arrays keep their declared element types; do not flatten them to scalar branches.
            var nested = branch.EndsWith("[]", StringComparison.Ordinal) ? null : Resolve(branch, shape.Context);
            if (nested is null) yield return branch;
            else foreach (var leaf in Leaves(nested, active)) yield return leaf;
        }
        active.Remove(shape.Key);
    }

    private UnionShape? Resolve(string type, string context)
    {
        if (type.TrimEnd('?').EndsWith("[]", StringComparison.Ordinal)) return null;
        var syntax = SyntaxFactory.ParseTypeName(type.TrimEnd('?').Replace("global::", "", StringComparison.Ordinal));
        var generic = RightmostName(syntax) as GenericNameSyntax;
        var arity = generic?.TypeArgumentList.Arguments.Count ?? 0;
        var name = Regex.Replace(syntax.ToString(), @"<.*>", "") + (arity == 0 ? "" : "`" + arity);
        UnionShape? shape = _unions.GetValueOrDefault(name);
        for (var scope = context; shape is null && scope.Length > 0; scope = scope.Contains('.') ? scope[..scope.LastIndexOf('.')] : "")
            shape = _unions.GetValueOrDefault(scope + "." + name);
        // Unqualified names are accepted only when unique across the complete catalog.
        // A new namespace with a same-named union must never silently change a hint.
        if (shape is null)
        {
            var candidates = _unions.Values.Where(candidate => candidate.Key.EndsWith("." + name, StringComparison.Ordinal) || candidate.Key == name).Take(2).ToArray();
            if (candidates.Length == 1) shape = candidates[0];
        }
        if (shape is null || generic is null || shape.Parameters.Length != arity) return shape;
        var substitutions = shape.Parameters.Zip(generic.TypeArgumentList.Arguments, (parameter, argument) => (parameter, argument.ToString()))
            .ToDictionary(item => item.parameter, item => item.Item2, StringComparer.Ordinal);
        string Substitute(string source) => Regex.Replace(source, @"\b[A-Za-z_]\w*\b", match => substitutions.GetValueOrDefault(match.Value) ?? match.Value);
        return shape with
        {
            Branches = shape.Branches.Select(Substitute).ToArray(), Inputs = shape.Inputs.Select(Substitute).ToArray(),
            Projections = shape.Projections.Select(projection => projection with { Type = Substitute(projection.Type) }).ToArray()
        };
    }

    private static bool IsNumber(string type) => Name(type) is "Number" or "double" or "float" or "int" or "uint" or "short" or "ushort" or "byte" or "sbyte" or "decimal";
    private static TypeSyntax RightmostName(TypeSyntax syntax) => syntax switch
    {
        QualifiedNameSyntax qualified => qualified.Right,
        AliasQualifiedNameSyntax alias => alias.Name,
        _ => syntax
    };
    private static bool IsScalar(string type) => IsNumber(type) || Name(type) is "bool" or "string" or "Date" or "BigInt" or "long" or "ulong";
    private static string Name(string type) => type.TrimEnd('?').Split('<')[0].Split('.').Last();
    private static string Context(SyntaxNode node) => string.Join(".", node.Ancestors().Reverse().Select(ancestor => ancestor switch
    {
        BaseNamespaceDeclarationSyntax ns => ns.Name.ToString(), TypeDeclarationSyntax declaration => declaration.Identifier.ValueText, _ => null
    }).Where(name => name is not null));
    private static string QualifiedKey(string context, string name, int arity) => (context.Length == 0 ? "" : context + ".") + name + (arity == 0 ? "" : "`" + arity);
    private static int LineStart(string source, int position) => source.LastIndexOf('\n', Math.Max(0, position - 1)) + 1;
    private static string Indent(string source, int position) => new(source[LineStart(source, position)..position].TakeWhile(character => character is ' ' or '\t').ToArray());
    private static string EscapeXml(string text) => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    private sealed record Projection(string Name, string Type, string Path);
    private sealed record UnionShape(string Key, string Context, string[] Parameters, string[] Branches, string[] Inputs, Projection[] Projections, bool Native);
}
