using System.Collections.Generic;
using Acornima.Ast;

namespace ECMAScript.Compiler;

/// <summary>
/// Walker 参数,用于在遍历过程中传递上下文信息,如变量声明等,以便生成正确的代码。
/// 支持变量声明的累积与刷新机制。
/// </summary>
public sealed class WalkerArgument
{
    private readonly Dictionary<string, VariableDeclarator> _declarators;

    public WalkerArgument()
    {
        _declarators = [];
    }

    private WalkerArgument((NodeType Type, Expression Target)? context, Dictionary<string, VariableDeclarator> declarators)
        => (Context, _declarators) = (context, declarators);

    /// <summary>
    /// 是否包含变量声明
    /// </summary>
    public bool HasVarDeclarator => _declarators.Count > 0;

    /// <summary>
    /// 上下文表达式,如果未设置，则默认会在使用时用标识符"@ctx"代替
    /// </summary>
    public (NodeType Type, Expression Target)? Context { get; }

    /// <summary>
    /// 添加变量声明,根据深度和名称生成唯一键,防止重复添加。
    /// </summary>
    /// <param name="declarator"></param>
    /// <param name="depth"></param>
    public void AddVarDeclarator(VariableDeclarator declarator, int depth)
    {
        var name = declarator.Id is Identifier identifier
            ? identifier.Name
            : declarator.Id.ToECMAScript();
        var key = $"{depth}:{name}";
        if (!_declarators.ContainsKey(key))
            _declarators.Add(key, declarator);
    }

    /// <summary>
    /// 刷新并获取当前累积的变量声明列表,然后清空内部存储。
    /// </summary>
    /// <returns></returns>
    public NodeList<VariableDeclarator> FlushVarDeclarator()
    {
        var list = NodeList.From(_declarators.Values);
        _declarators.Clear();
        return list;
    }

    /// <summary>
    /// 创建一个新的WalkerArgument实例，复用当前累积的变量声明列表，并更新上下文表达式。
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public WalkerArgument With(NodeType type, Expression target)
        => new((type, target), _declarators);
}
