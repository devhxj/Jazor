// File: AstReferenceAnalysis.cs
// Purpose: Collects identifier references from emitted ESTree nodes.
// 用于在模块头生成前过滤未被最终 AST 使用的 import binding，不参与 C# 语义判定。
using System;
using System.Collections.Generic;
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// 比较 JavaScript AST 中表示引用和值的节点是否语义等价。
/// </summary>
/// <remarks>
/// 该分析用于局部优化和结构判断，不是完整的 AST 深比较器，也不判断两个表达式执行结果
/// 是否相同。带有调用、赋值或其他副作用的节点不能仅凭结构相似就当作可合并引用。
/// </remarks>
public static class AstReferenceAnalysis
{
    public static bool AreEquivalentReference(Expression left, Expression right)
    {
        if (left is null)
            throw new ArgumentNullException(nameof(left));
        if (right is null)
            throw new ArgumentNullException(nameof(right));
        if (ReferenceEquals(left, right))
            return true;
        if (left.GetType() != right.GetType())
            return false;

        return (left, right) switch
        {
            (Identifier leftIdentifier, Identifier rightIdentifier) =>
                string.Equals(leftIdentifier.Name, rightIdentifier.Name, StringComparison.Ordinal),
            (ThisExpression, ThisExpression) => true,
            (Super, Super) => true,
            (NullLiteral, NullLiteral) => true,
            (BooleanLiteral leftLiteral, BooleanLiteral rightLiteral) =>
                leftLiteral.Value == rightLiteral.Value,
            (NumericLiteral leftLiteral, NumericLiteral rightLiteral) =>
                leftLiteral.Value.Equals(rightLiteral.Value),
            (BigIntLiteral leftLiteral, BigIntLiteral rightLiteral) =>
                leftLiteral.Value.Equals(rightLiteral.Value),
            (StringLiteral leftLiteral, StringLiteral rightLiteral) =>
                string.Equals(leftLiteral.Value, rightLiteral.Value, StringComparison.Ordinal),
            (MemberExpression leftMember, MemberExpression rightMember) =>
                leftMember.Computed == rightMember.Computed &&
                leftMember.Optional == rightMember.Optional &&
                AreEquivalentReference(leftMember.Object, rightMember.Object) &&
                AreEquivalentReference(leftMember.Property, rightMember.Property),
            _ => false
        };
    }

    public static bool ReferencesIdentifier(Node node, string name)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        var collector = new IdentifierCollector(name);
        collector.Visit(node);
        return collector.Found;
    }

    public static HashSet<string> CollectIdentifiers(IEnumerable<Node> nodes)
    {
        if (nodes is null)
            throw new ArgumentNullException(nameof(nodes));

        var collector = new IdentifierCollector(name: null);
        foreach (var node in nodes)
            collector.Visit(node);
        return collector.Names;
    }

    private sealed class IdentifierCollector(string? name) : AstVisitor
    {
        public HashSet<string> Names { get; } = new(StringComparer.Ordinal);

        public bool Found { get; private set; }

        protected override object VisitIdentifier(Identifier node)
        {
            Names.Add(node.Name);
            if (string.Equals(node.Name, name, StringComparison.Ordinal))
                Found = true;
            return node;
        }

        protected override object VisitObjectProperty(ObjectProperty node)
        {
            if (node.Computed)
                Visit(node.Key);
            Visit(node.Value);
            return node;
        }

        protected override object VisitMemberExpression(MemberExpression node)
        {
            Visit(node.Object);
            if (node.Computed)
                Visit(node.Property);
            return node;
        }

        protected override object VisitVariableDeclarator(VariableDeclarator node)
        {
            VisitBindingSideEffects(node.Id);
            if (node.Init is not null)
                Visit(node.Init);
            return node;
        }

        protected override object VisitFunctionDeclaration(FunctionDeclaration node)
        {
            VisitFunctionParameters(node.Params);
            Visit(node.Body);
            return node;
        }

        protected override object VisitFunctionExpression(FunctionExpression node)
        {
            VisitFunctionParameters(node.Params);
            Visit(node.Body);
            return node;
        }

        protected override object VisitArrowFunctionExpression(ArrowFunctionExpression node)
        {
            VisitFunctionParameters(node.Params);
            Visit(node.Body);
            return node;
        }

        protected override object VisitCatchClause(CatchClause node)
        {
            if (node.Param is not null)
                VisitBindingSideEffects(node.Param);
            Visit(node.Body);
            return node;
        }

        protected override object VisitClassDeclaration(ClassDeclaration node)
        {
            if (node.SuperClass is not null)
                Visit(node.SuperClass);
            Visit(node.Body);
            return node;
        }

        protected override object VisitClassExpression(ClassExpression node)
        {
            if (node.SuperClass is not null)
                Visit(node.SuperClass);
            Visit(node.Body);
            return node;
        }

        protected override object VisitMethodDefinition(MethodDefinition node)
        {
            if (node.Computed)
                Visit(node.Key);
            Visit(node.Value);
            return node;
        }

        protected override object VisitPropertyDefinition(PropertyDefinition node)
        {
            if (node.Computed)
                Visit(node.Key);
            if (node.Value is not null)
                Visit(node.Value);
            return node;
        }

        protected override object VisitLabeledStatement(LabeledStatement node)
        {
            Visit(node.Body);
            return node;
        }

        protected override object VisitBreakStatement(BreakStatement node) => node;

        protected override object VisitContinueStatement(ContinueStatement node) => node;

        private void VisitFunctionParameters(IEnumerable<Node> parameters)
        {
            foreach (var parameter in parameters)
                VisitBindingSideEffects(parameter);
        }

        private void VisitBindingSideEffects(Node node)
        {
            switch (node)
            {
                case Identifier:
                    return;
                case AssignmentPattern assignment:
                    VisitBindingSideEffects(assignment.Left);
                    Visit(assignment.Right);
                    return;
                case RestElement rest:
                    VisitBindingSideEffects(rest.Argument);
                    return;
                case ArrayPattern array:
                    foreach (var element in array.Elements)
                    {
                        if (element is not null)
                            VisitBindingSideEffects(element);
                    }
                    return;
                case ObjectPattern @object:
                    foreach (var property in @object.Properties)
                    {
                        switch (property)
                        {
                            case AssignmentProperty assignmentProperty:
                                if (assignmentProperty.Computed)
                                    Visit(assignmentProperty.Key);
                                VisitBindingSideEffects(assignmentProperty.Value);
                                break;
                            case ObjectProperty objectProperty:
                                if (objectProperty.Computed)
                                    Visit(objectProperty.Key);
                                VisitBindingSideEffects(objectProperty.Value);
                                break;
                            case RestElement objectRest:
                                VisitBindingSideEffects(objectRest.Argument);
                                break;
                            default:
                                Visit(property);
                                break;
                        }
                    }
                    return;
                default:
                    Visit(node);
                    return;
            }
        }
    }
}
