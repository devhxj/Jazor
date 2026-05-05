# WebIDL 绑定生成

> 对应源码：`src/ECMAScript.WebIDL/`（已归档的 TypeScript 版）、`src/ECMAScript.WebIDL.Generator/`（当前 .NET 版）

## 为什么需要

浏览器宿主 API 通过 WebIDL 定义。Jazor 需要把这些宿主能力投影成可被 C# 编译器和分析器消费的类型系统表面。靠手写几千个绑定既不可维护，也无法跟上规范演进，所以必须走生成路线。

## 解决什么问题

1. **自动化绑定生成**：从 WebIDL 规范生成 C# 类型定义。
2. **规范同步**：规范变化后可以重新生成。
3. **类型化宿主投影**：为 `ECMAScript` / 编译器 / 分析器提供一致的浏览器 API 表面。

## 两代实现

| 版本 | 语言 | 状态 | 说明 |
|------|------|------|------|
| `ECMAScript.WebIDL` | TypeScript | 已归档 | 旧版发射器 |
| `ECMAScript.WebIDL.Generator` | C# | 当前活跃 | 当前 .NET 宿主生成器 |

## 当前生成链路

```text
WebIDL 规范文件（.widl / .idl）
       ↓ 解析
类型模型（接口、属性、方法、继承关系）
       ↓ 代码生成
C# 绑定类型（ECMAScript 特性与宿主投影元数据）
```

这里的目标是生成“可进入 Jazor 编译域的宿主类型表面”，而不是直接生成完整运行时实现。

当前活跃输出边界是 `src/ECMAScript/webidl/`。

- `src/ECMAScript/webidl/generate/`：当前 .NET 生成器产物
- `src/ECMAScript/generate/`：历史遗留目录，已被 `ECMAScript.csproj` 排除，不参与当前编译主线

## 关键设计点

- 继承方法返回类型差异时，子类用 `new` 隐藏。
- 继承链中的重复方法签名只生成一次。
- 需要时生成主构造函数转发。
- 通过 `RepositoryLayout.Discover()` 从构建输出回溯仓库根目录。

## 测试

`src/ECMAScript.WebIDL.GeneratorTest/` 覆盖仓库布局发现和绑定生成正确性。
