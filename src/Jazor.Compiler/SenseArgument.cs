// File: SenseArgument.cs
// Purpose: Carries lowering context plus per-scope declarations and module-level import collection.
// 结构体副本共享导入状态但隔离局部声明，使嵌套 lowering 同时保持 lexical scope 与 stable imports。
using Acornima.Ast;
using Jazor.Common;
using Microsoft.CodeAnalysis;

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
public readonly record struct SenseArgument
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
    // 项目内模块的 import 必须写成相对 specifier；当前输出路径是计算该 specifier 的起点。
    // 模块声明路径本身就是完整项目路径，不再维护额外的 catalog 输出前缀。
    private readonly string? _currentModuleOutputPath;
    // Catalog edges are collected independently from resource-manifest imports. The emitted
    // JavaScript may use the same logical specifier text for both, so text alone is insufficient.
    private readonly HashSet<string>? _moduleCatalogImportPaths;
    // 项目源码引用的完整路径键。写出的 ESM specifier 是相对的，因此清单选择必须
    // 显式保留目标项目路径，不能从 import 文本反推，也不能按目录前缀猜测。
    private readonly HashSet<string>? _projectSourceImportKeys;

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
        _moduleCatalogImportPaths = null;
        _projectSourceImportKeys = null;
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
        _moduleCatalogImportPaths = null;
        _projectSourceImportKeys = null;
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
        HashSet<string>? currentModuleBindings,
        HashSet<string>? moduleCatalogImportPaths,
        string? currentModuleOutputPath = null,
        HashSet<string>? projectSourceImportKeys = null)
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
        _moduleCatalogImportPaths = moduleCatalogImportPaths;
        _projectSourceImportKeys = projectSourceImportKeys;
        _currentModuleOutputPath = currentModuleOutputPath;
    }

    // ===== 依赖项状态检查 =====
    /// <summary>是否包含变量声明</summary>
    public bool HasVarDeclarator => _declarators?.Count > 0;

    /// <summary>是否包含导入声明规范</summary>
    public bool HasVarImportDeclarationSpecifier => _specifiers?.Count > 0;

    /// <summary>lowering 期间引用到的完整项目源码路径。</summary>
    public IReadOnlyCollection<string> ProjectSourceImportKeys
        => _projectSourceImportKeys ?? (IReadOnlyCollection<string>)Array.Empty<string>();

    // ===== Sense 变更 =====
    /// <summary>创建新实例，设置 Sense</summary>
    public SenseArgument With(Sense sense)
        => new(sense, UseImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings, _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);

    // ===== 作用域隔离 =====
    /// <summary>
    /// 创建新实例，用于块级作用域隔离。
    /// 共享导入字典，创建新的变量声明字典。
    /// </summary>
    public SenseArgument WithNewScope()
        => new(Sense, UseImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, [], _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings, _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);

    internal SenseArgument WithScope(EmissionScopeContext scopeContext)
        => new(Sense, UseImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, scopeContext, _currentModuleImportPath, _currentModuleBindings, _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);

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
            _currentModuleBindings,
            _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);
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
            _currentModuleBindings,
            _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);
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
        => new(Sense, UseImportAliases, input, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings, _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);

    // ===== 异常处理上下文 =====
    /// <summary>设置 Catch 异常参数名</summary>
    public SenseArgument WithCatchVar(string? varName)
        => new(Sense, UseImportAliases, PatternInput, varName, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings, _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);

    // ===== Switch 表达式上下文 =====
    /// <summary>设置 Switch 表达式变量名</summary>
    public SenseArgument WithSwitchVar(string? varName)
        => new(Sense, UseImportAliases, PatternInput, CatchExceptionVar, varName, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings, _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);

    // ===== 组合设置 =====
    /// <summary>设置 Sense 和 PatternInput</summary>
    public SenseArgument With(Sense sense, Expression patternInput)
        => new(sense, UseImportAliases, patternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings, _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);

    public SenseArgument WithImportAliases(bool useImportAliases = true)
        => new(Sense, useImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, _importBindings, _importLocalBindings, _reservedImportNames, ScopeContext, _currentModuleImportPath, _currentModuleBindings, _moduleCatalogImportPaths, _currentModuleOutputPath, _projectSourceImportKeys);

    public SenseArgument WithImportContext(
        Dictionary<string, string> importBindings,
        Dictionary<string, string> importLocalBindings,
        HashSet<string> reservedImportNames,
        string? currentModuleImportPath,
        HashSet<string> currentModuleBindings,
        HashSet<string>? moduleCatalogImportPaths = null,
        string? currentModuleOutputPath = null,
        HashSet<string>? projectSourceImportKeys = null)
        => new(Sense, UseImportAliases, PatternInput, CatchExceptionVar, SwitchExpressionVar, _declarators, _specifiers, importBindings, importLocalBindings, reservedImportNames, ScopeContext, currentModuleImportPath, currentModuleBindings, moduleCatalogImportPaths, currentModuleOutputPath, projectSourceImportKeys);

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
        => BindImportSpecifierCore(modulePath, importedName, normalizeForCurrentModule: true);

    /// <summary>
    /// Binds a module supplied by a pure Jazor <c>ModuleCatalog</c> and records its catalog edge.
    /// The declared project path remains the catalog identity. The JavaScript import is the
    /// ordinary relative URL from the current module to that project file.
    /// </summary>
    internal Identifier BindModuleCatalogImportSpecifier(string? modulePath, string importedName)
    {
        if (string.IsNullOrWhiteSpace(modulePath))
            return BindImportSpecifierCore(modulePath, importedName, normalizeForCurrentModule: true);

        var projectPath = ECMAScriptModulePath.NormalizeRelativePath(modulePath!);

        // 产物图边始终按声明的项目路径记录：catalog 集合是"本图模块"的权威标识，
        // 用于把这些边从 packageImports 中排除（它们属于 dependencies）。
        _moduleCatalogImportPaths?.Add(projectPath);

        // 项目内模块一律按完整声明路径计算相对 specifier。
        return BindImportSpecifierCore(
            ResolveProjectImportSpecifier(projectPath),
            importedName,
            normalizeForCurrentModule: false);
    }

    /// <summary>
    /// Binds a module materialized as ordinary source in the generated JavaScript project.
    ///
    /// 这些路径是载体里的真实文件，不是包，因此与 catalog 导入走同一条路径规则：
    /// 记录边、并写成<b>相对</b> specifier。裸 <c>clr/**</c> 形态会让消费侧必须依赖
    /// import map 前缀映射才能解析，与"自有源码按普通项目源码交付"的契约不符。
    /// </summary>
    public Identifier BindProjectSourceImportSpecifier(string? modulePath, string importedName)
    {
        if (string.IsNullOrWhiteSpace(modulePath))
            return BindImportSpecifierCore(modulePath, importedName, normalizeForCurrentModule: false);

        var projectPath = ECMAScriptModulePath.NormalizeRelativePath(modulePath!);

        // 自引用（符号声明在目标模块自身）是本地绑定，不是产物图边。
        if (IsCurrentModuleImport(projectPath))
            return BindImportSpecifier(projectPath, importedName);

        _projectSourceImportKeys?.Add(projectPath);

        return BindImportSpecifierCore(
            ResolveProjectImportSpecifier(projectPath),
            importedName,
            normalizeForCurrentModule: false);
    }

    /// <summary>
    /// 把项目模块路径解析为相对当前模块输出位置的 import specifier。
    ///
    /// 输出路径未知时按项目根 importer 生成 <c>./</c> URL；这用于独立转换场景。
    /// </summary>
    private string ResolveProjectImportSpecifier(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(_currentModuleOutputPath))
            return ECMAScriptModulePath.NormalizeRootRelativeImportSpecifier(projectPath);

        if (string.Equals(projectPath, _currentModuleOutputPath, StringComparison.Ordinal))
            return projectPath;

        return ECMAScriptModulePath.ResolveRelativeToImporter(_currentModuleOutputPath!, projectPath);
    }

    /// <summary>
    /// Returns whether an import specifier resolves to the module currently being lowered.
    /// Self references are local bindings, not an artifact-graph edge, irrespective of where
    /// the referenced CLR helper symbol was originally declared.
    /// </summary>
    internal bool IsCurrentModuleImport(string? modulePath)
        => IsCurrentModulePath(modulePath);

    /// <summary>
    /// Binds an external ESM specifier without applying generated-module path normalization.
    /// 外部 ESM specifier 必须按原文保留，不能套用生成模块的 `.mjs`/目录逃逸规则。
    /// </summary>
    public Identifier BindExternalImportSpecifier(string? modulePath, string importedName)
    {
        if (_projectSourceImportKeys is not null && _currentModuleOutputPath is not null &&
            (modulePath?.StartsWith("./", System.StringComparison.Ordinal) == true ||
             modulePath?.StartsWith("../", System.StringComparison.Ordinal) == true))
        {
            // An external declaration is an authored ESM specifier: preserve its text and
            // importer-relative meaning. Only dependency bookkeeping needs the resolved file.
            _projectSourceImportKeys.Add(
                ECMAScriptModulePath.ResolveRelativePath(_currentModuleOutputPath, modulePath!));
        }
        return BindImportSpecifierCore(modulePath, importedName, normalizeForCurrentModule: false);
    }

    private Identifier BindImportSpecifierCore(
        string? modulePath,
        string importedName,
        bool normalizeForCurrentModule)
    {
        if (string.IsNullOrWhiteSpace(importedName))
            return new Identifier(importedName ?? string.Empty);

        if (string.IsNullOrWhiteSpace(modulePath))
            return new Identifier(importedName);

        if (normalizeForCurrentModule && IsCurrentModulePath(modulePath))
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

    private bool IsCurrentModulePath(string? modulePath)
    {
        if (string.IsNullOrWhiteSpace(modulePath))
            return false;

        // ECMAScriptModule 的作者声明可带标准 ./ 前缀，而 CurrentModuleOutputPath 是
        // 项目根下的文件名。二者是同一个项目文件身份，比较前只做项目路径归一化。
        if (!string.IsNullOrWhiteSpace(_currentModuleOutputPath))
        {
            return string.Equals(
                ECMAScriptModulePath.NormalizeRelativePath(modulePath!),
                ECMAScriptModulePath.NormalizeRelativePath(_currentModuleOutputPath!),
                System.StringComparison.Ordinal);
        }

        return !string.IsNullOrWhiteSpace(_currentModuleImportPath) &&
               string.Equals(
                   ECMAScriptModulePath.NormalizeImportSpecifier(modulePath!),
                   _currentModuleImportPath,
                   System.StringComparison.Ordinal);
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
