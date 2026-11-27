using Jero.Web.Infrastructure.Api.Database;
using System.Text;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Database;

[ECMAScriptModule("components/database-visual-page-state.js")]
public static class DatabaseVisualPageModule
{
    public static DatabaseVisualPageState Create() => new();

    public sealed class DatabaseVisualPageState : global::Jero.Web.Composition.IPageState<DatabaseVisualPageProps>
    {
        private ConfigurationOption[] _databaseOptions = [];
        private DatabaseVisualView _visual = new();
        private DatabaseDiagramNode[] _graphNodes = [];
        private DatabaseDiagramRelation[] _graphRelations = [];
        private string _configId = string.Empty;
        private bool _loadingDatabases;
        private bool _loadingVisual;
        private bool _pageActive;
        private string? _error;
        private int _visualRequestVersion;

        private readonly JeroNavigation _navigation = new();


        public void Initialize()
        {
            _pageActive = true;
            OnBeforeUnmount(() =>
            {
                _pageActive = false;
                ++_visualRequestVersion;
            });
            _configId = ReadQueryConfigId();
            LoadDatabases();
            if (_configId.Length > 0)
                LoadVisual();
        }

        private void LoadDatabases()
        {
            _loadingDatabases = true;
            DatabaseClient.Databases().Then(outcome =>
            {
                if (!_pageActive)
                    return;
                _loadingDatabases = false;
                if (!outcome.Ok)
                {
                    _databaseOptions = [];
                    _error = outcome.Error ?? "加载数据库列表失败。";
                    return;
                }

                var items = DatabaseClient.ReadDatabases(outcome.Data);
                var options = new ConfigurationOption[items.Length];
                for (var index = 0; index < items.Length; index++)
                    options[index] = new ConfigurationOption(items[index].ConfigId, DatabaseLabel(items[index]));
                _databaseOptions = options;
                if (_configId.Length == 0 && options.Length > 0)
                {
                    _configId = options[0].Value;
                    LoadVisual();
                }
            }, _ =>
            {
                if (!_pageActive)
                    return;
                _loadingDatabases = false;
                _databaseOptions = [];
                _error = "加载数据库列表失败，请检查网络后重试。";
            });
        }

        private void ChangeDatabase(string? value)
        {
            _configId = value ?? string.Empty;
            LoadVisual();
        }

        private void LoadVisual()
        {
            var requestConfigId = _configId.Trim();
            var requestVersion = ++_visualRequestVersion;
            _visual = new DatabaseVisualView();
            _graphNodes = [];
            _graphRelations = [];

            if (requestConfigId.Length == 0)
            {
                _loadingVisual = false;
                return;
            }

            _loadingVisual = true;
            DatabaseClient.Visual(requestConfigId).Then(outcome =>
            {
                if (!IsCurrentRequest(requestVersion, requestConfigId))
                    return;

                _loadingVisual = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载库表可视化数据失败。";
                    return;
                }

                _visual = DatabaseClient.ReadVisual(outcome.Data);
                var diagram = BuildGraphData(_visual);
                _graphNodes = diagram.Nodes;
                _graphRelations = diagram.Relations;
                _error = null;
            }, _ =>
            {
                if (!IsCurrentRequest(requestVersion, requestConfigId))
                    return;
                _loadingVisual = false;
                _error = "加载库表可视化数据失败，请检查网络后重试。";
            });
        }

        private bool IsCurrentRequest(int requestVersion, string requestConfigId)
            => _pageActive &&
               requestVersion == _visualRequestVersion &&
               requestConfigId == _configId.Trim();

        private void BackToDatabase()
            => _navigation.NavigateTo(_navigation.BaseUri + "database");

        private string ReadQueryConfigId()
        {
            var relative = _navigation.ToBaseRelativePath(_navigation.Uri);
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
            var nodes = new DatabaseDiagramNode[source.Tables.Length];
            var nodeCount = 0;
            var anchorIds = new string[source.Columns.Length];
            var anchorCount = 0;

            for (var index = 0; index < source.Tables.Length; index++)
            {
                var table = source.Tables[index];
                var tableName = table.TableName.Trim();
                if (tableName.Length == 0 || ContainsNode(nodes, nodeCount, tableName))
                    continue;

                nodes[nodeCount++] = new DatabaseDiagramNode
                {
                    Id = tableName,
                    Text = table.TableComents.Trim().Length == 0 ? tableName : table.TableComents.Trim(),
                    Html = BuildTableHtml(source.Columns, tableName, table.TableComents),
                    X = table.X,
                    Y = table.Y
                };
            }

            var graphNodes = new DatabaseDiagramNode[nodeCount];
            for (var index = 0; index < nodeCount; index++)
                graphNodes[index] = nodes[index];

            // Jazor 当前不会把 ref 参数修改回写调用方；anchor 必须在同一作用域预计算，
            // 否则生成的 elementLines 会因 anchorCount 始终为 0 被全部过滤。
            for (var index = 0; index < source.Columns.Length; index++)
            {
                var column = source.Columns[index];
                var tableName = column.TableName.Trim();
                var columnName = column.ColumnName.Trim();
                if (tableName.Length == 0 || columnName.Length == 0 ||
                    !ContainsNode(nodes, nodeCount, tableName))
                    continue;

                var anchorId = AnchorId(tableName, columnName);
                if (!ContainsString(anchorIds, anchorCount, anchorId))
                    anchorIds[anchorCount++] = anchorId;
            }

            var lines = new DatabaseDiagramRelation[source.Relations.Length];
            var lineCount = 0;
            for (var index = 0; index < source.Relations.Length; index++)
            {
                var relation = source.Relations[index];
                var sourceTable = relation.SourceTableName.Trim();
                var sourceColumn = relation.SourceColumnName.Trim();
                var targetTable = relation.TargetTableName.Trim();
                var targetColumn = relation.TargetColumnName.Trim();
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
                    Id = lineId,
                    From = from,
                    To = to,
                    Color = relation.Type == "ONE_TO_ONE"
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
                if (columns[index].TableName.Trim() == tableName && columns[index].ColumnName.Trim().Length > 0)
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
                var columnName = column.ColumnName.Trim();
                if (column.TableName.Trim() != tableName || columnName.Length == 0)
                    continue;

                builder.Append("<tr><td><div class=\"database-rg-card__anchor\" id=\"");
                builder.Append(EscapeHtml(AnchorId(tableName, columnName)));
                builder.Append("\">");
                builder.Append(EscapeHtml(columnName));
                builder.Append("</div></td><td>");
                builder.Append(EscapeHtml(column.DataType));
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(column.DataLength));
                builder.Append("</td><td>");
                builder.Append(EscapeHtml(column.ColumnDescription));
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
                if (nodes[index].Id == id)
                    return true;
            return false;
        }

        private static bool ContainsLine(DatabaseDiagramRelation[] lines, int count, string id)
        {
            for (var index = 0; index < count; index++)
                if (lines[index].Id == id)
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
            => item.DbNickName.Length == 0 ? item.ConfigId : item.DbNickName;

        public DatabaseVisualPageProps CreateProps() => new()
        {
            ConfigId = _configId,
            DatabaseOptions = _databaseOptions,
            Nodes = _graphNodes,
            Relations = _graphRelations,
            TableCount = _visual.Tables.Length,
            ColumnCount = _visual.Columns.Length,
            RelationCount = _visual.Relations.Length,
            LoadingDatabases = _loadingDatabases,
            LoadingVisual = _loadingVisual,
            Error = _error,
            ChangeDatabase = ChangeDatabase,
            BackToDatabase = BackToDatabase,
            Refresh = LoadVisual,
            DismissError = () => _error = null,
        };
    }
}

public sealed record DatabaseVisualPageProps : VueProps
{
    public required string ConfigId { get; init; }
    public required ConfigurationOption[] DatabaseOptions { get; init; }
    public required DatabaseDiagramNode[] Nodes { get; init; }
    public required DatabaseDiagramRelation[] Relations { get; init; }
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
