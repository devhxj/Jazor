using Jazor.Razor;

namespace Jazor.RazorVue;

/// <summary>
/// RazorVue 的基础组件类型，同时所在程序集也是 RazorVue 核心语义的归属层。
/// 为什么这样分层：Vue authoring surface 与 RazorVue descriptor/lowering/pipeline 属于同一个产品核心，
/// 而 Roslyn generator 入口只是在 Analysis 层做薄接线，不再承载核心实现。
/// </summary>
public interface IVueComponent : IJazorComponent
{
} 
