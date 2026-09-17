using System.ComponentModel;

namespace ECMAScript;

/// <summary>
/// vee-validate 入口；提供表单组合式函数、字段状态与提交状态。
/// vee-validate entry; provides the form composable, field state, and submission state.
/// </summary>
/// <remarks>
/// 首期只绑定组合式函数：表单与字段上下文、字段/表单状态读取与写入、提交与校验入口。
/// 组件（<c>Form</c>、<c>Field</c>、<c>FieldArray</c>、<c>ErrorMessage</c>）、<c>defineRule</c>
/// 自定义规则与 yup/zod 等 schema 适配器不在首期范围内。
/// The first slice binds composables only: form and field contexts, field/form state reads and
/// writes, and the submission and validation entries. Components (<c>Form</c>, <c>Field</c>,
/// <c>FieldArray</c>, <c>ErrorMessage</c>), <c>defineRule</c> custom rules, and yup/zod schema
/// adapters are outside the first slice.
/// </remarks>
[ECMAScript("vee-validate")]
[Description("@#")]
public static partial class VeeValidate
{
}
