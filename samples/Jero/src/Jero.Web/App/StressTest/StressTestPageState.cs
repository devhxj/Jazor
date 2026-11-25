using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Http;

namespace Jero.Web.App.StressTest;

[ECMAScriptModule("components/stress-test-page-state.js")]
public static class StressTestPageModule
{
    public static StressTestPageState Create() => new();

    public sealed class StressTestPageState : global::Jero.Web.App.IPageState<StressTestPageProps>
    {
        private StressSwaggerResource[] resources = [];
        private StressApiGroup[] groups = [];
        private StressApiNode? selectedNode;
        private StressTestResult result = new();
        private bool loadingResources;
        private bool loadingDocument;
        private bool dialogVisible;
        private bool submitting;
        private int documentRequestVersion;
        private string selectedSwaggerUrl = string.Empty;
        private string keywords = string.Empty;
        private string appliedKeywords = string.Empty;
        private string? error;

        private string editMethod = "GET";
        private string editUri = string.Empty;
        private int editRounds = 1;
        private int editRequests = 100;
        private int editParallelism = 200;
        private string activePairTab = "headers";
        private StressPair[] editHeaders = [];
        private StressPair[] editBody = [];
        private StressPair[] editPath = [];
        private StressPair[] editQuery = [];

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool Busy => loadingResources || loadingDocument;
        private StressPair[] ActivePairs => activePairTab == "body" ? editBody : activePairTab == "path" ? editPath : activePairTab == "query" ? editQuery : editHeaders;

        public void Initialize() => LoadResources();

        private void LoadResources()
        {
            if (loadingResources)
                return;
            loadingResources = true;
            error = null;
            StressTestClient.Resources().Then(outcome =>
            {
                loadingResources = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载 Swagger 分组失败。";
                    return;
                }
                resources = StressTestClient.ReadResources(outcome.Data);
                if (selectedSwaggerUrl.Length == 0 && resources.Length > 0)
                    selectedSwaggerUrl = resources[0].url;
                LoadDocument();
            }, _ =>
            {
                loadingResources = false;
                error = "加载 Swagger 分组失败，请检查网络后重试。";
            });
        }

        private void SwaggerChanged(string? value)
        {
            selectedSwaggerUrl = value ?? string.Empty;
            LoadDocument();
        }

        private void LoadDocument()
        {
            // Ignore late Swagger responses after a group switch/clear; otherwise an old request
            // can leave loading stuck or overwrite the newly selected document.
            var requestVersion = ++documentRequestVersion;
            if (selectedSwaggerUrl.Length == 0)
            {
                loadingDocument = false;
                groups = [];
                selectedNode = null;
                return;
            }
            loadingDocument = true;
            var requestUrl = selectedSwaggerUrl;
            StressTestClient.Document(requestUrl).Then(outcome =>
            {
                if (requestVersion != documentRequestVersion || requestUrl != selectedSwaggerUrl)
                    return;
                loadingDocument = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载 Swagger 文档失败。";
                    groups = [];
                    return;
                }
                groups = StressTestClient.ReadGroups(outcome.Data, string.Empty);
                appliedKeywords = keywords.Trim().ToLower();
                selectedNode = null;
                error = null;
            }, _ =>
            {
                if (requestVersion != documentRequestVersion || requestUrl != selectedSwaggerUrl)
                    return;
                loadingDocument = false;
                groups = [];
                error = "加载 Swagger 文档失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            // Apply the filter on an explicit search; preserve the selected request draft.
            appliedKeywords = keywords.Trim().ToLower();
        }

        private ConfigurationOption[] ResourceOptions()
        {
            var options = new ConfigurationOption[resources.Length];
            for (var index = 0; index < options.Length; index++)
                options[index] = new ConfigurationOption(resources[index].url, resources[index].name);
            return options;
        }

        private StressBrowseGroup[] VisibleGroups()
        {
            var visibleGroups = new List<StressBrowseGroup>();
            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var group = groups[groupIndex];
                var visibleNodes = new List<StressBrowseNode>();
                for (var nodeIndex = 0; nodeIndex < group.children.Length; nodeIndex++)
                {
                    var node = group.children[nodeIndex];
                    if (NodeVisible(node))
                        visibleNodes.Add(new StressBrowseNode(node.id, node.method, node.path, node.summary, groupIndex, nodeIndex));
                }
                // Keep the original indices so filtered rows still open the matching host draft.
                if (visibleNodes.Count > 0)
                    visibleGroups.Add(new StressBrowseGroup(group.name, group.summary, visibleNodes.ToArray()));
            }
            return visibleGroups.ToArray();
        }

        private bool NodeVisible(StressApiNode item)
        {
            if (appliedKeywords.Length == 0)
                return true;
            var searchable = (item.summary + " " + item.path + " " + item.method + " " + item.tagName).ToLower();
            return searchable.Contains(appliedKeywords);
        }

        private void SelectNode(StressApiNode item)
        {
            selectedNode = item;
            FillForm(item);
        }

        private void SelectNodeAt(int groupIndex, int nodeIndex)
        {
            if (groupIndex < 0 || groupIndex >= groups.Length || nodeIndex < 0 || nodeIndex >= groups[groupIndex].children.Length)
                return;
            SelectNode(groups[groupIndex].children[nodeIndex]);
        }

        private void OpenDialogAt(int groupIndex, int nodeIndex)
        {
            if (groupIndex < 0 || groupIndex >= groups.Length || nodeIndex < 0 || nodeIndex >= groups[groupIndex].children.Length)
                return;
            OpenEditor(groups[groupIndex].children[nodeIndex]);
        }

        private void OpenCurrentDialog()
        {
            if (selectedNode is not null)
                OpenEditor(selectedNode);
        }

        private void OpenEditor(StressApiNode item)
        {
            if (!IsSuperAdmin)
                return;
            selectedNode = item;
            FillForm(item);
            activePairTab = "headers";
            dialogVisible = true;
            error = null;
        }

        private void FillForm(StressApiNode item)
        {
            editMethod = item.method;
            editUri = StressTestClient.ResolveRequestUri(item.path);
            editRounds = 1;
            editRequests = 100;
            editParallelism = 200;
            editHeaders = Clone(item.headers);
            var token = BrowserApiClient.AuthToken;
            if (!string.IsNullOrEmpty(token) && !HasAuthorization(editHeaders))
                editHeaders = Prepend(editHeaders, new StressPair { key = "Authorization", value = "Bearer " + token });
            editBody = Clone(item.requestParameters);
            editPath = Clone(item.pathParameters);
            editQuery = Clone(item.queryParameters);
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
            SetPairs(activePairTab, value);
        }

        private void UpdatePair(string tab, double rowIndex, bool updateKey, string value)
        {
            var index = (int)rowIndex;
            var source = tab == "body" ? editBody : tab == "path" ? editPath : tab == "query" ? editQuery : editHeaders;
            if (index < 0 || index >= source.Length)
                return;

            // Jazor tracks the array root here. Replacing it is required after editing a nested DTO;
            // otherwise Element Plus receives the old modelValue and restores stale text on nextTick.
            var next = Clone(source);
            if (updateKey)
                next[index].key = value;
            else
                next[index].value = value;
            SetPairs(tab, next);
        }

        private void SetPairs(string tab, StressPair[] value)
        {
            if (tab == "body") editBody = value;
            else if (tab == "path") editPath = value;
            else if (tab == "query") editQuery = value;
            else editHeaders = value;
        }

        private void Submit()
        {
            if (submitting || !IsSuperAdmin)
                return;
            editUri = editUri.Trim();
            if (editUri.Length == 0)
            {
                error = "请求地址不能为空。";
                return;
            }
            if (editRounds < 1 || editRounds > 10000 || editRequests < 1 || editRequests > 100000 || editParallelism < 1 || editParallelism > 10000)
            {
                error = "轮数、每轮请求数或最大并发量超出后端允许范围。";
                return;
            }

            submitting = true;
            error = null;
            StressTestClient.Run(new StressTestInput
            {
                requestUri = editUri,
                requestMethod = NormalizeMethod(editMethod),
                numberOfRounds = editRounds,
                numberOfRequests = editRequests,
                maxDegreeOfParallelism = editParallelism,
                headers = ToDictionary(editHeaders),
                pathParameters = ToDictionary(editPath),
                queryParameters = ToDictionary(editQuery),
                requestParameters = ToInputs(editBody)
            }).Then(outcome =>
            {
                submitting = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "接口压测失败。";
                    return;
                }
                result = StressTestClient.ReadResult(outcome.Data);
                dialogVisible = false;
                ElementPlus.ElMessageMembers.Success("接口压测完成");
            }, _ =>
            {
                submitting = false;
                error = "接口压测失败，请检查网络后重试。";
            });
        }

        private static VueDictionary<string> ToDictionary(StressPair[] source)
        {
            var result = new VueDictionary<string>();
            for (var index = 0; index < source.Length; index++)
            {
                var key = source[index].key.Trim();
                if (key.Length > 0)
                    result[key] = source[index].value.Trim();
            }
            return result;
        }

        private static StressKeyValueInput[] ToInputs(StressPair[] source)
        {
            var count = 0;
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index].key.Trim().Length > 0)
                    count++;
            }
            var result = new StressKeyValueInput[count];
            var target = 0;
            for (var index = 0; index < source.Length; index++)
            {
                var key = source[index].key.Trim();
                if (key.Length == 0)
                    continue;
                result[target++] = new StressKeyValueInput { key = key, value = source[index].value.Trim() };
            }
            return result;
        }

        private static bool HasAuthorization(StressPair[] source)
        {
            for (var index = 0; index < source.Length; index++)
            {
                if (source[index].key.ToLower() == "authorization")
                    return true;
            }
            return false;
        }

        private static StressPairView[] PairViews(StressPair[] source)
        {
            var result = new StressPairView[source.Length];
            for (var index = 0; index < source.Length; index++)
                result[index] = new StressPairView(source[index].key, source[index].value);
            return result;
        }

        private static StressPair[] Clone(StressPair[] source)
        {
            var result = new StressPair[source.Length];
            for (var index = 0; index < source.Length; index++)
                result[index] = new StressPair { key = source[index].key, value = source[index].value };
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
            SelectedSwaggerUrl = selectedSwaggerUrl,
            ResourceOptions = ResourceOptions(),
            Groups = VisibleGroups(),
            SelectedNodeId = selectedNode?.id,
            Keywords = keywords,
            Error = error,
            IsSuperAdmin = IsSuperAdmin,
            Busy = Busy,
            LoadingResources = loadingResources,
            LoadingDocument = loadingDocument,
            EditMethod = editMethod,
            EditUri = editUri,
            EditRounds = editRounds,
            EditRequests = editRequests,
            EditParallelism = editParallelism,
            ParameterCount = editHeaders.Length + editBody.Length + editPath.Length + editQuery.Length,
            Result = result,
            LoadResources = LoadResources,
            SwaggerChanged = SwaggerChanged,
            ChangeKeywords = value => keywords = value ?? string.Empty,
            Search = Search,
            SelectNodeAt = SelectNodeAt,
            OpenDialogAt = OpenDialogAt,
            OpenCurrentDialog = OpenCurrentDialog,
            DialogVisible = dialogVisible,
            ActivePairTab = activePairTab,
            Headers = PairViews(editHeaders),
            Body = PairViews(editBody),
            PathPairs = PairViews(editPath),
            Query = PairViews(editQuery),
            Submitting = submitting,
            SetDialogVisible = value => dialogVisible = value,
            SetEditMethod = value => editMethod = value,
            SetEditUri = value => editUri = value ?? string.Empty,
            SetEditRounds = value => editRounds = (int)(value ?? 1),
            SetEditRequests = value => editRequests = (int)(value ?? 100),
            SetEditParallelism = value => editParallelism = (int)(value ?? 200),
            SetActivePairTab = value => activePairTab = value,
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
