#:property CETCompat=false
#:property PublishAot=false
#:project ../Jolt.Compiler/Jolt.Compiler.csproj

using Jolt.Compiler;
using Jolt.Npm;

var root = Path.GetFullPath("samples/JazorComponentProbe/Npm");
var provider = new NpmVueMetadataProvider(Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory")
    ?? Path.GetFullPath(".tmp/nodeapi-store"));
var current = Path.Combine(root, "current", "components", "Counter.jazor");
var previous = Path.Combine(root, "previous", "Counter.jazor");
var exports = await provider.ListExportsAsync(current, "reka-ui");
Check(exports.Contains("CollapsibleRoot"), "component export discovery");
Check(!exports.Contains("useForwardProps"), "non-component omission");
Check(NpmNames.NamespaceFor("@scope/my-package/widget") == "Npm.Scope.MyPackage.Widget", "scoped subpath namespace");
Check(NpmNames.TagFor("@scope/my-package/widget", "default") == "Widget", "default export tag");
var collapsible = await provider.ReadAsync(current, "reka-ui", "CollapsibleRoot");
Check(collapsible.Props["open"] is { Type: "boolean", Required: false }, "boolean prop");
Check(collapsible.Events["update:open"].Signatures is [[{ Name: "value", Type: "boolean" }]], "update event tuple payload");
Check(collapsible.Slots["default"].Signatures[0][0].Type.Contains("open: boolean"), "scoped slot");
Check(collapsible.Slots["default"].Signatures[0][0].Properties?["open"] == "boolean", "structured slot payload");
Check(collapsible.Props["open"].Definition is { Length: > 0 }, "prop definition");
Check(ReferenceEquals(collapsible, await provider.ReadAsync(current, "reka-ui", "CollapsibleRoot")), "cached metadata");
var generic = await provider.ReadAsync(current, "reka-ui", "SwitchRoot");
Check(generic.Props["disabled"].Type == "boolean" && generic.Events.ContainsKey("update:modelValue")
    && generic.Slots.ContainsKey("default"), "generic Vue callable component metadata");
var virtualizer = await provider.ReadAsync(current, "reka-ui", "ListboxVirtualizer");
Check(virtualizer.TypeParameter is { Name: "T" } typeParameter && typeParameter.PrimitiveKinds.Contains("string") &&
    virtualizer.Props["options"] is { Type: "T[]", Required: true } &&
    virtualizer.Props["textContent"].Type == "(option: T) => string" &&
    virtualizer.Slots["default"].Signatures[0][0].Properties?["option"] == "T",
    "published generic collection, callback and slot preserve the declared type parameter");
const string virtualizerSource = """
    @using Npm.RekaUi
    <ListboxVirtualizer Options="@Names" TextContent="@Label">
        <template #default context="row"><span>@row.option.Length</span></template>
    </ListboxVirtualizer>
    @code {
        private string[] Names = ["One"];
        private string Label(string item) => item;
    }
    """;
var virtualizerOptions = new JoltCompileOptions(NpmComponents: [virtualizer]);
var typed = await JoltProjectCompiler.CompileAsync([new("App.jazor", virtualizerSource)], virtualizerOptions);
Check(typed.Diagnostics.Count == 0 && typed.Modules.Single().Content.Contains("row[\"option\"].length"), "generic npm lowering");
foreach (var (invalid, diagnostic) in new[]
{
    (virtualizerSource.Replace("Label(string item) => item", "Label(bool item) => \"bad\""), "CS1503"),
    (virtualizerSource.Replace("row.option.Length", "row.option.Missing"), "CS1061"),
    (virtualizerSource.Replace("string[] Names = [\"One\"]", "bool[] Names = [true]"), "JOLT1021"),
})
{
    var invalidOutput = await JoltProjectCompiler.CompileAsync([new("App.jazor", invalid)], virtualizerOptions);
    Check(invalidOutput.Modules.Count == 0 && invalidOutput.Diagnostics.Any(item => item.Id == diagnostic), $"generic npm diagnostic {diagnostic}");
}
var number = await provider.ReadAsync(current, "reka-ui", "NumberFieldRoot");
Check(number.Version == "2.11.0" && number.Props.ContainsKey("allowInvalid"), "new package declaration");
var oldNumber = await provider.ReadAsync(previous, "reka-ui", "NumberFieldRoot");
Check(oldNumber.Version == "2.10.1" && !oldNumber.Props.ContainsKey("allowInvalid"), "previous package declaration");

var temporary = Directory.CreateTempSubdirectory("jolt-npm-");
try
{
    var project = temporary.FullName;
    var package = Directory.CreateDirectory(Path.Combine(project, "fixture")).FullName;
    await File.WriteAllTextAsync(Path.Combine(project, "package.json"), """
        {"private":true,"dependencies":{"component-fixture":"file:./fixture","vue":"3.5.42"}}
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "package.json"), """
        {"name":"component-fixture","version":"1.0.0","type":"module","exports":{".":{"types":"./index.d.ts","import":"./index.js"},"./widgets":{"types":"./index.d.ts","import":"./index.js"},"./values":{"types":"./values.d.ts","import":"./values.js"},"./other":{"types":"./other.d.ts","import":"./other.js"}}}
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "index.js"), "export default { render() {} };");
    await File.WriteAllTextAsync(Path.Combine(package, "index.d.ts"), """
        import type { DefineComponent } from 'vue';
        interface WidgetProps { tone: 'quiet' | 'strong'; }
        declare const Widget: DefineComponent<WidgetProps>;
        export default Widget;
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "values.js"), """
        export const count = 3;
        export default (value, transform) => transform(value);
        export function echo(value) { return value; }
        export function generic(value) { return value; }
        export function overloadedCallback(callback) { callback(3); }
        export function readonlyShallow() { return { value: 3 }; }
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "values.d.ts"), """
        import type { ShallowRef } from 'vue';
        export declare const count: 3;
        export default function apply(value: number, transform: (value: number) => string): string;
        export declare function echo(value: number): number;
        export declare function echo(value: string): string;
        export declare function generic<T>(value: T): T;
        export declare function overloadedCallback(callback: { (value: string): void; (value: number): void }): void;
        export declare function readonlyShallow(): Readonly<ShallowRef<number>>;
        export type { echo as TypeOnly };
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "other.js"), """
        export function echo(value) { return value; }
        export const foo_bar = 1, fooBar = 2, $invalid = 3;
        export function duplicateFields() { return { foo_bar: 1, fooBar: 2 }; }
        export function invalidField() { return { "['fake']": 1 }; }
        export function headers() { return { 'content-type': 'text/plain' }; }
        export function moveDate(date, days) { return new Date(+date + days * 86400000); }
        export function taggedDate(date) { return date; }
        export function intervalStart(interval) { return interval.lowerBound; }
        export function taggedIntervalStart(interval) { return interval.lowerBound; }
        export function optionalIntervalStart(interval) { return interval.lowerBound; }
        export function localDate(date) { return date; }
        export function standardDateCheck(value) { return value instanceof Date; }
        export function localDateCheck(value) { return typeof value?.ticks === 'number'; }
        export function plainDateCheck(value) { return Boolean(value); }
        export function overloadedDateCheck(value) { return typeof value === 'number' ? value : Boolean(value); }
        export function getElement(element) { return element; }
        export function getInput(input) { return input; }
        export function localInput(input) { return input; }
        export function augmentedInput(input) { return input; }
        export function localElement(element) { return element; }
        export function useEventListener(target, event, listener) { return () => {}; }
        export const registerDom = useEventListener, registerLocal = useEventListener;
        export function deferCall(fn, delay) { return (...args) => Promise.resolve(fn(...args)); }
        export const collectBatches = () => ({}), fixedBatches = collectBatches, undefinedBatches = collectBatches,
            objectBatches = collectBatches;
        export function submitRecord(handler) { return event => Promise.resolve(handler({}, event)); }
        export const unrelatedSubmit = submitRecord;
        export function firstLabel(label) { return label; }
        export function initial(value = "ready") { return value; }
        export function pick() { return 1; }
        export function constrained(value) { return value; }
        export function opaque() { return {}; }
        export function formatter() { return { label: key => key, ambiguous: key => key, tagged: key => key }; }
        export function getLabel(label) { return label === 'known' ? 1 : label; }
        export function classify(signal) { return signal; }
        export function later(date) { return Promise.resolve(date); }
        export function flush() { return Promise.resolve(); }
        export function schedule(deadline, obtain) { return obtain(); }
        export function counterOnly(obtain) { return Promise.resolve(1); }
        export function constrainedJob(obtain) { return obtain(); }
        export function localPromise() { return { state: 1 }; }
        export function namedInput(options) { return options; }
        export function secondNamedInput(options) {}
        export function genericInput(options) {}
        export function anotherInput(options) {}
        export function textInput(options) {}
        export function wideRef() { return { value: 'initial' }; }
        export function callbackResult(options) { return { data: { value: options.load() }, readTuple: pair => pair[0] }; }
        export function createScore(options) { return { data: { value: options.read() }, score: options.read(1) + 1 }; }
        export function measureText(options) { const value = options.fetch(); return { data: { value }, extra: typeof value === "string" ? value.length : false }; }
        export function measureMappedText(options) { const value = options.fetch(); return { data: { value }, extra: typeof value === "string" ? value.length : false, nested: { stable: true, extra: typeof value === "string" ? value.length : false } }; }
        export function narrowRef() { return { value: 'initial' }; }
        export function localeRef() { return { value: 'en' }; }
        export function narrowLocaleRef() { return { value: 'initial' }; }
        export function editWideRef(ref) { ref.value = null; }
        export function countDates(dates) { return dates.length; }
        export function replaceDate(dates) { dates[0] = 0; }
        export function flipValue(values) { values[0] = !values[0]; }
        export function replaceLiteral(values) { values[0] = "fixed"; }
        export function optionalNumbers() { return {}; }
        export function paint(settings) { return settings.tone; }
        export const tooling = { setMode(mode) { if (mode === "verbose") return; } };
        export const fakeTooling = { setMode(mode) { if (mode === "verbose") return; } };
        export function observeTarget() { return {}; }
        export const companion = { install(app) {} };
        export function assemble() { return { install(app) {}, release() {} }; }
        export function dubious() { return { install(app) {}, release() {} }; }
        export function configure() { return { install(app) {}, release() {} }; }
        export function conditional() { return { install(app) {}, release() {} }; }
        export function assembleVariant({ fast }) { return { install(app) {}, get state() { return fast ? { label: "fast" } : { count: 1 }; } }; }
        export function assembleVariantWithFastTrue() { return 1; }
        export const unrelatedDiscriminator = assembleVariant, unrelatedOverload = assembleVariant, buildAlternate = assembleVariant;
        export function emptyRegistration() { return { install(app) {}, release() {} }; }
        export const requiresConfiguration = { install(app, options) {} };
        export const needsNumber = { install(app) {} };
        export const invalidInstaller = { install: 12 };
        export function firstStage() { return {}; }
        export function secondStage() { return {}; }
        export function applyStages(options) { options.stages[0] = false; }
        export function assess(key, evaluator, config) { return { current: { value: config.seed } }; }
        export function assessOptional(id, evaluate, settings) { return { current: { value: settings?.seed }, evaluated: false }; }
        export function assessImmediately(key, evaluator, config) { return { current: { value: config.seed } }; }
        export function assessDeferred(key, evaluator, config) { return { current: { value: config.seed } }; }
        export function assessDifferent(key, evaluator, config) { return { current: { value: config.seed } }; }
        export function dispatchLiteral(key, evaluator, config) { return { current: { value: config.seed } }; }
        export function dispatchCustom(key, evaluator, config) { return { current: { value: config.seed } }; }
        export function assembleDraft(options) { return {}; }
        export function mismatchedDraft(options) { return {}; }
        export function restrictedDraft(options) { return {}; }
        export function collectNotices() { return {}; }
        export function collectSnapshot() { return {}; }
        export function selectCurrent() { return {}; }
        export function selectConstrained() { return {}; }
        export function selectDefault() { return {}; }
        export function writeNotices() { return () => {}; }
        export function markReview() { return () => {}; }
        export function patchUnchecked() { return () => {}; }
        export function markNarrow() { return () => {}; }
        export function restoreDraft() { return () => {}; }
        export function restoreRequired() { return () => {}; }
        export function inspectDraft() { return () => Promise.resolve({ approved: true, problems: [] }); }
        export function inspectOpaque() { return () => Promise.resolve({}); }
        export function trackRows() { return {}; }
        export function replaceRecord() { return () => {}; }
        export function replaceSelected() { return () => {}; }
        export function replaceRequired() { return () => {}; }
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "other.d.ts"), """
        import type { ComputedRef, Ref } from 'vue';
        type Draft = Record<string, unknown>;
        type Sparse<T> = { [K in keyof T]?: T[K] };
        interface Decoder<TSource, TDestination> {
            marker: 'transformed-draft';
            decode(source: TSource): Promise<{ entity?: TDestination; issues: { trail?: string; notes: string[] }[] }>;
        }
        interface SwappedDecoder<TSource, TDestination> {
            marker: 'transformed-draft';
            decode(source: TDestination): Promise<{ entity?: TSource; issues: { trail?: string; notes: string[] }[] }>;
        }
        type Commit<TSource, TDestination> = <TResult>(
            accept: (entity: TDestination) => TResult,
            reject?: (incident: { draft: TSource; problems: Sparse<Record<keyof TSource, string>> }) => void
        ) => (signal?: globalThis.Event) => Promise<TResult | undefined>;
        interface DraftHandle<TSource, TDestination> {
            snapshot: TSource;
            notices: ComputedRef<Sparse<Record<keyof TSource, string>>>;
            transformedNotices: ComputedRef<Sparse<Record<keyof TDestination, string>>>;
            numericNotices: ComputedRef<Sparse<Record<keyof TSource, number>>>;
            lookalikeNotices: { readonly value: Sparse<Record<keyof TSource, string>> };
            commit: Commit<TSource, TDestination> & { controlled: Commit<TSource, TDestination> };
            replace(patch: Sparse<TSource>, amend?: boolean): void;
        }
        export declare function assembleDraft<TDecoder extends Decoder<TSource, TDestination>,
            TSource extends Draft, TDestination extends Draft = TSource>(
            setup: { startingPoint: Sparse<TSource>; adapter: TDecoder extends Decoder<TSource, TDestination> ? Decoder<TSource, TDestination> : never }
        ): DraftHandle<TSource, TDestination>;
        export declare function mismatchedDraft<TSource extends Draft, TDestination extends Draft = TSource,
            TDecoder extends SwappedDecoder<TSource, TDestination> = SwappedDecoder<TSource, TDestination>>(
            setup: { startingPoint: Sparse<TSource>; adapter: TDecoder extends SwappedDecoder<Draft, Draft> ? SwappedDecoder<TSource, TDestination> : never }
        ): DraftHandle<TSource, TDestination>;
        export declare function restrictedDraft<TSource extends { joltInput: string },
            TDestination extends Draft = TSource, TDecoder extends Decoder<TSource, TDestination> = Decoder<TSource, TDestination>>(
            setup: { startingPoint: Sparse<TSource>; adapter: TDecoder extends Decoder<TSource, TDestination> ? Decoder<TSource, TDestination> : never }
        ): DraftHandle<TSource, TDestination>;
        export declare function collectNotices<TSource extends Draft = Draft>(): ComputedRef<Sparse<Record<keyof TSource, string>>>;
        export declare function collectSnapshot<TSource extends Record<string, any> = Record<string, any>>(): ComputedRef<Sparse<TSource>>;
        export declare function selectCurrent<TSelected = unknown>(selector?: string | Ref<string> | (() => string)): ComputedRef<TSelected>;
        export declare function selectConstrained<TSelected extends Draft>(selector?: string): ComputedRef<TSelected>;
        export declare function selectDefault<TSelected = string>(selector?: string): ComputedRef<TSelected>;
        export declare function writeNotices(): (notes: Sparse<Record<string, string | string[]>>) => void;
        export declare function markReview(): (markers: Record<string, boolean> | boolean) => void;
        export declare function patchUnchecked(): (patch: Record<string, unknown>) => void;
        export declare function markNarrow(): (markers: Record<string, boolean> | true) => void;
        export declare function restoreDraft<TData extends Draft = Draft>(): (state?: { items?: TData; visits?: number }) => void;
        export declare function restoreRequired<TData extends Draft = Draft>(): (state: { items: TData; visits?: number }) => void;
        export declare function inspectDraft<TParsed>(key?: string): () => Promise<{ approved: boolean; problems: string[]; decoded?: TParsed }>;
        export declare function inspectOpaque<TParsed>(key?: string): () => Promise<{ decoded: TParsed }>;
        interface RunRow<TData> { payload: TData; isHead: boolean; isTail: boolean; }
        export declare function trackRows<TData = unknown>(key: string): {
            rows: Ref<RunRow<TData>[]>;
            drop(index: number): void;
            add(payload: TData): void;
        };
        export declare function replaceRecord<TData extends Draft = Draft>(): (data: TData, enabled?: boolean) => void;
        export declare function replaceSelected<TData = unknown>(key?: string): (data: TData, enabled?: boolean) => void;
        export declare function replaceRequired<TData extends { key: string }>(): (data: TData) => void;
        export declare namespace tooling { function setMode(mode: string): void; }
        export declare const fakeTooling: { setMode(mode: string): void };
        interface StopListening { release(): void }
        interface ObservedTarget {
            readonly observe: (listener: (event: { readonly total: number }) => void) => StopListening;
            readonly requiresResult: (listener: (event: { readonly total: number }) => string) => StopListening;
            readonly uncertainPayload: (listener: (event: { readonly total: number } | symbol) => void) => StopListening;
        }
        export declare function observeTarget(): ObservedTarget;
        export declare const companion: { install(app: any, options?: { flag: boolean }): void };
        interface Configuration { specialized?: boolean; tag?: string; catalog?: Record<string, Record<string, string>> }
        interface Registration { install(app: import('vue').App): void; release(): void }
        export declare function assemble<T extends Configuration>(settings: T): T['specialized'] extends true ? Registration : Registration;
        export declare function dubious<T extends Configuration & { token: symbol }>(settings: T): Registration;
        export declare function configure<T extends { fast: boolean; strict?: boolean; count: number; ignored?: symbol }>(choices: T): Registration;
        export declare function conditional<T extends { enabled?: boolean }>(choices: T): T['enabled'] extends true ? string : Registration;
        interface VariantRegistration<Fast = unknown> extends Registration { readonly state: Fast extends true ? { label: string } : Fast extends false ? { count: number } : unknown }
        type FastRegistration = VariantRegistration<true>;
        type SlowRegistration = VariantRegistration<false>;
        export declare function assembleVariant<T extends { fast?: boolean }>(choices: T): T['fast'] extends true ? FastRegistration : T['fast'] extends false ? SlowRegistration : VariantRegistration<T['fast']>;
        export declare function assembleVariantWithFastTrue(): number;
        export declare function unrelatedDiscriminator<T extends { fast?: boolean }, U extends { fast?: boolean } = { fast: false }>(choices: T): U['fast'] extends true ? FastRegistration : SlowRegistration;
        export declare function unrelatedOverload(choices: { fast: true }): number;
        export declare function unrelatedOverload<T extends { fast?: boolean }>(choices: T): T['fast'] extends true ? FastRegistration : T['fast'] extends false ? SlowRegistration : VariantRegistration<T['fast']>;
        export declare function buildAlternate(choices: { fast: true }): Pick<Registration, 'install'>;
        export declare function buildAlternate<T extends { fast?: boolean }>(choices: T): VariantRegistration<T['fast']>;
        export declare function emptyRegistration<T extends { token?: symbol }>(choices: T): Registration;
        export declare const requiresConfiguration: { install(app: import('vue').App, options: { flag: boolean }): void };
        export declare const needsNumber: { install(app: number): void };
        export declare const invalidInstaller: { install: number };
        type Expand<T> = { [K in keyof T]: T[K] };
        type FirstStage = Expand<{ label: string; run(context: symbol): symbol }>;
        type SecondStage = Expand<{ label: string; run(context: symbol): symbol }>;
        export declare function firstStage(): FirstStage;
        export declare function secondStage(): SecondStage;
        export declare function applyStages(options: { stages: (FirstStage | false | null)[] }): void;
        type Verdict = boolean | string;
        export declare function assess<T>(id: string, evaluate: (candidate: T) => Verdict | Promise<Verdict>,
            settings: { seed: T }): { current: Ref<T> };
        export declare function assessOptional<T = unknown>(id: string, evaluate?: string | ((candidate: T) => Verdict),
            settings?: { seed: T }): { current: Ref<T>; readonly evaluated: boolean; accept(candidate: T): void };
        export declare function assessImmediately<T>(id: string, evaluate: (candidate: T) => string,
            settings: { seed: T }): { current: Ref<T> };
        export declare function assessDeferred<T>(id: number, evaluate: (candidate: T) => Promise<number>,
            settings: { seed: T }): { current: Ref<T> };
        export declare function assessDifferent<T>(id: string, evaluate: (candidate: boolean) => Verdict | Promise<Verdict>,
            settings: { seed: T }): { current: Ref<T> };
        export declare function dispatchLiteral<T>(id: string, evaluate: (candidate: T) => "one" | Promise<true>,
            settings: { seed: T }): { current: Ref<T> };
        export declare function dispatchCustom<T>(id: string, evaluate: (candidate: T) => Shadow.Promise<number>,
            settings: { seed: T }): { current: Ref<T> };
        declare namespace Shadow { interface Promise<T> { value: T } }
        export declare function echo(value: boolean): boolean;
        export declare const foo_bar: number, fooBar: number, $invalid: number;
        export declare function duplicateFields(): { foo_bar: number; fooBar: number };
        export declare function invalidField(): { "['fake']": number };
        export declare function headers(): { 'content-type': string };
        export declare function moveDate<D extends Date, R extends Date = D>(
            date: D | number | string, days: number, options?: { in?: (date: Date | number | string) => R }): R;
        export declare function taggedDate<D extends Date & { readonly tag: string }>(date: D): D;
        interface DateInterval { lowerBound: Date | number | string; "upper-bound": Date | number | string }
        export declare function intervalStart<I extends DateInterval, O extends { step: number } | undefined = undefined>(
            interval: I, option?: O): I['lowerBound'] extends Date ? I['lowerBound'] : Date;
        export declare function taggedIntervalStart<I extends DateInterval & { kind: 'tagged' }>(interval: I): Date;
        export declare function optionalIntervalStart<I extends Partial<DateInterval>>(interval: I): Date;
        export declare namespace Domain {
            interface Date { ticks: number }
            interface Element { label: string }
            interface Promise<T> { state: T }
        }
        export declare function localDate(date: Domain.Date): Domain.Date;
        export declare function standardDateCheck(value: unknown): value is Date;
        export declare function localDateCheck(value: unknown): value is Domain.Date;
        export declare function plainDateCheck(value: unknown): boolean;
        export declare function overloadedDateCheck(value: number): number;
        export declare function overloadedDateCheck(value: unknown): boolean;
        export declare function getElement(element: Element): Element;
        export declare function getInput(input: HTMLInputElement): HTMLInputElement;
        declare namespace LocalInputs {
            interface HTMLInputElement { marker: string }
        }
        export declare function localInput(input: LocalInputs.HTMLInputElement): LocalInputs.HTMLInputElement;
        declare global { interface HTMLTextAreaElement { packageMarker: symbol } }
        export declare function augmentedInput(input: HTMLTextAreaElement): HTMLTextAreaElement;
        export declare function localElement(element: Domain.Element): Domain.Element;
        export declare namespace palette {
            interface Colors { [key: string]: string }
            interface Rule { token: string }
            function apply(theme: { colors: Colors; rules: Rule[] }): void;
        }
        export declare namespace requiredPalette {
            interface Colors { [key: string]: string; background: string }
            function apply(theme: { colors: Colors }): void;
        }
        interface EventTarget { readonly kind: 'local'; }
        interface Event { readonly count: number; }
        export declare function useEventListener<E = Event>(target: EventTarget, event: string, listener: (event: E) => void): () => void;
        export declare function registerDom<E extends keyof globalThis.HTMLElementEventMap>(target: HTMLElement, event: E, listener: (event: globalThis.HTMLElementEventMap[E]) => void): () => void;
        export declare function registerDom<E = globalThis.Event>(target: globalThis.EventTarget, event: string, listener: (event: E) => void): () => void;
        interface HTMLElementEventMap { keydown: KeyboardEvent; }
        export declare function registerLocal<E extends keyof HTMLElementEventMap>(target: HTMLElement, event: E, listener: (event: HTMLElementEventMap[E]) => void): () => void;
        export declare function registerLocal<E = globalThis.Event>(target: globalThis.EventTarget, event: string, listener: (event: E) => void): () => void;
        export declare function deferCall<F extends (...args: any[]) => unknown>(callback: F, delay: number): (...args: Parameters<F>) => Promise<Awaited<ReturnType<F>>>;
        type BatchOptions<P, C, Stop = null> = {
            queryKey: string[];
            initialPageParam: C;
            queryFn: (context: { pageParam: C }) => Promise<P>;
            getNextPageParam: (last: P, pages: P[], cursor: C, cursors: C[]) => C | Stop;
        };
        type BatchResult<P> = { data: import('vue').Ref<{ pages: P[] } | undefined> };
        export declare function collectBatches<P, C>(options: BatchOptions<P, C>): BatchResult<P>;
        export declare function fixedBatches<P, C extends string>(options: BatchOptions<P, C>): BatchResult<P>;
        export declare function undefinedBatches<P, C>(options: BatchOptions<P, C, undefined>): BatchResult<P>;
        type ObjectPage<P> = P extends object ? string : P;
        export declare function objectBatches<P, C>(options: BatchOptions<P, C>): BatchResult<ObjectPage<P>>;
        export declare function submitRecord<T extends Record<string, unknown>>(handler: (values: T, context: { submitCount: number }) => unknown): (event?: globalThis.Event) => Promise<unknown>;
        export declare function unrelatedSubmit<T extends Record<string, unknown>>(handler: (values: T, context: { submitCount: number }) => unknown): (event?: string) => Promise<unknown>;
        export declare enum SignalLevel { Quiet = -2, Loud = 17, loud = 19, "loud-mode" = 23 }
        export declare enum UnrelatedLevel { Quiet = -2, Loud = 17 }
        export declare function classify(level: number): number;
        export declare function classify(level: SignalLevel): string;
        export declare namespace signals {
            interface Entry { level: SignalLevel; text: string }
            interface Seen { level: SignalLevel; text: string; owner: string }
            function write(entries: Entry[]): void;
            function read(filter: { owner?: string }): Seen[];
            function unrelated(): UnrelatedLevel;
        }
        export declare function firstLabel(label: string, special?: symbol): string;
        export declare function initial<T = string>(value?: T): T;
        export declare function pick(): number;
        export declare function pick<T = string>(): T;
        export declare function constrained<T = number>(value: T): T;
        export declare function opaque<T = unknown>(): T;
        export declare function formatter(): {
            label: {
                <Key extends string>(key: Key | number): string;
                (key: string, count: number): string;
                <Key extends string>(key: Key, replacements: Record<string, unknown>): string;
            };
            ambiguous: { (key: 'count'): number; <Key extends string>(key: Key): string };
            tagged: <Key extends string & { tag: true }>(key: Key) => string;
            numeric: <Key extends string>(key: Key, details: Record<string, number>) => string;
        };
        export declare function getLabel(label: 'known'): number;
        export declare function getLabel(label: string): string;
        export declare function later(date: Date): Promise<Date>;
        export declare function flush(): Promise<void>;
        export declare function schedule<R>(deadline: number, obtain: () => Promise<R>): Promise<R>;
        export declare function counterOnly<R>(obtain: () => Promise<R>): Promise<number>;
        export declare function constrainedJob<R extends string>(obtain: () => Promise<R>): Promise<R>;
        export declare function localPromise(): Domain.Promise<number>;
        interface SharedOptions { count: number }
        interface GenericOptions<T> { count: T }
        export declare function namedInput(options: SharedOptions): SharedOptions;
        export declare function loadNamedInput(): SharedOptions;
        export declare function secondNamedInput(options: SharedOptions): void;
        export declare function genericInput(options: GenericOptions<number>): void;
        type NumericSettings = GenericOptions<number>;
        export declare function anotherInput(options: NumericSettings): void;
        export declare function textInput(options: GenericOptions<string>): void;
        export declare function wideRef(): Ref<string, string | null>;
        export declare function readiness(read: () => unknown): ComputedRef<boolean>;
        export declare function needsInput(read: (value: number) => unknown): ComputedRef<boolean>;
        export declare function unrelatedResult(read: () => unknown): ComputedRef<string>;
        export declare function callbackResult<T = unknown>(options: { load: () => T }): {
            data: Readonly<Ref<T | undefined>>;
            readTuple: (pair: [text: T, count?: number]) => T;
        };
        export declare function createScore<T = unknown>(options: { read: { (): T; (score: number): number } }): {
            data: Readonly<Ref<T | undefined>>;
            score: number;
        };
        type TextMeasure<T> = T extends string ? number : boolean;
        export declare function measureText<T = unknown>(options: { fetch: () => T }): {
            data: Readonly<Ref<T | undefined>>;
            extra: TextMeasure<T>;
        };
        interface NestedMeasure<T> {
            stable: boolean;
            extra: TextMeasure<T>;
        }
        export declare function measureMappedText<T = unknown>(options: { fetch: () => T }): {
            data: Readonly<Ref<T | undefined>>;
            nested: NestedMeasure<T>;
        } & { [K in "extra"]: TextMeasure<T> };
        export declare function narrowRef(): Ref<string, 'initial'>;
        export declare function localeRef(): import('vue').WritableComputedRef<string>;
        export declare function narrowLocaleRef(): import('vue').WritableComputedRef<string, 'initial'>;
        export declare function editWideRef(ref: Ref<string, string | null>): void;
        export declare function countDates(dates: readonly (Date | number | string)[]): number;
        export declare function replaceDate(dates: (Date | number | string)[]): void;
        export declare function flipValue(values: (number | boolean)[]): void;
        export declare function replaceLiteral(values: (Date | "fixed")[]): void;
        export declare function optionalNumbers(): { absent?: number; nullable?: number | null };
        export declare function paint(settings: { tone: "" | "warm-tone" | "warm_tone" }): string;
        """);
    var document = Path.Combine(project, "views", "Widget.jazor");
    var registrations = await provider.ReadValuesAsync(document, "component-fixture/other", ["assemble", "conditional", "configure", "dubious", "emptyRegistration"]);
    Check(registrations is [{ ExportName: "assemble", Signatures: [{ Parameters: [{ Type.Members:
        [{ Name: "specialized", Type.Kind: "boolean" }, { Name: "tag", Type.Kind: "string" },
         { Name: "catalog", Optional: true, Type: { Kind: "dictionary", ElementType.Kind: "dictionary" } }] }],
        ReturnType: { Kind: "vuePlugin", Members: [{ Name: "release", Type.Kind: "method" }] } }] },
        { ExportName: "configure", Signatures: [{ Parameters: [{ Type.Members: [
            { Name: "fast", Optional: false, Type.Kind: "boolean" },
            { Name: "strict", Optional: true, Type.Kind: "boolean" },
            { Name: "count", Optional: false, Type.Kind: "number" }] }], ReturnType.Kind: "vuePlugin" }] },
        { ExportName: "emptyRegistration", Signatures: [{ Parameters: [{ Type.Members: [] }], ReturnType.Kind: "vuePlugin" }] }],
        "generic plugin settings preserve individual supported fields and reject unsupported required options and non-plugin modes");
    var variants = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["assembleVariant", "unrelatedDiscriminator", "unrelatedOverload", "buildAlternate"])).ToDictionary(value => value.ExportName);
    Check(variants["assembleVariant"].Signatures is [
        { ManagedNameSuffix: null },
        { ManagedNameSuffix: "WithFastFalse", Parameters: [{ Type.Members: [{ Name: "fast", FixedBoolean: false }] }],
            ReturnType.Members: { } falseMembers }] && falseMembers.Any(member => member is { Name: "state", Type.Members: [{ Name: "count" }] }),
        "an input discriminator is fixed only on its derived factory and real exports retain their names: " +
        System.Text.Json.JsonSerializer.Serialize(variants["assembleVariant"].Signatures));
    Check(variants["unrelatedDiscriminator"].Signatures.All(signature => signature.ManagedNameSuffix is null) &&
        (!variants.TryGetValue("unrelatedOverload", out var overlapping) ||
            overlapping.Signatures.All(signature => signature.ManagedNameSuffix != "WithFastTrue")),
        "a discriminator must belong to the input and the probe must resolve to the factory overload");
    Check(variants["buildAlternate"].Signatures is [{ ManagedNameSuffix: null, ReturnType: { Kind: "vuePlugin", Members: [] } }],
        "a wide boolean plugin call exposes only the plugin contract when a reachable literal overload omits cleanup");
    var formatter = await provider.ReadValueAsync(document, "component-fixture/other", "formatter");
    Check(formatter.Signatures is [{ ReturnType.Members: [{ Name: "label", Type.Kind: "method", Type.Signatures:
        [{ Parameters: [{ Type.Kind: "string" }], ReturnType.Kind: "string" },
         { Parameters: [{ Type.Kind: "string" }, { Type.Kind: "number" }], ReturnType.Kind: "string" },
         { Parameters: [{ Type.Kind: "string" }, { Name: "replacements", Type: { Kind: "dictionary", ElementType.Kind: "string" } }],
           ReturnType.Kind: "string" }] }] }],
        "returned generic callables support checker-validated scalar and string dictionary inputs");
    Check((await provider.ListSpecifiersAsync(document, "component-fixture")).SequenceEqual(
        ["component-fixture", "component-fixture/other", "component-fixture/values", "component-fixture/widgets"]),
        "fresh install with explicit subpaths");
    var first = await provider.ReadAsync(document, "component-fixture/widgets", "default");
    Check(first.PackageName == "component-fixture", "subpath package identity");
    Check(first.Props["tone"].Required && first.Props["tone"].LiteralValues.SequenceEqual(["quiet", "strong"]), "required literal-union prop");
    Check(first.Definition is { Length: > 0 } && File.Exists(first.Definition.Path), "export definition");
    File.AppendAllText(first.DeclarationPath, "\n");
    var second = await provider.ReadAsync(document, "component-fixture/widgets", "default");
    Check(!ReferenceEquals(first, second), "installed declaration content invalidation");

    var candidates = await provider.ListValueExportsAsync(document, "component-fixture/values");
    Check(candidates.SequenceEqual(["count", "default", "echo", "generic", "overloadedCallback", "readonlyShallow"]), "runtime value candidate discovery");
    var projections = await provider.ReadValueProjectionsAsync(document, "component-fixture/values",
        ["count", "default", "echo", "generic", "overloadedCallback", "readonlyShallow", "TypeOnly", "absent"]);
    var batch = projections.Values;
    Check(projections.Errors.Keys.Order().SequenceEqual(new[] { "absent", "overloadedCallback", "TypeOnly" }.Order()) &&
        projections.Errors["absent"].Contains("no export", StringComparison.Ordinal),
        "batch keeps per-export diagnostics and valid neighboring signatures");
    Check(batch.Single(value => value.ExportName == "readonlyShallow").Signatures is
        [{ ReturnType: { Kind: "ref", ReadOnly: true, Name: null } }], "readonly shallow ref retains read-only projection");
    Check(batch.Count == 5, "batch omits unsupported signatures and type-only exports");
    Check(batch.Single(value => value.ExportName == "count").ConstantType is { Kind: "number" }, "named numeric constant");
    Check(batch.Single(value => value.ExportName == "default").Signatures is
        [{ Parameters: [_, { Type: { Kind: "function", Signatures: [{ ReturnType.Kind: "string" }] } }] }], "default export and callback");
    var echo = batch.Single(value => value.ExportName == "echo");
    Check(echo.Signatures is [{ ReturnType.Kind: "number" }, { ReturnType.Kind: "string" }] &&
        ReferenceEquals(echo, await provider.ReadValueAsync(document, "component-fixture/values", "echo")), "named overloads and shared cache");
    var otherEcho = await provider.ReadValueAsync(document, "component-fixture/other", "echo");
    Check(otherEcho.Signatures is [{ ReturnType.Kind: "boolean" }] && echo.Definition?.Path != otherEcho.Definition?.Path, "same export name in distinct subpaths");
    Check(batch.Single(value => value.ExportName == "generic").Signatures is [{
        TypeParameters: ["T"], Parameters: [{ Type: { Kind: "typeParameter", Name: "T" } }],
        ReturnType: { Kind: "typeParameter", Name: "T" } }],
        "a direct unconstrained generic input retains its identity in the result");
    await CheckUnsupported("component-fixture/other", "useEventListener", "generic, rest or explicit this signature");
    var registerDom = await provider.ReadValueAsync(document, "component-fixture/other", "registerDom");
    Check(registerDom.Signatures.Any(signature => signature.Parameters[1].Type is
        { Name: "KeyboardEventName", StringLiterals: ["keydown", "keypress", "keyup"] }) &&
        registerDom.Signatures.Any(signature => signature.Parameters[1].Type is { Name: "MouseEventName" } mouse &&
            mouse.StringLiterals!.Contains("dblclick")), "renamed DOM event registration derives both key domains: " +
        string.Join(", ", registerDom.Signatures.Select(signature => string.Join("/", signature.Parameters.Select(parameter =>
            parameter.Type.Name + "=" + string.Join(",", parameter.Type.StringLiterals ?? []))))));
    var registerLocal = await provider.ReadValueAsync(document, "component-fixture/other", "registerLocal");
    Check(registerLocal.Signatures is [{ Parameters: [_, { Type.Kind: "string" }, _] }],
        "package-local event map does not acquire standard DOM overloads");
    var deferCall = await provider.ReadValueAsync(document, "component-fixture/other", "deferCall");
    var batches = await provider.ReadValueAsync(document, "component-fixture/other", "collectBatches");
    Check(batches.Signatures.Single().Parameters.Single().Type.Members!.Single(member => member.Name == "queryKey").Type
        is { Kind: "array", ElementType.Kind: "string" }, "narrow paged query keeps the declared string key");
    Check(batches.Signatures is [{ TypeParameters: ["TPage", "TPageParam"], ScalarResultTypeParameter: "TPageParam" }] &&
        batches.Signatures[0].ReturnType.Members!.Single().Type.ElementType is
            { Kind: "undefinedable", ElementType.Members: [{ Name: "pages", Type.ElementType.Name: "TPage" }] },
        "renamed paged query projects independently typed pages and cursors");
    var rejectedBatches = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["fixedBatches", "undefinedBatches", "objectBatches"]);
    Check(rejectedBatches.Values.Count == 0 && rejectedBatches.Errors.Keys.Order(StringComparer.Ordinal)
        .SequenceEqual(["fixedBatches", "objectBatches", "undefinedBatches"]),
        "constrained cursors, conditional pages and undefined-only completion are rejected");
    var submitRecord = await provider.ReadValueAsync(document, "component-fixture/other", "submitRecord");
    Check(submitRecord.Signatures is [{ ReturnType.Signatures: [{ ReturnType: { Kind: "undefinedable", ElementType.Kind: "promise" } }] }],
        "renamed submission factory retains the optional completion Promise");
    await CheckUnsupported("component-fixture/other", "unrelatedSubmit", "generic, rest or explicit this signature");
    Check(deferCall.Signatures.Any(signature => signature.TypeParameters is ["TValue", "TResult"] &&
        signature.ReturnType.Signatures is [{ Parameters: [{ Type.Name: "TValue" }], ReturnType.Kind: "promise" }]),
        "renamed callback factory preserves callback parameter and awaited result slots");
    await CheckUnsupported("component-fixture/values", "TypeOnly", "expected a runtime value");
    await CheckUnsupported("component-fixture/other", "foo_bar", "both project to 'FooBar'");
    await CheckUnsupported("component-fixture/other", "$invalid", "cannot form a C# member");
    await CheckUnsupported("component-fixture/other", "duplicateFields", "both project to 'FooBar'");
    await CheckUnsupported("component-fixture/other", "invalidField", "cannot form a C# identifier");
    var headers = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["foo_bar", "fooBar", "$invalid", "duplicateFields", "invalidField", "headers"]);
    Check(headers is [{ ExportName: "headers", Signatures: [{ ReturnType.Members: [{ Name: "content-type" }] }] }] &&
        NpmNames.ValueNameFor("content-type") == "ContentType", "valid hyphenated property preserves its npm spelling");
    var tooling = await provider.ReadValuesAsync(document, "component-fixture/other", ["tooling", "fakeTooling"]);
    var subscriptions = await provider.ReadValueAsync(document, "component-fixture/other", "observeTarget");
    var plugins = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["companion", "requiresConfiguration", "needsNumber", "invalidInstaller"]);
    Check(plugins is [{ ExportName: "companion", ConstantType.Kind: "vuePlugin" }],
        "Vue permits optional plugin options and rejects required options, incompatible app arguments, and non-callable installers");
    var stages = await provider.ReadValuesAsync(document, "component-fixture/other", ["applyStages", "firstStage", "secondStage"]);
    var firstStage = stages.Single(value => value.ExportName == "firstStage").Signatures.Single().ReturnType;
    var secondStage = stages.Single(value => value.ExportName == "secondStage").Signatures.Single().ReturnType;
    var applyStages = stages.Single(value => value.ExportName == "applyStages").Signatures.Single();
    Check(firstStage is { Kind: "handle", Name: "FirstStage", Members: [{ Name: "label" }] } &&
        secondStage is { Kind: "handle", Name: "SecondStage", Members: [{ Name: "label" }] } &&
        firstStage.TypeDefinition != secondStage.TypeDefinition &&
        applyStages.Parameters is [{ Type: { Kind: "object", Members:
            [{ Name: "stages", Type: { Kind: "inputArray", ElementType.Name: "FirstStage" } }] } }],
        "array inputs preserve named callable protocol identity, data reads, and the constructible options record");
    const string stageSource = """
        @using Package = Npm.ComponentFixture.Other.Exports
        @code {
            private void Apply() {
                Package.ApplyStages(new() {
                    Stages = Package.FirstStageSequence.From(new[] { Package.FirstStage() })
                });
            }
        }
        """;
    var stageOptions = new JoltCompileOptions { NpmValues = stages };
    var stageOutput = await JoltProjectCompiler.CompileAsync([new("Stages.jazor", stageSource)], stageOptions);
    Check(stageOutput.Diagnostics.Count == 0, string.Join(Environment.NewLine, stageOutput.Diagnostics));
    var wrongStage = JoltProjectCompiler.Analyze(
        [new("Stages.jazor", stageSource.Replace("Package.FirstStage()", "Package.SecondStage()", StringComparison.Ordinal))], stageOptions);
    Check(wrongStage.Diagnostics.Any(diagnostic => diagnostic.Id == "CS1503"), "an identically shaped second protocol cannot enter the first sequence");
    var subscription = subscriptions.Signatures.Single().ReturnType.Members!.Single();
    Check(subscription is { Name: "observe", Type.Signatures: [{
        Parameters: [{ Type.Signatures: [{ Parameters: [{ Type.Members: [{ Name: "total", Type.Kind: "number" }] }],
            ReturnType.Kind: "void" }] }],
        ReturnType: { Kind: "handle", Members: [{ Name: "release" }] } }] },
        "typed event registration preserves the callback payload and release handle");
    Check(tooling is [{ ExportName: "tooling", ConstantType.Members:
        [{ Name: "setMode", Type: { Kind: "method", Signatures: [{ Parameters: [{ Type.Kind: "string" }] }] } }] }],
        "runtime namespace methods come from ValueModule symbols rather than shaped object constants");
    const string namespaceSource = """
        @using Package = Npm.ComponentFixture.Other.Exports
        <p>fixture</p>
        @code {
            [Setup]
            private void Configure() { Package.Tooling.SetMode("verbose"); }
        }
        """;
    var namespaceOptions = new JoltCompileOptions { NpmValues = tooling };
    var namespaceOutput = await JoltProjectCompiler.CompileAsync([new("Namespace.jazor", namespaceSource)], namespaceOptions);
    Check(namespaceOutput.Diagnostics.Count == 0 && namespaceOutput.Modules.Single().Content.Contains(".setMode(\"verbose\")"),
        "namespace call retains the JavaScript receiver");
    var invalidNamespaceOutput = await JoltProjectCompiler.CompileAsync(
        [new("Namespace.jazor", namespaceSource.Replace("\"verbose\"", "true"))], namespaceOptions);
    Check(invalidNamespaceOutput.Diagnostics.Any(item => item.Id == "CS1503"), "namespace method rejects wrong argument types");

    var hostExports = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["moveDate", "taggedDate", "intervalStart", "taggedIntervalStart", "optionalIntervalStart", "localDate",
         "getElement", "getInput", "localInput", "augmentedInput", "localElement", "firstLabel", "getLabel"]);
    var projected = hostExports.ToDictionary(value => value.ExportName);
    var predicates = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["standardDateCheck", "localDateCheck", "plainDateCheck", "overloadedDateCheck"]))
        .ToDictionary(value => value.ExportName);
    Check(predicates["standardDateCheck"].Signatures.Select(signature => signature.Parameters.Single().Type.Kind)
        .SequenceEqual(["date", "string", "number", "boolean"]) &&
        !predicates.ContainsKey("localDateCheck"), "only a predicate targeting the standard Date symbol narrows to Date");
    Check(predicates["plainDateCheck"].Signatures.Select(signature => signature.Parameters.Single().Type.Kind)
        .SequenceEqual(["date", "string", "number", "boolean"]) &&
        predicates["overloadedDateCheck"].Signatures is [{ Parameters: [{ Type.Kind: "number" }], ReturnType.Kind: "number" }],
        "a single unknown-to-boolean call exposes the closed host inputs without changing overload selection");
    Check(projected["moveDate"].Signatures.Any(signature => signature.ReturnType.Kind == "date" &&
        signature.Parameters is [{ Type.Kind: "date" }, { Type.Kind: "number" }, ..] &&
        signature.Parameters.Skip(2).All(parameter => parameter.Optional)), "base Date specialization retains Date result");
    Check(!projected.ContainsKey("taggedDate"), "a branded Date constraint cannot accept every Date");
    Check(projected["intervalStart"].Signatures is
        [{ Parameters: [
            { Type.Members: [{ Name: "lowerBound", Type.Kind: "date", Definition: null }, { Name: "upper-bound", Type.Kind: "date", Definition: null }] },
            { Optional: true, Type.Members: [{ Name: "step", Type.Kind: "number", Optional: false }] }],
            ReturnType.Kind: "date" }],
        "date interval specialization retains typed options and resolves the conditional Date result");
    Check(!projected.ContainsKey("taggedIntervalStart"), "TypeScript rejects a date interval that lacks a required brand");
    Check(!projected.ContainsKey("optionalIntervalStart"), "optional dates do not establish a concrete date record");
    Check(projected["localDate"].Signatures is [{ ReturnType.Members: [{ Name: "ticks" }] }] &&
        projected["localElement"].Signatures is [{ ReturnType.Members: [{ Name: "label" }] }],
        "package-local names preserve their declared properties");
    Check(projected["getElement"].Signatures is [{ ReturnType: { Kind: "web", Name: "Element" } }],
        "DOM Element resolves to the existing WebIDL carrier");
    Check(projected["getInput"].Signatures is [{
        Parameters: [{ Type: { Kind: "web", Name: "HTMLInputElement" } }],
        ReturnType: { Kind: "web", Name: "HTMLInputElement" } }] &&
        projected["localInput"].Signatures is [{ ReturnType: { Kind: "object", Members: [{ Name: "marker" }] } }] &&
        (!projected.TryGetValue("augmentedInput", out var augmented) ||
            augmented.Signatures.All(signature => signature.ReturnType.Kind != "web")),
        "only the unmodified DOM interface resolves to the WebIDL carrier");
    const string webSource = """
        @using Dom = Npm.ComponentFixture.Other.Exports
        @using ECMAScript
        <p>dom</p>
        @code {
            public static Element Forward(HTMLInputElement input) => Dom.GetElement(Dom.GetInput(input));
        }
        """;
    var webOptions = new JoltCompileOptions { NpmValues = [projected["getInput"], projected["getElement"], projected["localInput"]] };
    var webCompilation = await JoltProjectCompiler.CompileAsync([new("Dom.jazor", webSource)], webOptions);
    Check(webCompilation.Diagnostics.Count == 0, "HTMLInputElement passes through Element: " + string.Join(", ", webCompilation.Diagnostics));
    var invalidWebCompilation = await JoltProjectCompiler.CompileAsync(
        [new("Dom.jazor", webSource.Replace("Dom.GetInput(input)", "Dom.LocalInput(new() { Marker = \"different\" })"))], webOptions);
    Check(invalidWebCompilation.Diagnostics.Any(item => item.Id == "CS1503"),
        "a package-local HTMLInputElement cannot be passed as a DOM Element");
    Check(projected["firstLabel"].Signatures is [{ Parameters: [{ Type.Kind: "string" }], ReturnType.Kind: "string" }],
        "usable prefix of an unsupported optional parameter");
    Check(!projected.TryGetValue("getLabel", out var labels) || !labels.Signatures.Any(signature =>
        signature.Parameters is [{ Type.Kind: "string" }] && signature.ReturnType.Kind == "string"),
        "overlapping literal overload prevents an unsound broad return type");
    var deferred = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["later", "flush", "localPromise"])).ToDictionary(value => value.ExportName);
    Check(deferred["later"].Signatures is [{ ReturnType: { Kind: "promise", ElementType.Kind: "date" } }] &&
        deferred["flush"].Signatures is [{ ReturnType: { Kind: "promise", ElementType.Kind: "void" } }] &&
        deferred["localPromise"].Signatures is [{ ReturnType.Members: [{ Name: "state", Type.Kind: "number" }] }],
        "only standard promises project as host promises and retain their fulfillment type");
    var forwardedJobs = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["schedule", "counterOnly", "constrainedJob"]);
    Check(forwardedJobs is [{ ExportName: "schedule", Signatures: [{
        TypeParameters: ["R"], ReturnType: { Kind: "promise", ElementType.Name: "R" },
        Parameters: [_, { Type.Signatures: [{ ReturnType: { Kind: "promise", ElementType.Name: "R" } }] }]
    }] }], "a Promise callback carries its own unconstrained result through another export");
    var namedInputs = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["namedInput", "loadNamedInput", "secondNamedInput", "genericInput", "anotherInput", "textInput"])).ToDictionary(value => value.ExportName);
    var firstInput = namedInputs["namedInput"].Signatures.Single();
    Check(firstInput.Parameters.Single().Type is { Name: "SharedOptions", TypeDefinition: { } } named &&
        named.TypeDefinition == namedInputs["secondNamedInput"].Signatures.Single().Parameters.Single().Type.TypeDefinition &&
        firstInput.ReturnType.TypeDefinition == named.TypeDefinition, "named input and output record identity");
    const string reuseResultSource = """
        @using Tools = Npm.ComponentFixture.Other.Exports
        <p>reuse</p>
        @code {
            [Setup]
            private void Forward() {
                Tools.SecondNamedInput(Tools.NamedInput(new() { Count = 2 }));
                Tools.SharedOptionsResult result = Tools.LoadNamedInput();
                result = Tools.NamedInput(new() { Count = 3 });
                Tools.SecondNamedInput(result);
            }
        }
        """;
    var reuseResult = await JoltProjectCompiler.CompileAsync([new("ReuseResult.jazor", reuseResultSource)],
        new() { NpmValues = namedInputs.Values.ToArray() });
    Check(reuseResult.Diagnostics.Count == 0,
        "a named result flows to an input carrying the same declaration: " + string.Join(", ", reuseResult.Diagnostics));
    var signals = await provider.ReadValuesAsync(document, "component-fixture/other", ["signals"]);
    var palette = (await provider.ReadValuesAsync(document, "component-fixture/other", ["palette", "requiredPalette"]))
        .ToDictionary(value => value.ExportName);
    var paletteInput = palette["palette"].ConstantType!.Members!.Single().Type.Signatures!.Single().Parameters.Single().Type;
    Check(paletteInput.Members!.Any(member => member is { Name: "colors", Type: { Kind: "dictionary", ElementType.Kind: "string" } }) &&
        paletteInput.Members!.Any(member => member is { Name: "rules", Type.Kind: "recordArray" }),
        "a pure string index signature composes with record arrays");
    Check(!palette.ContainsKey("requiredPalette"), "a same-named mixed index signature cannot erase a required named property");
    const string signalSource = """
        @using Tools = Npm.ComponentFixture.Other.Exports
        <p>signals</p>
        @code {
            [Setup]
            private void Forward() {
                Tools.Signals.Write([new() { Level = Tools.SignalLevel.Loud, Text = "loud" }]);
                Tools.SignalLevel level = Tools.Signals.Read(new() { Owner = "receiver" })[0].Level;
                Tools.Signals.Write([new() { Level = Tools.SignalLevel.Loud_2, Text = "other" },
                    new() { Level = Tools.SignalLevel.LoudMode, Text = "quoted" }]);
            }
        }
        """;
    var compiledSignals = await JoltProjectCompiler.CompileAsync([new("Signals.jazor", signalSource)],
        new() { NpmValues = signals });
    Check(compiledSignals.Diagnostics.Count == 0 && compiledSignals.Modules.Single().Content.Contains("23", StringComparison.Ordinal) &&
        compiledSignals.Modules.Single().Content.Contains("19", StringComparison.Ordinal),
        "namespaced record arrays retain their declared enum: " + string.Join(", ", compiledSignals.Diagnostics));
    var invalidSignals = await JoltProjectCompiler.CompileAsync([new("Signals.jazor",
        signalSource.Replace("Tools.SignalLevel.Loud", "Tools.UnrelatedLevel.Loud", StringComparison.Ordinal))],
        new() { NpmValues = signals });
    Check(invalidSignals.Diagnostics.Any(diagnostic => diagnostic.Id == "CS0266"),
        "identically valued enums from separate declarations stay distinct: " + string.Join(", ", invalidSignals.Diagnostics));
    var classify = await provider.ReadValueAsync(document, "component-fixture/other", "classify");
    Check(classify.Signatures is [{ Parameters: [{ Type.Kind: "number" }], ReturnType.Kind: "number" }],
        "numeric enum inputs overlap an earlier number overload at runtime");
    const string sharedSettings = """
        @using Tools = Npm.ComponentFixture.Other.Exports
        @code {
            private void Send() {
                var settings = new Tools.GenericOptionsOfNumber { Count = 2 };
                Tools.GenericInput(settings);
                Tools.AnotherInput(settings);
                Tools.TextInput(new() { Count = "ok" });
            }
        }
        """;
    var settingsOptions = new JoltCompileOptions { NpmValues = namedInputs.Values.ToArray() };
    var settingsResult = await JoltProjectCompiler.CompileAsync([new("Settings.jazor", sharedSettings)], settingsOptions);
    Check(settingsResult.Diagnostics.Count == 0, "reuse a closed generic input across a type alias: " +
        string.Join(Environment.NewLine, settingsResult.Diagnostics));
    var mixedSettings = sharedSettings.Replace("Tools.AnotherInput(settings)", "Tools.TextInput(settings)");
    Check(JoltProjectCompiler.Analyze([new("Settings.jazor", mixedSettings)], settingsOptions).Diagnostics
        .Any(diagnostic => diagnostic.Id == "CS1503"), "different generic arguments stay distinct");
    var defaultCalls = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["initial", "pick", "constrained", "opaque"])).ToDictionary(value => value.ExportName);
    Check(defaultCalls.Count == 3 && !defaultCalls.ContainsKey("opaque") &&
        defaultCalls["initial"].Signatures is [{ Parameters: [], ReturnType.Kind: "string" }] &&
        defaultCalls["pick"].Signatures is [{ Parameters: [], ReturnType.Kind: "number" }] &&
        defaultCalls["constrained"].Signatures is [{
            TypeParameters: ["T"], Parameters: [{ Optional: false, Type: { Kind: "typeParameter", Name: "T" } }],
            ReturnType: { Kind: "typeParameter", Name: "T" },
        }], "an omitted call uses the selected overload's result, while a required identity argument stays required");
    var readinessPredicates = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["readiness", "needsInput", "unrelatedResult"]);
    Check(readinessPredicates is [{ ExportName: "readiness", Signatures: [{
        Parameters: [{ Type.Signatures: [{ Parameters.Count: 0, ReturnType.Kind: "boolean" }] }],
        ReturnType: { Name: "VueComputedRef", ElementType.Kind: "boolean" },
    }] }], "renamed unknown getter accepts a boolean predicate only for a compatible computed-ref result");
    var refs = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["wideRef", "narrowRef", "editWideRef", "localeRef", "narrowLocaleRef"])).ToDictionary(value => value.ExportName);
    Check(refs["wideRef"].Signatures is [{ ReturnType: { Kind: "ref", ElementType.Kind: "string", ReadOnly: false } }] &&
        refs["narrowRef"].Signatures is [{ ReturnType: { Kind: "ref", ElementType.Kind: "string", ReadOnly: true } }] &&
        !refs.ContainsKey("editWideRef"),
        "an asymmetric Vue ref offers only safe writes and keeps asymmetric input refs unsupported");
    var callbackResult = await provider.ReadValueAsync(document, "component-fixture/other", "callbackResult");
    Check(callbackResult.Signatures.Single().ReturnType.Members is [{ Name: "data" }],
        "an ordinary tuple parameter retains its arity and cannot become a tuple-rest call");
    var callbackProjections = await provider.ReadValuesAsync(document, "component-fixture/other", ["createScore", "measureText"]);
    Check(callbackProjections is [{ ExportName: "measureText", Signatures: [{ ReturnType.Members: [{ Name: "data" }] }] }],
        "overloaded callbacks and conditional results cannot be inferred from a probe's stand-in type");
    var mappedCallback = await provider.ReadValueAsync(document, "component-fixture/other", "measureMappedText");
    Check(mappedCallback.Signatures.Single().ReturnType.Members is
        [{ Name: "data" }, { Name: "nested", Type.Members: [{ Name: "stable" }] }],
        "conditional mapped and nested output members cannot be inferred from a probe's stand-in type");
    Check(refs["localeRef"].Signatures is [{ ReturnType: { Name: "VueWritableComputedRef", ElementType.Kind: "string" } }] &&
        refs["narrowLocaleRef"].Signatures is [{ ReturnType: { Name: null, ReadOnly: true } }],
        "a writable computed ref exposes a setter only for an accepted read domain");
    var arrayInputs = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["countDates", "replaceDate", "flipValue", "replaceLiteral"])).ToDictionary(value => value.ExportName);
    Check(arrayInputs["countDates"].Signatures is [{ Parameters: [{
        Type: { Kind: "array", ReadOnly: true, ElementType.Kind: "date" } }] }] &&
        arrayInputs["replaceDate"].Signatures is [{ Parameters: [{
            Type: { Kind: "array", ReadOnly: false, ElementType: {
                Kind: "union", Variants: [{ Kind: "date" }, { Kind: "number" }, { Kind: "string" }] } } }] }] &&
        arrayInputs["flipValue"].Signatures is [{ Parameters: [{
            Type.ElementType: { Kind: "union", Variants: [{ Kind: "boolean" }, { Kind: "number" }] } }] }] &&
        !arrayInputs.ContainsKey("replaceLiteral"),
        "mutable arrays preserve the exact element domain and avoid widening literal branches");
    var paint = await provider.ReadValueAsync(document, "component-fixture/other", "paint");
    var paintOptions = new JoltCompileOptions { NpmValues = [paint] };
    const string useLiteral = """
        @using Brush = Npm.ComponentFixture.Other.Exports
        @code {
            public static string Paint() => Brush.Paint(new() { Tone = Brush.Tone.WarmTone_2 });
        }
        """;
    var painted = await JoltProjectCompiler.CompileAsync([new("Canvas.jazor", useLiteral)], paintOptions);
    Check(painted.Diagnostics.Count == 0 && painted.Modules.Single().Content.Contains("warm_tone", StringComparison.Ordinal),
        "colliding string-enum names preserve the exact upstream wire literal: " + string.Join(", ", painted.Diagnostics));
    var optionalNumbers = await provider.ReadValueAsync(document, "component-fixture/other", "optionalNumbers");
    Check(optionalNumbers.Signatures is [{ ReturnType.Members: [
        { Name: "absent", Optional: true, ReadOnly: true, Type.Kind: "number" },
        { Name: "nullable", Optional: true, ReadOnly: false, Type: { Kind: "nullable", ElementType.Kind: "number" } }] }],
        "an optional numeric result can accept null only if null is in the write domain");
    var validators = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["assess", "assessOptional", "assessImmediately", "assessDeferred", "assessDifferent", "dispatchLiteral", "dispatchCustom"]))
        .ToDictionary(value => value.ExportName);
    Check(validators["assess"].Signatures.Select(GetResult).Select(result => result.Kind).SequenceEqual(["union", "promise"]) &&
        validators["assessImmediately"].Signatures is [var immediate] && GetResult(immediate).Kind == "string" &&
        validators["assessDeferred"].Signatures is [var numericValidator] && numericValidator.Parameters[0].Type.Kind == "number" &&
        GetResult(numericValidator) is { Kind: "promise", ElementType.Kind: "number" } &&
        !validators.ContainsKey("assessDifferent") &&
        !validators.ContainsKey("dispatchLiteral") &&
        !validators.ContainsKey("dispatchCustom"),
        "a generic callback is exposed only in the sync and async forms proved by TypeScript");
    Check(validators["assessOptional"].Signatures is [
        { TypeParameters: null, Parameters: [{ Name: "id", Type.Kind: "string" },
            { Name: "evaluate", Optional: true, Type.Kind: "string" }], ReturnType.Members: [{ Name: "evaluated", ReadOnly: true }] },
        { TypeParameters: ["TValue"], Parameters: [{ Type.Kind: "string" }, { Type.Kind: "function" }, { Type.Kind: "object" }] }] &&
        validators["assessOptional"].Signatures[1].ReturnType.Members!.Any(member =>
            member is { Name: "current", Type.ElementType.Kind: "typeParameter" }),
        "the renamed validator keeps its shorter declared calls and only exposes the value with an initialized generic type");

    static NpmValueType GetResult(NpmValueSignature signature) =>
        signature.Parameters[1].Type.Signatures!.Single().ReturnType;

    var draftExports = await provider.ReadValuesAsync(document, "component-fixture/other", ["assembleDraft", "mismatchedDraft", "restrictedDraft"]);
    Check(draftExports is [{ ExportName: "assembleDraft", Signatures: [var draft] }] &&
        draft.Parameters is [{ Type.Members: [
            { Name: "startingPoint", Type.Kind: "recordTypeParameter" },
            { Name: "adapter", Type.Members: [
                { Name: "marker", Type.StringLiterals: ["transformed-draft"] },
                { Name: "decode", Type.Signatures: [{ ReturnType.ElementType.Members: [
                    { Name: "entity", Type.Kind: "recordTypeParameter", Optional: false },
                    { Name: "issues", Type.ElementType.Members: [{ Name: "trail" }, { Name: "notes" }] }] }] }] }] }] &&
        draft.ReturnType.Members is [
            { Name: "snapshot" },
            { Name: "notices", ReadOnly: true, Type: { Kind: "ref", Name: "VueComputedRef", ReadOnly: true,
                ElementType: { Kind: "readonlyDictionary", ElementType.Kind: "string" } } },
            { Name: "commit", Type.Kind: "method", Type.Signatures: [{ Parameters: [{ Name: "accept" }] }, { Parameters: [
                { Name: "accept" }, { Name: "reject", Type.Signatures: [{ Parameters: [
                    { Type.Members: [{ Name: "draft" }, { Name: "problems", Type.Kind: "readonlyDictionary" }] }] }] }] }] },
            { Name: "replace", Type.Kind: "method" }],
        "renamed schema and form roles preserve the input-keyed computed errors and exclude incompatible refs");
    var draftContext = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["collectNotices", "collectSnapshot", "writeNotices", "markReview", "patchUnchecked", "markNarrow",
            "restoreDraft", "restoreRequired", "inspectDraft", "inspectOpaque", "trackRows",
            "replaceRecord", "replaceSelected", "replaceRequired", "selectCurrent", "selectConstrained", "selectDefault"]);
    var draftByName = draftContext.ToDictionary(value => value.ExportName);
    var selections = draftByName["selectCurrent"].Signatures;
    Check(selections.Where(signature => signature.ValueTypeParameters is null)
        .Select(signature => signature.Parameters.SingleOrDefault()?.Type.Kind).SequenceEqual([null, "string", "ref", "function"]) &&
        selections.Where(signature => signature.ValueTypeParameters is ["TSelected"]).Select(signature => signature.Parameters.Single()).ToArray() is [
            { Type.Kind: "string", Optional: true, ReadOnlyReference: true },
            { Type.Kind: "ref", Optional: false, ReadOnlyReference: true },
            { Type.Kind: "function", Optional: false, ReadOnlyReference: true }] &&
        selections.All(signature => signature is {
            TypeParameters: ["TSelected"], ReturnType: { Kind: "ref", ReadOnly: true,
                ElementType: { Kind: "undefinedable", ElementType: { Kind: "typeParameter", Name: "TSelected" } } } }) &&
        !draftByName.ContainsKey("selectConstrained") &&
        (!draftByName.TryGetValue("selectDefault", out var selectDefault) ||
            selectDefault.Signatures.All(signature => signature.TypeParameters is null)),
        "a context-selected computed preserves its explicit result slot and can omit the path or the value");
    Check(draftByName["collectNotices"].Signatures is [{ Parameters: [], TypeParameters: null,
        ReturnType: { Kind: "ref", Name: "VueComputedRef", ReadOnly: true,
            ElementType: { Kind: "readonlyDictionary", ElementType.Kind: "string" } } }] &&
        !draftByName.ContainsKey("collectSnapshot"), "an unbound generic context can project its default sparse string index only");
    Check(draftByName["writeNotices"].Signatures is [{ Parameters: [], ReturnType.Signatures: [
        { Parameters: [{ Type: { Kind: "dictionary", ElementType: { Kind: "union" } } }], ReturnType.Kind: "void" }] }] &&
        draftByName["markReview"].Signatures is [{ Parameters: [], ReturnType.Signatures: [
            { Parameters: [{ Type: { Kind: "union", Variants: [
                { Kind: "dictionary", ElementType.Kind: "boolean" }, { Kind: "boolean" }] } }], ReturnType.Kind: "void" }] }] &&
        !draftByName.ContainsKey("patchUnchecked") && !draftByName.ContainsKey("markNarrow"),
        "renamed bulk writers preserve known dictionary and boolean arms without widening unknown entries or a boolean literal");
    Check(draftByName["restoreDraft"].Signatures is [{ ReturnType.Signatures: [
        { Parameters: [{ Optional: true, Type.Members: [{ Name: "visits", Type.Kind: "number" }] }] }] }] &&
        !draftByName.ContainsKey("restoreRequired"),
        "a returned reset closure projects known optional state and rejects an opaque required input");
    Check(draftByName["inspectDraft"].Signatures.Any(signature => signature.Parameters is [{ Optional: true }] &&
        signature.ReturnType.Signatures is [{ ReturnType: { Kind: "promise", ElementType.Members: [
            { Name: "approved", Type.Kind: "boolean" }, { Name: "problems", Type.ElementType.Kind: "string" }] } }]) &&
        !draftByName.ContainsKey("inspectOpaque"),
        "an asynchronous result exposes concrete validation data while opaque-only results are rejected");
    Check(draftByName["trackRows"].Signatures is [{ TypeParameters: null, ReturnType.Members: [
        { Name: "rows", Type: { Kind: "ref", ReadOnly: true, ElementType: {
            Kind: "array", ReadOnly: true, ElementType.Members: [
                { Name: "isHead", ReadOnly: true }, { Name: "isTail", ReadOnly: true }] } } },
        { Name: "drop" }] }],
        "a generic collection view preserves fixed item metadata and excludes unknown writes");
    Check(draftByName["replaceRecord"].Signatures is [{ TypeParameters: ["TData"], ReturnType.Signatures: [
        { Parameters: [{ Type.Kind: "recordTypeParameter" }, { Optional: true, Type.Kind: "boolean" }] }] }] &&
        draftByName["replaceSelected"].Signatures.Any(signature => signature.TypeParameters is ["TData"] &&
            signature.ReturnType.Signatures is [{ Parameters: [{ Type.Kind: "typeParameter" }, { Optional: true }] }]) &&
        !draftByName.ContainsKey("replaceRequired"),
        "an explicit type argument follows the input slot, with stronger upstream constraints rejected");

    var declared = Path.Combine(project, "package.json");
    await File.WriteAllTextAsync(declared, """{"private":true,"dependencies":{"reka-ui":"2.10.1","vue":"3.5.42"}}""");
    var beforeUpgrade = await provider.ReadAsync(document, "reka-ui", "NumberFieldRoot");
    await File.WriteAllTextAsync(declared, """{"private":true,"dependencies":{"reka-ui":"2.11.0","vue":"3.5.42"}}""");
    var afterUpgrade = await provider.ReadAsync(document, "reka-ui", "NumberFieldRoot");
    Check(beforeUpgrade.Version == "2.10.1" && !beforeUpgrade.Props.ContainsKey("allowInvalid") &&
        afterUpgrade.Version == "2.11.0" && afterUpgrade.Props.ContainsKey("allowInvalid"), "in-place dependency upgrade invalidates metadata");

    await File.WriteAllTextAsync(declared, """{"private":true,"dependencies":{"es-toolkit":"1.52.0","@vueuse/core":"14.4.0","vue":"3.5.42"}}""");
    var oldCamel = await provider.ReadValueAsync(document, "es-toolkit/string", "camelCase");
    await File.WriteAllTextAsync(declared, """{"private":true,"dependencies":{"es-toolkit":"1.53.2","@vueuse/core":"14.4.0","vee-validate":"4.15.1","vue":"3.5.42"}}""");
    var camel = await provider.ReadValueAsync(document, "es-toolkit/string", "camelCase");
    Check(oldCamel.Version == "1.52.0" && camel.Version == "1.53.2" &&
        camel.Signatures is [{ Parameters: [{ Type.Kind: "string" }], ReturnType.Kind: "string" }] &&
        camel.Definition is { } camelDefinition &&
        File.ReadAllText(camelDefinition.Path).AsSpan(camelDefinition.Start, camelDefinition.Length).SequenceEqual("camelCase"),
        "published es-toolkit subpath signature, declaration navigation, and in-place version upgrade");
    var mediaQuery = await provider.ReadValueAsync(document, "@vueuse/core", "useMediaQuery");
    var windowFocus = await provider.ReadValueAsync(document, "@vueuse/core", "useWindowFocus");
    Check(mediaQuery.Signatures.Count == 3 &&
        mediaQuery.Signatures.All(signature => signature.Parameters is [_, { Optional: true, Type.Kind: "object" }] &&
            signature.ReturnType is { Kind: "ref", Name: "VueComputedRef", ReadOnly: true, ElementType.Kind: "boolean" }) &&
        mediaQuery.Signatures.Any(signature => signature.Parameters[0].Type.Kind == "string") &&
        mediaQuery.Signatures.Any(signature => signature.Parameters is [{
            Type: { Kind: "ref", ElementType.Kind: "string", ReadOnly: false } }, _]) &&
        mediaQuery.Signatures.Any(signature => signature.Parameters is [{
            Type: { Kind: "function", Signatures: [{ Parameters: [], ReturnType.Kind: "string" }] } }, _]),
        "published media query accepts a string, Vue ref, or getter and returns a readonly Vue computed ref");
    Check(windowFocus.Signatures is [{ ReturnType: { Kind: "ref", Name: "VueShallowRef", ReadOnly: false } }],
        "published window focus uses the canonical writable Vue shallow ref");
    const string vueRefSource = """
        @using VueUse = Npm.Vueuse.Core.Exports
        <p>@Current</p>
        @code {
            private bool Current {
                get {
                    ECMAScript.Vue.VueComputedRef<bool> media = VueUse.UseMediaQuery("(min-width: 700px)");
                    var query = ECMAScript.Vue.Ref("(min-width: 700px)");
                    var fromRef = VueUse.UseMediaQuery(query);
                    var fromGetter = VueUse.UseMediaQuery(() => "(min-width: 700px)");
                    ECMAScript.Vue.VueShallowRef<bool> focus = VueUse.UseWindowFocus();
                    ECMAScript.Vue.VueReadonlyRef<bool> readonlyRef = media;
                    ECMAScript.Vue.IVueRef<bool> writable = focus;
                    return readonlyRef.Value && writable.Value && fromRef.Value && fromGetter.Value;
                }
            }
        }
        """;
    var vueRefs = await JoltProjectCompiler.CompileAsync([new("App.jazor", vueRefSource)],
        new JoltCompileOptions { NpmValues = [mediaQuery, windowFocus] });
    Check(vueRefs.Diagnostics.Count == 0 && vueRefs.Modules.Single().Content.Contains(
        "readonlyRef.value && writable.value && fromRef.value && fromGetter.value"),
        "canonical Vue npm refs compile and lift directly to JavaScript .value");
    var counter = await provider.ReadValueAsync(document, "@vueuse/core", "useCounter");
    Check(counter.Signatures.Any(signature => signature.Parameters is []) &&
        counter.Signatures.Any(signature => signature.Parameters is [{ Type.Kind: "number" }]) &&
        counter.Signatures.Any(signature => signature.Parameters is [{ Type.Kind: "ref" }]) &&
        counter.Signatures.First().ReturnType.Members is { } members &&
        members.Single(member => member.Name == "count").Type is { Kind: "ref", ElementType.Kind: "number", ReadOnly: true } &&
        members.Single(member => member.Name == "inc").Type.Signatures is
            [{ Parameters: [{ Optional: true, Type.Kind: "number" }], ReturnType.Kind: "number" }],
        "published VueUse composable returns a numeric readonly ref and typed methods");

    var setFieldError = await provider.ReadValueAsync(document, "vee-validate", "useSetFieldError");
    Check(setFieldError.Signatures is [.., {
        ReturnType: { Kind: "function", Signatures: [{
            Parameters: [{ Optional: false, Type: { Kind: "union", Variants: [
                { Kind: "array", ElementType.Kind: "string" }, { Kind: "string" }] } }],
            ReturnType.Kind: "void" }] } }],
        "published field error setter preserves the required string or string array argument");
    const string setterSource = """
        @using Validation = Npm.VeeValidate.Exports
        <button @onclick="Report">Report</button>
        @code {
            private void Report() {
                var report = Validation.UseSetFieldError("email");
                report("Invalid");
                report(new string[] { "Missing", "Invalid" });
            }
        }
        """;
    var setterOptions = new JoltCompileOptions { NpmValues = [setFieldError] };
    var setter = await JoltProjectCompiler.CompileAsync([new("App.jazor", setterSource)], setterOptions);
    Check(setter.Diagnostics.Count == 0 && setter.Modules.Single().Content.Contains("report(\"Invalid\")") &&
        setter.Modules.Single().Content.Contains("report([\"Missing\", \"Invalid\"])"),
        $"the field error setter emits existing JavaScript string and array values: {string.Join(", ", setter.Diagnostics)}");
    foreach (var (invalid, expectedDiagnostic) in new[]
    {
        ("report();", "CS7036"),
        ("report(true);", "CS1503"),
        ("report(null);", "CS0037"),
        ("report(new bool[] { true });", "CS1503"),
    })
    {
        var result = await JoltProjectCompiler.CompileAsync(
            [new("App.jazor", setterSource.Replace("report(\"Invalid\");", invalid, StringComparison.Ordinal))], setterOptions);
        Check(result.Diagnostics.Any(diagnostic => diagnostic.Id == expectedDiagnostic) && result.Modules.Count == 0,
            $"the field error setter rejects the invalid call {invalid}: {string.Join(", ", result.Diagnostics)}");
    }

    var formExports = await provider.ReadValuesAsync(document, "vee-validate",
        ["useField", "useFieldValue", "useForm", "useFormErrors", "useFormValues", "useSetFormErrors", "useSetFormTouched", "useSetFormValues",
            "useIsFieldTouched", "useFieldArray", "useResetForm", "useValidateField", "useValidateForm", "useSubmitForm", "useSetFieldValue"]);
    var formOptions = new JoltCompileOptions { NpmValues = formExports };
    var formSignatures = formExports.Single(value => value.ExportName == "useForm").Signatures;
    Check(formSignatures.Count(signature => signature is { TypeParameters: null, Parameters: [] }) == 1 &&
        formSignatures.Count(signature => signature is { TypeParameters: ["TInput", "TOutput"], Parameters: [{ Optional: false }] }) == 1,
        "the typed-schema projection preserves the separate parameterless call");
    Check(formExports.Single(value => value.ExportName == "useFormErrors").Signatures is
        [{ Parameters: [], TypeParameters: null, ReturnType.ElementType.Kind: "readonlyDictionary" }],
        "the context call exposes sparse string errors without promising a caller-selected record");
    Check(formExports.Single(value => value.ExportName == "useFormValues").Signatures.Any(signature =>
        signature is { Parameters: [], TypeParameters.Count: 1, ReturnType.ElementType.Kind: "partialRecord" }),
        "form values are read as a sparse record");
    var formSource = await File.ReadAllTextAsync("scripts/csharp/fixtures/jolt-npm-runtime/Vee.jazor");
    foreach (var (invalid, expectedDiagnostic) in new[]
    {
        (formSource.Replace("value => value.MissingCount", "value => value.UnknownCount", StringComparison.Ordinal), "CS1061"),
        (formSource.Replace("value => value.MissingCount", "value => value.MissingCount + 1", StringComparison.Ordinal), "JOLT2001"),
        (formSource.Replace("form.HandleSubmit(values =>", "form.HandleSubmit((RegistrationInput values) =>", StringComparison.Ordinal), "CS1661"),
        (formSource.Replace("Parse = values =>", "Parse = (RegistrationOutput values) =>", StringComparison.Ordinal), "CS1661"),
        (formSource.Replace("Value = new(0),", "", StringComparison.Ordinal), "CS9035"),
        (formSource.Replace("form.Values.Address", "form.Values.AddressLength", StringComparison.Ordinal), "CS1061"),
        (formSource.Replace("var unruled = VeeValidate.UseField(\"unruled\");", "var unruled = VeeValidate.UseField(\"unruled\"); var untypedValue = unruled.Value;", StringComparison.Ordinal), "CS1061"),
        (formSource.Replace("var unruled = VeeValidate.UseField(\"unruled\");", "var unruled = VeeValidate.UseField(\"unruled\"); unruled.SetValue(1);", StringComparison.Ordinal), "CS1061"),
        (formSource.Replace("var missingPath = VeeValidate.UseFieldValue<string>(\"missing-path\");", "var missingPath = VeeValidate.UseFieldValue<string>(\"missing-path\"); missingPath.Value = \"invalid\";", StringComparison.Ordinal), "CS0200"),
        (formSource.Replace("var errors = form.Errors;", """var errors = form.Errors; errors.Value["Address"] = "wrong";""", StringComparison.Ordinal), "CS0200"),
        (formSource.Replace("VeeValidate.UseFormErrors()", "VeeValidate.UseFormErrors<RegistrationOutput>()", StringComparison.Ordinal), "CS0308"),
        (formSource.Replace("[errorKey] = \"one\"", "[errorKey] = 1", StringComparison.Ordinal), "CS0029"),
        (formSource.Replace("setTouched(false);", "setTouched(\"false\");", StringComparison.Ordinal), "CS1503"),
        (formSource.Replace("array.Remove(1);", "array.Push(\"unsupported\");", StringComparison.Ordinal), "CS1061"),
        (formSource.Replace("array.Remove(1);", "array.Fields.Value[0].IsFirst = false;", StringComparison.Ordinal), "CS0200"),
        (formSource.Replace("writeField(\"field\", false);", "writeField(1, false);", StringComparison.Ordinal), "CS1503"),
        (formSource.Replace("writeForm(new(\"ada@example.test\", [\"changed\"]), false);", "writeForm(new RegistrationOutput(1), false);", StringComparison.Ordinal), "CS1503"),
    })
    {
        var result = await JoltProjectCompiler.CompileAsync([new("Vee.jazor", invalid)], formOptions);
        Check(result.Modules.Count == 0 && result.Diagnostics.Any(diagnostic => diagnostic.Id == expectedDiagnostic),
            $"the schema, submit callback and parse result preserve their record constraints: {string.Join(", ", result.Diagnostics)}");
    }

    async Task CheckUnsupported(string module, string export, string diagnostic)
    {
        try
        {
            await provider.ReadValueAsync(document, module, export);
        }
        catch (Exception error) when (error.Message.Contains(diagnostic, StringComparison.Ordinal))
        {
            return;
        }
        throw new InvalidOperationException($"Expected npm metadata diagnostic for {module}/{export}.");
    }
}
finally
{
    temporary.Delete(recursive: true);
}
Console.WriteLine("Verified npm component and value metadata, public subpaths, declarations, and version invalidation.");

static void Check(bool value, string description)
{
    if (!value)
        throw new InvalidOperationException($"Verification failed for {description}.");
}
