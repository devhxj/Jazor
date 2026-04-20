# WebIDL 绑定生成

> 对应源码：`src/ECMAScript.WebIDL/`（已归档的 TypeScript 版）、`src/ECMAScript.WebIDL.Generator/`（当前 .NET 版）

## 为什么需要

ECMAScript 的宿主 API（浏览器 DOM、CSSOM 等）通过 WebIDL 规范定义。Jazor 需要这些 API 的 C# 类型投影来让用户用 C# 调用浏览器原生接口。手写几千个绑定类既不现实也难以维护——WebIDL 规范持续更新，绑定必须能跟随规范自动重新生成。

## 解决什么问题

1. **自动化绑定**：从 WebIDL 规范文件自动生成 C# 类型定义，免除手写负担
2. **规范同步**：WebIDL 规范更新后，重新生成即可同步最新 API
3. **类型安全**：生成的 C# 绑定保留完整的类型信息，配合 Analyzer 在编译时检查用法合法性

## 大致实现思路

### 两代实现

| 版本 | 语言 | 状态 | 说明 |
|------|------|------|------|
| `ECMAScript.WebIDL` | TypeScript | 已归档 | 旧版 TypeScript 发射器，不再维护 |
| `ECMAScript.WebIDL.Generator` | C# | 当前活跃 | 新版 .NET 宿主，使用 Roslyn 生成 C# 绑定 |

### Generator 工作流程

```
WebIDL 规范文件（.widl / .idl）
       ↓ 解析
类型模型（接口、属性、方法、继承关系）
       ↓ 代码生成
C# 绑定类（带 [ECMAScriptModule]、[WhiteList] 等特性）
```

### 关键设计决策

- **继承方法处理**：子类继承父类方法时，返回类型不同需用 `new` 修饰符
- **重复方法抑制**：继承链中的重复方法签名只生成一次
- **主构造函数转发**：父类无无参构造函数时，子类需转发基类参数
- **仓库布局发现**：`RepositoryLayout.Discover()` 从构建输出向上遍历找到仓库根目录

### 测试

`src/ECMAScript.WebIDL.GeneratorTest/` 覆盖仓库布局发现和绑定生成正确性。
