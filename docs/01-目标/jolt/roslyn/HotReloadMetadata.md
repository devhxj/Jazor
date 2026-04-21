# Jazor Hot Reload 元数据提供器

> 状态：已实现
> 定位：Jazor HMR 边界检测与签名生成核心服务

## 1. 文档定位

本文档描述 `JazorHotReloadMetadataProvider` 的实现，这是 Jolt 项目中用于分析 Jazor 组件、生成 HMR（热模块替换）元数据的核心服务。该服务通过 Roslyn 投影提取组件的属性、状态、计算属性和方法，构建 SHA256 签名，并分类 HMR 边界类型。

**源文件位置**：
- `src/Jolt/Roslyn/InProc/JazorHotReloadMetadataProvider.cs`（约 750 行）

## 2. 核心类型

### 2.1 JazorHotReloadMetadataProvider

主服务类，负责分析 Jazor 文件并生成 HMR 元数据。

**依赖项**：
- `InProcRoslynCodeService _projectionService`：用于创建投影代码

**输出**：
```csharp
public JazorVueHotReloadMetadata CreateMetadata(
    JazorVueDocument document,
    IReadOnlyList<string> loweringDiagnostics,
    IReadOnlyList<DocumentSnapshot>? companionDocuments = null)
```

### 2.2 JazorVueHotReloadMetadata（输出）

HMR 元数据结构：

```csharp
public class JazorVueHotReloadMetadata
{
    public string PropsSignature { get; }           // Props SHA256 签名
    public string TemplateSignature { get; }         // 模板 SHA256 签名
    public string LogicSignature { get; }            // 逻辑 SHA256 签名
    public RazorVueHmrBoundaryKind BoundaryKind { get; }  // HMR 边界类型
}
```

### 2.3 内部 Record 类型

#### SemanticPropDescriptor
```csharp
private sealed record SemanticPropDescriptor(
    string SourceName,           // C# 源代码名称（PascalCase）
    string RuntimeName,          // Vue 运行时名称（camelCase）
    string VueTypeExpression);   // Vue 类型表达式（String/Number/Boolean/null）
```

#### SemanticLogicDescriptor
```csharp
private sealed record SemanticLogicDescriptor(
    string Name,                 // 成员名称
    string Signature,            // 完整签名（类型 + 名称）
    string Body);                // 归一化的语法体
```

#### SemanticHotReloadParts
```csharp
private sealed record SemanticHotReloadParts(
    IReadOnlyList<SemanticPropDescriptor> Props,
    IReadOnlyList<SemanticLogicDescriptor> States,
    IReadOnlyList<SemanticLogicDescriptor> Computeds,
    IReadOnlyList<SemanticLogicDescriptor> Methods,
    int MappedUserDeclarationCount);  // 成功映射的用户声明数量
```

## 3. 核心算法

### 3.1 元数据创建流程（CreateMetadata）

**目的**：从 Jazor 文档生成完整的 HMR 元数据。

**流程**：

1. **创建文档快照**：
   ```csharp
   var snapshot = new DocumentSnapshot(
       document.FilePath,
       DocumentKind.Jazor,
       document.SourceText,
       version: null);
   ```

2. **加载代码隐藏文档**（`LoadCodeBehindDocuments`）：
   - 查找同目录下的 `.cs` 代码隐藏文件（通过 `JoltWorkspaceResolver.GetCoLocatedCodeBehindPaths`）
   - 优先使用 `companionDocuments` 参数提供的文档
   - 回退到文件系统读取（`File.ReadAllText`）
   - 过滤掉重复路径（使用 `HashSet` 去重）

3. **创建投影**：
   ```csharp
   var projection = _projectionService.CreateProjection(snapshot, document);
   ```
   - 优先使用 Razor 设计时投影
   - 失败则回退到 `InProcRoslynCodeService.CreateFallbackProjection`

4. **分析投影**（`AnalyzeProjection`）：
   - 解析投影代码，提取 `Props`, `States`, `Computeds`, `Methods`
   - 分析代码隐藏文档中的 partial class 扩展
   - 返回 `SemanticHotReloadParts`

5. **Fallback 重试**（`ShouldRetryWithFallbackProjection`）：
   - 如果 `MappedUserDeclarationCount == 0` 且有 `@code` 块
   - 使用 Fallback 投影重新分析
   - 选择映射声明数量更多的结果

6. **构建签名**：
   - `PropsSignature`：`BuildDescriptorSignature(props, methods)` 的 SHA256
   - `TemplateSignature`：`document.Template` 的 SHA256
   - `LogicSignature`：`BuildLogicSignature(states, computeds, methods)` 的 SHA256

7. **分类边界**（`ClassifyBoundary`）：
   - 检查 `loweringDiagnostics` 是否包含 "could not be lowered"
   - 根据是否存在 `States`, `Computeds`, `Methods` 决定边界类型

**源代码引用**：`JazorHotReloadMetadataProvider.cs:34-63`

### 3.2 投影分析（AnalyzeProjection）

**目的**：从 Roslyn 投影中提取组件的语义信息。

**流程**：

1. **创建编译**：
   ```csharp
   var syntaxTree = CSharpSyntaxTree.ParseText(
       projection.SourceText,
       ParseOptions,
       path: projection.ProjectedDocumentPath,
       encoding: Encoding.UTF8);
   var companionTrees = companionDocuments
       .Select(document => CSharpSyntaxTree.ParseText(...))
       .ToArray();
   var supportTree = CSharpSyntaxTree.ParseText(
       CreateSupportSource(),  // PropAttribute, StateAttribute, ComputedAttribute
       ...);
   var compilation = CSharpCompilation.Create(
       assemblyName: "__JoltHotReloadMetadata",
       syntaxTrees: [syntaxTree, .. companionTrees, supportTree],
       references: MetadataReferences,
       options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
   ```

2. **获取语义模型**：
   ```csharp
   var semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
   ```

3. **遍历成员声明**：
   ```csharp
   foreach (var member in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
   {
       if (!IsUserCodeDeclaration(document, projection.ProjectionMap, member))
           continue;

       mappedUserDeclarationCount++;
       switch (member)
       {
           case PropertyDeclarationSyntax propertyDeclaration:
               AddPropertyDescriptor(semanticModel, propertyDeclaration, props, states, computeds);
               break;
           case FieldDeclarationSyntax fieldDeclaration:
               AddFieldDescriptors(semanticModel, fieldDeclaration, states);
               break;
           case MethodDeclarationSyntax methodDeclaration:
               AddMethodDescriptor(semanticModel, methodDeclaration, computeds, methods);
               break;
       }
   }
   ```

4. **过滤用户代码**（`IsUserCodeDeclaration`）：
   - 检查成员的 `Span` 是否与投影映射的 `Segment` 相交
   - 检查 `Segment` 的 `OriginalStart/OriginalEnd` 是否与 `document.CodeStartIndex/CodeLength` 相交
   - 排除生成的 Razor 脚手架代码

5. **添加代码隐藏描述符**（`AddCompanionCodeBehindDescriptors`）：
   - 查找与组件名称匹配的 `partial class`
   - 遍历其成员，提取 `State`, `Computed`, `Method`

**源代码引用**：`JazorHotReloadMetadataProvider.cs:65-146`

### 3.3 属性描述符提取（AddPropertyDescriptor）

**目的**：从属性声明中提取 `Prop`, `State`, `Computed` 描述符。

**流程**：

1. **获取符号**：
   ```csharp
   var symbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);
   ```

2. **检查 Prop 特性**：
   ```csharp
   if (HasAttribute(symbol, propertyDeclaration.AttributeLists, "Prop"))
   {
       var sourceName = symbol?.Name ?? propertyDeclaration.Identifier.ValueText;
       props.Add(new SemanticPropDescriptor(
           sourceName,
           JazorVueNaming.ToCamelCase(sourceName),
           MapVueType(symbol?.Type, propertyDeclaration.Type)));
   }
   ```
   - `[Prop]` 标记的属性会被提取为组件 Props
   - 运行时名称转换为 camelCase（如 `PropertyName` → `propertyName`）
   - 类型映射到 Vue 类型（String/Number/Boolean/null）

3. **检查 State 特性**：
   ```csharp
   if (HasAttribute(symbol, propertyDeclaration.AttributeLists, "State"))
   {
       var sourceName = symbol?.Name ?? propertyDeclaration.Identifier.ValueText;
       states.Add(new SemanticLogicDescriptor(
           sourceName,
           CreatePropertySignature(symbol, propertyDeclaration),
           NormalizeSyntax(propertyDeclaration)));
   }
   ```
   - `[State]` 标记的属性会被提取为响应式状态
   - 签名包含类型和名称（如 `int Count`）
   - 语法体为归一化的属性声明

4. **检查 Computed 特性**：
   ```csharp
   if (HasAttribute(symbol, propertyDeclaration.AttributeLists, "Computed"))
   {
       var sourceName = symbol?.Name ?? propertyDeclaration.Identifier.ValueText;
       computeds.Add(new SemanticLogicDescriptor(
           sourceName,
           CreatePropertySignature(symbol, propertyDeclaration),
           NormalizeSyntax(propertyDeclaration)));
   }
   ```
   - `[Computed]` 标记的属性会被提取为计算属性

**源代码引用**：`JazorHotReloadMetadataProvider.cs:251-285`

### 3.4 字段描述符提取（AddFieldDescriptors）

**目的**：从字段声明中提取 `State` 描述符。

**流程**：

```csharp
foreach (var variable in fieldDeclaration.Declaration.Variables)
{
    var symbol = semanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
    if (!HasAttribute(symbol, fieldDeclaration.AttributeLists, "State"))
        continue;

    var sourceName = symbol?.Name ?? variable.Identifier.ValueText;
    states.Add(new SemanticLogicDescriptor(
        sourceName,
        CreateFieldSignature(symbol, fieldDeclaration, variable),
        NormalizeStateInitializer(variable)));
}
```

**关键点**：
- 仅支持 `[State]` 标记的字段（不支持 Prop 或 Computed）
- 语法体为归一化的初始化器（如 `= 0`）

**源代码引用**：`JazorHotReloadMetadataProvider.cs:313-330`

### 3.5 方法描述符提取（AddMethodDescriptor）

**目的**：从方法声明中提取 `Computed` 或公共实例方法。

**流程**：

1. **检查 Computed 特性**：
   ```csharp
   if (HasAttribute(symbol, methodDeclaration.AttributeLists, "Computed"))
   {
       var sourceName = symbol?.Name ?? methodDeclaration.Identifier.ValueText;
       computeds.Add(new SemanticLogicDescriptor(
           sourceName,
           CreateMethodSignature(symbol, methodDeclaration),
           NormalizeSyntax(methodDeclaration)));
       return;
   }
   ```
   - `[Computed]` 标记的方法会被提取为计算属性（get-only）

2. **检查公共实例方法**（`IsPublicInstanceMethod`）：
   ```csharp
   if (!IsPublicInstanceMethod(symbol, methodDeclaration))
       return;

   var methodName = symbol?.Name ?? methodDeclaration.Identifier.ValueText;
   methods.Add(new SemanticLogicDescriptor(
       methodName,
       CreateMethodSignature(symbol, methodDeclaration),
       NormalizeSyntax(methodDeclaration)));
   ```
   - 非静态、公共的实例方法会被提取为组件方法
   - 排除构造函数、运算符、显式接口实现

**源代码引用**：`JazorHotReloadMetadataProvider.cs:332-357`

### 3.6 类型映射（MapVueType）

**目的**：将 C# 类型映射到 Vue 类型表达式。

**映射规则**：

| C# 类型 | Vue 类型 | SpecialType |
|---------|---------|-------------|
| `string`, `string?` | `"String"` | `System_String` |
| `bool`, `bool?` | `"Boolean"` | `System_Boolean` |
| 数值类型（`int`, `double`, `decimal` 等） | `"Number"` | 所有数值 SpecialType |
| 其他类型 | `"null"` | - |

**实现**：

```csharp
private static string MapVueType(ITypeSymbol? typeSymbol, TypeSyntax fallbackType)
{
    if (typeSymbol is not null)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_String => "String",
            SpecialType.System_Boolean => "Boolean",
            SpecialType.System_Byte or /* ... 其他数值类型 ... */ => "Number",
            _ => "null"
        };
    }

    return fallbackType.ToString() switch
    {
        "string" or "String" or "string?" or "String?" => "String",
        "bool" or "Boolean" or "bool?" or "Boolean?" => "Boolean",
        "byte" or "sbyte" or /* ... 其他数值类型 ... */ => "Number",
        _ => "null"
    };
}
```

**源代码引用**：`JazorHotReloadMetadataProvider.cs:591-622`

### 3.7 HMR 边界分类（ClassifyBoundary）

**目的**：根据组件内容分类 HMR 边界类型。

**分类规则**：

```csharp
private static RazorVueHmrBoundaryKind ClassifyBoundary(
    JazorVueDocument document,
    SemanticHotReloadParts parts,
    IReadOnlyList<string>? loweringDiagnostics)
{
    // 1. 检查 lowering 诊断
    if (loweringDiagnostics?.Any(static diagnostic =>
            diagnostic.Contains("could not be lowered", StringComparison.Ordinal)) == true)
    {
        return RazorVueHmrBoundaryKind.FullReloadRequired;
    }

    // 2. 检查是否存在逻辑成员
    if (parts.States.Count > 0 || parts.Computeds.Count > 0 || parts.Methods.Count > 0)
        return RazorVueHmrBoundaryKind.LogicSafe;

    // 3. 仅模板或未知
    return string.IsNullOrWhiteSpace(document.Template)
        ? RazorVueHmrBoundaryKind.Unknown
        : RazorVueHmrBoundaryKind.TemplateOnly;
}
```

**边界类型说明**：

| 边界类型 | 含义 | HMR 策略 |
|---------|------|---------|
| `TemplateOnly` | 仅模板变更 | 安全替换模板，保留状态 |
| `LogicSafe` | 逻辑变更但无 lowering 错误 | 安全替换整个组件 |
| `FullReloadRequired` | Lowering 失败 | 需要完全重新加载 |
| `Unknown` | 空文档或无法分析 | 保守策略：完全重新加载 |

**源代码引用**：`JazorHotReloadMetadataProvider.cs:475-492`

## 4. 线程安全模型

### 4.1 无状态设计

**特点**：
- `JazorHotReloadMetadataProvider` 本身是无状态的
- 所有方法都是纯函数（输入 → 输出）
- 不维护缓存或可变状态

### 4.2 依赖服务的线程安全

**InProcRoslynCodeService**：
- 内部有编译缓存（使用 `Lock` 保护）
- 多线程调用是安全的

**结论**：整个服务是线程安全的，适合并发调用。

## 5. 错误处理

### 5.1 投影创建失败

**策略**：
- Razor 投影失败 → 使用 Fallback 投影
- Fallback 也失败 → 返回空元数据或抛出异常

### 5.2 文件系统访问失败

**策略**（`SafeFileExists`）：
```csharp
private static bool SafeFileExists(string filePath)
{
    try
    {
        return File.Exists(filePath);
    }
    catch (IOException) { return false; }
    catch (UnauthorizedAccessException) { return false; }
}
```

### 5.3 元数据引用加载失败

**策略**：
- 跳过无法加载的引用
- 仅记录警告，不中断流程

### 5.4 符号解析失败

**策略**：
- 使用语法回退（如 `propertyDeclaration.Identifier.ValueText`）
- 不因缺少符号信息而中断分析

## 6. 配置选项

### 6.1 解析选项

```csharp
private static readonly CSharpParseOptions ParseOptions = new(
    languageVersion: LanguageVersion.Preview);
```

**说明**：使用 C# 预览版，支持最新语言特性。

### 6.2 类型显示格式

```csharp
private static readonly SymbolDisplayFormat TypeDisplayFormat = new(
    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
    miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
```

**用途**：生成完整的类型签名（包括命名空间、泛型参数、可空性）。

### 6.3 支持代码生成（CreateSupportSource）

**内容**：
```csharp
[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]
internal sealed class PropAttribute : global::System.Attribute { }

[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]
internal sealed class StateAttribute : global::System.Attribute { }

[global::System.AttributeUsage(global::System.AttributeTargets.Property | global::System.AttributeTargets.Field | global::System.AttributeTargets.Method)]
internal sealed class ComputedAttribute : global::System.Attribute { }
```

**用途**：在编译时提供 `[Prop]`, `[State]`, `[Computed]` 特性定义，供语义分析使用。

## 7. 与其他子系统的交互

### 7.1 InProcRoslynCodeService

**接口**：
```csharp
(string ProjectedDocumentPath, string SourceText, ProjectionMap ProjectionMap) CreateProjection(
    DocumentSnapshot document,
    JazorVueDocument parsed)
```

**用途**：创建 Jazor 文件的 Roslyn 投影代码。

### 7.2 JazorVueNaming

**方法**：
```csharp
string ToCamelCase(string pascalCaseName)
```

**用途**：将 PascalCase 转换为 camelCase（如 `PropertyName` → `propertyName`）。

### 7.3 JoltWorkspaceResolver

**方法**：
```csharp
IReadOnlyList<string> GetCoLocatedCodeBehindPaths(string jazorDocumentPath)
string NormalizePath(string path)
```

**用途**：
- 查找同目录下的代码隐藏文件
- 规范化路径比较

### 7.4 DocumentSnapshot

**来源**：`Jazor.VueContracts.Protocol`

**用途**：封装文档内容、路径、类型、版本。

## 8. 设计权衡

### 8.1 特性驱动设计

**选择**：使用 `[Prop]`, `[State]`, `[Computed]` 特性标记成员。

**权衡**：
- **优点**：明确区分组件成员类型，支持 C# 字段作为 State
- **缺点**：需要显式标记，增加代码量
- **替代方案**：基于命名约定（如 `_count` 为 State），但不够明确

### 8.2 代码隐藏文档支持

**选择**：自动查找并分析同目录下的 `.cs` 代码隐藏文件。

**权衡**：
- **优点**：分离关注点，支持大型组件拆分
- **缺点**：增加文件系统访问开销
- **优化**：使用 `companionDocuments` 参数避免重复读取

### 8.3 Fallback 投影重试

**选择**：当 Razor 投影映射失败时，回退到 JazorVueParser 投影。

**权衡**：
- **优点**：提高鲁棒性，避免因 Razor 工具链问题导致功能完全失效
- **缺点**：Fallback 仅支持 `@code` 块，功能受限
- **指标**：使用 `MappedUserDeclarationCount` 选择更好的结果

### 8.4 SHA256 签名

**选择**：使用 SHA256 对 Props、Template、Logic 分别签名。

**权衡**：
- **优点**：精确检测变更，支持细粒度 HMR
- **缺点**：计算开销较大，签名长度较长
- **替代方案**：使用时间戳或版本号，但不精确

### 8.5 归一化语法体

**选择**：在 `SemanticLogicDescriptor.Body` 中存储归一化的语法。

**权衡**：
- **优点**：签名基于语法结构，不受格式化影响
- **缺点**：归一化可能丢失某些语义信息
- **实现**：使用 `node.NormalizeWhitespace(elasticTrivia: false).ToFullString()`

## 9. 签名构建细节

### 9.1 Props 签名（BuildDescriptorSignature）

**格式**：
```
props:
Prop1Name|prop1RuntimeName|String
Prop2Name|prop2RuntimeName|Number
methods:
void Method1()
int Method2(string arg)
```

**SHA256 输入**：上述字符串的 UTF-8 字节。

### 9.2 Logic 签名（BuildLogicSignature）

**格式**：
```
states:
state1|int State = 0;
state2|string Name { get; set; }
computeds:
computed1|int Calc() => Count * 2;
methods:
void Method1() { ... }
int Method2(string arg) { ... }
```

**SHA256 输入**：上述字符串的 UTF-8 字节。

### 9.3 Template 签名

**SHA256 输入**：`document.Template` 的 UTF-8 字节。

## 10. 完整示例

### 10.1 Jazor 源文件（`Counter.jazor`）

```razor
@implements Jazor.Vue.IComponent

<int Count="0" />
<button @onclick="Increment">Increment</button>
<span>@Count</span>

@code {
    [Prop]
    public int Count { get; set; }

    [State]
    private int _doubled = 0;

    [Computed]
    private int Doubled => _doubled * 2;

    public void Increment()
    {
        Count++;
        _doubled = Count * 2;
    }
}
```

### 10.2 代码隐藏文件（`Counter.cs`）

```csharp
namespace MyApp;

public partial class Counter
{
    [State]
    private string _message = "Hello";

    [Computed]
    private string Greeting => $"{_message}, World!";
}
```

### 10.3 提取的元数据

**Props**：
- `Count` (camelCase: `count`, VueType: `"Number"`)

**States**：
- `_doubled` (signature: `int _doubled`, body: `= 0;`)
- `_message` (signature: `string _message`, body: `= "Hello";`)

**Computeds**：
- `Doubled` (signature: `int Doubled`, body: `=> _doubled * 2;`)
- `Greeting` (signature: `string Greeting`, body: `=> $"{_message}, World!";`)

**Methods**：
- `Increment` (signature: `void Increment()`, body: `{ ... }`)

**边界类型**：`RazorVueHmrBoundaryKind.LogicSafe`（存在 State 和 Computed）

---

**文档维护者**：developerhan
**最后更新**：2026-04-21
**文档版本**：v1.0
