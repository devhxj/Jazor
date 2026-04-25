using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Globalization;

namespace Jazor.Compiler;

/// <summary>
/// 为当前 operation 树提供 session 内部 identity。
/// 这里的 identity 只用于同一轮发射中的分配缓存区分，不参与最终可见名称的稳定主键。
/// </summary>
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
