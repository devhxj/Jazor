# ECMAScript.VeeValidate

vee-validate 的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游版本、`manifest.json`（schema 2）、`dist/` 资源、许可证与 inventory。

Strongly typed C# bindings for vee-validate, shipped as a Jazor JS resource library with a locked upstream version, package-local `manifest.json` (schema 2), `dist/` runtime assets, licenses, and inventory.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| 作者入口 | `vee-validate` |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 许可证 | MIT（`licenses/vee-validate-LICENSE`） |
| peer 依赖 | `vue`（由 `ECMAScript.Vue` 资源库提供） |

上游发布自包含 ESM bundle（`dist/vee-validate.mjs`），闭包只有该入口一个模块，唯一裸导入是 `vue`。

## 首期范围 First slice

- `useForm`、`useFormContext`、`useField`、`useFieldArray`
- 字段状态：`useFieldError`、`useFieldValue`、`useIsFieldDirty/Touched/Valid`
- 表单状态：`useFormErrors`、`useFormValues`、`useIsFormDirty/Touched/Valid`、`useIsSubmitting`、`useIsValidating`、`useSubmitCount`
- 状态写入：`useSetFieldError/Touched/Value`、`useSetFormErrors/Touched/Values`、`useResetForm`
- 校验与提交：`useValidateField`、`useValidateForm`、`useSubmitForm`、`configure`

未绑定：`Form`/`Field`/`FieldArray`/`ErrorMessage` 组件、`defineRule`/`normalizeRules` 自定义规则（需要 `object` 值域）、`validationSchema` 的 yup/zod 适配器（需要 `@vee-validate/*` 独立包）、`validate`/`validateObject` 直接校验入口。需要时按需扩充 `Api/VeeValidate.Api.cs`。

Not bound: the `Form`/`Field`/`FieldArray`/`ErrorMessage` components, `defineRule`/`normalizeRules` custom rules (they need an `object` value domain), yup/zod `validationSchema` adapters (they need separate `@vee-validate/*` packages), and the direct `validate`/`validateObject` entries.

## SSR 边界 SSR boundary

`useForm`、`useField` 与状态读写组合式函数依赖组件实例与 provide/inject，必须在组件 setup 作用域内调用；`handleSubmit` 产生的提交处理器依赖浏览器事件与 DOM 生命周期。SSR 期间不要调用这些组合式函数，包也不提供服务器端等价实现。

## 使用示例 Authoring

```csharp
using static ECMAScript.VeeValidate;

var form = UseForm(new VeeValidateFormOptions<LoginForm>
{
    InitialValues = new LoginForm { Email = "", Password = "" },
    ValidateOnMount = false
});

var email = UseField<string>("email", "required|email");
var submit = form.HandleSubmit(values => SignIn(values));

// 渲染：submit 直接绑定到提交事件；email.ErrorMessage 显示错误
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-vee-validate.cs -- --version <version>
dotnet run --file scripts/csharp/generate-vee-validate.cs -- --source .tmp/p3b/node_modules
```

## 测试 Tests

`src/ECMAScript.VeeValidate.Test` 覆盖 import/manifest/inventory 元数据、vendored 哈希、上游导出 drift 与编译器 emission；`Jazor.EmitTest` 覆盖真实 materialization 闭包。
