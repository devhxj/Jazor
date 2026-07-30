using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// 负责把语义遍历阶段收集的导入项整理成稳定的 ES module 导入声明。
/// </summary>
/// <remarks>
/// 导入项可能在不同的 lowering 分支中重复收集，因此这里统一去重、排序并拆分
/// namespace/default/named 三种声明。排序是输出确定性的一部分，不能依赖遍历顺序。
/// </remarks>
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

        // 先统一去重和排序，再按 ECMAScript 语法允许的组合拆分声明。
        // 不能直接保留收集顺序，否则不同 lowering 路径会产生不稳定的模块头。
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
