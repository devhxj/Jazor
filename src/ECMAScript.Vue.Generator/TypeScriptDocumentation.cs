using System.Text.RegularExpressions;
using TreeSitter;

internal static class TypeScriptDocumentation
{
    internal static void Visit(Node parent, string path, Action<Node, string, string?> visit)
    {
        string? comment = null;
        foreach (var child in parent.NamedChildren)
        {
            if (child.Type == "comment")
            {
                comment = child.Text.StartsWith("/**", StringComparison.Ordinal) ? child.Text : null;
                continue;
            }
            if (child.Type == "export_statement")
            {
                // JSDoc precedes export, while the declaration itself is its child.
                var declaration = child.NamedChildren.FirstOrDefault(static node =>
                    node.Type is "interface_declaration" or "type_alias_declaration" or "class_declaration");
                if (declaration is not null)
                    VisitDeclaration(declaration, path, comment, visit);
                comment = null;
                continue;
            }
            VisitDeclaration(child, path, comment, visit);
            comment = null;
        }
    }

    private static void VisitDeclaration(Node node, string path, string? comment, Action<Node, string, string?> visit)
    {
        if (node.Type is "interface_declaration" or "type_alias_declaration" or "class_declaration" or "property_signature")
        {
            var name = node.GetChildForField("name")?.Text.Trim('\'', '"');
            if (name is not null)
            {
                path = path.Length == 0 ? name : path + "." + name;
                visit(node, path, comment);
            }
        }
        Visit(node, path, visit);
    }

    public static string? ReadComment(string comment)
    {
        var lines = comment.Trim().TrimStart('/').TrimStart('*').TrimEnd('/').TrimEnd('*')
            .Replace("\r", string.Empty, StringComparison.Ordinal).Split('\n')
            .Select(static line => Regex.Replace(line.Trim(), @"^\*\s?", string.Empty));
        var summary = new List<string>();
        foreach (var line in lines)
        {
            if (line.StartsWith("@default ", StringComparison.Ordinal))
                summary.Add("默认值：" + line[9..]);
            else if (line.StartsWith("@deprecated", StringComparison.Ordinal))
                summary.Add("已弃用：" + line[11..].Trim());
            else if (!line.StartsWith('@') && line != "null")
                summary.Add(line);
        }
        var text = string.Join("\n", summary).Trim();
        return text.Length == 0 ? null : text;
    }
}