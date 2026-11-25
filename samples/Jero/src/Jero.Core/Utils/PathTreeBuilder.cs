namespace Jero.Core;

/// <summary>
/// 树形节点
/// </summary>
public class TreeNode
{
    public int Id { get; set; }
    public int Pid { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TreeNode> Children { get; set; } = new();
}

/// <summary>
/// 根据路径数组生成树结构
/// </summary>
public class PathTreeBuilder
{
    private int _nextId = 1;

    public TreeNode BuildTree(List<string> paths)
    {
        var root = new TreeNode { Id = 1, Pid = 0, Name = "文件目录" }; // 根节点
        var dict = new Dictionary<string, TreeNode>();

        foreach (var path in paths)
        {
            var parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            TreeNode currentNode = root;

            foreach (var part in parts)
            {
                var key = currentNode.Id + "_" + part; // 生成唯一键
                if (!dict.ContainsKey(key))
                {
                    var newNode = new TreeNode
                    {
                        Id = _nextId++,
                        Pid = currentNode.Id,
                        Name = part
                    };
                    currentNode.Children.Add(newNode);
                    dict[key] = newNode;
                }
                currentNode = dict[key]; // 更新当前节点
            }
        }

        return root;
    }
}