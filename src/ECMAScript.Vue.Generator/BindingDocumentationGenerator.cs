using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using File = System.IO.File;

/// <summary>
/// Adds reviewed authoring documentation to handwritten bindings. Generated component
/// files remain owned by their existing generators; unknown semantics are reported,
/// never filled with a sentence that merely repeats the member name.
/// </summary>
internal static class BindingDocumentationGenerator
{
    internal static readonly string[] Libraries =
    [
        "ECMAScript.Contract",
        "ECMAScript",
        "ECMAScript.Vue", "ECMAScript.VueContract", "ECMAScript.Pinia", "ECMAScript.Pinia.Testing",
        "ECMAScript.VueRoute", "ECMAScript.Vue.Devtools", "ECMAScript.VueDataUi",
        "ECMAScript.ElementPlus", "ECMAScript.Vuetify", "ECMAScript.TDesign", "ECMAScript.VuIcons", "ECMAScript.Style"
    ];

    public static void Run(string[] args)
    {
        var check = args.Contains("--check", StringComparer.Ordinal);
        var repo = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(repo, "Jazor.slnx")))
            repo = Directory.GetParent(repo)?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        var inputPath = Path.Combine(repo, "src", "ECMAScript.Vue.Generator", "documentation", "authoring.json");
        var descriptions = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(inputPath))!;
        var upstream = new BindingUpstreamDocumentation(repo);
        var missing = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var changed = new List<string>();
        foreach (var library in Libraries)
        {
            foreach (var path in Directory.GetFiles(Path.Combine(repo, "src", library), "*.cs", SearchOption.AllDirectories)
                .Where(static path => !path.Contains("\\bin\\", StringComparison.Ordinal) &&
                    !path.Contains("\\obj\\", StringComparison.Ordinal) &&
                    !path.Contains("\\webidl\\generate\\", StringComparison.Ordinal) &&
                    !path.EndsWith(".g.cs", StringComparison.Ordinal) &&
                    !path.EndsWith(".generated.cs", StringComparison.Ordinal)))
            {
                var original = File.ReadAllText(path);
                var source = NormalizeDocumentationPlacement(original);
                var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
                var edits = new List<(int Position, string Text)>();
                foreach (var member in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
                {
                    if (!IsVisible(member) || HasDocumentation(member))
                        continue;
                    if (member is BaseTypeDeclarationSyntax type && type.Modifiers.Any(SyntaxKind.PartialKeyword)
                        && upstream.HasTypeDocumentation(library, type.Identifier.ValueText))
                        continue;
                    var name = MemberName(member);
                    var key = library + "|" + name;
                    var summary = upstream.Find(library, member) ?? descriptions.GetValueOrDefault(key) ?? DescribeStructure(member);
                    if (summary is null)
                    {
                        missing.TryAdd(key, member.ToString().Split('\n')[0].Trim());
                        continue;
                    }
                    var lineStart = source.LastIndexOf('\n', Math.Max(0, member.SpanStart - 1)) + 1;
                    var indent = new string(source[lineStart..member.SpanStart].TakeWhile(char.IsWhiteSpace).ToArray());
                    var documentation = RenderDocumentation(member, summary, indent);
                    edits.Add((member.SpanStart, documentation));
                }
                if (edits.Count == 0 && source == original)
                    continue;
                changed.Add(Path.GetRelativePath(repo, path));
                if (check) continue;
                var builder = new StringBuilder(source);
                foreach (var edit in edits.OrderByDescending(static edit => edit.Position))
                    builder.Insert(edit.Position, edit.Text);
                File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
            }
        }
        var reportPath = Path.Combine(repo, ".tmp", "binding-doc-audit", "pending-authoring.json");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
        File.WriteAllText(reportPath, JsonSerializer.Serialize(missing, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Binding documentation: {changed.Count} files {(check ? "need updates" : "updated")}; {missing.Count} descriptions await review. {reportPath}");
        if (check && (changed.Count > 0 || missing.Count > 0))
            throw new InvalidOperationException("Binding authoring documentation is incomplete.");
    }

    internal static IReadOnlyDictionary<string, string> ReadReviewedDescriptions()
    {
        var repo = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(repo, "Jazor.slnx")))
            repo = Directory.GetParent(repo)?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(
            Path.Combine(repo, "src", "ECMAScript.Vue.Generator", "documentation", "authoring.json")))!;
    }

    internal static bool HasDocumentation(MemberDeclarationSyntax node)
        => node.GetLeadingTrivia().Select(trivia => trivia.GetStructure()).OfType<DocumentationCommentTriviaSyntax>()
            .Any(doc => doc.DescendantNodes().OfType<XmlElementSyntax>().Any(element => element.StartTag.Name.LocalName.ValueText == "summary")
                || doc.DescendantNodes().OfType<XmlEmptyElementSyntax>().Any(element => element.Name.LocalName.ValueText == "inheritdoc"));

    internal static string NormalizeDocumentationPlacement(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));
        var edits = new List<(int Position, int Length, string Text)>();
        foreach (var member in tree.GetRoot().DescendantNodes().OfType<MemberDeclarationSyntax>())
        {
            var misplaced = member.DescendantTrivia(descendIntoTrivia: false).FirstOrDefault(trivia =>
                trivia.SpanStart >= member.SpanStart && trivia.GetStructure() is DocumentationCommentTriviaSyntax &&
                trivia.Token.Parent?.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().FirstOrDefault() == member);
            if (misplaced == default) continue;
            // XML docs after an attribute are not attached to the public declaration.
            // Move the exact authored text before the attribute list; retain its wording.
            var docStart = source.LastIndexOf('\n', misplaced.FullSpan.Start - 1) + 1;
            var target = source.LastIndexOf('\n', Math.Max(0, member.SpanStart - 1)) + 1;
            if (docStart <= target) continue;
            var text = source[docStart..misplaced.FullSpan.End];
            edits.Add((docStart, misplaced.FullSpan.End - docStart, string.Empty));
            edits.Add((target, 0, text));
        }
        var builder = new StringBuilder(source);
        foreach (var edit in edits.OrderByDescending(static edit => edit.Position))
            builder.Remove(edit.Position, edit.Length).Insert(edit.Position, edit.Text);
        return builder.ToString();
    }

    private static bool IsVisible(MemberDeclarationSyntax member)
    {
        // The extension declaration is a host syntax container, not a public API symbol.
        // Its methods/properties are visited separately and must retain their own docs.
        if (member is ExtensionBlockDeclarationSyntax) return false;
        if (member is NamespaceDeclarationSyntax or FileScopedNamespaceDeclarationSyntax or GlobalStatementSyntax) return false;
        if (member.Ancestors().OfType<BaseTypeDeclarationSyntax>().Any(parent =>
            parent is not ExtensionBlockDeclarationSyntax && !IsVisible(parent))) return false;
        if (member is EnumMemberDeclarationSyntax) return true;
        if (member.Modifiers.Any(SyntaxKind.PrivateKeyword)) return false;
        return member.Modifiers.Any(SyntaxKind.PublicKeyword) || member.Modifiers.Any(SyntaxKind.ProtectedKeyword)
            || member.Parent is InterfaceDeclarationSyntax;
    }

    private static string MemberName(MemberDeclarationSyntax member)
    {
        var parents = member.Ancestors().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(static type => type.Identifier.ValueText);
        var name = member switch
        {
            BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
            EnumMemberDeclarationSyntax value => value.Identifier.ValueText,
            PropertyDeclarationSyntax property => property.Identifier.ValueText,
            DelegateDeclarationSyntax callback => callback.Identifier.ValueText,
            MethodDeclarationSyntax method => method.Identifier.ValueText,
            ConstructorDeclarationSyntax constructor => constructor.Identifier.ValueText,
            _ => member.Kind().ToString()
        };
        return string.Join(".", parents.Append(name));
    }

    private static string? DescribeStructure(MemberDeclarationSyntax member)
    {
        var parent = member.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault();
        if (member is ConstructorDeclarationSyntax constructor && constructor.ParameterList.Parameters.Count == 0 &&
            constructor.Modifiers.Any(SyntaxKind.ProtectedKeyword) && constructor.Body?.Statements.Count == 0)
            return "供派生类型声明宿主对象的强类型投影；实际对象由 JavaScript 运行时 API 创建或返回。";
        if (member is ConversionOperatorDeclarationSyntax conversion)
        {
            var parameter = conversion.ParameterList.Parameters.Single();
            var collection = conversion.ToString().Contains("Array.ConvertAll", StringComparison.Ordinal);
            return collection
                ? $"将 {parameter.Type} 的每一项转换为对应联合分支，保持原有顺序并创建新的数组。"
                : $"将 {parameter.Type} 值转换为 {conversion.Type}，保留输入值供 JavaScript API 使用。";
        }
        if (member is OperatorDeclarationSyntax operation)
        {
            var token = operation.OperatorToken.Text;
            return token is "==" or "!="
                ? $"使用 JavaScript {token} 比较宿主值；Date 对象按对象身份比较，不按日期时间数值比较。"
                : $"使用 JavaScript {token} 进行关系比较；Date 对象转换为时间戳，Number 与 BigInt 按 JavaScript 数值比较规则处理。";
        }
        if (member is PropertyDeclarationSyntax property && property.Identifier.ValueText.StartsWith("As", StringComparison.Ordinal) &&
            property.ExpressionBody is not null)
        {
            var nullable = property.Type.ToString().EndsWith('?');
            if (nullable)
                return $"读取当前值的 {property.Type.ToString().TrimEnd('?')} 分支；不属于该分支时返回 null。";
        }
        if (member is MethodDeclarationSyntax method && method.Identifier.ValueText == "Create" &&
            parent?.Identifier.ValueText.EndsWith("CollectionBuilder", StringComparison.Ordinal) == true)
            return "为 C# 集合表达式创建有序集合；复制传入的元素，不保留临时 Span。";
        if (member is BaseTypeDeclarationSyntax type && type.Identifier.ValueText.EndsWith("CollectionBuilder", StringComparison.Ordinal))
            return "供 C# 集合表达式调用的构建器；应用可直接使用 [item1, item2] 语法构造对应集合。";
        if (member is BaseTypeDeclarationSyntax unionType && unionType.Kind().ToString() == "UnionDeclaration")
        {
            var signature = Regex.Match(unionType.ToString(), @"\bunion\s+\w+\s*\(([^)]+)\)");
            if (signature.Success)
                return "C# 联合参数，允许 " + Regex.Replace(signature.Groups[1].Value.Trim(), @"\s+", " ") +
                    "。传给 JavaScript 时使用所选分支的原始值；As* 属性用于读取相应分支。";
        }
        if (member is MethodDeclarationSyntax { Identifier.ValueText: "Null" } nullFactory && nullFactory.ParameterList.Parameters.Count == 0)
            return "构造显式的 JavaScript null 分支，用于向宿主 API 传入空值。";
        if (member is PropertyDeclarationSyntax { Identifier.ValueText: "Value", ExpressionBody: { } body } &&
            parent?.BaseList?.ToString().Contains("IUnion", StringComparison.Ordinal) == true)
            return "读取当前联合分支保存的原始值，供宿主 API 传递；不会转换到其他分支。";
        return null;
    }

    private static string RenderDocumentation(MemberDeclarationSyntax member, string summary, string indent)
    {
        var lines = new List<string> { "/// <summary>" };
        lines.AddRange(summary.Split('\n').Select(line => "/// " + EscapeXml(line)));
        lines.Add("/// </summary>");
        var parameters = member switch
        {
            BaseMethodDeclarationSyntax method => method.ParameterList.Parameters,
            DelegateDeclarationSyntax callback => callback.ParameterList.Parameters,
            _ => default
        };
        foreach (var parameter in parameters)
        {
            var description = parameter.Identifier.ValueText switch
            {
                "value" => "要传入的值，保持其声明的类型和数据。",
                "values" or "items" => "按期望顺序排列的元素。",
                _ when member is ConversionOperatorDeclarationSyntax => $"要转换的 {parameter.Type} 值。",
                _ => "传给该 API 的参数值。"
            };
            if (description is not null)
                lines.Add($"/// <param name=\"{EscapeXml(parameter.Identifier.ValueText)}\">{description}</param>");
        }
        if (member is ConversionOperatorDeclarationSyntax)
            lines.Add("/// <returns>转换后的强类型值。</returns>");
        return string.Join(Environment.NewLine + indent, lines) + Environment.NewLine + indent;
    }

    private static string EscapeXml(string text)
        => text.Replace("&", "&amp;", StringComparison.Ordinal).Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal).Replace("\"", "&quot;", StringComparison.Ordinal);
}