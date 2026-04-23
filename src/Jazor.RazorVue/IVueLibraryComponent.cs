namespace Jazor.RazorVue;

/// <summary>
/// 外部 Vue 库组件标记接口。库组件参与 descriptor/registry 流程，
/// 但不会被当作普通 RazorVue 用户组件生成独立入口。
/// </summary>
public interface IVueLibraryComponent : IVueComponent
{
}
