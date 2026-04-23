using Jazor.Razor;

namespace Jazor.RazorVue;

/// <summary>
/// RazorVue 用户组件标记接口。生产 authoring 模型要求组件继承
/// <c>ComponentBase</c> 并实现该接口，避免通过旧基类抬高库的 TFM 或隐藏组件边界。
/// </summary>
public interface IVueComponent : IJazorComponent
{
}
