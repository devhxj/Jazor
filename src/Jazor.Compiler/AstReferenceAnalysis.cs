using System;
using System.Collections.Generic;
using Acornima;
using Acornima.Ast;

namespace Jazor.Compiler;

public static class AstReferenceAnalysis
{
    public static bool AreEquivalentReference(Expression left, Expression right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null)
            throw new ArgumentNullException(nameof(left));
        if (right is null)
            throw new ArgumentNullException(nameof(right));
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
            if (node.Init is not null)
                Visit(node.Init);
            return node;
        }

        protected override object VisitFunctionDeclaration(FunctionDeclaration node)
        {
            foreach (var parameter in node.Params)
                Visit(parameter);
            Visit(node.Body);
            return node;
        }
    }
}
