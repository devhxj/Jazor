# ECMAScript.DateFns

date-fns 4 的 C# 绑定，作为 Jazor 的 JS resource library 交付：包内锁定上游版本，
`manifest.json`（schema 2）和 `inventory.json` 记录 npm package、入口、完整性与许可证元数据。

Strongly typed C# bindings for date-fns 4, shipped as a Jazor JS resource library with a locked
upstream npm version. The package carries metadata in `manifest.json` (schema 2) and
`inventory.json`; Emit restores the runtime package into the generated `jazor/node_modules` project.

## 上游锁定 Upstream lock

| 项 | 值 |
| --- | --- |
| npm 包 | `date-fns` |
| 版本 | 见 `manifest.json` / `inventory.json` |
| 许可证 | MIT（`licenses/LICENSE`） |
| 入口 | `date-fns`（函数 barrel）、`date-fns/locale`（精选 locale 桥） |

上游以 npm registry SHA-512 integrity 锁定；`date-fns/locale` 是按上游 exports 验证的精选
locale 入口，由生成器维护。

## 首期范围 First slice

- 解析：`toDate`、`parse`、`parseISO`、`parseJSON`、`fromUnixTime`
- 格式化：`format`、`formatISO`、`formatDistance`、`formatDistanceStrict`、`formatDistanceToNow`、`formatDistanceToNowStrict`、`formatDuration`、`formatRelative`
- 加减：`add`、`sub`、`addDays` … `addYears`、`subDays` … `subYears`、`addBusinessDays`、`subBusinessDays`
- 比较：`isBefore`、`isAfter`、`isEqual`、`isDate`、`isValid`、`compareAsc`、`compareDesc`
- 区间：`differenceInMilliseconds` … `differenceInYears`、`intervalToDuration`、`isWithinInterval`、`areIntervalsOverlapping`、`eachDayOfInterval`、`min`、`max`、`clamp`
- 默认选项：`getDefaultOptions`、`setDefaultOptions`
- locale：`DateFnsLocale.EnUS`/`ZhCN` 等精选 locale（见 `Types/DateFnsLocale.generated.cs`）

未绑定：`context` 构造函数（TZDate 扩展点）、`fp` 函数式入口、`startOf*`/`endOf*`/`set*`/`get*` 族、ISO 周族与其他长尾函数。需要时按需扩充 `Api/DateFns.Api.cs` 并重跑生成器。

Not bound yet: the `context` constructor (TZDate extension seam), the `fp` functional entry, the `startOf*`/`endOf*`/`set*`/`get*` families, ISO-week families, and other long-tail functions. Extend `Api/DateFns.Api.cs` and rerun the generator on demand.

## 使用示例 Authoring

```csharp
using static ECMAScript.DateFns;
using static ECMAScript.DateFnsLocale;

var text = Format(AddDays(ParseISO("2026-09-17"), 3), "yyyy-MM-dd");
var localized = Format(date, "MMM d日", new DateFnsFormatOptions { Locale = ZhCN });
var left = FormatDistanceToNow(ParseISO("2026-01-01"), new DateFnsFormatDistanceOptions { AddSuffix = true });
```

## 再生成 Regeneration

```bash
dotnet run --file scripts/csharp/generate-date-fns.cs -- --version <upstream-version>
```

生成器会重新下载并校验 npm tarball，更新 `manifest.json`、`inventory.json` 与 locale 契约；
升级上游时必须复核 bound 函数与 locale 清单的 contract drift。

## 测试 Tests

`src/ECMAScript.DateFns.Test` 覆盖：import/manifest/inventory 元数据一致性、npm integrity、
上游导出 drift 检查、编译器 emission（对象字面量、枚举选项、locale 导入）。
