namespace ECMAScript.Contract;

/// <summary>
/// Jazor UI authoring surface 使用的最小组件标记契约。
/// </summary>
/// <remarks>
/// 该接口只用于标识和约束 authoring 类型，不承载渲染实现、生命周期逻辑或编译器分析状态。
/// 将实现放在宿主/组件层，避免最低层 contract 程序集依赖 Razor、Vue 或 Roslyn。
/// </remarks>
public interface IUIComponent
{
    // 仅保留为 Jazor Razor 基础标记，避免把 authoring API 与编译分析逻辑耦合在同一个基类中。
}
