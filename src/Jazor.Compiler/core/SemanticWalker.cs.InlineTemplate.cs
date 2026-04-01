using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

public sealed partial class SemanticWalker
{
    private const string InlinePlaceholderPrefix = "__jz_arg";
    private static readonly Regex LegacyInlinePlaceholderRegex = new(@"@#\{(\d+)\}", RegexOptions.Compiled);
    private static readonly Regex InlinePlaceholderRegex = new(@"__arg([1-9]\d*)", RegexOptions.Compiled);

    private static readonly ConcurrentDictionary<string, InlineTemplate> InlineTemplateCache = new();

    private sealed record InlineTemplate(Expression Ast, int PlaceholderCount);

    private static Expression InstantiateInlineTemplate(string signature, string template, IReadOnlyList<Expression> arguments)
    {
        var parsedTemplate = InlineTemplateCache.GetOrAdd(signature, _ => ParseInlineTemplate(signature, template));
        if (parsedTemplate.PlaceholderCount > arguments.Count)
            throw new InvalidOperationException($"Inline template '{signature}' expects at least {parsedTemplate.PlaceholderCount} arguments, but received {arguments.Count}.");

        return (Expression) (new InlinePlaceholderRewriter(signature, arguments).Visit(parsedTemplate.Ast)
            ?? throw new InvalidOperationException($"Inline template '{signature}' produced a null AST."));
    }

    private static InlineTemplate ParseInlineTemplate(string signature, string template)
    {
        if (template.Contains(InlinePlaceholderPrefix, StringComparison.Ordinal))
            throw new InvalidOperationException($"Inline template '{signature}' contains the reserved placeholder prefix '{InlinePlaceholderPrefix}'.");

        if (LegacyInlinePlaceholderRegex.IsMatch(template))
            throw new InvalidOperationException($"Inline template '{signature}' uses the legacy placeholder syntax. Use __arg1, __arg2, ... instead.");

        var maxPlaceholder = -1;
        var normalized = InlinePlaceholderRegex.Replace(template, match =>
        {
            var index = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) - 1;
            maxPlaceholder = Math.Max(maxPlaceholder, index);
            return $"{InlinePlaceholderPrefix}{index}";
        });

        var parser = new Parser();
        return new InlineTemplate(parser.ParseExpression(normalized, null, true), maxPlaceholder + 1);
    }

    private sealed class InlinePlaceholderRewriter(string signature, IReadOnlyList<Expression> arguments) : AstRewriter
    {
        protected override object VisitIdentifier(Identifier node)
        {
            if (!TryGetPlaceholderIndex(node.Name, out var index))
                return node;

            if ((uint) index >= (uint) arguments.Count)
                throw new InvalidOperationException($"Inline template '{signature}' references argument index {index}, but only {arguments.Count} argument(s) were supplied.");

            return arguments[index];
        }

        private static bool TryGetPlaceholderIndex(string? name, out int index)
        {
            index = -1;
            if (name is null || name.Length == 0 || !name.StartsWith(InlinePlaceholderPrefix, StringComparison.Ordinal))
                return false;

            return int.TryParse(name.Substring(InlinePlaceholderPrefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out index);
        }
    }
}
