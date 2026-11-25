using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Cache;

[ECMAScriptModule("components/cache-page-state.js")]
public static class CachePageModule
{
    public static CachePageState Create() => new();

    public sealed class CachePageState : global::Jero.Web.App.IPageState<CachePageProps>
    {
        private string[] keys = [];
        private CacheTreeNode[] treeNodes = [];
        private CacheTreeNode[] visibleNodes = [];
        private CacheTreeNode? selectedNode;
        private string[] selectedKeys = [];
        private string keyFilter = string.Empty;
        private bool loadingKeys;
        private bool loadingValue;
        private bool deletingKey;
        private bool deletingPrefix;
        private bool clearing;
        private bool valueLoaded;
        private string loadedValueKey = string.Empty;
        private CacheValuePreview valuePreview = new()
        {
            typeLabel = "unknown",
            displayText = "请选择左侧缓存键查看值。"
        };
        private string? error;

        private bool CanQuery => ApplicationState.HasPermission("sysCache:keyList");
        private bool CanDelete => ApplicationState.HasPermission("sysCache:delete");
        private bool CanClear => ApplicationState.HasPermission("sysCache:clear");
        private bool HasSelectedKey => selectedNode is not null && selectedNode.cacheKey.Length > 0;
        private bool HasSelectedPrefixChildren => selectedNode is not null &&
            selectedKeys.Length > (selectedNode.cacheKey.Length > 0 ? 1 : 0);

        public void Initialize()
            => LoadKeys();

        private void LoadKeys()
        {
            if (!CanQuery)
            {
                keys = [];
                treeNodes = [];
                visibleNodes = [];
                ClearSelection();
                error = "当前账号没有缓存查询权限。";
                return;
            }

            loadingKeys = true;
            CacheClient.Keys().Then(outcome =>
            {
                loadingKeys = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载缓存键失败。";
                    keys = [];
                    treeNodes = [];
                    visibleNodes = [];
                    ClearSelection();
                    return;
                }

                var selectedPrefix = selectedNode?.prefixKey ?? string.Empty;
                var selectedCacheKey = selectedNode?.cacheKey ?? string.Empty;
                keys = CacheClient.ReadKeys(outcome.Data);
                treeNodes = BuildTree(keys);
                ApplyFilter();
                RestoreSelection(selectedPrefix, selectedCacheKey);
                error = null;
            }, _ =>
            {
                loadingKeys = false;
                error = "加载缓存键失败，请检查网络后重试。";
            });
        }

        private void FilterChanged(string value)
        {
            keyFilter = value;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var normalized = keyFilter.Trim().ToLowerInvariant();
            if (normalized.Length == 0)
            {
                visibleNodes = treeNodes;
                return;
            }

            var count = 0;
            for (var index = 0; index < treeNodes.Length; index++)
                if (MatchesFilter(treeNodes[index], normalized))
                    count++;

            var result = new CacheTreeNode[count];
            var target = 0;
            for (var index = 0; index < treeNodes.Length; index++)
            {
                if (!MatchesFilter(treeNodes[index], normalized))
                    continue;
                result[target] = treeNodes[index];
                target++;
            }
            visibleNodes = result;
        }

        private bool MatchesFilter(CacheTreeNode node, string filter)
        {
            if (node.label.ToLowerInvariant().Contains(filter) ||
                node.prefixKey.ToLowerInvariant().Contains(filter) ||
                node.cacheKey.ToLowerInvariant().Contains(filter))
                return true;

            var descendantPrefix = node.prefixKey + ":";
            for (var index = 0; index < treeNodes.Length; index++)
            {
                var candidate = treeNodes[index];
                if (!candidate.prefixKey.StartsWith(descendantPrefix))
                    continue;
                if (candidate.prefixKey.ToLowerInvariant().Contains(filter) ||
                    candidate.cacheKey.ToLowerInvariant().Contains(filter))
                    return true;
            }
            return false;
        }

        private void SelectNode(CacheTreeNode node)
        {
            selectedNode = node;
            selectedKeys = RelatedKeys(node.prefixKey);
            ResetValue();
            if (node.cacheKey.Length > 0)
                LoadValue(node.cacheKey);
        }

        private void LoadValue(string key)
        {
            loadingValue = true;
            ResetValue();
            CacheClient.Value(key).Then(outcome =>
            {
                loadingValue = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载缓存值失败。";
                    return;
                }

                loadedValueKey = key;
                valueLoaded = true;
                valuePreview = CacheClient.DescribeValue(outcome.Data);
                error = null;
            }, _ =>
            {
                loadingValue = false;
                error = "加载缓存值失败，请检查网络后重试。";
            });
        }

        private void RefreshValue()
        {
            if (selectedNode is not null && selectedNode.cacheKey.Length > 0)
                LoadValue(selectedNode.cacheKey);
        }

        private void DeleteSelectedKey()
        {
            var key = selectedNode?.cacheKey ?? string.Empty;
            if (!CanDelete || key.Length == 0 || deletingKey)
                return;

            deletingKey = true;
            CacheClient.Delete(key).Then(outcome =>
            {
                deletingKey = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除缓存键失败。";
                    return;
                }

                ElementPlus.ElMessageMembers.Success("缓存键已删除");
                error = null;
                LoadKeys();
            }, _ =>
            {
                deletingKey = false;
                error = "删除缓存键失败，请检查网络后重试。";
            });
        }

        private void DeleteSelectedPrefix()
        {
            var prefix = selectedNode?.prefixKey ?? string.Empty;
            if (!CanDelete || prefix.Length == 0 || !HasSelectedPrefixChildren || deletingPrefix)
                return;

            deletingPrefix = true;
            CacheClient.DeletePrefix(prefix).Then(outcome =>
            {
                deletingPrefix = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "删除缓存前缀失败。";
                    return;
                }

                ElementPlus.ElMessageMembers.Success("缓存前缀已删除");
                error = null;
                ClearSelection();
                LoadKeys();
            }, _ =>
            {
                deletingPrefix = false;
                error = "删除缓存前缀失败，请检查网络后重试。";
            });
        }

        private void ClearAll()
        {
            if (!CanClear || clearing)
                return;

            clearing = true;
            CacheClient.Clear().Then(outcome =>
            {
                clearing = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "清空缓存失败。";
                    return;
                }

                ElementPlus.ElMessageMembers.Success("缓存已清空");
                error = null;
                ClearSelection();
                LoadKeys();
            }, _ =>
            {
                clearing = false;
                error = "清空缓存失败，请检查网络后重试。";
            });
        }

        private void RestoreSelection(string prefix, string cacheKey)
        {
            if (prefix.Length == 0 && cacheKey.Length == 0)
            {
                ClearSelection();
                return;
            }

            for (var index = 0; index < treeNodes.Length; index++)
            {
                var node = treeNodes[index];
                if ((cacheKey.Length > 0 && node.cacheKey == cacheKey) ||
                    (cacheKey.Length == 0 && node.prefixKey == prefix))
                {
                    SelectNode(node);
                    return;
                }
            }
            ClearSelection();
        }

        private void ClearSelection()
        {
            selectedNode = null;
            selectedKeys = [];
            ResetValue();
        }

        private void ResetValue()
        {
            valueLoaded = false;
            loadedValueKey = string.Empty;
            valuePreview = new CacheValuePreview
            {
                typeLabel = "unknown",
                displayText = "当前缓存值为空，或读取尚未完成。"
            };
        }

        private string[] RelatedKeys(string prefix)
        {
            var count = 0;
            var descendantPrefix = prefix + ":";
            for (var index = 0; index < keys.Length; index++)
                if (keys[index] == prefix || keys[index].StartsWith(descendantPrefix))
                    count++;

            var result = new string[count];
            var target = 0;
            for (var index = 0; index < keys.Length; index++)
            {
                if (keys[index] != prefix && !keys[index].StartsWith(descendantPrefix))
                    continue;
                result[target] = keys[index];
                target++;
            }
            return result;
        }

        private static CacheTreeNode[] BuildTree(string[] source)
        {
            var capacity = 0;
            for (var index = 0; index < source.Length; index++)
                capacity += source[index].Split(':').Length;

            var buffer = new CacheTreeNode[capacity];
            var count = 0;
            for (var keyIndex = 0; keyIndex < source.Length; keyIndex++)
            {
                var key = source[keyIndex];
                var segments = key.Split(':');
                var prefix = string.Empty;
                for (var depth = 0; depth < segments.Length; depth++)
                {
                    prefix = depth == 0 ? segments[depth] : prefix + ":" + segments[depth];
                    var nodeIndex = FindNode(buffer, count, prefix);
                    if (nodeIndex < 0)
                    {
                        buffer[count] = new CacheTreeNode
                        {
                            id = prefix,
                            label = segments[depth].Length == 0 ? "(空片段)" : segments[depth],
                            prefixKey = prefix,
                            depth = depth
                        };
                        nodeIndex = count;
                        count++;
                    }

                    if (depth == segments.Length - 1)
                        buffer[nodeIndex].cacheKey = key;
                }
            }

            for (var nodeIndex = 0; nodeIndex < count; nodeIndex++)
            {
                var prefix = buffer[nodeIndex].prefixKey;
                var descendantPrefix = prefix + ":";
                var keyCount = 0;
                for (var keyIndex = 0; keyIndex < source.Length; keyIndex++)
                    if (source[keyIndex] == prefix || source[keyIndex].StartsWith(descendantPrefix))
                        keyCount++;
                buffer[nodeIndex].keyCount = keyCount;
            }

            var result = new CacheTreeNode[count];
            for (var index = 0; index < count; index++)
                result[index] = buffer[index];
            return result;
        }

        private static int FindNode(CacheTreeNode[] values, int count, string prefix)
        {
            for (var index = 0; index < count; index++)
                if (values[index].prefixKey == prefix)
                    return index;
            return -1;
        }

        private void SelectPrefix(string prefix)
        {
            var index = FindNode(visibleNodes, visibleNodes.Length, prefix);
            if (index >= 0)
                SelectNode(visibleNodes[index]);
        }
        private string DetailTitle()
        {
            if (selectedNode is null)
                return "请选择左侧缓存节点";
            return selectedNode.cacheKey.Length > 0
                ? selectedNode.cacheKey
                : "前缀：" + selectedNode.prefixKey;
        }

        private string PrefixDeleteTitle()
            => "确定删除此前缀下的 " + selectedKeys.Length + " 项缓存【" + (selectedNode?.prefixKey ?? string.Empty) + "】吗？";

        public CachePageProps CreateProps() => new()
        {
            Model = CreateModel(),
        };

        private CachePageModel CreateModel() => new()
        {
            Error = error,
            CanQuery = CanQuery,
            CanClear = CanClear,
            CanDelete = CanDelete,
            LoadingKeys = loadingKeys,
            LoadingValue = loadingValue,
            DeletingKey = deletingKey,
            DeletingPrefix = deletingPrefix,
            Clearing = clearing,
            VisibleNodes = visibleNodes,
            SelectedNode = selectedNode,
            KeyFilter = keyFilter,
            SelectedKeys = selectedKeys,
            ValueLoaded = valueLoaded,
            LoadedValueKey = loadedValueKey,
            ValuePreview = valuePreview,
            HasSelectedKey = HasSelectedKey,
            HasSelectedPrefixChildren = HasSelectedPrefixChildren,
            DetailTitle = DetailTitle(),
            PrefixDeleteTitle = PrefixDeleteTitle(),
            DismissError = () => error = null,
            LoadKeys = LoadKeys,
            ClearAll = ClearAll,
            FilterChanged = FilterChanged,
            SelectPrefix = SelectPrefix,
            RefreshValue = RefreshValue,
            DeleteSelectedKey = DeleteSelectedKey,
            DeleteSelectedPrefix = DeleteSelectedPrefix,
        };
    }
}

public sealed record CachePageProps : VueProps
{
    public required CachePageModel Model { get; init; }
}
