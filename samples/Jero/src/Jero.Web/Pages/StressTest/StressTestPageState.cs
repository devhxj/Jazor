using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.StressTest;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Infrastructure.Http;

namespace Jero.Web.Pages.StressTest;

[ECMAScriptModule("components/stress-test-page-state.js")]
public static class StressTestPageModule
{
    public static StressTestPageState Create() => new();

    public sealed class StressTestPageState : global::Jero.Web.Composition.IPageState<StressTestPageProps>
    {
        private StressSwaggerResource[] _resources = [];
        private StressApiGroup[] _groups = [];
        private StressApiNode? _selectedNode;
        private StressTestResult _result = new();
        private bool _loadingResources;
        private bool _loadingDocument;
        private bool _dialogVisible;
        private bool _submitting;
        private int _documentRequestVersion;
        private string _selectedSwaggerUrl = string.Empty;
        private string _keywords = string.Empty;
        private string _appliedKeywords = string.Empty;
        private string? _error;

        private string _editMethod = "GET";
        private string _editUri = string.Empty;
        private int _editRounds = 1;
        private int _editRequests = 100;
        private int _editParallelism = 200;
        private string _activePairTab = "headers";
        private StressPair[] _editHeaders = [];
        private StressPair[] _editBody = [];
        private StressPair[] _editPath = [];
        private StressPair[] _editQuery = [];

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool Busy => _loadingResources || _loadingDocument;
        private StressPair[] ActivePairs => _activePairTab == "body" ? _editBody : _activePairTab == "path" ? _editPath : _activePairTab == "query" ? _editQuery : _editHeaders;

        public void Initialize() => LoadResources();

        private void LoadResources()
        {
            if (_loadingResources)
                return;
            _loadingResources = true;
            _error = null;
            StressTestClient.Resources().Then(outcome =>
            {
                _loadingResources = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载 Swagger 分组失败。";
                    return;
                }
                _resources = StressTestClient.ReadResources(outcome.Data);
                if (_selectedSwaggerUrl.Length == 0 && _resources.Length > 0)
                    _selectedSwaggerUrl = _resources[0].Url;
                LoadDocument();
            }, _ =>
            {
                _loadingResources = false;
                _error = "加载 Swagger 分组失败，请检查网络后重试。";
            });
        }

        private void SwaggerChanged(string? value)
        {
            _selectedSwaggerUrl = value ?? string.Empty;
            LoadDocument();
        }

        private void LoadDocument()
        {
            // 切换或清空分组后忽略旧的 Swagger 响应，
            // 避免加载状态卡住或新选中的文档被覆盖。
            var requestVersion = ++_documentRequestVersion;
            if (_selectedSwaggerUrl.Length == 0)
            {
                _loadingDocument = false;
                _groups = [];
                _selectedNode = null;
                return;
            }
            _loadingDocument = true;
            var requestUrl = _selectedSwaggerUrl;
            StressTestClient.Document(requestUrl).Then(outcome =>
            {
                if (requestVersion != _documentRequestVersion || requestUrl != _selectedSwaggerUrl)
                    return;
                _loadingDocument = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载 Swagger 文档失败。";
                    _groups = [];
                    return;
                }
                _groups = StressTestClient.ReadGroups(outcome.Data, string.Empty);
                _appliedKeywords = _keywords.Trim().ToLower();
                _selectedNode = null;
                _error = null;
            }, _ =>
            {
                if (requestVersion != _documentRequestVersion || requestUrl != _selectedSwaggerUrl)
                    return;
                _loadingDocument = false;
                _groups = [];
                _error = "加载 Swagger 文档失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            // 只在用户提交搜索时筛选，保留当前选中请求的草稿。
            _appliedKeywords = _keywords.Trim().ToLower();
        }

        private ConfigurationOption[] ResourceOptions()
        {
            var options = new ConfigurationOption[_resources.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(_resources[index].Url, _resources[index].Name);
            return options;
        }

        private StressBrowseGroup[] VisibleGroups()
        {
            var visibleGroups = new List<StressBrowseGroup>();
            for (var groupIndex = 0; groupIndex < _groups.Length; groupIndex++)
            {
                var group = _groups[groupIndex];
                var visibleNodes = new List<StressBrowseNode>();
                for (var nodeIndex = 0; nodeIndex < group.Children.Length; nodeIndex++)
                {
                    var node = group.Children[nodeIndex];
                    if (NodeVisible(node))
                        visibleNodes.Add(new StressBrowseNode(node.Id, node.Method, node.Path, node.Summary, groupIndex, nodeIndex));
                }
                // 保留原始索引，筛选后的条目仍能打开对应的请求草稿。
                if (visibleNodes.Count > 0)
                    visibleGroups.Add(new StressBrowseGroup(group.Name, group.Summary, visibleNodes.ToArray()));
            }
            return visibleGroups.ToArray();
        }

        private bool NodeVisible(StressApiNode item)
        {
            if (_appliedKeywords.Length == 0)
                return true;
            var searchable = (item.Summary + " " + item.Path + " " + item.Method + " " + item.TagName).ToLower();
            return searchable.Contains(_appliedKeywords);
        }

        private void SelectNode(StressApiNode item)
        {
            _selectedNode = item;
            FillForm(item);
        }

        private void SelectNodeAt(int groupIndex, int nodeIndex)
        {
            if (groupIndex < 0 || groupIndex >= _groups.Length || nodeIndex < 0 || nodeIndex >= _groups[groupIndex].Children.Length)
                return;
            SelectNode(_groups[groupIndex].Children[nodeIndex]);
        }

        private void OpenDialogAt(int groupIndex, int nodeIndex)
        {
            if (groupIndex < 0 || groupIndex >= _groups.Length || nodeIndex < 0 || nodeIndex >= _groups[groupIndex].Children.Length)
                return;
            OpenEditor(_groups[groupIndex].Children[nodeIndex]);
        }

        private void OpenCurrentDialog()
        {
            if (_selectedNode is not null)
                OpenEditor(_selectedNode);
        }

        private void OpenEditor(StressApiNode item)
        {
            if (!IsSuperAdmin)
                return;
            _selectedNode = item;
            FillForm(item);
            _activePairTab = "headers";
            _dialogVisible = true;
            _error = null;
        }

        private void FillForm(StressApiNode item)
        {
            _editMethod = item.Method;
            _editUri = StressTestClient.ResolveRequestUri(item.Path);
            _editRounds = 1;
            _editRequests = 100;
            _editParallelism = 200;
            _editHeaders = Clone(item.Headers);
            var token = BrowserApiClient.AuthToken;
            if (!string.IsNullOrEmpty(token) && !HasAuthorization(_editHeaders))
                _editHeaders = Prepend(_editHeaders, new StressPair { Key = "Authorization", Value = "Bearer " + token });
            _editBody = Clone(item.RequestParameters);
            _editPath = Clone(item.PathParameters);
            _editQuery = Clone(item.QueryParameters);
        }

        private void AddPair()
        {
            var next = Append(ActivePairs, new StressPair());
            SetActivePairs(next);
        }

        private void RemovePair(int index)
        {
            var source = ActivePairs;
            if (index < 0 || index >= source.Length)
                return;
            var result = new StressPair[source.Length - 1];
            var target = 0;
            for (var sourceIndex = 0; sourceIndex < source.Length; sourceIndex++)
            {
                if (sourceIndex != index)
                    result[target++] = source[sourceIndex];
            }
            SetActivePairs(result);
        }

        private void SetActivePairs(StressPair[] value)
        {
            SetPairs(_activePairTab, value);
        }

        private void UpdatePair(string tab, double rowIndex, bool updateKey, string value)
        {
            var index = (int)rowIndex;
            var source = tab == "body" ? _editBody : tab == "path" ? _editPath : tab == "query" ? _editQuery : _editHeaders;
            if (index < 0 || index >= source.Length)
                return;

            // Jazor 在这里跟踪的是数组引用。修改嵌套 DTO 后要替换整个数组，
            // 否则 Element Plus 会在 nextTick 根据旧 modelValue 恢复输入框内容。
            var next = Clone(source);
            if (updateKey)
                next[index].Key = value;
            else
                next[index].Value = value;
            SetPairs(tab, next);
        }

        private void SetPairs(string tab, StressPair[] value)
        {
            if (tab == "body") _editBody = value;
            else if (tab == "path") _editPath = value;
            else if (tab == "query") _editQuery = value;
            else _editHeaders = value;
        }

        private void Submit()
        {
            if (_submitting || !IsSuperAdmin)
                return;
            _editUri = _editUri.Trim();
            if (_editUri.Length == 0)
            {
                _error = "请求地址不能为空。";
                return;
            }
            if (_editRounds < 1 || _editRounds > 10000 || _editRequests < 1 || _editRequests > 100000 || _editParallelism < 1 || _editParallelism > 10000)
            {
                _error = "轮数、每轮请求数或最大并发量超出后端允许范围。";
                return;
            }

            _submitting = true;
            _error = null;
            StressTestClient.Run(new StressTestInput
            {
                RequestUri = _editUri,
                RequestMethod = NormalizeMethod(_editMethod),
                NumberOfRounds = _editRounds,
                NumberOfRequests = _editRequests,
                MaxDegreeOfParallelism = _editParallelism,
                Headers = ToDictionary(_editHeaders),
                PathParameters = ToDictionary(_editPath),
                QueryParameters = ToDictionary(_editQuery),
                RequestParameters = ToInputs(_editBody)
            }).Then(outcome =>
            {
                _submitting = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "接口压测失败。";
                    return;
                }
                _result = StressTestClient.ReadResult(outcome.Data);
                _dialogVisible = false;
                ElementPlus.ElMessageMembers.Success("接口压测完成");
            }, _ =>
            {
                _submitting = false;
                _error = "接口压测失败，请检查网络后重试。";
            });
        }

        private static VueDictionary<string> ToDictionary(StressPair[] source)
        {
            var result = new VueDictionary<string>();
            for (var index = 0; index < source.Length; index++)
            {
                var key = source[index].Key.Trim();
                if (key.Length > 0)
                    result[key] = source[index].Value.Trim();
            }
            return result;
        }

        private static StressKeyValueInput[] ToInputs(StressPair[] source)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index].Key.Trim().Length > 0)
                    count++;
            }
            var result = new StressKeyValueInput[count];
            var target = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var key = source[index].Key.Trim();
                if (key.Length == 0)
                    continue;
                result[target++] = new StressKeyValueInput { Key = key, Value = source[index].Value.Trim() };
            }
            return result;
        }

        private static bool HasAuthorization(StressPair[] source)
        {
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index].Key.ToLower() == "authorization")
                    return true;
            }
            return false;
        }

        private static StressPairView[] PairViews(StressPair[] source)
        {
            var result = new StressPairView[source.Length];
            for (var index = 0; index < source.Length; index++)
                result[index] = new StressPairView(source[index].Key, source[index].Value);
            return result;
        }

        private static StressPair[] Clone(StressPair[] source)
        {
            var result = new StressPair[source.Length];
            for (var index = 0; index < source.Length; index++)
                result[index] = new StressPair { Key = source[index].Key, Value = source[index].Value };
            return result;
        }

        private static StressPair[] Append(StressPair[] source, StressPair item)
        {
            var result = new StressPair[source.Length + 1];
            for (var index = 0; index < source.Length; index++) result[index] = source[index];
            result[source.Length] = item;
            return result;
        }

        private static StressPair[] Prepend(StressPair[] source, StressPair item)
        {
            var result = new StressPair[source.Length + 1];
            result[0] = item;
            for (var index = 0; index < source.Length; index++) result[index + 1] = source[index];
            return result;
        }

        private static string NormalizeMethod(string value)
        {
            var method = value.Trim().ToUpper();
            return method == "POST" || method == "PUT" || method == "DELETE" ? method : "GET";
        }

        public StressTestPageProps CreateProps() => new()
        {
            SelectedSwaggerUrl = _selectedSwaggerUrl,
            ResourceOptions = ResourceOptions(),
            Groups = VisibleGroups(),
            SelectedNodeId = _selectedNode?.Id,
            Keywords = _keywords,
            Error = _error,
            IsSuperAdmin = IsSuperAdmin,
            Busy = Busy,
            LoadingResources = _loadingResources,
            LoadingDocument = _loadingDocument,
            EditMethod = _editMethod,
            EditUri = _editUri,
            EditRounds = _editRounds,
            EditRequests = _editRequests,
            EditParallelism = _editParallelism,
            ParameterCount = _editHeaders.Length + _editBody.Length + _editPath.Length + _editQuery.Length,
            Result = _result,
            LoadResources = LoadResources,
            SwaggerChanged = SwaggerChanged,
            ChangeKeywords = value => _keywords = value ?? string.Empty,
            Search = Search,
            SelectNodeAt = SelectNodeAt,
            OpenDialogAt = OpenDialogAt,
            OpenCurrentDialog = OpenCurrentDialog,
            DialogVisible = _dialogVisible,
            ActivePairTab = _activePairTab,
            Headers = PairViews(_editHeaders),
            Body = PairViews(_editBody),
            PathPairs = PairViews(_editPath),
            Query = PairViews(_editQuery),
            Submitting = _submitting,
            SetDialogVisible = value => _dialogVisible = value,
            SetEditMethod = value => _editMethod = value,
            SetEditUri = value => _editUri = value ?? string.Empty,
            SetEditRounds = value => _editRounds = (int)(value ?? 1),
            SetEditRequests = value => _editRequests = (int)(value ?? 100),
            SetEditParallelism = value => _editParallelism = (int)(value ?? 200),
            SetActivePairTab = value => _activePairTab = value,
            AddPair = AddPair,
            RemovePair = index => RemovePair((int)index),
            UpdatePair = UpdatePair,
            Submit = Submit,
        };
    }
}

public sealed record StressTestPageProps : VueProps
{
    public required string SelectedSwaggerUrl { get; init; }
    public required ConfigurationOption[] ResourceOptions { get; init; }
    public required StressBrowseGroup[] Groups { get; init; }
    public required string? SelectedNodeId { get; init; }
    public required string Keywords { get; init; }
    public required string? Error { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool Busy { get; init; }
    public required bool LoadingResources { get; init; }
    public required bool LoadingDocument { get; init; }
    public required string EditMethod { get; init; }
    public required string EditUri { get; init; }
    public required int EditRounds { get; init; }
    public required int EditRequests { get; init; }
    public required int EditParallelism { get; init; }
    public required int ParameterCount { get; init; }
    public required StressTestResult Result { get; init; }
    public required Action LoadResources { get; init; }
    public required Action<string> SwaggerChanged { get; init; }
    public required Action<string?> ChangeKeywords { get; init; }
    public required Action Search { get; init; }
    public required Action<int, int> SelectNodeAt { get; init; }
    public required Action<int, int> OpenDialogAt { get; init; }
    public required Action OpenCurrentDialog { get; init; }
    public required bool DialogVisible { get; init; }
    public required string ActivePairTab { get; init; }
    public required StressPairView[] Headers { get; init; }
    public required StressPairView[] Body { get; init; }
    public required StressPairView[] PathPairs { get; init; }
    public required StressPairView[] Query { get; init; }
    public required bool Submitting { get; init; }
    public required Action<bool> SetDialogVisible { get; init; }
    public required Action<string> SetEditMethod { get; init; }
    public required Action<string?> SetEditUri { get; init; }
    public required Action<double?> SetEditRounds { get; init; }
    public required Action<double?> SetEditRequests { get; init; }
    public required Action<double?> SetEditParallelism { get; init; }
    public required Action<string> SetActivePairTab { get; init; }
    public required Action AddPair { get; init; }
    public required Action<double> RemovePair { get; init; }
    public required Action<string, double, bool, string> UpdatePair { get; init; }
    public required Action Submit { get; init; }
}
