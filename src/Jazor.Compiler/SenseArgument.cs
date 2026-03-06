using Acornima.Ast;
using System.Collections.Generic;

namespace Jazor.Compiler;

/// <summary>
/// 语义上下文参数，传递给 Visit 方法。
/// 包含语义场景标识和依赖项收集（变量声明、导入管理）。
/// </summary>
public record struct SenseArgument
{
    /// <summary>语义场景标识</summary>
    public Sense Sense { get; init; }

    /// <summary>模式匹配输入表达式</summary>
    public Expression? PatternInput { get; init; }

    /// <summary>Catch 子句异常参数名</summary>
    public string? CatchExceptionVar { get; init; }

    /// <summary>Switch 表达式输入变量名</summary>
    public string? SwitchExpressionVar { get; init; }

    // ===== 依赖项收集（原 WalkerArgument 功能，直接内联）=====
    private readonly Dictionary<string, VariableDeclarator>? _declarators;
    private readonly Dictionary<string, List<ImportDeclarationSpecifier>>? _specifiers;

    /// <summary>默认参数</summary>
    public static SenseArgument Default => new();

    /// <summary>无参构造函数，初始化依赖项字典</summary>
    public SenseArgument()
    {
        Sense = Sense.Any;
        PatternInput = null;
        CatchExceptionVar = null;
        SwitchExpressionVar = null;
        _declarators = [];
        _specifiers = [];
    }

    /// <summary>完整构造函数</summary>
    public SenseArgument(
        Sense Sense = Sense.Any,
        Expression? PatternInput = null,
        string? CatchExceptionVar = null,
        string? SwitchExpressionVar = null)
    {
        this.Sense = Sense;
        this.PatternInput = PatternInput;
        this.CatchExceptionVar = CatchExceptionVar;
        this.SwitchExpressionVar = SwitchExpressionVar;
        _declarators = [];
        _specifiers = [];
    }

    /// <summary>内部构造函数（用于 WithNewScope，共享 specifiers）</summary>
    private SenseArgument(
        Sense sense,
        Expression? patternInput,
        string? catchExceptionVar,
        string? switchExpressionVar,
        Dictionary<string, VariableDeclarator>? declarators,
        Dictionary<string, List<ImportDeclarationSpecifier>>? specifiers)
    {
        Sense = sense;
        PatternInput = patternInput;
        CatchExceptionVar = catchExceptionVar;
        SwitchExpressionVar = switchExpressionVar;
        _declarators = declarators;
        _specifiers = specifiers;
    }

    // ===== 依赖项状态检查 =====
    /// <summary>是否包含变量声明</summary>
    public bool HasVarDeclarator => _declarators?.Count > 0;

    /// <summary>是否包含导入声明规范</summary>
    public bool HasVarImportDeclarationSpecifier => _specifiers?.Count > 0;

    // ===== Sense 变更 =====
    /// <summary>创建新实例，设置 Sense</summary>
    public SenseArgument With(Sense sense)
        => new(sense, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers);

    // ===== 作用域隔离 =====
    /// <summary>
    /// 创建新实例，用于块级作用域隔离。
    /// 共享导入字典，创建新的变量声明字典。
    /// </summary>
    public SenseArgument WithNewScope()
        => new(Sense, PatternInput, CatchExceptionVar, SwitchExpressionVar, [], _specifiers);

    // ===== 模式匹配上下文 =====
    /// <summary>设置模式匹配输入表达式</summary>
    public SenseArgument WithPatternInput(Expression? input)
        => new(Sense, input, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers);

    // ===== 异常处理上下文 =====
    /// <summary>设置 Catch 异常参数名</summary>
    public SenseArgument WithCatchVar(string? varName)
        => new(Sense, PatternInput, varName, SwitchExpressionVar, _declarators, _specifiers);

    // ===== Switch 表达式上下文 =====
    /// <summary>设置 Switch 表达式变量名</summary>
    public SenseArgument WithSwitchVar(string? varName)
        => new(Sense, PatternInput, CatchExceptionVar, varName, _declarators, _specifiers);

    // ===== 组合设置 =====
    /// <summary>设置 Sense 和 PatternInput</summary>
    public SenseArgument With(Sense sense, Expression patternInput)
        => new(sense, patternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers);

    // ===== 依赖项操作 =====
    /// <summary>
    /// 添加变量声明，根据深度和名称生成唯一键，防止重复添加。
    /// </summary>
    public void AddVarDeclarator(VariableDeclarator declarator, int depth)
    {
        if (_declarators is null) return;
        var name = declarator.Id is Identifier identifier
            ? identifier.Name
            : declarator.Id.ToECMAScript();
        var key = $"{depth}:{name}";
        if (!_declarators.ContainsKey(key))
            _declarators.Add(key, declarator);
    }

    /// <summary>
    /// 添加导入声明规范，根据模块路径进行分组存储。
    /// </summary>
    public void MergeImportSpecifier(string modulePath, ImportDeclarationSpecifier specifier)
    {
        if (_specifiers is null) return;
        if (_specifiers.TryGetValue(modulePath, out var list))
            list.Add(specifier);
        else
            _specifiers.Add(modulePath, [specifier]);
    }

    /// <summary>
    /// 刷新并获取当前累积的变量声明列表，然后清空内部存储。
    /// </summary>
    public NodeList<VariableDeclarator> FlushVarDeclarator()
    {
        if (_declarators is null || _declarators.Count == 0)
            return NodeList.From<VariableDeclarator>();
        var list = NodeList.From(_declarators.Values);
        _declarators.Clear();
        return list;
    }
}
