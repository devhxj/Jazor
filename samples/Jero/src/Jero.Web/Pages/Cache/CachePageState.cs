using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Cache;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Cache;

[ECMAScriptModule("components/cache-page-state.js")]
public static class CachePageModule
{
    public static CachePageState Create() => new();

    public sealed class CachePageState : global::Jero.Web.Composition.IPageState<CachePageProps>
    {
        private string[] _keys = [];
        private CacheTreeNode[] _treeNodes = [];
        private CacheTreeNode[] _visibleNodes = [];
        private CacheTreeNode? _selectedNode;
        private string[] _selectedKeys = [];
        private string _keyFilter = string.Empty;
        private bool _loadingKeys;
        private bool _loadingValue;
        private bool _deletingKey;
        private bool _deletingPrefix;
        private bool _clearing;
        private bool _valueLoaded;
        private string _loadedValueKey = string.Empty;
        private CacheValuePreview _valuePreview = new()
        {
            TypeLabel = "unknown",
            DisplayText = "请选择左侧缓存键查看值。"
        };
        private string? _error;

        private bool CanQuery => ApplicationState.HasPermission("sysCache:keyList");
        private bool CanDelete => ApplicationState.HasPermission("sysCache:delete");
        private bool CanClear => ApplicationState.HasPermission("sysCache:clear");
        private bool HasSelectedKey => _selectedNode is not null && _selectedNode.CacheKey.Length > 0;
        private bool HasSelectedPrefixChildren => _selectedNode is not null &&
            _selectedKeys.Length > (_selectedNode.CacheKey.Length > 0 ? 1 : 0);

        public void Initialize()
            => LoadKeys();

        private void LoadKeys()
        {
            if (!CanQuery)
            {
                _keys = [];
                _treeNodes = [];
                _visibleNodes = [];
                ClearSelection();
                _error = "当前账号没有缓存查询权限。";
                return;
            }

            _loadingKeys = true;
            CacheClient.Keys().Then(outcome =>
            {
                _loadingKeys = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载缓存键失败。";
                    _keys = [];
                    _treeNodes = [];
                    _visibleNodes = [];
                    ClearSelection();
                    return;
                }

                var selectedPrefix = _selectedNode?.PrefixKey ?? string.Empty;
                var selectedCacheKey = _selectedNode?.CacheKey ?? string.Empty;
                _keys = CacheClient.ReadKeys(outcome.Data);
                _treeNodes = BuildTree(_keys);
                ApplyFilter();
                RestoreSelection(selectedPrefix, selectedCacheKey);
                _error = null;
            }, _ =>
            {
                _loadingKeys = false;
                _error = "加载缓存键失败，请检查网络后重试。";
            });
        }

        private void FilterChanged(string value)
        {
            _keyFilter = value;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var normalized = _keyFilter.Trim().ToLowerInvariant();
            if (normalized.Length == 0)
            {
                _visibleNodes = _treeNodes;
                return;
            }

            var count = 0;
            for (var index = 0; index < _treeNodes.Length; index++)
                if (MatchesFilter(_treeNodes[index], normalized))
                    count++;

            var result = new CacheTreeNode[count];
            var target = 0;
            for (var index = 0; index < _treeNodes.Length; index++)
            {
                if (!MatchesFilter(_treeNodes[index], normalized))
                    continue;
                result[target] = _treeNodes[index];
                target++;
            }
            _visibleNodes = result;
        }

        private bool MatchesFilter(CacheTreeNode node, string filter)
        {
            if (node.Label.ToLowerInvariant().Contains(filter) ||
                node.PrefixKey.ToLowerInvariant().Contains(filter) ||
                node.CacheKey.ToLowerInvariant().Contains(filter))
                return true;

            var descendantPrefix = node.PrefixKey + ":";
            for (var index = 0; index < _treeNodes.Length; index++)
            {
                var candidate = _treeNodes[index];
                if (!candidate.PrefixKey.StartsWith(descendantPrefix))
                    continue;
                if (candidate.PrefixKey.ToLowerInvariant().Contains(filter) ||
                    candidate.CacheKey.ToLowerInvariant().Contains(filter))
                    return true;
            }
            return false;
        }

        private void SelectNode(CacheTreeNode node)
        {
            _selectedNode = node;
            _selectedKeys = RelatedKeys(node.PrefixKey);
            ResetValue();
            if (node.CacheKey.Length > 0)
                LoadValue(node.CacheKey);
        }

        private void LoadValue(string key)
        {
            _loadingValue = true;
            ResetValue();
            CacheClient.Value(key).Then(outcome =>
            {
                _loadingValue = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载缓存值失败。";
                    return;
                }

                _loadedValueKey = key;
                _valueLoaded = true;
                _valuePreview = CacheClient.DescribeValue(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loadingValue = false;
                _error = "加载缓存值失败，请检查网络后重试。";
            });
        }

        private void RefreshValue()
        {
            if (_selectedNode is not null && _selectedNode.CacheKey.Length > 0)
                LoadValue(_selectedNode.CacheKey);
        }

        private void DeleteSelectedKey()
        {
            var key = _selectedNode?.CacheKey ?? string.Empty;
            if (!CanDelete || key.Length == 0 || _deletingKey)
                return;

            _deletingKey = true;
            CacheClient.Delete(key).Then(outcome =>
            {
                _deletingKey = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除缓存键失败。";
                    return;
                }

                ElementPlus.ElMessageMembers.Success("缓存键已删除");
                _error = null;
                LoadKeys();
            }, _ =>
            {
                _deletingKey = false;
                _error = "删除缓存键失败，请检查网络后重试。";
            });
        }

        private void DeleteSelectedPrefix()
        {
            var prefix = _selectedNode?.PrefixKey ?? string.Empty;
            if (!CanDelete || prefix.Length == 0 || !HasSelectedPrefixChildren || _deletingPrefix)
                return;

            _deletingPrefix = true;
            CacheClient.DeletePrefix(prefix).Then(outcome =>
            {
                _deletingPrefix = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "删除缓存前缀失败。";
                    return;
                }

                ElementPlus.ElMessageMembers.Success("缓存前缀已删除");
                _error = null;
                ClearSelection();
                LoadKeys();
            }, _ =>
            {
                _deletingPrefix = false;
                _error = "删除缓存前缀失败，请检查网络后重试。";
            });
        }

        private void ClearAll()
        {
            if (!CanClear || _clearing)
                return;

            _clearing = true;
            CacheClient.Clear().Then(outcome =>
            {
                _clearing = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "清空缓存失败。";
                    return;
                }

                ElementPlus.ElMessageMembers.Success("缓存已清空");
                _error = null;
                ClearSelection();
                LoadKeys();
            }, _ =>
            {
                _clearing = false;
                _error = "清空缓存失败，请检查网络后重试。";
            });
        }

        private void RestoreSelection(string prefix, string cacheKey)
        {
            if (prefix.Length == 0 && cacheKey.Length == 0)
            {
                ClearSelection();
                return;
            }

            for (var index = 0; index < _treeNodes.Length; index++)
            {
                var node = _treeNodes[index];
                if ((cacheKey.Length > 0 && node.CacheKey == cacheKey) ||
                    (cacheKey.Length == 0 && node.PrefixKey == prefix))
                {
                    SelectNode(node);
                    return;
                }
            }
            ClearSelection();
        }

        private void ClearSelection()
        {
            _selectedNode = null;
            _selectedKeys = [];
            ResetValue();
        }

        private void ResetValue()
        {
            _valueLoaded = false;
            _loadedValueKey = string.Empty;
            _valuePreview = new CacheValuePreview
            {
                TypeLabel = "unknown",
                DisplayText = "当前缓存值为空，或读取尚未完成。"
            };
        }

        private string[] RelatedKeys(string prefix)
        {
            var count = 0;
            var descendantPrefix = prefix + ":";
            for (var index = 0; index < _keys.Length; index++)
                if (_keys[index] == prefix || _keys[index].StartsWith(descendantPrefix))
                    count++;

            var result = new string[count];
            var target = 0;
            for (var index = 0; index < _keys.Length; index++)
            {
                if (_keys[index] != prefix && !_keys[index].StartsWith(descendantPrefix))
                    continue;
                result[target] = _keys[index];
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
                            Id = prefix,
                            Label = segments[depth].Length == 0 ? "(空片段)" : segments[depth],
                            PrefixKey = prefix,
                            Depth = depth
                        };
                        nodeIndex = count;
                        count++;
                    }

                    if (depth == segments.Length - 1)
                        buffer[nodeIndex].CacheKey = key;
                }
            }

            for (var nodeIndex = 0; nodeIndex < count; nodeIndex++)
            {
                var prefix = buffer[nodeIndex].PrefixKey;
                var descendantPrefix = prefix + ":";
                var keyCount = 0;
                for (var keyIndex = 0; keyIndex < source.Length; keyIndex++)
                    if (source[keyIndex] == prefix || source[keyIndex].StartsWith(descendantPrefix))
                        keyCount++;
                buffer[nodeIndex].KeyCount = keyCount;
            }

            var result = new CacheTreeNode[count];
            for (var index = 0; index < count; index++)
                result[index] = buffer[index];
            return result;
        }

        private static int FindNode(CacheTreeNode[] values, int count, string prefix)
        {
            for (var index = 0; index < count; index++)
                if (values[index].PrefixKey == prefix)
                    return index;
            return -1;
        }

        private void SelectPrefix(string prefix)
        {
            var index = FindNode(_visibleNodes, _visibleNodes.Length, prefix);
            if (index >= 0)
                SelectNode(_visibleNodes[index]);
        }
        private string DetailTitle()
        {
            if (_selectedNode is null)
                return "请选择左侧缓存节点";
            return _selectedNode.CacheKey.Length > 0
                ? _selectedNode.CacheKey
                : "前缀：" + _selectedNode.PrefixKey;
        }

        private string PrefixDeleteTitle()
            => "确定删除此前缀下的 " + _selectedKeys.Length + " 项缓存【" + (_selectedNode?.PrefixKey ?? string.Empty) + "】吗？";

        public CachePageProps CreateProps() => new()
        {
            Model = CreateModel(),
        };

        private CachePageModel CreateModel() => new()
        {
            Error = _error,
            CanQuery = CanQuery,
            CanClear = CanClear,
            CanDelete = CanDelete,
            LoadingKeys = _loadingKeys,
            LoadingValue = _loadingValue,
            DeletingKey = _deletingKey,
            DeletingPrefix = _deletingPrefix,
            Clearing = _clearing,
            VisibleNodes = _visibleNodes,
            SelectedNode = _selectedNode,
            KeyFilter = _keyFilter,
            SelectedKeys = _selectedKeys,
            ValueLoaded = _valueLoaded,
            LoadedValueKey = _loadedValueKey,
            ValuePreview = _valuePreview,
            HasSelectedKey = HasSelectedKey,
            HasSelectedPrefixChildren = HasSelectedPrefixChildren,
            DetailTitle = DetailTitle(),
            PrefixDeleteTitle = PrefixDeleteTitle(),
            DismissError = () => _error = null,
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
