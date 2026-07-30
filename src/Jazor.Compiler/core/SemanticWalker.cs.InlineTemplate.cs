using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// 解析并实例化白名单中的 JavaScript inline 模板。
/// </summary>
/// <remarks>
/// inline 只适合短小、稳定的表达式模板；模板中的 <c>__argN</c> 不是用户变量，而是
/// 编译器约定的参数占位符。需要建立求值边界时会绑定到保留前缀参数，避免重复或延迟求值。
/// 需要分支、临时变量或共享 helper 的行为应升级为 Import/Compile，而不是继续拉长模板。
/// </remarks>
public sealed partial class SemanticWalker
{
    private const string InlinePlaceholderPrefix = "__arg";
    private const string InternalInlineBindingPrefix = "__jz_arg";
    private static readonly Regex LegacyInlinePlaceholderRegex = new(@"@#\{(\d+)\}");

    // 独立 compilation 可以声明相同成员签名但使用不同模板，缓存身份必须包含原始模板。
    private static readonly ConcurrentDictionary<(string Signature, string Template), InlineTemplate> InlineTemplateCache = new();

    private sealed record InlineTemplate(
        Expression Ast,
        int PlaceholderCount,
        InlineTemplateEvaluation Evaluation);

    private sealed record InlineTemplateEvaluation(
        IReadOnlyList<int> UsageCounts,
        IReadOnlyList<int> EvaluationOrder,
        IReadOnlyList<bool> ConditionalUsage);

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
    private static Expression InstantiateInlineTemplate(
        string signature,
        string template,
        IReadOnlyList<Expression> arguments,
        string? importedIdentifierName = null,
        Identifier? importedBinding = null)
    {
        var parsedTemplate = InlineTemplateCache.GetOrAdd(
            (signature, template),
            static key => ParseInlineTemplate(key.Signature, key.Template));
        if (parsedTemplate.PlaceholderCount > arguments.Count)
            throw new InvalidOperationException($"Inline template '{signature}' expects at least {parsedTemplate.PlaceholderCount} arguments, but received {arguments.Count}.");

        Expression Rewrite(IReadOnlyList<Expression> replacements)
            => (Expression) (new InlinePlaceholderRewriter(replacements, importedIdentifierName, importedBinding).Visit(parsedTemplate.Ast)
                ?? throw new InvalidOperationException($"Inline template '{signature}' produced a null AST."));

        if (!RequiresInlineEvaluationBoundary(parsedTemplate, arguments))
            return Rewrite(arguments);

        // C# evaluates the receiver and every argument once, left to right, before entering
        // the method. An inline template may reorder, repeat, conditionally consume, or omit
        // placeholders; the arrow call preserves the original protocol without serializing AST.
        var inputs = new (string ParameterName, Expression Value)[arguments.Count];
        for (var index = 0; index < arguments.Count; index++)
            inputs[index] = ($"{InternalInlineBindingPrefix}{index}", arguments[index]);

        return JavaScriptAstFactory.CreateSingleEvaluationArrowInvocation(
            inputs,
            parameters => Rewrite(parameters));
    }

    private static bool RequiresInlineEvaluationBoundary(
        InlineTemplate template,
        IReadOnlyList<Expression> arguments)
    {
        for (var index = 0; index < arguments.Count; index++)
        {
            if (!NeedsSingleEvaluationCaching(arguments[index]))
                continue;

            var usageCount = index < template.Evaluation.UsageCounts.Count
                ? template.Evaluation.UsageCounts[index]
                : 0;
            var conditionallyUsed = index < template.Evaluation.ConditionalUsage.Count &&
                template.Evaluation.ConditionalUsage[index];
            if (usageCount != 1 || conditionallyUsed)
                return true;
        }

        var previousEffectfulIndex = -1;
        foreach (var index in template.Evaluation.EvaluationOrder)
        {
            if ((uint) index >= (uint) arguments.Count ||
                !NeedsSingleEvaluationCaching(arguments[index]))
            {
                continue;
            }

            if (index < previousEffectfulIndex)
                return true;

            previousEffectfulIndex = index;
        }

        return false;
    }

    /// <summary>
    /// 预解析 Inline 模板。
    /// <para/>
    /// 模板外部语法使用 __arg1 / __arg2 ...。模板先原样解析，随后仅把 AST 中作为
    /// 表达式出现的完整 Identifier 识别为占位符；字符串内容和标识符子串不会被改写。
    /// <para/>
    /// 这里仍然保留 Parser，但只用于“模板预解析一次”。
    /// 每次调用不会重复 parse。
    /// </summary>
    private static InlineTemplate ParseInlineTemplate(string signature, string template)
    {
        try
        {
            var parser = new Parser();
            var ast = parser.ParseExpression(template, null, true);
            var analysis = InlineTemplateEvaluationCollector.Collect(ast, signature);
            return new InlineTemplate(
                ast,
                analysis.PlaceholderCount,
                analysis.Evaluation);
        }
        catch (ParseErrorException exception)
        {
            if (LegacyInlinePlaceholderRegex.IsMatch(template))
            {
                throw new InvalidOperationException(
                    $"Inline template '{signature}' uses the legacy placeholder syntax. Use __arg1, __arg2, ... instead.",
                    exception);
            }

            throw new InvalidOperationException(
                $"Inline template '{signature}' is not a valid JavaScript expression: {exception.Message}",
                exception);
        }
    }

    /// <summary>
    /// Inline 占位符重写器。
    /// <para/>
    /// 当前只替换完整 Identifier 形式的内部占位符。
    /// 这意味着 Inline 适合表达“结构稳定的表达式模板”，
    /// 不适合承担需要动态拼接标识符片段、引入语句、控制求值顺序的复杂语义。
    /// 这类场景应升级到 Op.Compile。
    /// </summary>
    private sealed class InlinePlaceholderRewriter(
        IReadOnlyList<Expression> arguments,
        string? importedIdentifierName,
        Identifier? importedBinding) : AstRewriter
    {
        protected override object VisitIdentifier(Identifier node)
        {
            if (!TryGetPlaceholderIndex(node.Name, out var index))
                return importedBinding is not null &&
                       !string.IsNullOrWhiteSpace(importedIdentifierName) &&
                       string.Equals(node.Name, importedIdentifierName, StringComparison.Ordinal)
                    ? importedBinding
                    : node;

            // 直接把占位符节点替换成真实参数 AST。
            // 这里不做字符串展开，也不重新 parse。
            return arguments[index];
        }

        protected override object VisitMemberExpression(MemberExpression node)
        {
            var @object = (Expression?) Visit(node.Object) ?? node.Object;
            var property = node.Computed
                ? (Expression?) Visit(node.Property) ?? node.Property
                : node.Property;

            if (ReferenceEquals(@object, node.Object) &&
                ReferenceEquals(property, node.Property))
            {
                return node;
            }

            return new MemberExpression(@object, property, node.Computed, node.Optional);
        }

    }

    private sealed record InlineTemplateAnalysis(
        int PlaceholderCount,
        InlineTemplateEvaluation Evaluation);

    private sealed class InlineTemplateEvaluationCollector(string signature) : AstVisitor
    {
        private readonly HashSet<int> _conditionalUsage = new();
        private readonly List<int> _evaluationOrder = new();
        private readonly Dictionary<int, int> _usageCounts = new();
        private int _conditionalDepth;
        private int _maxPlaceholderIndex = -1;

        public static InlineTemplateAnalysis Collect(Expression ast, string signature)
        {
            var collector = new InlineTemplateEvaluationCollector(signature);
            collector.Visit(ast);

            var placeholderCount = collector._maxPlaceholderIndex + 1;
            var usageCounts = new int[placeholderCount];
            foreach (var entry in collector._usageCounts)
                usageCounts[entry.Key] = entry.Value;

            var conditionalUsage = new bool[placeholderCount];
            foreach (var index in collector._conditionalUsage)
                conditionalUsage[index] = true;

            return new InlineTemplateAnalysis(
                placeholderCount,
                new InlineTemplateEvaluation(
                    usageCounts,
                    collector._evaluationOrder,
                    conditionalUsage));
        }

        protected override object VisitIdentifier(Identifier node)
        {
            if (node.Name.StartsWith(InternalInlineBindingPrefix, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Inline template '{signature}' contains the reserved placeholder prefix '{InternalInlineBindingPrefix}'.");
            }

            if (IsZeroBasedPlaceholder(node.Name))
            {
                throw new InvalidOperationException(
                    $"Inline template '{signature}' uses an invalid zero-based placeholder. Placeholders are 1-based: __arg1, __arg2, ...");
            }

            if (TryGetPlaceholderIndex(node.Name, out var index))
            {
                _maxPlaceholderIndex = Math.Max(_maxPlaceholderIndex, index);
                _usageCounts.TryGetValue(index, out var usageCount);
                _usageCounts[index] = usageCount + 1;
                _evaluationOrder.Add(index);
                if (_conditionalDepth > 0)
                    _conditionalUsage.Add(index);
            }

            return node;
        }

        protected override object VisitBinaryExpression(BinaryExpression node)
        {
            Visit(node.Left);
            if (node is LogicalExpression)
                VisitConditionally(node.Right);
            else
                Visit(node.Right);
            return node;
        }

        protected override object VisitCallExpression(CallExpression node)
        {
            Visit(node.Callee);
            foreach (var argument in node.Arguments)
            {
                if (node.Optional)
                    VisitConditionally(argument);
                else
                    Visit(argument);
            }
            return node;
        }

        protected override object VisitChainExpression(ChainExpression node)
        {
            // The chain may stop at any optional segment. Conservatively classify all
            // placeholders inside it as conditional; this affects only non-trivial inputs.
            VisitConditionally(node.Expression);
            return node;
        }

        protected override object VisitConditionalExpression(ConditionalExpression node)
        {
            Visit(node.Test);
            VisitConditionally(node.Consequent);
            VisitConditionally(node.Alternate);
            return node;
        }

        protected override object VisitMemberExpression(MemberExpression node)
        {
            Visit(node.Object);
            if (node.Computed)
            {
                if (node.Optional)
                    VisitConditionally(node.Property);
                else
                    Visit(node.Property);
            }
            return node;
        }

        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node)
            => VisitDeferred(node, () => base.VisitArrowFunctionExpression(node));

        protected override object VisitFunctionExpression(FunctionExpression node)
            => VisitDeferred(node, () => base.VisitFunctionExpression(node));

        private void VisitConditionally(Node node)
            => _ = VisitDeferred(node, () => Visit(node));

        private object VisitDeferred(Node fallback, Func<object?> visit)
        {
            _conditionalDepth++;
            try
            {
                return visit() ?? fallback;
            }
            finally
            {
                _conditionalDepth--;
            }
        }
    }

    private static bool TryGetPlaceholderIndex(string? name, out int index)
    {
        index = -1;
        if (name is null ||
            !name.StartsWith(InlinePlaceholderPrefix, StringComparison.Ordinal) ||
            name.Length == InlinePlaceholderPrefix.Length ||
            name[InlinePlaceholderPrefix.Length] == '0')
        {
            return false;
        }

        var digits = name.Substring(InlinePlaceholderPrefix.Length);
        for (var digitIndex = 0; digitIndex < digits.Length; digitIndex++)
        {
            if (digits[digitIndex] is < '0' or > '9')
                return false;
        }

        if (!int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var oneBasedIndex))
        {
            throw new InvalidOperationException(
                $"Inline placeholder '{name}' exceeds the supported index range.");
        }

        index = oneBasedIndex - 1;
        return true;
    }

    private static bool IsZeroBasedPlaceholder(string name)
    {
        const string zeroBasedPrefix = "__arg0";
        if (!name.StartsWith(zeroBasedPrefix, StringComparison.Ordinal))
            return false;

        for (var index = zeroBasedPrefix.Length; index < name.Length; index++)
        {
            if (name[index] is < '0' or > '9')
                return false;
        }

        return true;
    }
}
