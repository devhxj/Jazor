using ECMAScript.Contract;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

internal static class SharedGeneration
{
    public static string EscapeForCSharpStringLiteral(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
    }

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

    public static IEnumerable<string> GetSourceFiles(string root)
    {
        return Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<string> GetWhiteListSourceRoots(string repoRoot)
    {
        var src = Path.Combine(repoRoot, "src");
        var roots = new[]
        {
            Path.Combine(src, "ECMAScript"),
            Path.Combine(src, "Jazor.CLR"),
            Path.Combine(src, "ECMAScript.Vue3"),
            Path.Combine(src, "ECMAScript.Vuetify"),
        };

        foreach (var root in roots)
        {
            if (Directory.Exists(root))
                yield return root;
        }
    }

    public static IEnumerable<string> GetClrSourceFiles(string repoRoot)
    {
        var root = Path.Combine(repoRoot, "src", "Jazor.CLR");
        if (!Directory.Exists(root))
            yield break;

        foreach (var file in GetSourceFiles(root))
            yield return file;
    }

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

    public static SyntaxTree CreateSyntaxTree(string path)
    {
        var text = SourceText.From(File.ReadAllText(path, Encoding.UTF8), Encoding.UTF8);
        return CSharpSyntaxTree.ParseText(text, new CSharpParseOptions(LanguageVersion.Preview), path);
    }

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

    public static string GetAttributeName(NameSyntax name)
        => name switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            QualifiedNameSyntax qualified => GetAttributeName(qualified.Right),
            AliasQualifiedNameSyntax alias => alias.Name.Identifier.ValueText,
            GenericNameSyntax generic => generic.Identifier.ValueText,
            _ => name.ToString()
        };

    public static (string Op, string? Member, string? Value) ReadJazorAttribute(AttributeSyntax attr, SemanticModel semanticModel)
    {
        var arguments = attr.ArgumentList?.Arguments;
        if (arguments is null || arguments.Value.Count == 0)
            return (nameof(Op.Compile), null, null);

        if (arguments.Value.Count == 1)
            return (nameof(Op.Inline), null, ReadString(arguments.Value[0].Expression, semanticModel));

        return (
            ReadOp(arguments.Value[0].Expression, semanticModel),
            ReadString(arguments.Value[1].Expression, semanticModel),
            arguments.Value.Count > 2 ? ReadString(arguments.Value[2].Expression, semanticModel) : null);
    }

    public static string? ReadModulePath(SyntaxList<AttributeListSyntax> attributes, SemanticModel semanticModel)
    {
        var attr = FindAttribute(attributes, "ECMAScriptModule");
        var argument = attr?.ArgumentList?.Arguments.FirstOrDefault();
        if (argument is null)
            return null;

        return ReadString(argument.Expression, semanticModel);
    }

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
