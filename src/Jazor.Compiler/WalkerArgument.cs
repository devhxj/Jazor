using System.Collections.Generic;
using Acornima.Ast;

namespace Jazor.Compiler;

/// <summary>
/// Walker 参数,用于在遍历过程中传递上下文信息,如变量声明等,以便生成正确的代码。
/// 支持变量声明的累积与刷新机制。
/// </summary>
public sealed class WalkerArgument
{
    private readonly Dictionary<string, List<ImportDeclarationSpecifier>> _specifiers = [];

    private readonly Dictionary<string, VariableDeclarator> _declarators;

    public WalkerArgument()
    {
        _specifiers = [];
        _declarators = [];
    }

    private WalkerArgument(
        Dictionary<string, List<ImportDeclarationSpecifier>> specifiers,
        Dictionary<string, VariableDeclarator> declarators)
        => (_specifiers, _declarators) = (specifiers, declarators);

    /// <summary>
    /// 是否包含导入声明规范
    /// </summary>
    public bool HasVarImportDeclarationSpecifier => _specifiers.Count > 0;

    /// <summary>
    /// 是否包含变量声明
    /// </summary>
    public bool HasVarDeclarator => _declarators.Count > 0;

    /// <summary>
    /// 添加导入声明规范,根据模块路径进行分组存储。
    /// </summary>
    /// <param name="declarator"></param>
    /// <param name="depth"></param>
    public void MergeImportSpecifier(string modulePath, ImportDeclarationSpecifier specifier)
    {
        if (_specifiers.TryGetValue(modulePath, out var list))
            list.Add(specifier);
        else
            _specifiers.Add(modulePath, [specifier]);
    }

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
    /// 创建新实例，共享导入字典，但使用新的变量声明字典。
    /// 用于块级作用域隔离。
    /// </summary>
    /// <returns>新的 WalkerArgument 实例</returns>
    public WalkerArgument WithNewDeclarators()
        => new(_specifiers, new Dictionary<string, VariableDeclarator>());
}
