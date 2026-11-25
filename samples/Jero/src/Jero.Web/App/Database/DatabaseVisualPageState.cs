using System.Text;
using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Database;

[ECMAScriptModule("components/database-visual-page-state.js")]
public static class DatabaseVisualPageModule
{
    public static DatabaseVisualPageState Create(DatabaseGraphActions graph) => new(graph);

    public sealed class DatabaseVisualPageState : global::Jero.Web.App.IPageState<DatabaseVisualPageProps>
    {
        private ConfigurationOption[] databaseOptions = [];
        private DatabaseVisualView visual = new();
        private DatabaseDiagramNode[] graphNodes = [];
        private DatabaseDiagramRelation[] graphRelations = [];
        private string configId = string.Empty;
        private bool loadingDatabases;
        private bool loadingVisual;
        private bool pageActive;
        private string? error;
        private int visualRequestVersion;

        private readonly DatabaseGraphActions graph;
        private readonly JeroNavigation Navigation = new();

        public DatabaseVisualPageState(DatabaseGraphActions graph) => this.graph = graph;

        public void Initialize()
        {
            pageActive = true;
            OnBeforeUnmount(() =>
            {
                pageActive = false;
                ++visualRequestVersion;
            });
            configId = ReadQueryConfigId();
            LoadDatabases();
            if (configId.Length > 0)
                LoadVisual();
        }

        private void LoadDatabases()
        {
            loadingDatabases = true;
            DatabaseClient.Databases().Then(outcome =>
            {
                if (!pageActive)
                    return;
                loadingDatabases = false;
                if (!outcome.Ok)
                {
                    databaseOptions = [];
                    error = outcome.Error ?? "加载数据库列表失败。";
                    return;
                }

                var items = DatabaseClient.ReadDatabases(outcome.Data);
                var options = new ConfigurationOption[items.Length];
                for (var index = 0; index < items.Length; index++)
                    options[index] = new ConfigurationOption(items[index].configId, DatabaseLabel(items[index]));
                databaseOptions = options;
                if (configId.Length == 0 && options.Length > 0)
                {
                    configId = options[0].value;
                    LoadVisual();
                }
            }, _ =>
            {
                if (!pageActive)
                    return;
                loadingDatabases = false;
                databaseOptions = [];
                error = "加载数据库列表失败，请检查网络后重试。";
            });
        }

        private void ChangeDatabase(string? value)
        {
            configId = value ?? string.Empty;
            LoadVisual();
        }

        private void LoadVisual()
        {
            var requestConfigId = configId.Trim();
            var requestVersion = ++visualRequestVersion;
            visual = new DatabaseVisualView();
            graphNodes = [];
            graphRelations = [];

            if (requestConfigId.Length == 0)
            {
                loadingVisual = false;
                return;
            }

            loadingVisual = true;
            DatabaseClient.Visual(requestConfigId).Then(outcome =>
            {
                if (!IsCurrentRequest(requestVersion, requestConfigId))
                    return;

                loadingVisual = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载库表可视化数据失败。";
                    return;
                }

                visual = DatabaseClient.ReadVisual(outcome.Data);
                var diagram = BuildGraphData(visual);
                graphNodes = diagram.Nodes;
                graphRelations = diagram.Relations;
                error = null;
            }, _ =>
            {
                if (!IsCurrentRequest(requestVersion, requestConfigId))
                    return;
                loadingVisual = false;
                error = "加载库表可视化数据失败，请检查网络后重试。";
            });
        }

        private bool IsCurrentRequest(int requestVersion, string requestConfigId)
            => pageActive &&
               requestVersion == visualRequestVersion &&
               requestConfigId == configId.Trim();

        private void BackToDatabase()
            => Navigation.NavigateTo(Navigation.BaseUri + "database");

        private string ReadQueryConfigId()
        {
            var relative = Navigation.ToBaseRelativePath(Navigation.Uri);
            var marker = "configId=";
            var markerIndex = relative.IndexOf(marker);
            if (markerIndex < 0)
                return string.Empty;

            var valueStart = markerIndex + marker.Length;
            var valueEnd = relative.IndexOf('&', valueStart);
            var encoded = valueEnd < 0
                ? relative.Substring(valueStart)
                : relative.Substring(valueStart, valueEnd - valueStart);
            return encoded.Length == 0 ? string.Empty : DecodeURIComponent(encoded);
        }

        private static DatabaseDiagram BuildGraphData(DatabaseVisualView source)
        {
            var nodes = new DatabaseDiagramNode[source.tables.Length];
            var nodeCount = 0;
            var anchorIds = new string[source.columns.Length];
            var anchorCount = 0;

            for (var index = 0; index < source.tables.Length; index++)
            {
                var table = source.tables[index];
                var tableName = table.tableName.Trim();
                if (tableName.Length == 0 || ContainsNode(nodes, nodeCount, tableName))
                    continue;

                nodes[nodeCount++] = new DatabaseDiagramNode
                {
                    id = tableName,
                    text = table.tableComents.Trim().Length == 0 ? tableName : table.tableComents.Trim(),
                    html = BuildTableHtml(source.columns, tableName, table.tableComents),
                    x = table.x,
                    y = table.y
                };
            }

            var graphNodes = new DatabaseDiagramNode[nodeCount];
            for (var index = 0; index < nodeCount; index++)
                graphNodes[index] = nodes[index];

            // Jazor 当前不会把 ref 参数修改回写调用方；anchor 必须在同一作用域预计算，
            // 否则生成的 elementLines 会因 anchorCount 始终为 0 被全部过滤。
            for (var index = 0; index < source.columns.Length; index++)
            {
                var column = source.columns[index];
                var tableName = column.tableName.Trim();
                var columnName = column.columnName.Trim();
                if (tableName.Length == 0 || columnName.Length == 0 ||
                    !ContainsNode(nodes, nodeCount, tableName))
                    continue;

                var anchorId = AnchorId(tableName, columnName);
                if (!ContainsString(anchorIds, anchorCount, anchorId))
                    anchorIds[anchorCount++] = anchorId;
            }

            var lines = new DatabaseDiagramRelation[source.relations.Length];
            var lineCount = 0;
            for (var index = 0; index < source.relations.Length; index++)
            {
                var relation = source.relations[index];
                var sourceTable = relation.sourceTableName.Trim();
                var sourceColumn = relation.sourceColumnName.Trim();
                var targetTable = relation.targetTableName.Trim();
                var targetColumn = relation.targetColumnName.Trim();
                if (sourceTable.Length == 0 || sourceColumn.Length == 0 ||
                    targetTable.Length == 0 || targetColumn.Length == 0)
                    continue;

                var from = AnchorId(sourceTable, sourceColumn);
                var to = AnchorId(targetTable, targetColumn);
                var lineId = from + "->" + to;
                if (!ContainsString(anchorIds, anchorCount, from) ||
                    !ContainsString(anchorIds, anchorCount, to) ||
                    ContainsLine(lines, lineCount, lineId))
                    continue;

                lines[lineCount++] = new DatabaseDiagramRelation
                {
                    id = lineId,
                    from = from,
                    to = to,
                    color = relation.type == "ONE_TO_ONE"
                        ? "rgba(255, 0, 0, 0.5)"
                        : "rgba(0, 255, 0, 0.5)"
                };
            }

            var elementLines = new DatabaseDiagramRelation[lineCount];
            for (var index = 0; index < lineCount; index++)
                elementLines[index] = lines[index];

            return new DatabaseDiagram(graphNodes, elementLines);
        }

        private static string BuildTableHtml(
            DatabaseVisualColumnView[] columns,
            string tableName,
            string tableComments)
        {
            var columnCount = 0;
            for (var index = 0; index < columns.Length; index++)
                if (columns[index].tableName.Trim() == tableName && columns[index].columnName.Trim().Length > 0)
                    columnCount++;

            var title = tableComments.Trim().Length == 0 ? tableName : tableComments.Trim();
            var builder = new StringBuilder();
            builder.Append("<div class=\"database-rg-card\"><div class=\"database-rg-card__title\">");
            builder.Append(EscapeHtml(title));
            builder.Append(" - 【");
            builder.Append(columnCount.ToString());
            builder.Append("列】</div><table class=\"database-rg-card__table\"><thead><tr><th>列名</th><th>类型</th><th>长度</th><th>描述</th></tr></thead><tbody>");

            for (var index = 0; index < columns.Length; index++)
            {
                var column = columns[index];
                var columnName = column.columnName.Trim();
                if (column.tableName.Trim() != tableName || columnName.Length == 0)
                    continue;

                builder.Append("<tr><td><div class=\"database-rg-card__anchor\" id=\"");
                builder.Append(EscapeHtml(AnchorId(tableName, columnName)));
                builder.Append("\">");
                builder.Append(EscapeHtml(columnName));
                builder.Append("</div></td><td>");
                builder.Append(EscapeHtml(column.dataType));
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(column.dataLength));
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(column.columnDescription));
                builder.Append("</td></tr>");
            }

            builder.Append("</tbody></table></div>");
            return builder.ToString();
        }

        private static string AnchorId(string tableName, string columnName)
            => "dbcol-" + EncodeURIComponent(tableName) + "-" + EncodeURIComponent(columnName);

        private static bool ContainsNode(DatabaseDiagramNode[] nodes, int count, string id)
        {
            for (var index = 0; index < count; index++)
                if (nodes[index].id == id)
                    return true;
            return false;
        }

        private static bool ContainsLine(DatabaseDiagramRelation[] lines, int count, string id)
        {
            for (var index = 0; index < count; index++)
                if (lines[index].id == id)
                    return true;
            return false;
        }

        private static bool ContainsString(string[] values, int count, string value)
        {
            for (var index = 0; index < count; index++)
                if (values[index] == value)
                    return true;
            return false;
        }

        private static string EscapeHtml(string value)
            => value.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");

        private static string DatabaseLabel(DatabaseOptionView item)
            => item.dbNickName.Length == 0 ? item.configId : item.dbNickName;

        public DatabaseVisualPageProps CreateProps() => new()
        {
            ConfigId = configId,
            DatabaseOptions = databaseOptions,
            Nodes = graphNodes,
            Relations = graphRelations,
            Graph = graph,
            TableCount = visual.tables.Length,
            ColumnCount = visual.columns.Length,
            RelationCount = visual.relations.Length,
            LoadingDatabases = loadingDatabases,
            LoadingVisual = loadingVisual,
            Error = error,
            ChangeDatabase = ChangeDatabase,
            BackToDatabase = BackToDatabase,
            Refresh = LoadVisual,
            DismissError = () => error = null,
        };
    }
}

public sealed record DatabaseVisualPageProps : VueProps
{
    public required string ConfigId { get; init; }
    public required ConfigurationOption[] DatabaseOptions { get; init; }
    public required DatabaseDiagramNode[] Nodes { get; init; }
    public required DatabaseDiagramRelation[] Relations { get; init; }
    public required DatabaseGraphActions Graph { get; init; }
    public required int TableCount { get; init; }
    public required int ColumnCount { get; init; }
    public required int RelationCount { get; init; }
    public required bool LoadingDatabases { get; init; }
    public required bool LoadingVisual { get; init; }
    public required string? Error { get; init; }
    public required Action<string> ChangeDatabase { get; init; }
    public required Action BackToDatabase { get; init; }
    public required Action Refresh { get; init; }
    public required Action DismissError { get; init; }
}
