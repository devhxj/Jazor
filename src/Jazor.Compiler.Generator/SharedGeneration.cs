using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

/// <summary>
/// 为白名单和 CLR runtime 生成流程提供共享的源码扫描、Roslyn 读取和字面量转义工具。
/// </summary>
/// <remarks>
/// 这里集中维护扫描根目录、属性读取和 symbol 显示格式，保证不同生成阶段使用同一套规则。
/// 该类只读取源码并返回结构化信息，不负责修改生成文件或执行编译器 lowering。
/// </remarks>
internal static class SharedGeneration
{
    /// <summary>把文本转义为可安全嵌入生成 C# 字符串字面量的内容。</summary>
    public static string EscapeForCSharpStringLiteral(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }

    /// <summary>从当前进程位置向上查找仓库根目录。</summary>
    public static string FindRepositoryRoot()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            var current = new DirectoryInfo(start);
            while (current is not null)
            {
                var srcDirectory = Path.Combine(current.FullName, "src");
                if (Directory.Exists(Path.Combine(srcDirectory, "Jazor.Compiler"))
                    && Directory.Exists(Path.Combine(srcDirectory, "Jazor.CLR"))
                    && Directory.Exists(Path.Combine(srcDirectory, "ECMAScript")))
                    return current.FullName;

                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    /// <summary>枚举目录下的源码文件，并排除构建输出目录。</summary>
    public static IEnumerable<string> GetSourceFiles(string root)
    {
        return Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>返回需要生成白名单的源码根目录。</summary>
    public static IEnumerable<string> GetWhiteListSourceRoots(string repoRoot)
    {
        var src = Path.Combine(repoRoot, "src");
        var roots = new[]
        {
            Path.Combine(src, "ECMAScript"),
            Path.Combine(src, "Jazor.CLR"),
            Path.Combine(src, "ECMAScript.Vue"),
            Path.Combine(src, "ECMAScript.Vuetify"),
        };

        foreach (var root in roots)
        {
            if (Directory.Exists(root))
                yield return root;
        }
    }

    /// <summary>返回 Jazor.CLR 的全部源码文件。</summary>
    public static IEnumerable<string> GetClrSourceFiles(string repoRoot)
    {
        var root = Path.Combine(repoRoot, "src", "Jazor.CLR");
        if (!Directory.Exists(root))
            yield break;

        foreach (var file in GetSourceFiles(root))
            yield return file;
    }

    /// <summary>返回 CLR runtime 编译所需的 ECMAScript 和 Jazor.CLR 源码。</summary>
    public static IEnumerable<string> GetClrCompilationSourceFiles(string repoRoot)
    {
        var roots = new[]
        {
            Path.Combine(repoRoot, "src", "ECMAScript"),
            Path.Combine(repoRoot, "src", "Jazor.CLR"),
        };

        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
                continue;

            foreach (var file in GetSourceFiles(root))
                yield return file;
        }
    }

    /// <summary>以预览版 C# 语法配置创建带路径的 Roslyn syntax tree。</summary>
    public static SyntaxTree CreateSyntaxTree(string path)
    {
        var text = SourceText.From(File.ReadAllText(path, Encoding.UTF8), Encoding.UTF8);
        return CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview), path);
    }

    /// <summary>按短名称查找属性，兼容带或不带 Attribute 后缀的写法。</summary>
    public static AttributeSyntax? FindAttribute(SyntaxList<AttributeListSyntax> attributes, string name)
    {
        foreach (var attr in attributes.SelectMany(x => x.Attributes))
        {
            var attrName = GetAttributeName(attr.Name);
            if (attrName == name || attrName == $"{name}Attribute")
                return attr;
        }

        return null;
    }

    /// <summary>提取属性名称的最右侧标识符。</summary>
    public static string GetAttributeName(NameSyntax name)
        => name switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            QualifiedNameSyntax qualified => GetAttributeName(qualified.Right),
            AliasQualifiedNameSyntax alias => alias.Name.Identifier.ValueText,
            GenericNameSyntax generic => generic.Identifier.ValueText,
            _ => name.ToString()
        };

    /// <summary>读取 Jazor 属性并按 contract 规则返回 Op、成员 key 和附加值。</summary>
    public static (string Op, string? Member, string? Value, string? ModulePath) ReadJazorAttribute(AttributeSyntax attr, SemanticModel semanticModel)
    {
        var arguments = attr.ArgumentList?.Arguments;
        if (arguments is null || arguments.Value.Count == 0)
            return (nameof(Op.Compile), null, null, null);

        if (arguments.Value.Count == 1)
            return (nameof(Op.Inline), null, ReadString(arguments.Value[0].Expression, semanticModel), null);

        return (
            ReadOp(arguments.Value[0].Expression, semanticModel),
            ReadString(arguments.Value[1].Expression, semanticModel),
            arguments.Value.Count > 2 ? ReadString(arguments.Value[2].Expression, semanticModel) : null,
            arguments.Value.Count > 3 ? ReadString(arguments.Value[3].Expression, semanticModel) : null);
    }

    /// <summary>读取当前声明上的 ECMAScript module 路径。</summary>
    public static string? ReadModulePath(SyntaxList<AttributeListSyntax> attributes, SemanticModel semanticModel)
    {
        var attr = FindAttribute(attributes, "ECMAScriptModule");
        var argument = attr?.ArgumentList?.Arguments.FirstOrDefault();
        if (argument is null)
            return null;

        return ReadString(argument.Expression, semanticModel);
    }

    /// <summary>将支持的成员声明语法解析为 Roslyn declared symbol。</summary>
    public static ISymbol? GetDeclaredSymbol(MemberDeclarationSyntax declaration, SemanticModel semanticModel)
        => declaration switch
        {
            BaseMethodDeclarationSyntax method => semanticModel.GetDeclaredSymbol(method),
            BasePropertyDeclarationSyntax property => semanticModel.GetDeclaredSymbol(property),
            FieldDeclarationSyntax field when field.Declaration.Variables.Count == 1
                => semanticModel.GetDeclaredSymbol(field.Declaration.Variables[0]),
            DelegateDeclarationSyntax @delegate => semanticModel.GetDeclaredSymbol(@delegate),
            TypeDeclarationSyntax type => semanticModel.GetDeclaredSymbol(type),
            EnumDeclarationSyntax @enum => semanticModel.GetDeclaredSymbol(@enum),
            _ => null
        };

    private static string ReadOp(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var constant = semanticModel.GetConstantValue(expression);
        if (constant.HasValue && constant.Value is int opValue)
            return ((Op)opValue).ToString();

        var text = expression.ToString();
        var index = text.LastIndexOf('.');
        return index >= 0 ? text[(index + 1)..] : text;
    }

    private static string? ReadString(ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var constant = semanticModel.GetConstantValue(expression);
        if (constant.HasValue)
            return constant.Value?.ToString();

        return expression switch
        {
            LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) => literal.Token.ValueText,
            _ => null
        };
    }
}
