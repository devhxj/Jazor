using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acornima.Ast;

namespace Jazor.Compiler;

public static class ImportDeclarationFactory
{
    public static ImmutableArray<ImportDeclaration> Create(
        string modulePath,
        IEnumerable<ImportDeclarationSpecifier> specifiers)
    {
        if (modulePath is null)
            throw new ArgumentNullException(nameof(modulePath));
        if (specifiers is null)
            throw new ArgumentNullException(nameof(specifiers));

        var uniqueSpecifiers = NormalizeSpecifiers(specifiers);
        var defaultSpecifiers = uniqueSpecifiers
            .OfType<ImportDefaultSpecifier>()
            .OrderBy(static specifier => specifier.Local.Name, StringComparer.Ordinal)
            .ToArray();
        var namespaceSpecifiers = uniqueSpecifiers
            .OfType<ImportNamespaceSpecifier>()
            .OrderBy(static specifier => specifier.Local.Name, StringComparer.Ordinal)
            .ToArray();
        var namedSpecifiers = uniqueSpecifiers
            .OfType<ImportSpecifier>()
            .Cast<ImportDeclarationSpecifier>()
            .ToArray();

        var declarations = ImmutableArray.CreateBuilder<ImportDeclaration>();
        var defaultIndex = 0;
        foreach (var namespaceSpecifier in namespaceSpecifiers)
        {
            var declarationSpecifiers = defaultIndex >= defaultSpecifiers.Length
                ? NodeList.From<ImportDeclarationSpecifier>(namespaceSpecifier)
                : NodeList.From<ImportDeclarationSpecifier>(defaultSpecifiers[defaultIndex++], namespaceSpecifier);
            declarations.Add(CreateDeclaration(modulePath, declarationSpecifiers));
        }

        var remainingSpecifiers = new List<ImportDeclarationSpecifier>(namedSpecifiers.Length + 1);
        if (defaultIndex < defaultSpecifiers.Length)
            remainingSpecifiers.Add(defaultSpecifiers[defaultIndex++]);
        remainingSpecifiers.AddRange(namedSpecifiers);
        if (remainingSpecifiers.Count > 0)
            declarations.Add(CreateDeclaration(modulePath, NodeList.From(remainingSpecifiers)));

        while (defaultIndex < defaultSpecifiers.Length)
        {
            declarations.Add(CreateDeclaration(
                modulePath,
                NodeList.From<ImportDeclarationSpecifier>(defaultSpecifiers[defaultIndex++])));
        }

        return declarations.ToImmutable();
    }

    public static ImmutableArray<ImportDeclarationSpecifier> NormalizeSpecifiers(
        IEnumerable<ImportDeclarationSpecifier> specifiers)
    {
        if (specifiers is null)
            throw new ArgumentNullException(nameof(specifiers));

        return specifiers
            .GroupBy(CreateSpecifierKey)
            .Select(static group => group.First())
            .OrderBy(static specifier => CreateSpecifierKey(specifier).Kind)
            .ThenBy(static specifier => CreateSpecifierKey(specifier).ImportedName, StringComparer.Ordinal)
            .ThenBy(static specifier => CreateSpecifierKey(specifier).LocalName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    public static ImportDeclaration WithModulePath(
        ImportDeclaration declaration,
        string modulePath)
    {
        if (declaration is null)
            throw new ArgumentNullException(nameof(declaration));
        if (modulePath is null)
            throw new ArgumentNullException(nameof(modulePath));

        return new ImportDeclaration(
            declaration.Specifiers,
            CreateModulePathLiteral(modulePath),
            declaration.Attributes,
            declaration.Phase);
    }

    private static ImportDeclaration CreateDeclaration(
        string modulePath,
        NodeList<ImportDeclarationSpecifier> specifiers)
        => new(
            specifiers,
            CreateModulePathLiteral(modulePath),
            NodeList.From<ImportAttribute>());

    private static StringLiteral CreateModulePathLiteral(string modulePath)
        => JavaScriptAstFactory.CreateStringLiteral(modulePath);

    private static ImportSpecifierKey CreateSpecifierKey(ImportDeclarationSpecifier specifier)
        => specifier switch
        {
            ImportDefaultSpecifier value => new(ImportSpecifierKind.Default, string.Empty, value.Local.Name),
            ImportNamespaceSpecifier value => new(ImportSpecifierKind.Namespace, string.Empty, value.Local.Name),
            ImportSpecifier value => new(ImportSpecifierKind.Named, GetImportedName(value.Imported), value.Local.Name),
            _ => throw new NotSupportedException("Unsupported ECMAScript import specifier: " + specifier.Type)
        };

    private static string GetImportedName(Expression imported)
        => imported switch
        {
            Identifier identifier => identifier.Name,
            StringLiteral literal => literal.Value,
            _ => throw new NotSupportedException("Unsupported ECMAScript named import key: " + imported.Type)
        };

    private enum ImportSpecifierKind
    {
        Default,
        Namespace,
        Named
    }

    private readonly record struct ImportSpecifierKey(
        ImportSpecifierKind Kind,
        string ImportedName,
        string LocalName);
}
