using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Linq;

namespace Jazor.Compiler;

/// <summary>
/// 语义上下文参数，传递给 Visit 方法。
/// 包含语义场景标识和依赖项收集（变量声明、导入管理）。
/// </summary>
/// <remarks>
/// SenseArgument 是值类型，但其中的收集字典会在同一发射作用域内共享。
/// WithNewScope 只隔离变量声明集合，保留导入集合和导入绑定，以保证嵌套 lowering 不会
/// 重复生成或重新命名同一模块导入。
/// </remarks>
public record struct SenseArgument
{
    /// <summary>语义场景标识</summary>
    public Sense Sense { get; init; }

    /// <summary>
    /// 是否在最终模块输出阶段启用导入别名。
    /// 仅在真正会把导入 hoist 到模块顶层时才需要开启，
    /// 这样可以避免普通 walker 直测被内部别名噪音污染。
    /// </summary>
    public bool UseImportAliases { get; init; }

    /// <summary>模式匹配输入表达式</summary>
    public Expression? PatternInput { get; init; }

    /// <summary>Catch 子句异常参数名</summary>
    public string? CatchExceptionVar { get; init; }

    /// <summary>Switch 表达式输入变量名</summary>
    public string? SwitchExpressionVar { get; init; }

    internal EmissionScopeContext? ScopeContext { get; init; }

    // ===== 依赖项收集（原 WalkerArgument 功能，直接内联）=====
    private readonly Dictionary<VariableDeclaratorKey, VariableDeclarator>? _declarators;
    private readonly Dictionary<string, List<ImportDeclarationSpecifier>>? _specifiers;
    private readonly Dictionary<string, string>? _importBindings;
    private readonly Dictionary<string, string>? _importLocalBindings;
    private readonly HashSet<string>? _reservedImportNames;
    private readonly string? _currentModuleImportPath;
    private readonly HashSet<string>? _currentModuleBindings;

    /// <summary>默认参数</summary>
    public static SenseArgument Default => new();

    /// <summary>无参构造函数，初始化依赖项字典</summary>
    public SenseArgument()
    {
        Sense = Sense.Any;
        UseImportAliases = false;
        PatternInput = null;
        CatchExceptionVar = null;
        SwitchExpressionVar = null;
        _declarators = [];
        _specifiers = [];
        _importBindings = [];
        _importLocalBindings = [];
        _reservedImportNames = [];
        _currentModuleImportPath = null;
        _currentModuleBindings = null;
    }

    /// <summary>完整构造函数</summary>
    public SenseArgument(
        Sense Sense = Sense.Any,
        bool UseImportAliases = false,
        Expression? PatternInput = null,
        string? CatchExceptionVar = null,
        string? SwitchExpressionVar = null)
    {
        this.Sense = Sense;
        this.UseImportAliases = UseImportAliases;
        this.PatternInput = PatternInput;
        this.CatchExceptionVar = CatchExceptionVar;
        this.SwitchExpressionVar = SwitchExpressionVar;
        ScopeContext = null;
        _declarators = [];
        _specifiers = [];
        _importBindings = [];
        _importLocalBindings = [];
        _reservedImportNames = [];
        _currentModuleImportPath = null;
        _currentModuleBindings = null;
    }

    /// <summary>内部构造函数（用于 WithNewScope，共享 specifiers）</summary>
    private SenseArgument(
        Sense sense,
        bool useImportAliases,
        Expression? patternInput,
        string? catchExceptionVar,
        string? switchExpressionVar,
        Dictionary<VariableDeclaratorKey, VariableDeclarator>? declarators,
        Dictionary<string, List<ImportDeclarationSpecifier>>? specifiers,
        Dictionary<string, string>? importBindings,
        Dictionary<string, string>? importLocalBindings,
        HashSet<string>? reservedImportNames,
        EmissionScopeContext? scopeContext,
        string? currentModuleImportPath,
        HashSet<string>? currentModuleBindings)
    {
        Sense = sense;
        UseImportAliases = useImportAliases;
        PatternInput = patternInput;
        CatchExceptionVar = catchExceptionVar;
        SwitchExpressionVar = switchExpressionVar;
        ScopeContext = scopeContext;
        _declarators = declarators;
        _specifiers = specifiers;
        _importBindings = importBindings;
        _importLocalBindings = importLocalBindings;
        _reservedImportNames = reservedImportNames;
        _currentModuleImportPath = currentModuleImportPath;
        _currentModuleBindings = currentModuleBindings;
    }

    // ===== 依赖项状态检查 =====
    /// <summary>是否包含变量声明</summary>
    public bool HasVarDeclarator => _declarators?.Count > 0;

    /// <summary>是否包含导入声明规范</summary>
    public bool HasVarImportDeclarationSpecifier => _specifiers?.Count > 0;

    // ===== Sense 变更 =====
    /// <summary>创建新实例，设置 Sense</summary>
    public SenseArgument With(Sense sense)
        => new(sense, UseImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings);

    // ===== 作用域隔离 =====
    /// <summary>
    /// 创建新实例，用于块级作用域隔离。
    /// 共享导入字典，创建新的变量声明字典。
    /// </summary>
    public SenseArgument WithNewScope()
        => new(Sense, UseImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, [], _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings);

    internal SenseArgument WithScope(EmissionScopeContext scopeContext)
        => new(Sense, UseImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, scopeContext, _currentModuleImportPath, _currentModuleBindings);

    internal SenseArgument EnterScope(IOperation anchor, ScopeSite site)
    {
        if (anchor is null)
            throw new InvalidOperationException("Jazor 无法进入空的发射作用域。");

        if (ScopeContext is null)
            throw new InvalidOperationException($"Jazor 无法为 {anchor.Kind} 创建子作用域，因为当前上下文缺少发射作用域。");

        return new(
            Sense,
            UseImportAliases,
            PatternInput,
            CatchExceptionVar,
            SwitchExpressionVar,
            [],
            _specifiers,
            _importBindings,
            _importLocalBindings,
            _reservedImportNames,
            ScopeContext.Enter(anchor, site),
            _currentModuleImportPath,
            _currentModuleBindings);
    }

    internal SenseArgument EnterEmissionScope(IOperation anchor, ScopeSite site)
    {
        if (anchor is null)
            throw new InvalidOperationException("Jazor 无法进入空的发射作用域。");

        if (ScopeContext is null)
            throw new InvalidOperationException($"Jazor 无法为 {anchor.Kind} 创建发射作用域，因为当前上下文缺少父作用域。");

        return new(
            Sense,
            UseImportAliases,
            PatternInput,
            CatchExceptionVar,
            SwitchExpressionVar,
            [],
            _specifiers,
            _importBindings,
            _importLocalBindings,
            _reservedImportNames,
            ScopeContext.Enter(anchor, site),
            _currentModuleImportPath,
            _currentModuleBindings);
    }

    internal string AllocateName(LoweringNameOwner owner, LoweringSite site)
    {
        if (ScopeContext is null)
            throw new InvalidOperationException("Jazor 无法分配稳定名称，因为当前上下文缺少发射作用域。");

        return ScopeContext.Allocate(owner, site);
    }

    // ===== 模式匹配上下文 =====
    /// <summary>设置模式匹配输入表达式</summary>
    public SenseArgument WithPatternInput(Expression? input)
        => new(Sense, UseImportAliases, input, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings);

    // ===== 异常处理上下文 =====
    /// <summary>设置 Catch 异常参数名</summary>
    public SenseArgument WithCatchVar(string? varName)
        => new(Sense, UseImportAliases, PatternInput, varName, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings);

    // ===== Switch 表达式上下文 =====
    /// <summary>设置 Switch 表达式变量名</summary>
    public SenseArgument WithSwitchVar(string? varName)
        => new(Sense, UseImportAliases, PatternInput, CatchExceptionVar, varName, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings);

    // ===== 组合设置 =====
    /// <summary>设置 Sense 和 PatternInput</summary>
    public SenseArgument With(Sense sense, Expression patternInput)
        => new(sense, UseImportAliases, patternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings);

    public SenseArgument WithImportAliases(bool useImportAliases = true)
        => new(Sense, useImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings);

    public SenseArgument WithImportContext(
        Dictionary<string, string> importBindings,
        Dictionary<string, string> importLocalBindings,
        HashSet<string> reservedImportNames,
        string? currentModuleImportPath,
        HashSet<string> currentModuleBindings)
        => new(Sense, UseImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, importBindings, importLocalBindings, reservedImportNames, ScopeContext, currentModuleImportPath, currentModuleBindings);

    // ===== 依赖项操作 =====
    /// <summary>
    /// 添加变量声明，根据深度和名称生成唯一键，防止重复添加。
    /// </summary>
    public void AddVarDeclarator(VariableDeclarator declarator, int depth)
    {
        if (_declarators is null) return;
        if (declarator.Id is not Identifier identifier)
        {
            throw new NotSupportedException(
                "Collected JavaScript variable declarators require an identifier binding, but received '" +
                declarator.Id.Type + "'.");
        }

        var key = new VariableDeclaratorKey(depth, identifier.Name);
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
    /// 为模块导入返回一个稳定的内部绑定名。
    /// 当前策略是“模块级保守判定”：
    /// 只要整个模块里已经存在同名声明，或已有其它导入占用了该名字，
    /// 就为当前导入分配一个稳定的别名；否则保留原始导入名。
    /// 这样做不会逐使用点分析词法可见性，但可以保证输出稳定、决策一致，
    /// 也避免在不同 walker 路径里出现同一导入被重复分配不同本地名的情况。
    /// </summary>
    public Identifier BindImportSpecifier(string? modulePath, string importedName)
    {
        if (string.IsNullOrWhiteSpace(importedName))
            return new Identifier(importedName ?? string.Empty);

        if (string.IsNullOrWhiteSpace(modulePath))
            return new Identifier(importedName);

        if (string.Equals(
                ECMAScriptModulePath.NormalizeImportSpecifier(modulePath!),
                _currentModuleImportPath,
                System.StringComparison.Ordinal))
        {
            if (_currentModuleBindings?.Contains(importedName) == true)
                return new Identifier(importedName);
        }

        if (_specifiers is null || _importBindings is null)
            return new Identifier(importedName);

        var requiresAlias = !JavaScriptAstFactory.IsJavaScriptBindingIdentifier(importedName);
        if (!UseImportAliases && !requiresAlias)
        {
            MergeImportSpecifier(modulePath!, new ImportSpecifier(new Identifier(importedName)));
            return new Identifier(importedName);
        }

        var key = $"{modulePath}\0{importedName}";
        if (_importBindings.TryGetValue(key, out var localName))
            return new Identifier(localName);

        var preferRawImportName =
            !requiresAlias &&
            (_reservedImportNames is null || !_reservedImportNames.Contains(importedName)) &&
            (_importLocalBindings is null || !_importLocalBindings.TryGetValue(importedName, out var existingKey) || existingKey == key);

        if (preferRawImportName)
        {
            localName = importedName;
            MergeImportSpecifier(modulePath!, new ImportSpecifier(new Identifier(importedName)));
        }
        else
        {
            localName = AllocateImportAlias(key);
            var specifier = CreateAliasedImportSpecifier(importedName, localName);
            if (_specifiers.TryGetValue(modulePath!, out var list))
                list.Add(specifier);
            else
                _specifiers.Add(modulePath!, [specifier]);
        }

        if (_importLocalBindings is not null)
            _importLocalBindings[localName] = key;

        _importBindings.Add(key, localName);
        return new Identifier(localName);
    }

    private string AllocateImportAlias(string key)
    {
        var prefix = $"i${Format.HashName(key).TrimStart('_')}";
        for (var suffix = 0; ; suffix++)
        {
            var candidate = suffix == 0 ? prefix : prefix + suffix;
            // Import aliases share module scope with explicitly configured exports. A hash avoids
            // ordinary collisions, but it is still a valid user-authored JavaScript binding.
            if (_reservedImportNames?.Contains(candidate) == true ||
                _importLocalBindings?.ContainsKey(candidate) == true)
            {
                continue;
            }

            return candidate;
        }
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

    /// <summary>
    /// 刷新并获取当前累积的导入声明分组，然后清空内部存储。
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, NodeList<ImportDeclarationSpecifier>>> FlushImportSpecifiers()
    {
        if (_specifiers is null || _specifiers.Count == 0)
            return [];

        var result = new List<KeyValuePair<string, NodeList<ImportDeclarationSpecifier>>>(_specifiers.Count);
        foreach (var pair in _specifiers)
        {
            var specifiers = ImportDeclarationFactory.NormalizeSpecifiers(pair.Value);
            result.Add(new KeyValuePair<string, NodeList<ImportDeclarationSpecifier>>(pair.Key, NodeList.From(specifiers)));
        }

        _specifiers.Clear();
        return result;
    }

    private static ImportDeclarationSpecifier CreateAliasedImportSpecifier(string importedName, string localName)
    {
        if (string.Equals(importedName, "default", System.StringComparison.Ordinal))
            return new ImportDefaultSpecifier(new Identifier(localName));

        return new ImportSpecifier(
            JavaScriptAstFactory.CreateModuleExportName(importedName),
            new Identifier(localName));
    }

    private readonly record struct VariableDeclaratorKey(int Depth, string Name);
}
