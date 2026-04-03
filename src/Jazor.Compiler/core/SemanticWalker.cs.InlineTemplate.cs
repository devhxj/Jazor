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
    // 外部模板使用 __arg1 / __arg2 ...，
    // 内部会统一规范化成不会与真实参数名混淆的保留前缀。
    private const string InlinePlaceholderPrefix = "__jz_arg";
    private static readonly Regex LegacyInlinePlaceholderRegex = new(@"@#\{(\d+)\}", RegexOptions.Compiled);
    private static readonly Regex InlinePlaceholderRegex = new(@"__arg([1-9]\d*)", RegexOptions.Compiled);

    // Inline 模板按白名单成员签名缓存。
    // 当前约定下，同一签名对应同一模板；因此可以安全地“一次 parse，多次实例化”。
    private static readonly ConcurrentDictionary<string, InlineTemplate> InlineTemplateCache = new();

    private sealed record InlineTemplate(Expression Ast, int PlaceholderCount);

    /// <summary>
    /// 实例化 Inline AST 模板。
    /// <para/>
    /// 这里的关键约束是：
    /// 1. 模板本身只 parse 一次并缓存；
    /// 2. 调用阶段只做 AST 级占位符替换；
    /// 3. 参数始终以 Expression AST 参与替换，不会先序列化成字符串。
    /// <para/>
    /// 这让 Inline 保持“声明式模板”的写法，同时避免旧方案里
    /// “参数转字符串再整体 parse”带来的结构不稳定问题。
    /// </summary>
    private static Expression InstantiateInlineTemplate(string signature, string template, IReadOnlyList<Expression> arguments)
    {
        var parsedTemplate = InlineTemplateCache.GetOrAdd(signature, _ => ParseInlineTemplate(signature, template));
        if (parsedTemplate.PlaceholderCount > arguments.Count)
            throw new InvalidOperationException($"Inline template '{signature}' expects at least {parsedTemplate.PlaceholderCount} arguments, but received {arguments.Count}.");

        return (Expression) (new InlinePlaceholderRewriter(signature, arguments).Visit(parsedTemplate.Ast)
            ?? throw new InvalidOperationException($"Inline template '{signature}' produced a null AST."));
    }

    /// <summary>
    /// 预解析 Inline 模板。
    /// <para/>
    /// 模板外部语法使用 __arg1 / __arg2 ...，
    /// 解析前会先规范化成内部保留标识符 __jz_arg0 / __jz_arg1 ...，
    /// 这样后续只需要在 AST 中查找普通 Identifier，就能稳定替换占位符。
    /// <para/>
    /// 这里仍然保留 Parser，但只用于“模板预解析一次”。
    /// 每次调用不会重复 parse。
    /// </summary>
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

    /// <summary>
    /// Inline 占位符重写器。
    /// <para/>
    /// 当前只替换完整 Identifier 形式的内部占位符。
    /// 这意味着 Inline 适合表达“结构稳定的表达式模板”，
    /// 不适合承担需要动态拼接标识符片段、引入语句、控制求值顺序的复杂语义。
    /// 这类场景应升级到 Op.Compile。
    /// </summary>
    private sealed class InlinePlaceholderRewriter(string signature, IReadOnlyList<Expression> arguments) : AstRewriter
    {
        protected override object VisitIdentifier(Identifier node)
        {
            if (!TryGetPlaceholderIndex(node.Name, out var index))
                return node;

            if ((uint) index >= (uint) arguments.Count)
                throw new InvalidOperationException($"Inline template '{signature}' references argument index {index}, but only {arguments.Count} argument(s) were supplied.");

            // 直接把占位符节点替换成真实参数 AST。
            // 这里不做字符串展开，也不重新 parse。
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
