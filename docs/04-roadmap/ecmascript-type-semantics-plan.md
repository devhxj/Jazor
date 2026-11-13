# ECMAScript 标注与类型语义调整计划

> 状态：源码实现与切片验收已完成，尚未发布。稳定规则已同步到架构、指南和 Compiler README；已发布的 1.1.0 仍按其 tag 契约使用。
> 主要责任层：`Jazor.Compiler`、`Jazor.Analyzer`、`Jazor.Common`；绑定维护、生成器、RazorVue 与 Emit 按影响范围配合。

本次调整取消 `[ECMAScript]` 的通用编译准入职责：普通 C# 类型无需标注即可在有效编译范围内参与转换；带标注的声明必须具有明确的 JavaScript 宿主含义。同时统一默认对象模型：`class` 保留类的构造与实例语义，`record` 承担结构化对象和对象字面量。清理源码、生成器和示例中的冗余标注是正式交付项。

## 已确认的目标契约

### 编译参与由源码与实际语义决定

普通类型能否参与编译，应由源码是否进入编译范围、运行时实现是否可获得、具体操作是否受支持共同决定。不得以缺少 `[ECMAScript]` 为理由拒绝本可转换的普通类型，也不得要求作者通过添加标注绕过失败。

“参与编译”不等于为每个类型单独生成模块。`record`、接口、委托、枚举等继续遵循各自已有的结构化、擦除或协议语义；模块入口和输出位置仍由现有模块契约表达。

| 类型或使用场景 | 目标判定依据 |
| --- | --- |
| 有效编译范围内的普通源码类型及成员 | Roslyn 绑定的声明与实际操作能否正确转换 |
| 同一源码项目中从入口可达的普通辅助类型 | 纳入明确的源码依赖闭包，生成必要实现；不要求逐类型添加标注 |
| 其他 Jazor 模块或类库中的类型与成员 | 已有模块产物、导出和符号引用关系是否完整 |
| 外部 CLR 类型和成员 | 是否有受支持的编译语义或 `WhiteList` 映射；运行时敏感使用缺少实现时失败 |
| JavaScript 宿主类型和成员 | 是否声明了具体的环境宿主、模块绑定或其他宿主映射语义 |
| 仅处于泛型实参、数组元素等擦除位置的类型 | Compiler 按实际运行时使用判断；Analyzer 保持已有更早、更严格的具体类型诊断 |

本次不承诺自动翻译任意引用程序集中的 CLR 实现。仅有元数据声明、没有可用源码产物或宿主映射的成员，不能据名称直接生成 JavaScript 调用。

### class 与 record 决定默认对象模型

| 声明 | 默认运行时语义 |
| --- | --- |
| 普通源码 `class` | 生成类的实现，保留当前受支持范围内的构造、字段初始化、实例成员和原型分派 |
| `record` / `record struct` | 沿用结构化值语义；创建生成对象字面量，`with` 使用对象 spread，不生成同名运行时 class |
| 宿主 `class` | 按已声明的外部 JS 类型、构造器或明确映射访问；缺少所需运行时身份时在使用点失败 |
| `[ECMAScriptName]` / `[Description("@#...")]` | 只参与名称与属性键解析，不改变 class/record 的对象模型 |

例如，下列纯数据声明不需要 `[ECMAScript]`：

```csharp
public sealed record JsOptions
{
    public string? Title { get; init; }
}

var options = new JsOptions { Title = "release" };
```

创建结果的关键形状是：

```javascript
const options = { Title: "release" };
```

普通 `class` 的创建必须保留构造和初始化行为。不得因为空名称配置、宿主标注或缺少构造器信息，将其自动转换成 `{}`。已有显式 CLR/host 映射仍按对应语义处理；本次清理的是默认类型推断路径，不机械改写已声明的 `Compile` / `Alias` / `Inline` / `Import` 实现。

### ECMAScript 标注只表达特殊用途

| 声明形式 | 调整后的职责 |
| --- | --- |
| 无参 `[ECMAScript]` | 声明已有的环境 JS 宿主契约，或确有需要的特殊宿主值语义 |
| `[ECMAScript("specifier")]` | 声明外部 ESM 导入来源；成员级入口优先于类型级入口 |
| `[ECMAScriptModule("path")]` | 声明生成模块及其输出路径，不承担“该类型才允许编译”的许可职责 |
| `[ECMAScriptName]`、`[Style]`、`[ECMAScriptInline]` | 分别保留已有的命名、样式依赖和表达式模板职责 |

宿主 union 的擦除行为与普通类型准入分开判断。本次不将所有 union 自动视为原始 JS 值，也不顺带重设计 union 协议；已有宿主 union 如需保留标注，理由必须是其明确的宿主擦除语义。`Value`、`AsX`、精确分支判别和接收者单次求值维持已有契约。

## 实现入口与职责

以下入口已按本计划完成调整。表中保留原问题与责任边界，具体支持范围由行为回归和消费者验证共同确认。

| 入口 | 原问题或关联行为 | 已落实的职责 |
| --- | --- | --- |
| [`Jazor.Compiler/Util.cs`](../../src/Jazor.Compiler/Util.cs) 的 `HasECMAScriptSupportMarker`、`IsECMAScriptRuntimeType` 等 | support marker 同时被用于许可、宿主身份、成员命名和 union 判断 | 将“源码可转换”与“特殊宿主语义”分开，逐调用点判断用途 |
| [`SemanticWalker.cs`](../../src/Jazor.Compiler/core/SemanticWalker.cs) 的外部类型与成员检查 | 当前源码边界、模块声明、宿主标注与白名单共同决定接受路径 | 普通源码按实际归属与依赖闭包接受；未实现的外部操作继续在使用点失败 |
| `Util.IsObjectLiteralHostType` 与 [`SemanticWalker.cs.Creation.cs`](../../src/Jazor.Compiler/core/SemanticWalker.cs.Creation.cs) | 宿主标注加空名称边界，可以把非 record 的 class 识别成对象字面量 | 对象字面量默认按 record 结构语义识别；删除 class 的隐式 `{}` 路径 |
| [`StructuralRecordSupport`](../../src/Jazor.Common/StructuralRecordSupport.cs) | 已按 `IsRecord` 识别结构化 record，不要求许可标注 | 作为结构化声明判断的共享依据；保留宿主 record 的 extern/indexer 等特殊成员边界 |
| [`Jazor.Analyzer/Analyzer.cs`](../../src/Jazor.Analyzer/Analyzer.cs) | 特性同时影响分析范围和类型/成员许可 | 普通类型按前端编译范围分析，宿主声明按宿主语义分析 |
| [`AstConverter`](../../src/Jazor.Compiler/AstConverter.cs)、[`ESGenerator`](../../src/Jazor.Compiler/ESGenerator.cs) | 负责声明、模块组织、导入和 catalog；模块入口使用 `[ECMAScriptModule]` | 确保放开后被使用的源码具有真实声明或产物，不产生悬空引用 |
| [`RazorVue MemberClosureBuilder`](../../src/Jazor.RazorVue/RazorSdk/MemberClosureBuilder.cs) 与 Analyzer scope 测试 | RazorVue 使用官方 SG 的最终编译与自身成员闭包 | 普通辅助类型沿 compiler 服务参与，保持前端/后端分析范围清晰 |
| [`WebIDL PreviewBindingEmitter`](../../src/ECMAScript.WebIDL.Generator/PreviewBindingEmitter.cs) 与各绑定生成器 | 生成声明可能沿用批量添加标注的模板 | 按生成声明的实际用途保留或移除标注，从生成源重建输出 |

现有 `VueProps`、`VueDictionary`、`VueObject` 和 WebIDL dictionary 已采用 record，应优先复用这条结构化路线。`Dayjs`、`VueRenderHost` 等带空名称的宿主 class 不能只按标注组合批量转成 record；应检查其外部实例、调用或构造用途，仅迁移实际被当作数据对象创建的声明。

## 实施顺序与交付物

各阶段依次完成。以最小行为切片推进，新增用例必须验证实际输出或运行结果，不以反射内部方法存在作为主要验收。

| 阶段 | 责任层 | 依赖 | 工作与交付物 | 完成条件 |
| --- | --- | --- | --- | --- |
| S0：分类与基线 | Compiler、Analyzer、绑定维护 | 本计划 | 按“普通源码、环境宿主、ESM 绑定、特殊值语义、冗余标注”审阅现有调用点与声明；确定普通源码依赖闭包的最小实现范围 | 每条保留的标注有可说明的特殊用途；建立放开准入和 class/record 的行为回归 |
| S1：源码准入与实现闭包 | Compiler、Common | S0 | 取消普通源码对 support marker 的依赖，衔接声明归属、源码可达闭包及既有模块导入 | 未标注的嵌套类型和同项目顶层辅助类型能在已承诺的范围内实际执行；缺失实现的外部调用继续失败 |
| S2：class/record 收敛 | Compiler、绑定维护 | S1 | 删除非 record class 的隐式对象字面量路径；迁移受影响的配置对象及测试；校对 union 分支判别 | class 构造和初始化未被抹除；record 保持结构化输出；名称配置不再改变对象模型 |
| S3：Analyzer 范围与诊断 | Analyzer、RazorVue | S1、S2 | 将分析范围与宿主语义分离，普通辅助类型不靠标注才接受检查；更新诊断及替代建议 | 对真实前端闭包覆盖有效，普通后端代码不被全局扫描；不再建议添加 `[ECMAScript]` 来获得编译许可 |
| S4：标注与生成模板清理 | 各 `ECMAScript.*` 绑定、生成器、sample 维护 | S1—S3 | 删除已证明仅用于准入的标注，更新模板、输出、文档和样例 | 普通业务类型、纯数据 record 等不再冗余标注；仍需宿主语义的声明保持明确；生成检查通过 |
| S5：消费者与文档闭环 | Compiler、RazorVue、Emit、文档维护 | S1—S4 | 验证源码与 NuGet 消费，补齐迁移说明，按触及面运行门禁 | 行为矩阵通过，产物与诊断可复现，稳定契约同步到架构和指南 |

S1 的源码依赖闭包必须由 Compiler 维护符号归属与翻译语义，RazorVue 通过正式扩展接口消费。跨模块继续使用现有模块与 catalog 契约。不得以自动全程序集扫描、名称猜测或手写 JS 调用替代缺失实现，也不得为每个辅助类型强制新建模块。

同一个普通辅助 class 被多个生成模块引用时，需要通过已有共享模块导出表达唯一实现归属；编译器应拒绝把它复制到多个模块，避免静态状态和运行时类型身份分裂。record 等擦除声明继续按使用点处理。

S3 应复用或按必要范围补足编译根与依赖归属信息。`Jazor.Analyzer` 不能简单取消早退后分析整个 ASP.NET Core 项目；前端入口可达的普通类型需要接受分析，未参与前端编译的控制器、数据库和服务端实现应保持在该检查范围之外。

## 冗余标注清理与迁移

清理按实际用途进行，不对所有 `[ECMAScript]` 批量删除，也不引入新的“允许编译”特性、marker 接口或平行类型登记表。

| 原有写法或用途 | 处理方式 |
| --- | --- |
| 普通业务 class，仅为避免不支持诊断而添加 `[ECMAScript]` | 移除标注，确保其实现纳入编译闭包，按普通 class 转换 |
| 纯数据 record / record struct，仅为获得使用许可而添加标注 | 移除标注，继续依据 record 声明生成结构化值 |
| 普通接口、委托或枚举，仅为满足类型准入而添加标注 | 移除标注，分别沿用契约、回调和标量擦除路线 |
| class 实际依赖“宿主标注＋空名称”生成 options 对象 | 改为 record；同步必要的继承、initializer、调用签名和官方 Razor SG 验证 |
| 宿主 record 包含 extern 属性、索引器或映射方法 | 保留实际宿主契约所需标注；不能因为是 record 就一律删除 |
| 环境 JS 对象、构造器、原生运行时类型或明确宿主值协议 | 保留标注与专用映射，确认其作用是表达运行时语义 |
| 外部 ESM 入口、生成模块输出路径 | 保留对应有参标注，它们表达实际导入或产物身份 |
| 生成代码中的冗余标注 | 修改维护中的生成器模板并重新生成；不直接手改生成结果 |

绑定 class 迁移为 record 可能影响继承、构造和公开 API 形状，应逐项检查。不可构造的外部实例契约不需要为了清理旧的 `{}` 路径而机械迁移。

对清理前后的标注按用途给出可复现的差异说明；验收关注每条保留标注的语义与生成一致性，不设没有行为依据的数量目标，不新增专门的静态审计系统。

## 验收矩阵

| 场景 | 必须证明的行为 |
| --- | --- |
| 未标注的普通 class | 在编译闭包内生成真实类；构造参数、字段初始化、实例成员及现有支持的继承行为正确 |
| 同项目未标注的辅助类型 | 入口调用后实现可达且只生成所需声明；不能仅生成未定义标识符的调用 |
| 未标注的 record / record struct | 主构造、initializer、`with`、解构和已有 `[Spread]` 路线保持结构化行为 |
| class 带名称或空名称配置 | 不会生成对象字面量；无法确定所需运行时名称时给出明确诊断 |
| 原有 class 配置对象迁移 | 改成 record 后，目标 JS 对象的属性键、值和调用结果符合预期 |
| 普通接口、委托与枚举 | 无许可标注也能按既有支持路线使用；接口不生成运行时声明，枚举不生成枚举对象 |
| 外部未映射 CLR 成员 | 仍在调用、构造、成员访问或类型检查等运行时敏感位置明确失败 |
| 擦除位置的具体外部类型 | 保持 Analyzer 与 Compiler 的有意差异；不把泛型实参的存在自动当成运行时物化 |
| 环境宿主与 ESM 绑定 | 保留已有全局对象、成员名称、named/default export、成员级 import 与 `[Style]` 行为 |
| 宿主 union | 原始值擦除、`Value`、`AsX`、不匹配返回 null 和单次求值保持；多个不可区分对象分支继续明确失败 |
| 宿主 class 与 union 对象分支 | 删除 class→`{}` 推断后，不再把 class 错认成结构对象分支；运行时类型判别仍有依据 |
| Analyzer 分析范围 | 前端普通辅助类型中的不支持调用得到诊断；无关后端代码不被纳入前端规则 |
| RazorVue 消费者 | 官方 SG 的 C# 绑定通过，未标注业务类型经 compiler 转换，浏览器交互结果正确 |
| 跨模块与 NuGet 消费者 | 导出、稳定 import alias、ModuleCatalog、源码映射及 bundle 依赖闭包完整 |
| 生成确定性 | 相同输入的声明顺序、临时名、导入和 source map 稳定；不引入遍历顺序相关命名 |

建议在 `Jazor.CompilerTest` 的类型边界、runtime class、结构化 record、宿主命名与 union 用例附近补充回归；Analyzer 与 RazorVue 作者范围回归放入 `Jazor.RazorVue.Sg.Test` 的现有 scope 测试；跨包和产物回归放入 `Jazor.EmitTest`。生成器回归随对应模板修改加入所属测试项目。

## 验证入口

实施期间先运行所修改行为的聚焦测试，默认控制在约两分钟内。成功构建后优先使用聚焦的 `--no-build` 重跑；并发测试使用独立输出与临时资源。

```text
dotnet test src/Jazor.CompilerTest/Jazor.CompilerTest.csproj --filter "FullyQualifiedName~SourceTypeCompilationTests|FullyQualifiedName~SemanticWalkerLanguageProtocolScenarioTests|FullyQualifiedName~UtilBoundaryScenarioTests|FullyQualifiedName~AstConverterRuntimeClassScenarioTests|FullyQualifiedName~ErasedUnionProjectionTests"
dotnet test src/Jazor.RazorVue.Sg.Test/Jazor.RazorVue.Sg.Test.csproj --filter "FullyQualifiedName~RazorVueAnalyzerScopeTests|FullyQualifiedName~RazorSgOfficialUnmarkedSourceTypeRuntimeTests"
```

上述筛选覆盖源码闭包、class/record、union 与 Analyzer 范围的聚焦回归。切片闭环时按实际触及面运行 Compiler、Razor SG、Emit 和生成器套件；发布前运行完整主线与现有覆盖率、绑定契约和消费者门禁，具体门槛以[当前状态](./current-status.md)和[发版规则](../03-guides/release-and-versioning.md)为准。

```text
dotnet run --file scripts/csharp/test-dotnet.cs
dotnet run --file scripts/csharp/verify-compiler-coverage.cs
dotnet run --file scripts/csharp/verify-razorvue-coverage.cs
dotnet run --file scripts/csharp/verify-vue-binding-coverage.cs
dotnet run --file scripts/csharp/verify-vue-binding-contracts.cs
dotnet run --file scripts/csharp/verify-webidl-nodeapi.cs
dotnet run --file scripts/csharp/verify-binding-documentation.cs -- --no-build --configuration Release
```

绑定与 WebIDL 生成检查仅在对应源码或模板受影响时加入切片验收。JavaScript 工具与运行验证继续使用进程内 NodeApi + LibNode + pnpm API；复用现有浏览器与发布消费者，不引入 Node/npm/pnpm/Deno CLI 子进程或新 carrier。

本切片已通过完整解决方案构建、仓库主线、Compiler/RazorVue/Vue binding 覆盖率、绑定契约与 XML 文档、WebIDL NodeApi 及生成器测试。`Jazor.Admin` 测试和 `samples/JazorAdmin` 构建验证了业务声明清理；Emit 的 `Build_LocalReleasePackage_CoreConsumer_ExcludesBlazorAndVuePackages` 与 `Build_LocalPackages_WithExternalRazorSgConsumer_RunsCounterInRealBrowser` 分别验证本地 Release NuGet 消费和真实 Chrome 交互中的未标注 helper。发布仍由目标 tag 重新运行适用门禁。

## 文档、兼容性与完成条件

稳定规则已同步到 [`ImplementationPrinciples.md`](../../src/Jazor.Compiler/ImplementationPrinciples.md)、[编译器架构](../02-architecture/compiler.md)、[类库与产物契约](../02-architecture/library-artifact-contract.md)、[绑定指南](../03-guides/js-resource-binding.md)、Compiler 与测试项目 README，以及 RazorVue 作者指南、sample 和[当前状态](./current-status.md)。公共 API 已从当前程序集生成候选快照并与冻结基线比较；冻结基线与既有已审阅删除记录保持不变，class→record 的语义兼容性按下述规则处理。

取消许可限制属于支持面扩大；class 配置对象改为 record、去除旧对象创建解释可能影响现有消费者。发布时必须按正式版后的兼容规则判断：兼容新增走 MINOR，破坏已发布公共契约走 MAJOR，不能作为纯 PATCH 携带；本计划不预设版本号，也不引入旧行为 fallback。CHANGELOG 必须给出删除冗余标注与 class→record 的具体迁移示例。

- [x] 普通类型在已明确的编译范围内无需 `[ECMAScript]` 即可参与转换。
- [x] 被使用的源码具有真实实现或明确模块/宿主来源，外部 CLR 使用点检查仍有效。
- [x] 默认 class 创建不再隐式生成对象字面量；record 结构化行为具有完整回归。
- [x] 命名配置、编译准入、宿主身份和特殊值语义各自具有清楚职责。
- [x] Analyzer 覆盖前端普通源码，保持擦除位置诊断规则与后端范围隔离。
- [x] 普通业务类型、纯数据声明、生成模板和示例中的冗余标注完成清理。
- [x] 保留标注均有具体用途，未引入替代性的许可标记。
- [x] 源码、NuGet、RazorVue 与适用发布消费者完成验收，现有质量门槛通过。
- [x] 架构、指南、迁移说明和公共契约按交付结果同步。

计划完成后，当前规则收敛到上述架构和指南；实施过程由 Git 历史保留。
