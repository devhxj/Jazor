// File: OperationIdentityIndex.cs
// Purpose: Assigns stable identities to Roslyn operations within one conversion root.
// 为临时名和 scope key 提供与树遍历顺序无关的操作身份，不能跨不同 operation tree 复用。
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Globalization;

namespace Jazor.Compiler;

/// <summary>
/// 为当前 operation 树提供 session 内部 identity。
/// 这里的 identity 只用于同一轮发射中的分配缓存区分，不参与最终可见名称的稳定主键。
/// </summary>
/// <remarks>
/// 已挂在根 operation 树上的节点使用结构路径；临时构造、脱离根树的节点使用 session 内递增
/// 的 detached 编号。后者只保证本轮可区分，不能被当成跨编译的稳定标识。
/// </remarks>
internal sealed class OperationIdentityIndex
{
    private readonly Dictionary<IOperation, string> _paths =
        new(ReferenceEqualityComparer<IOperation>.Instance);

    private readonly Dictionary<IOperation, string> _detachedPaths =
        new(ReferenceEqualityComparer<IOperation>.Instance);

    private int _detachedSequence;

    public OperationIdentityIndex(IOperation root)
    {
        if (root is null)
            throw new System.ArgumentNullException(nameof(root));

        BuildPath(root, "r");
    }

    public string GetIdentity(IOperation operation)
    {
        if (operation is null)
            throw new System.ArgumentNullException(nameof(operation));

        if (_paths.TryGetValue(operation, out var path))
            return path;

        if (_detachedPaths.TryGetValue(operation, out path))
            return path;

        path = "d:" + _detachedSequence.ToString(CultureInfo.InvariantCulture);
        _detachedSequence++;
        _detachedPaths.Add(operation, path);
        return path;
    }

    private void BuildPath(IOperation operation, string path)
    {
        _paths[operation] = path;

        var childOrdinal = 0;
        foreach (var child in operation.ChildOperations)
        {
            if (child is null)
                continue;

            var childPath = string.Concat(
                path,
                "/",
                childOrdinal.ToString(CultureInfo.InvariantCulture));
            BuildPath(child, childPath);
            childOrdinal++;
        }
    }
}
