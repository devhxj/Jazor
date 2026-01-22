using System.Collections.Generic;
using Acornima.Ast;

namespace ECMAScript.Compiler;

public sealed class WalkerArgument
{
    private readonly Dictionary<string, VariableDeclarator> _declarators = [];

    public bool HasVarDeclarator => _declarators.Count > 0;

    public void AddVarDeclarator(VariableDeclarator declarator, int depth)
    {
        var name = declarator.Id is Identifier identifier
            ? identifier.Name
            : declarator.Id.ToECMAScript();     
        var key = $"{depth}:{name}";
        if (!_declarators.ContainsKey(key))
            _declarators.Add(key, declarator);
    }

    public NodeList<VariableDeclarator> FlushVarDeclarator()
    {
        var list = NodeList.From(_declarators.Values);
        _declarators.Clear();
        return list;
    }
}
