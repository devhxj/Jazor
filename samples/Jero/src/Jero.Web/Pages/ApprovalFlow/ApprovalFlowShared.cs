using static ECMAScript.Global;

namespace Jero.Web.Pages.ApprovalFlow;

/// <summary>表单配置与 LogicFlow 持久化 JSON 的唯一规范化边界。</summary>
[ECMAScriptModule("components/approval-flow-shared.js")]
public static class ApprovalFlowShared
{
    public static string? Optional(string value)
    {
        var normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    public static ApprovalFormConfig ParseForm(string raw)
    {
        var normalized = raw.Trim();
        if (normalized.Length == 0)
            return new ApprovalFormConfig();

        var value = JSON.Parse(normalized);
        if (!IsObject(value))
            throw new Exception("审批表单配置必须是 JSON 对象");

        var entityName = ReadString(value!, "entityName");
        var typeName = ReadString(value!, "typeName");
        return new ApprovalFormConfig
        {
            ConfigId = ReadString(value!, "configId"),
            TableName = ReadString(value!, "tableName"),
            EntityName = entityName,
            TypeName = typeName,
            Route = PreviewRoute(entityName, typeName)
        };
    }

    public static string StringifyForm(ApprovalFormConfig value)
    {
        var configId = value.ConfigId.Trim();
        var tableName = value.TableName.Trim();
        var entityName = value.EntityName.Trim();
        var typeName = value.TypeName.Trim();

        if (configId.Length == 0)
            throw new Exception("请选择库定位器");
        if (tableName.Length == 0)
            throw new Exception("请选择数据库表");
        if (entityName.Length == 0)
            throw new Exception("实体名称不能为空");
        if (!new RegExp("^[A-Za-z][A-Za-z0-9]*$").Test(entityName))
            throw new Exception("实体名称必须为字母开头的标识符");
        if (typeName.Length == 0)
            throw new Exception("操作标识不能为空");
        if (!new RegExp("^[a-z][A-Za-z0-9-]*$").Test(typeName))
            throw new Exception("操作标识必须为小写字母开头的英文标识");

        return JSON.Stringify(new ApprovalFormPersisted
        {
            ConfigId = configId,
            TableName = tableName,
            EntityName = entityName,
            TypeName = typeName
        }, (object?)null, (Number)2) ?? "{}";
    }

    public static string PreviewRoute(string entityName, string typeName)
    {
        var entity = entityName.Trim();
        var operation = typeName.Trim();
        if (entity.Length == 0 || operation.Length == 0)
            return string.Empty;
        return entity.Substring(0, 1).ToLowerCase() + entity.Substring(1) + "/" + operation;
    }

    public static ApprovalFlowGraph CreateDefaultGraph()
        => new()
        {
            Nodes =
            [
                Node("start", "bpmn:startEvent", 120, 220, "开始", 120, 178),
                Node("approve", "bpmn:userTask", 360, 220, "审批节点", 360, 178),
                Node("end", "bpmn:endEvent", 600, 220, "结束", 600, 178)
            ],
            Edges =
            [
                Edge("edge_start_approve", "start", "approve", 140, 220, 320, 220, "提交审批", 230, 186),
                Edge("edge_approve_end", "approve", "end", 400, 220, 560, 220, "审批完成", 480, 186)
            ]
        };

    public static ApprovalFlowGraph ParseGraph(string raw)
    {
        var normalized = raw.Trim();
        return normalized.Length == 0 ? CreateDefaultGraph() : NormalizeGraph(JSON.Parse(normalized));
    }

    public static ApprovalFlowGraph NormalizeGraph(object? value)
    {
        if (!IsObject(value))
            throw new Exception("审批流程必须是 JSON 对象");

        var nodeValues = Reflect.Get(value!, "nodes") as object?[];
        var edgeValues = Reflect.Get(value!, "edges") as object?[];
        if (nodeValues is null || edgeValues is null)
            throw new Exception("审批流程必须包含 nodes 与 edges 数组");

        var nodes = new ApprovalFlowNode[nodeValues.Length];
        for (var index = 0; index < nodeValues.Length; index++)
            nodes[index] = NormalizeNode(nodeValues[index]);

        var edges = new ApprovalFlowEdge[edgeValues.Length];
        for (var index = 0; index < edgeValues.Length; index++)
            edges[index] = NormalizeEdge(edgeValues[index]);

        for (var index = 0; index < edges.Length; index++)
        {
            if (!ContainsNode(nodes, edges[index].SourceNodeId) || !ContainsNode(nodes, edges[index].TargetNodeId))
                throw new Exception("存在连接到不存在节点的流程连线");
        }

        return new ApprovalFlowGraph { Nodes = nodes, Edges = edges };
    }

    public static string StringifyGraph(object? value)
    {
        var graph = NormalizeGraph(value);
        var summary = Summarize(graph);
        if (summary.NodeCount == 0)
            throw new Exception("请先设计审批流程");
        if (summary.Issues.Length > 0)
            throw new Exception("审批流程不完整：" + Join(summary.Issues, "；"));
        return JSON.Stringify(graph, (object?)null, (Number)2) ?? "{}";
    }

    public static ApprovalFlowSummary Summarize(ApprovalFlowGraph graph)
    {
        var startCount = 0;
        var userCount = 0;
        var serviceCount = 0;
        var gatewayCount = 0;
        var endCount = 0;
        var labels = new string[graph.Nodes.Length];
        var issueBuffer = new string[graph.Nodes.Length + 3];
        var issueCount = 0;

        for (var index = 0; index < graph.Nodes.Length; index++)
        {
            var node = graph.Nodes[index];
            if (node.Type == "bpmn:startEvent") startCount++;
            else if (node.Type == "bpmn:userTask") userCount++;
            else if (node.Type == "bpmn:serviceTask") serviceCount++;
            else if (node.Type == "bpmn:exclusiveGateway") gatewayCount++;
            else if (node.Type == "bpmn:endEvent") endCount++;

            labels[index] = node.Text?.Value ?? NodeTypeLabel(node.Type);
            if (!IsConnected(graph.Edges, node.Id))
                issueBuffer[issueCount++] = "存在未连接节点：" + node.Id;
        }

        if (startCount == 0)
            issueBuffer[issueCount++] = "缺少开始节点";
        if (endCount == 0)
            issueBuffer[issueCount++] = "缺少结束节点";
        if (graph.Nodes.Length > 0 && graph.Edges.Length == 0)
            issueBuffer[issueCount++] = "流程图缺少连接关系";

        var issues = new string[issueCount];
        for (var index = 0; index < issueCount; index++)
            issues[index] = issueBuffer[index];

        return new ApprovalFlowSummary
        {
            NodeCount = graph.Nodes.Length,
            EdgeCount = graph.Edges.Length,
            StartCount = startCount,
            UserCount = userCount,
            ServiceCount = serviceCount,
            GatewayCount = gatewayCount,
            EndCount = endCount,
            Labels = labels,
            Issues = issues
        };
    }

    public static ApprovalFlowSummary SummarizeText(string raw)
        => Summarize(ParseGraph(raw));

    public static string FormatJson(string raw, string fallback)
    {
        var normalized = raw.Trim();
        if (normalized.Length == 0)
            return fallback;
        try
        {
            return JSON.Stringify(JSON.Parse(normalized), (object?)null, (Number)2) ?? fallback;
        }
        catch
        {
            return normalized;
        }
    }

    public static string NodeTypeLabel(string type)
    {
        if (type == "bpmn:startEvent") return "开始";
        if (type == "bpmn:userTask") return "审批节点";
        if (type == "bpmn:serviceTask") return "系统节点";
        if (type == "bpmn:exclusiveGateway") return "分支网关";
        if (type == "bpmn:endEvent") return "结束";
        return type;
    }

    public static string Join(string[] values, string separator)
    {
        var result = string.Empty;
        for (var index = 0; index < values.Length; index++)
            result += (index == 0 ? string.Empty : separator) + values[index];
        return result;
    }

    private static ApprovalFlowNode NormalizeNode(object? value)
    {
        if (!IsObject(value))
            throw new Exception("流程节点必须是 JSON 对象");
        var id = ReadString(value!, "id");
        var type = ReadString(value!, "type");
        if (id.Length == 0)
            throw new Exception("流程节点缺少有效 ID");
        if (!IsNodeType(type))
            throw new Exception("不支持的流程节点类型：" + type);
        var x = ReadNumber(value!, "x");
        var y = ReadNumber(value!, "y");
        return new ApprovalFlowNode
        {
            Id = id,
            Type = type,
            X = x,
            Y = y,
            Properties = new ApprovalFlowProperties(),
            Text = NormalizeText(Reflect.Get(value!, "text"), x, y - 42)
        };
    }

    private static ApprovalFlowEdge NormalizeEdge(object? value)
    {
        if (!IsObject(value))
            throw new Exception("流程连线必须是 JSON 对象");
        var id = ReadString(value!, "id");
        var type = ReadString(value!, "type");
        var source = ReadString(value!, "sourceNodeId");
        var target = ReadString(value!, "targetNodeId");
        if (id.Length == 0)
            throw new Exception("流程连线缺少有效 ID");
        if (source.Length == 0 || target.Length == 0)
            throw new Exception("流程连线必须同时配置起点和终点");
        if (type != "bpmn:sequenceFlow")
            throw new Exception("不支持的流程连线类型：" + type);

        var start = NormalizePoint(Reflect.Get(value!, "startPoint"), 0, 0);
        var end = NormalizePoint(Reflect.Get(value!, "endPoint"), start.X, start.Y);
        var rawPoints = Reflect.Get(value!, "pointsList") as object?[] ?? [];
        ApprovalFlowPoint[] points;
        if (rawPoints.Length == 0)
        {
            points = [start, end];
        }
        else
        {
            points = new ApprovalFlowPoint[rawPoints.Length];
            for (var index = 0; index < rawPoints.Length; index++)
                points[index] = NormalizePoint(rawPoints[index], index == 0 ? start.X : end.X, index == 0 ? start.Y : end.Y);
        }

        return new ApprovalFlowEdge
        {
            Id = id,
            Type = type,
            SourceNodeId = source,
            TargetNodeId = target,
            StartPoint = start,
            EndPoint = end,
            PointsList = points,
            Properties = new ApprovalFlowProperties(),
            Text = NormalizeText(Reflect.Get(value!, "text"), (start.X + end.X) / 2, (start.Y + end.Y) / 2)
        };
    }

    private static ApprovalFlowText? NormalizeText(object? value, double fallbackX, double fallbackY)
    {
        if (value is null)
            return null;
        if (TypeOf(value) == "string")
        {
            var direct = StringValue(value).Trim();
            return direct.Length == 0 ? null : new ApprovalFlowText { Value = direct, X = fallbackX, Y = fallbackY };
        }
        if (!IsObject(value))
            return null;
        var text = ReadString(value, "value");
        if (text.Length == 0)
            return null;
        return new ApprovalFlowText
        {
            Value = text,
            X = ReadNumber(value, "x", fallbackX),
            Y = ReadNumber(value, "y", fallbackY)
        };
    }

    private static ApprovalFlowPoint NormalizePoint(object? value, double fallbackX, double fallbackY)
    {
        if (!IsObject(value))
            return new ApprovalFlowPoint { X = fallbackX, Y = fallbackY };
        return new ApprovalFlowPoint
        {
            X = ReadNumber(value!, "x", fallbackX),
            Y = ReadNumber(value!, "y", fallbackY)
        };
    }

    private static ApprovalFlowNode Node(string id, string type, double x, double y, string text, double textX, double textY)
        => new()
        {
            Id = id,
            Type = type,
            X = x,
            Y = y,
            Properties = new ApprovalFlowProperties(),
            Text = new ApprovalFlowText { Value = text, X = textX, Y = textY }
        };

    private static ApprovalFlowEdge Edge(
        string id,
        string source,
        string target,
        double startX,
        double startY,
        double endX,
        double endY,
        string text,
        double textX,
        double textY)
    {
        var start = new ApprovalFlowPoint { X = startX, Y = startY };
        var end = new ApprovalFlowPoint { X = endX, Y = endY };
        return new ApprovalFlowEdge
        {
            Id = id,
            Type = "bpmn:sequenceFlow",
            SourceNodeId = source,
            TargetNodeId = target,
            StartPoint = start,
            EndPoint = end,
            PointsList = [start, end],
            Properties = new ApprovalFlowProperties(),
            Text = new ApprovalFlowText { Value = text, X = textX, Y = textY }
        };
    }

    private static bool ContainsNode(ApprovalFlowNode[] nodes, string id)
    {
        for (var index = 0; index < nodes.Length; index++)
            if (nodes[index].Id == id)
                return true;
        return false;
    }

    private static bool IsConnected(ApprovalFlowEdge[] edges, string id)
    {
        for (var index = 0; index < edges.Length; index++)
            if (edges[index].SourceNodeId == id || edges[index].TargetNodeId == id)
                return true;
        return false;
    }

    private static bool IsNodeType(string type)
        => type == "bpmn:startEvent" || type == "bpmn:userTask" ||
           type == "bpmn:serviceTask" || type == "bpmn:exclusiveGateway" ||
           type == "bpmn:endEvent";

    private static bool IsObject(object? value)
        => value is not null && value is not object?[] && TypeOf(value) == "object";

    private static string ReadString(object value, string key)
    {
        var raw = Reflect.Get(value, key);
        return raw is null ? string.Empty : StringValue(raw).Trim();
    }

    private static double ReadNumber(object value, string key, double fallback = 0)
    {
        var raw = Reflect.Get(value, key);
        return raw is null ? fallback : NumberValue(raw);
    }
}

public sealed record ApprovalFormConfig : VueProps
{
    public string ConfigId { get; init; } = string.Empty;
    public string TableName { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
}

public sealed record ApprovalFormPersisted : VueProps
{
    [ECMAScriptName("configId")]
    public string ConfigId { get; init; } = string.Empty;
    [ECMAScriptName("tableName")]
    public string TableName { get; init; } = string.Empty;
    [ECMAScriptName("entityName")]
    public string EntityName { get; init; } = string.Empty;
    [ECMAScriptName("typeName")]
    public string TypeName { get; init; } = string.Empty;
}

public sealed record ApprovalFlowGraph : VueProps
{
    [ECMAScriptName("nodes")]
    public ApprovalFlowNode[] Nodes { get; init; } = [];
    [ECMAScriptName("edges")]
    public ApprovalFlowEdge[] Edges { get; init; } = [];
}

public sealed record ApprovalFlowNode : VueProps
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public string Type { get; init; } = string.Empty;
    [ECMAScriptName("x")]
    public double X { get; init; }
    [ECMAScriptName("y")]
    public double Y { get; init; }
    [ECMAScriptName("properties")]
    public ApprovalFlowProperties Properties { get; init; } = new();
    [ECMAScriptName("text")]
    public ApprovalFlowText? Text { get; init; }
}

public sealed record ApprovalFlowEdge : VueProps
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("type")]
    public string Type { get; init; } = string.Empty;
    [ECMAScriptName("sourceNodeId")]
    public string SourceNodeId { get; init; } = string.Empty;
    [ECMAScriptName("targetNodeId")]
    public string TargetNodeId { get; init; } = string.Empty;
    [ECMAScriptName("startPoint")]
    public ApprovalFlowPoint StartPoint { get; init; } = new();
    [ECMAScriptName("endPoint")]
    public ApprovalFlowPoint EndPoint { get; init; } = new();
    [ECMAScriptName("pointsList")]
    public ApprovalFlowPoint[] PointsList { get; init; } = [];
    [ECMAScriptName("properties")]
    public ApprovalFlowProperties Properties { get; init; } = new();
    [ECMAScriptName("text")]
    public ApprovalFlowText? Text { get; init; }
}

public sealed record ApprovalFlowPoint : VueProps
{
    [ECMAScriptName("x")]
    public double X { get; init; }
    [ECMAScriptName("y")]
    public double Y { get; init; }
}

public sealed record ApprovalFlowText : VueProps
{
    [ECMAScriptName("value")]
    public string Value { get; init; } = string.Empty;
    [ECMAScriptName("x")]
    public double X { get; init; }
    [ECMAScriptName("y")]
    public double Y { get; init; }
}

public sealed record ApprovalFlowProperties : VueProps;

public sealed record ApprovalFlowSummary : VueProps
{
    public int NodeCount { get; init; }
    public int EdgeCount { get; init; }
    public int StartCount { get; init; }
    public int UserCount { get; init; }
    public int ServiceCount { get; init; }
    public int GatewayCount { get; init; }
    public int EndCount { get; init; }
    public string[] Labels { get; init; } = [];
    public string[] Issues { get; init; } = [];
}
