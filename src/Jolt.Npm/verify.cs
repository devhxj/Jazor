#:property CETCompat=false
#:property PublishAot=false
#:property AssemblyName=Jazor.NodeApiVerification
#:project ../Jolt.Compiler/Jolt.Compiler.csproj

using Jazor.NodeHost;
using Jolt.Compiler;
using Jolt.Npm;
using Jazor.Npm;

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
    foreach (var (directory, version) in new[] { ("v1", "1.0.0"), ("v2", "2.0.0") })
    {
        var versionedCopy = Directory.CreateDirectory(Path.Combine(package, directory)).FullName;
        await File.WriteAllTextAsync(Path.Combine(versionedCopy, "package.json"), $$"""
            {"name":"versioned-data","version":"{{version}}","type":"module"}
            """);
        await File.WriteAllTextAsync(Path.Combine(versionedCopy, "shape.d.ts"), """
            import type { Ref } from 'vue';
            export interface Sample<T> { value: T }
            export type Instrument<T> = Ref<T> & { read(): T };
            """);
    }
    await File.WriteAllTextAsync(Path.Combine(package, "index.js"),
        "export default { render() {} }; export const PointerWidget = {}, ShadowPointerWidget = {}, OptionsWidget = {}, RecordWidget = {};");
    await File.WriteAllTextAsync(Path.Combine(package, "index.d.ts"), """
        import type { DefineComponent, ExtractPublicPropTypes, PropType } from 'vue';
        interface WidgetProps { tone: 'quiet' | 'strong'; }
        declare const Widget: DefineComponent<WidgetProps>;
        export default Widget;
        type Installable<C> = C & { install(): void };
        interface MouseEvent { unrelated: string }
        type Component<T> = abstract new () => {
            $props: {
                tone?: 'quiet' | 'strong';
                width?: number | string;
                fixed?: boolean | 'left' | 'right';
                pageSizes?: number[];
                rows?: ReadonlyArray<Readonly<{ id: string; rank: number; note: string | null; score: number | null; visible: boolean | null }>>;
                optionalRows?: ReadonlyArray<Readonly<{ note?: string | null }>>;
                undefinedRows?: ReadonlyArray<Readonly<{ note: string | null | undefined }>>;
                mixedRows?: ReadonlyArray<Readonly<{ note: string | number | null }>>;
                literalRows?: ReadonlyArray<Readonly<{ note: 'brief' | null }>>;
                showTooltip?: boolean | Partial<{ placement: string }>;
            };
            $emit: {
                (event: 'click', payload: T): void;
                (event: 'selection', ids: string[]): void;
                (event: 'snapshot', ids: readonly string[]): void;
                (event: 'spread', ...ids: string[]): void;
            };
            $slots: {};
        };
        export declare const PointerWidget: Installable<Component<globalThis.MouseEvent>>;
        export declare const ShadowPointerWidget: Installable<Component<MouseEvent>>;
        type Selection = string | number;
        type RuntimeOption<T> = { readonly type: PropType<T>; readonly required: false; readonly validator: ((value: unknown) => boolean) | undefined };
        export declare const OptionsWidget: abstract new () => {
            $props: ExtractPublicPropTypes<{
                readonly selected: RuntimeOption<Selection>;
                readonly label: RuntimeOption<string>;
            }>;
            $emit: {
                (event: 'update:selected', selection: Selection): void;
                (event: 'unsafe', selection: any): void;
                (event: 'more', selection: string | number | { name: string }): void;
            };
            $slots: { default?: (scope: {}) => unknown };
        };
        export declare const RecordWidget: abstract new () => {
            $props: { selected?: RuntimeOption<Selection> };
            $emit: (event: 'update:selected', selection: Selection) => void;
            $slots: {};
        };
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "values.js"), """
        export const count = 3;
        export default (value, transform) => transform(value);
        export function echo(value) { return value; }
        export function generic(value) { return value; }
        export function overloadedCallback(callback) { callback(3); }
        export function readonlyShallow() { return { value: 3 }; }
        export function finiteOptions(options) { return options.ratio; }
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
        export declare enum NumericMode { First = 1, Second = 2 }
        type Ratio = -0.25 | 0.5 | 1.5;
        export declare function finiteOptions(options: { ratio: Ratio; mode: NumericMode }): number;
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
        export function observerError() { return new Error("observer failure"); }
        export function messageStore() { return {}; }
        export function lookalikeObserverError() { return { name: "LookupError", message: "observer failure" }; }
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
        export function preserve(value) { return value; }
        export function preserveUnknown(value) { return value; }
        export function selectFrom(value, select) { return select(value); }
        export function inspectAndKeep(inspect, value) { inspect(value); return value; }
        export function nestSelection(value, select) { return select(value); }
        export function transformSequence(items, select) { return items.map(select); }
        export function loadSequence(items, select) { return select(items); }
        export function transformPair(items, select) { return items.map(select); }
        export function transformMutable(items, select) { return items.map(select); }
        export function measureSelection(value, select) { select(value); return 1; }
        export function constrainedSelection(value, select) { return select(value); }
        export function chooseSubtype(base, value) { return value; }
        export function indexedSelection(value, select) { return select(value); }
        export function conditionalSelection(value, select) { return select(value); }
        export function resultWithoutSource(value) { return value; }
        export function reversePair(pair) { return { left: pair.right, right: pair.left }; }
        export function pickLabel(source) { return { data: source.data, label: source.label }; }
        export function createPickSource(data) { return { data, label: "label", count: 3 }; }
        export function pickCount(source) { return source.count === undefined ? {} : { count: source.count }; }
        export function acceptCount(source, patch) { return patch.count ?? source.count; }
        export function packPair(pair) { return Promise.resolve({ payload: pair }); }
        export function packTwice(value) { return { payload: { payload: value } }; }
        export function chooseVariant(left, right) { return left; }
        export const chooseVersion = chooseVariant;
        export function makeTree(value) { return { value, children: [{ value, children: [] }] }; }
        export const passTree = preserve, passMutual = preserve;
        export function makeMixed(value, alternate) { return { value, children: [{ value: alternate, children: [] }] }; }
        export const fromPhantom = preserve, fromCallbackOnly = preserve, fromOptional = preserve,
            fromIndexed = preserve, fromMethod = preserve, fromConstraint = preserve, fromRecursive = preserve,
            fromExpandingCycle = preserve, fromPhantomCycle = preserve, fromUnknownCycle = preserve;
        export const collectBatches = () => ({}), fixedBatches = collectBatches, undefinedBatches = collectBatches,
            objectBatches = collectBatches, chooseBatchView = collectBatches, fixedBatchView = collectBatches,
            ambiguousBatchView = collectBatches;
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
        export function editComputedRef(source) { source.value = null; }
        export function keepClosedComputedRef(source) { return source; }
        export function keepGenericComputedRef(source) { return source; }
        export function keepIndependentComputedRef(source) { return source; }
        export function keepNullableComputedRef(source) { return source; }
        export function receiveLocalComputedRef(source) { return source.value; }
        export function receiveExtendedComputedRef(source) { source.reset(); return source.value; }
        export function receiveNarrowComputedRef(source) { return source.value; }
        export function receiveUndefinedComputedRef(source) { return source.value; }
        export function editWideRef(ref) { ref.value = null; }
        export function optionalNumberRef(present) { return { value: present ? 0 : undefined }; }
        export function editOptionalNumberRef(ref) { ref.value = undefined; }
        export function countDates(dates) { return dates.length; }
        export function replaceDate(dates) { dates[0] = 0; }
        export function flipValue(values) { values[0] = !values[0]; }
        export function replaceLiteral(values) { values[0] = "fixed"; }
        export function optionalNumbers() { return {}; }
        export function optionalRecord(initial) { return { hasValue: "wire" }; }
        export function optionalMapped(initial) { return {}; }
        export function optionalNullable(initial) { return {}; }
        export function requiredNullable(initial) { return { value: null }; }
        export function optionalUndefined(initial) { return { value: undefined }; }
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
        export function assessOptional(id, evaluate, settings) { return { current: { value: settings?.seed }, evaluated: false, label: id }; }
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
        export function currentDraft() { return undefined; }
        export function startDraft() { return {}; }
        export function startRequiredDraft() { return {}; }
        export function startFilteredDraft() { return {}; }
        export function startUnspecifiedDraft() { return {}; }
        export function narrowDraft() { return undefined; }
        export function inspectOpaque() { return () => Promise.resolve({}); }
        export function trackRows() { return {}; }
        export function trackSeededRows() { return {}; }
        export function replaceRecord() { return () => {}; }
        export function replaceSelected() { return () => {}; }
        export function replaceRequired() { return () => {}; }
        """);
    await File.WriteAllTextAsync(Path.Combine(package, "other.d.ts"), """
        import type { ComputedRef, Ref } from 'vue';
        export declare function readOpaque(): Ref<unknown>;
        export declare function readUnspecified(): Ref<any>;
        export declare function startDeferred<T>(load: Promise<T> | (() => Promise<T>), initial: T): {
            current: Ref<T>;
            rejected: Ref<unknown>;
            unchecked: Ref<any>;
        } & PromiseLike<{ current: Ref<T> }>;
        type PulseArgs<T> = T extends unknown[] ? T : [T];
        type PulseListener<T> = (...args: PulseArgs<T>) => void;
        export declare function makePulse<T>(): {
            subscribe: (listener: PulseListener<T>) => { dispose: () => void };
            forget: (listener: PulseListener<T>) => void;
            broadcast: (...args: Parameters<PulseListener<T>>) => Promise<unknown[]>;
            reset: () => void;
        };
        export declare function mismatchedPulse<T>(): {
            subscribe: (listener: PulseListener<T>) => { dispose: () => void };
            forget: (listener: (value: boolean) => void) => void;
            broadcast: (...args: Parameters<PulseListener<T>>) => Promise<unknown[]>;
        };
        export declare function queryDataProtocol(): QueryDataProtocol;
        declare const dataBrand: unique symbol;
        type TaggedResult<K, Data> = K extends { readonly [dataBrand]: infer Branded } ? Branded : Data;
        declare class QueryDataProtocol {
            valid<T>(key: readonly string[]): T | undefined;
            reordered<T, K extends readonly unknown[] = readonly unknown[]>(key: K): TaggedResult<K, T> | undefined;
            returnsKey<T>(key: T): T | undefined;
            narrowFirst(key: readonly ['fixed']): string;
            narrowFirst<T>(key: readonly string[]): T | undefined;
            customThis<T>(this: { context: boolean }, key: readonly string[]): T | undefined;
            requiredCallbackField<T>(key: readonly string[],
                updater: T | undefined | { (old: T | undefined): T | undefined; marker: string }): T | undefined;
        }
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
        interface ControlEvents {
            onBlur: () => void;
            onInput: () => void;
            onChange: () => void;
        }
        interface ControlSetup<TValue, TExtras> {
            check?: boolean;
            enrich?: (snapshot: { current?: TValue; changed: boolean; notes: readonly string[] }) => TExtras;
        }
        type DeferredControlSetup<TValue, TExtras> = (snapshot: { current?: TValue; changed: boolean }) => {
            attributes?: TExtras;
            readonly refresh?: boolean;
            opaque?: unknown;
        };
        interface DraftHandle<TSource, TDestination> {
            snapshot: TSource;
            notices: ComputedRef<Sparse<Record<keyof TSource, string>>>;
            groupedNotices: Ref<Sparse<Record<keyof TSource, string[]>>>;
            conditionalScalarNotices: ComputedRef<Sparse<Record<keyof TSource, TSource extends { joltInput: string } ? string : number>>>;
            fieldScalarNotices: ComputedRef<Sparse<{ [K in keyof TSource]: TSource[K] }>>;
            conditionalGroups: Ref<Partial<Record<keyof TSource, TSource extends { joltInput: string } ? string[] : number[]>>>;
            fieldGroups: Ref<Partial<{ [K in keyof TSource]: TSource[K][] }>>;
            transformedGroups: Ref<Sparse<Record<keyof TDestination, string[]>>>;
            transformedNotices: ComputedRef<Sparse<Record<keyof TDestination, string>>>;
            numericNotices: ComputedRef<Sparse<Record<keyof TSource, number>>>;
            lookalikeNotices: { readonly value: Sparse<Record<keyof TSource, string>> };
            commit: Commit<TSource, TDestination> & { controlled: Commit<TSource, TDestination> };
            replace(patch: Sparse<TSource>, amend?: boolean): void;
            revise<TKey extends DraftRoute<TSource>>(key: TKey, next: DraftValue<TSource, TKey>, amend?: boolean): void;
            disregard<TKey extends DraftRoute<TSource>>(key: TKey, next: unknown, amend?: boolean): void;
            reviseFiltered<TKey extends FilteredRoute<TSource>>(key: TKey, next: DraftValue<TSource, TKey>, amend?: boolean): void;
            attach<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>>(key: TKey, config?: unknown): [Ref<TValue>, Ref<ControlEvents>];
            attachConfigured<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>>(key: TKey,
                setup?: { caption?: string | Ref<string>; check?: boolean; flag?: true; props?: () => object } | (() => object)): [Ref<TValue>, Ref<ControlEvents>];
            attachProps<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>, TExtras extends Draft = Draft>(key: TKey,
                setup?: ControlSetup<TValue, TExtras> | DeferredControlSetup<TValue, TExtras>): [Ref<TValue>, Ref<ControlEvents & TExtras>];
            attachWrongState<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>, TExtras extends Draft = Draft>(key: TKey,
                setup?: ControlSetup<TSource, TExtras> | DeferredControlSetup<TSource, TExtras>): [Ref<TValue>, Ref<ControlEvents & TExtras>];
            attachWrongProps<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>, TExtras extends Draft = Draft>(key: TKey,
                setup?: ControlSetup<TValue, Draft> | DeferredControlSetup<TValue, Draft>): [Ref<TValue>, Ref<ControlEvents & TExtras>];
            attachRestrictedSetup<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>, TExtras extends Draft = Draft>(key: TKey,
                setup?: (state: { current?: TValue }) => { attributes?: TExtras; requiredToken: symbol }): [Ref<TValue>, Ref<ControlEvents & TExtras>];
            attachWidened<TKey extends DraftRoute<TSource>, TValue = string>(key: TKey): [Ref<TValue>, Ref<ControlEvents>];
            attachTransformed<TKey extends DraftRoute<TDestination>, TValue = DraftValue<TDestination, TKey>>(key: TKey): [Ref<TValue>, Ref<ControlEvents>];
            attachReversed<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>>(key: TKey): [Ref<ControlEvents>, Ref<TValue>];
            attachUnrefed<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>>(key: TKey): [Ref<TValue>, ControlEvents];
            attachRequiredConfig<TKey extends DraftRoute<TSource>, TValue = DraftValue<TSource, TKey>>(key: TKey, config: { marker: string }): [Ref<TValue>, Ref<ControlEvents>];
        }
        interface ActiveDraft<TSource, TDestination> extends DraftHandle<TSource, TDestination> {
            recheck(): Promise<{ accepted: boolean }>;
            review<TKey extends DraftRoute<TSource>>(key: TKey): Promise<{ accepted: boolean; notes: string[] }>;
            reviewFiltered<TKey extends FilteredRoute<TSource>>(key: TKey): Promise<{ accepted: boolean; notes: string[] }>;
            reviewForeign<TKey extends DraftRoute<{ joltInput: string; different: number }>>(key: TKey): Promise<{ accepted: boolean; notes: string[] }>;
            reviewTransformed<TKey extends DraftRoute<TDestination>>(key: TKey): Promise<{ accepted: boolean; notes: string[] }>;
            reviewUnclassified<TKey extends DraftRoute<TSource>>(key: TKey): Promise<{ notes: string[] }>;
            reviewOptional<TKey extends DraftRoute<TSource>>(key: TKey): Promise<{ accepted?: boolean; notes: string[] }>;
            reviewText<TKey extends DraftRoute<TSource>>(key: TKey): Promise<{ accepted: string; notes: string[] }>;
        }
        type DraftRoute<T> = { [K in keyof T & string]: K | `${K}.${DraftRoute<T[K]>}` }[keyof T & string];
        type FilteredRoute<T> = T extends { blocked: true } ? never : DraftRoute<T>;
        type DraftValue<T, P extends DraftRoute<T>> = P extends `${infer K}.${infer R}`
            ? K extends keyof T ? DraftValue<T[K], R & DraftRoute<T[K]>> : never
            : P extends keyof T ? T[P] : never;
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
        export declare function currentDraft<TSource extends Draft = Draft,
            TDestination extends Draft = TSource>(): ActiveDraft<TSource, TDestination> | undefined;
        export declare function startDraft<TSource extends Draft = Draft>(
            options?: { startingPoint?: Sparse<TSource> }): Omit<ActiveDraft<TSource, TSource>,
                'revise' | 'transformedGroups' | 'transformedNotices' | 'attachTransformed' | 'reviewTransformed'> & {
                revise<TKey extends DraftRoute<TSource>>(key: TKey, next: DraftValue<TSource, TKey>): void;
            };
        export declare function startRequiredDraft<TSource extends Draft = Draft>(
            options: { startingPoint: Sparse<TSource> }): ActiveDraft<TSource, TSource>;
        type FilteredSeed<T> = T extends { joltInput: string } ? T : never;
        export declare function startFilteredDraft<TSource extends Draft = Draft>(
            options?: { startingPoint?: FilteredSeed<TSource>; marks?: { [K in keyof TSource]?: boolean } }
        ): ActiveDraft<TSource, TSource>;
        export declare function startUnspecifiedDraft<TSource extends Draft, TDestination extends Draft>(
            options?: { startingPoint?: Sparse<TSource> }): ActiveDraft<TSource, TDestination>;
        export declare function narrowDraft<TSource extends { joltInput: string } = { joltInput: string },
            TDestination extends Draft = TSource>(): ActiveDraft<TSource, TDestination> | undefined;
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
        interface RunRow<TData> { readonly payload: TData; isHead: boolean; isTail: boolean; }
        export declare function trackRows<TData = unknown>(key: string): {
            rows: Ref<RunRow<TData>[]>;
            drop(index: number): void;
            add(payload: TData): void;
        };
        export declare function trackSeededRows<TData = unknown>(options: { label: string; seed?: TData }): {
            rows: Ref<RunRow<TData>[]>;
            drop(index: number): void;
            add(payload: TData): void;
        };
        export declare function replaceRecord<TData extends Draft = Draft>(): (data: TData, enabled?: boolean) => void;
        export declare function replaceSelected<TData = unknown>(key?: string): (data: TData, enabled?: boolean) => void;
        export declare function replaceRequired<TData extends { key: string }>(): (data: TData) => void;
        export declare function readColumn<TEntry, TColumn extends keyof TEntry>(entry: TEntry, column: TColumn): TEntry[TColumn];
        export declare function columnHandle<P extends object, K extends keyof P, Name extends string>(entry: P, key?: K, emit?: (name: Name) => void): import('vue').WritableComputedRef<P[K]>;
        export declare function columnHandle<P extends object, K extends keyof P>(entry: P, key?: K, options?: boolean): Ref<NonNullable<P[K]>>;
        export declare function connectCell<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce: (notice: string, payload: Row[Cell]) => void): import('vue').WritableComputedRef<Row[Cell]>;
        export declare function spreadCell<Row extends object, Cell extends keyof Row, Notice extends string>(row: Row, cell: Cell, announce?: (notice: Notice, ...payload: unknown[]) => void): import('vue').WritableComputedRef<Row[Cell]>;
        export declare function wrongCell<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce?: (notice: string, payload: Row[keyof Row]) => void): import('vue').WritableComputedRef<Row[Cell]>;
        export declare function narrowNotice<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce?: (notice: 'changed', payload: Row[Cell]) => void): import('vue').WritableComputedRef<Row[Cell]>;
        export declare function wrongRest<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce?: (notice: string, ...payload: number[]) => void): import('vue').WritableComputedRef<Row[Cell]>;
        export declare function wrongResult<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce?: (notice: string, payload: Row[Cell]) => boolean): import('vue').WritableComputedRef<Row[Cell]>;
        export declare function shadowedCell<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce?: (notice: string, payload: Row[Cell]) => void): string;
        export declare function shadowedCell<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce?: (notice: string, payload: Row[Cell]) => void): import('vue').WritableComputedRef<Row[Cell]>;
        export declare function functionShadowedCell<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce: Function): string;
        export declare function functionShadowedCell<Row extends object, Cell extends keyof Row>(row: Row, cell: Cell, announce: (notice: string, payload: Row[Cell]) => void): import('vue').WritableComputedRef<Row[Cell]>;
        export declare function observedColumn<P, K extends keyof P>(entry: P, key: K): ComputedRef<P[K]>;
        interface ColumnBehavior<T, B extends boolean> {
            fallback?: T;
            deferred?: B;
            accept?: (value: T) => boolean;
        }
        export declare function trackColumn<P extends object, K extends keyof P>(entry: P, key: K, notify?: () => void, behavior?: ColumnBehavior<P[K], true>): Ref<P[K]>;
        export declare function trackColumn<P extends object, K extends keyof P>(entry: P, key: K, notify?: () => void, behavior?: ColumnBehavior<P[K], false>): import('vue').WritableComputedRef<P[K]>;
        type WatchColumn<B extends boolean> = { observe?: B; subtree?: boolean; name?: string };
        export declare function watchColumn<P extends object, K extends keyof P>(entry: P, key: K, options: WatchColumn<false>): import('vue').WritableComputedRef<P[K]>;
        export declare function watchColumn<P extends object, K extends keyof P>(entry: P, key: K, options: WatchColumn<true>): Ref<import('vue').UnwrapRef<P[K]>>;
        type UnwrapRef<T> = T extends { leaf: infer S } ? S : T;
        export declare function watchLookalike<P extends object, K extends keyof P>(entry: P, key: K, options: WatchColumn<true>): Ref<UnwrapRef<P[K]>>;
        export declare function wrongBehaviorSlot<P extends object, K extends keyof P>(entry: P, key: K, notify?: () => void, behavior?: ColumnBehavior<P[keyof P], true>): Ref<P[K]>;
        export declare function requiredBehaviorHole<P extends object, K extends keyof P>(entry: P, key: K, notify: () => void, behavior?: ColumnBehavior<P[K], true>): Ref<P[K]>;
        export declare function changedBehaviorReturn<P extends object, K extends keyof P>(entry: P, key: K, notify?: () => void, behavior?: ColumnBehavior<P[K], true>): Ref<P[keyof P]>;
        export declare function competingBehavior<P extends object, K extends keyof P>(entry: P, key: K, notify?: () => void, behavior?: { deferred?: boolean }): Ref<boolean>;
        export declare function competingBehavior<P extends object, K extends keyof P>(entry: P, key: K, notify?: () => void, behavior?: ColumnBehavior<P[K], true>): Ref<P[K]>;
        export declare function mixedHandle<P extends object, K extends keyof P>(entry: P, key: K): Ref<P[K]>;
        export declare function mixedHandle(entry: string, key: string, count: number): number;
        export declare function reorderedHandle<P extends object, K extends keyof P>(entry: P, key?: K, options?: boolean): Ref<NonNullable<P[K]>>;
        export declare function reorderedHandle<P extends object, K extends keyof P>(entry: P, key?: K): import('vue').WritableComputedRef<P[K]>;
        export declare function requiredHandleTail<P extends object, K extends keyof P>(entry: P, key: K, emit: () => void): Ref<P[K]>;
        export declare function changedHandleIndex<P extends object, K extends keyof P>(entry: P, key: K): Ref<P[keyof P]>;
        export declare function specializedHandle(entry: { id: string }, key: 'id'): Ref<string>;
        export declare function specializedHandle<P extends object, K extends keyof P>(entry: P, key: K): Ref<P[K]>;
        export declare function putCell<TIndex extends string & keyof TRow, TRow extends object>(row: TRow, index: TIndex, cell: TRow[TIndex]): void;
        export declare function readRequired<TEntry extends { id: string }, TColumn extends keyof TEntry>(entry: TEntry, column: TColumn): TEntry[TColumn];
        export declare function readTextKeys<TEntry, TColumn extends keyof TEntry & 'id'>(entry: TEntry, column: TColumn): TEntry[TColumn];
        export declare function readMismatched<TEntry, TColumn extends keyof TEntry>(entry: TEntry, column: TColumn): TEntry[keyof TEntry];
        export declare function putUnrelated<TEntry, TColumn extends keyof TEntry>(entry: TEntry, column: TColumn, newValue: unknown): void;
        export declare function readExtra<TKey extends keyof TRight, TLeft, TRight>(entry: TLeft, column: TKey): TRight[TKey];
        export declare function readNested<TEntry, TColumn extends keyof TEntry>(entry: TEntry, column: `${TColumn & string}.value`): TEntry[TColumn];
        export declare namespace tooling { function setMode(mode: string): void; }
        export declare const fakeTooling: { setMode(mode: string): void };
        interface StopListening { release(): void }
        interface ObservedTarget {
            readonly tick: (listener: (unused: void) => void) => StopListening;
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
            settings?: { seed?: T; evaluateOnMount?: boolean }): { current: Ref<T>; readonly evaluated: boolean; label: string | Ref<string>; accept(candidate: T): void };
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
        export declare function observerError(): Error | null;
        type IsAbsent<T> = [T] extends [never] ? true : false;
        type MessageDictionary = Record<string, unknown>;
        export declare function messageStore(): {
            resolveText: {
                (text: string | (() => string)): string;
                (text: string | (() => string), count: number): string;
            };
            ambiguousText: {
                (text: string): number;
                (text: string | (() => string)): string;
            };
            retrieve<S extends MessageDictionary = never, L extends string = string, K extends string = string,
                R = IsAbsent<S> extends true ? {} : S>(language: L | K): R;
            replace<S extends MessageDictionary = never, L extends string = string, K extends string = string,
                P = IsAbsent<S> extends true ? MessageDictionary : S, M extends P = P>(language: L | K, contents: M): void;
            numeric<S extends MessageDictionary = never, L extends string = string, K extends string = string,
                P = IsAbsent<S> extends true ? Record<string, number> : S>(language: L | K, contents: P): void;
            optionalNumber<S extends MessageDictionary = never, L extends string = string, K extends string = string,
                P = IsAbsent<S> extends true ? { label?: number } : S>(language: L | K, contents: P): void;
            unrelated<S extends MessageDictionary = never, L extends string = string, K extends string = string,
                P = IsAbsent<L> extends true ? MessageDictionary : S>(language: L | K, contents: P): void;
            wrongConstraint<S extends MessageDictionary = never, L extends string = string, K extends string = string,
                P = IsAbsent<S> extends true ? MessageDictionary : S,
                M extends Record<string, number> = never>(language: L | K, contents: M): void;
        };
        export declare namespace ErrorDomain {
            interface Error { name: string; message: string; stack?: string }
        }
        export declare function lookalikeObserverError(): ErrorDomain.Error | null;
        export declare function localDateCheck(value: unknown): value is Domain.Date;
        export declare function plainDateCheck(value: unknown): boolean;
        export declare function overloadedDateCheck(value: number): number;
        export declare function overloadedDateCheck(value: unknown): boolean;
        export declare function interiorOption(key: string, backend?: { file: string }, options?: { verbose?: boolean }): string;
        export declare function interiorOption(key: string, backend?: { host: string }, options?: { retries?: number }): string;
        export declare function competingOption(key: string, backend: undefined, options: { retries?: number }): number;
        export declare function competingOption(key: string, backend?: { host: string }, options?: { retries?: number }): string;
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
        export declare function preserve<S>(value: S): S;
        export declare function preserveUnknown<S extends unknown>(value: S): S;
        export declare function selectFrom<R, S>(value: S, select: (value: S) => R): R;
        export declare function inspectAndKeep<R, S>(inspect: (value: S) => R, value: S): S;
        export declare function nestSelection<S, R>(value: S, select: (value: S) => Promise<(next: S) => Promise<R>>): Promise<(next: S) => Promise<R>>;
        export declare function transformSequence<R, S>(items: readonly S[], select: (item: S) => R): ReadonlyArray<R>;
        export declare function loadSequence<S, R>(items: ReadonlyArray<S>, select: (items: readonly S[]) => Promise<readonly R[]>): Promise<ReadonlyArray<R>>;
        export declare function transformPair<S, R>(items: readonly [S, S], select: (item: S) => R): readonly [R, R];
        export declare function transformMutable<S, R>(items: S[], select: (item: S) => R): R[];
        export declare function doublePromise<S, R>(value: S, select: (value: S) => Promise<Promise<R>>): Promise<Promise<R>>;
        export declare function measureSelection<S, R>(value: S, select: (value: S) => R): number;
        export declare function constrainedSelection<S extends { requiredTag: string }, R>(value: S, select: (value: S) => R): R;
        export declare function chooseSubtype<T, U extends T>(base: T, value: U): U;
        export declare function indexedSelection<S, R>(value: S, select: { (item: S): R; [key: string]: boolean }): R;
        export declare function conditionalSelection<S, R>(value: S, select: (value: S) => R): S extends string ? R : S;
        export declare function resultWithoutSource<S, R>(value: S): R;
        interface RightData<T> { right: T }
        interface PairData<TLeft, TRight> extends RightData<TRight> { readonly left: TLeft }
        type ReverseAlias<L, R> = PairData<R, L>;
        type Envelope<T> = { payload: T };
        export declare function reversePair<A, B>(pair: ReverseAlias<B, A>): PairData<B, A>;
        export interface PickSource<T> { data: T; readonly label: string; count: number; }
        export type CountPatch<T> = Partial<Pick<PickSource<T> & { nullableCount: number | null }, 'count' | 'nullableCount'>>;
        export declare function createPickSource<T>(data: T): PickSource<T>;
        export declare function pickLabel<T>(source: PickSource<T>): Pick<PickSource<T>, 'data' | 'label'>;
        export declare function pickCount<TProduced>(source: PickSource<TProduced>): CountPatch<TProduced>;
        export declare function acceptCount<TRequested>(source: PickSource<TRequested>, patch: CountPatch<TRequested>): number;
        export declare function openPick<T, K extends keyof PickSource<T>>(source: PickSource<T>): Pick<PickSource<T>, K>;
        declare const hiddenPickKey: unique symbol;
        export declare function symbolPick<T>(source: PickSource<T>): Pick<PickSource<T> & { [hiddenPickKey]: number }, typeof hiddenPickKey>;
        export declare function packPair<U, V>(pair: PairData<U, V>): Promise<Envelope<PairData<U, V>>>;
        export declare function packTwice<T>(value: T): Envelope<Envelope<T>>;
        declare namespace LeftVariant { interface Sample<T> { value: T } }
        declare namespace RightVariant { interface Sample<T> { value: T } }
        export declare function chooseVariant<A, B>(left: LeftVariant.Sample<A>, right: RightVariant.Sample<B>): RightVariant.Sample<A>;
        export declare function chooseVersion<A, B>(left: import('./v1/shape.js').Sample<A>, right: import('./v2/shape.js').Sample<B>): import('./v2/shape.js').Sample<A>;
        interface Tree<T> { value: T; readonly children: readonly Tree<T>[] }
        type Mutual<A, B> = { value: A; branch: MutualBranch<B, A> };
        interface MutualBranch<A, B> { value: A; readonly descendants: readonly Mutual<B, A>[] }
        interface Mixed<T, U> { value: T; readonly children: readonly Mixed<U, T>[] }
        export declare function makeTree<T>(value: T): Tree<T>;
        export declare function passTree<T>(value: Tree<T>): Tree<T>;
        export declare function passMutual<T, U>(value: Mutual<T, U>): Mutual<T, U>;
        export declare function makeMixed<T>(value: T, alternate: import('vue').Ref<T>): Mixed<T, import('vue').Ref<T>>;
        interface Phantom<T> { name: string }
        interface CallbackOnly<T> { invoke: (value: T) => void }
        interface OptionalData<T> { value?: T }
        interface IndexedData<T> { value: T; [key: string]: T }
        interface MethodData<T> { value: T; invoke(): void }
        interface ConstrainedData<T extends string> { value: T }
        interface RecursiveData<T> { value: T; next: RecursiveData<readonly T[]> }
        interface ExpandingStart<T> { value: T; next: ExpandingTail<readonly T[]> }
        interface ExpandingTail<T> { value: T; next: ExpandingStart<T> }
        interface PhantomCycle<T> { readonly children: readonly PhantomCycle<T>[] }
        interface UnknownCycle<T> { value: T; readonly children: readonly UnknownCycle<unknown>[] }
        export declare function fromPhantom<T>(record: Phantom<T>): T;
        export declare function fromCallbackOnly<T>(record: CallbackOnly<T>): T;
        export declare function fromOptional<T>(record: OptionalData<T>): T;
        export declare function fromIndexed<T>(record: IndexedData<T>): T;
        export declare function fromMethod<T>(record: MethodData<T>): T;
        export declare function fromConstraint<T>(value: T): ConstrainedData<string>;
        export declare function fromRecursive<T>(record: RecursiveData<T>): T;
        export declare function fromExpandingCycle<T>(record: ExpandingStart<T>): T;
        export declare function fromPhantomCycle<T>(record: PhantomCycle<T>): T;
        export declare function fromUnknownCycle<T>(record: UnknownCycle<T>): T;
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
        type BatchView<P, C> = { pages: P[]; pageParams: C[] };
        type BatchViewOptions<P, C, R> = BatchOptions<P, C> & { projectView?: (data: BatchView<P, C>) => R };
        export declare function chooseBatchView<P, C, R = BatchView<P, unknown>>(options: BatchViewOptions<P, C, R>):
            { data: import('vue').Ref<R | undefined> };
        export declare function fixedBatchView<P, C, R>(options: BatchViewOptions<P, C, R>): BatchResult<P>;
        export declare function ambiguousBatchView<P, C, R = BatchView<P, unknown>>(
            options: BatchViewOptions<P, C, R> & { anotherView?: (data: BatchView<P, C>) => R }):
            { data: import('vue').Ref<R | undefined> };
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
        export declare function editComputedRef(source: import('vue').WritableComputedRef<string, string | null>): void;
        type CalculatedWritable<R, W = R> = import('vue').WritableComputedRef<R, W>;
        export declare function keepClosedComputedRef(source: CalculatedWritable<string>): CalculatedWritable<string>;
        export declare function keepGenericComputedRef<T>(source: CalculatedWritable<T>): CalculatedWritable<T>;
        export declare function keepIndependentComputedRef<R, W>(source: CalculatedWritable<R, W>): CalculatedWritable<R, W>;
        export declare function keepNullableComputedRef<T>(source: CalculatedWritable<T, T | null>): CalculatedWritable<T, T | null>;
        declare const LocalComputedRefSymbol: unique symbol;
        interface LocalComputedRef extends Ref<string> {
            readonly [LocalComputedRefSymbol]: true;
        }
        interface ExtendedComputedRef extends CalculatedWritable<string> {
            reset(): void;
        }
        export declare function receiveLocalComputedRef(source: LocalComputedRef): string;
        export declare function receiveExtendedComputedRef(source: ExtendedComputedRef): string;
        export declare function receiveNarrowComputedRef(source: CalculatedWritable<string, 'initial'>): string;
        export declare function receiveUndefinedComputedRef(source: CalculatedWritable<string, string | null | undefined>): string;
        export declare function editWideRef(ref: Ref<string, string | null>): void;
        export declare function optionalNumberRef(present: boolean): Ref<number, number | null | undefined>;
        export declare function editOptionalNumberRef(ref: Ref<number, number | null | undefined>): void;
        type SourceRef<T> = Readonly<Ref<T>>;
        type Calculated<T> = ComputedRef<T>;
        export declare function transferRef<S, R>(source: SourceRef<S>, map: (value: S) => Calculated<R>): Calculated<R>;
        export declare function keepGenericRef<T>(source: Ref<T>): Ref<T>;
        export declare function receiveComputed<T>(source: Calculated<T>): T;
        interface ExtendedRef<T> extends Ref<T> { reset(): void }
        interface VueProjectionPayload {
            title: Ref<string>;
            nested: { count: Ref<number> };
            items: Ref<number>[];
        }
        export declare function buildDeepPayload(): Ref<import('vue').UnwrapRef<VueProjectionPayload>, VueProjectionPayload | import('vue').UnwrapRef<VueProjectionPayload>>;
        type ShallowPayload = import('vue').ShallowRef<VueProjectionPayload>;
        export declare function buildShallowPayload(): ShallowPayload;
        export declare function buildSymmetricPayload(): Ref<import('vue').UnwrapRef<VueProjectionPayload>>;
        export declare function buildWiderPayload(): Ref<import('vue').UnwrapRef<VueProjectionPayload>, VueProjectionPayload | import('vue').UnwrapRef<VueProjectionPayload> | string>;
        type UnwrapRefSimple<T> = { [Key in keyof T]: T[Key] };
        export declare function buildImitationPayload(): Ref<UnwrapRefSimple<VueProjectionPayload>>;
        export declare function buildReactivePayload(): import('vue').ShallowReactive<VueProjectionPayload>;
        type SignalMode = 'enabled' | 'disabled';
        type GaugeReading = Ref<SignalMode> & {
            readonly status: ComputedRef<SignalMode>;
            readonly source: Ref<SignalMode>;
        };
        export declare function buildGauge(): GaugeReading;
        type PulseMode = 'quick' | 'slow' | null;
        type PulseLink = 'local' | 'remote' | undefined;
        export declare function readPulse(): {
            mode: import('vue').ShallowRef<PulseMode>;
            equivalentMode: import('vue').ShallowRef<PulseMode>;
            link: Readonly<import('vue').ShallowRef<PulseLink>>;
        };
        type GaugeMode<T extends string> = 'disabled' | T;
        export declare function readFastGauge(): Ref<GaugeMode<'fast'>>;
        export declare function readSlowGauge(): Ref<GaugeMode<'slow'>>;
        export declare function buildConflictingGauge(): GaugeReading & { readonly value: 'enabled' };
        export declare function acceptGauge(source: GaugeReading): SignalMode;
        type Instrument<T> = Ref<T> & { readonly label: string; read(): T; replace(value: T): void };
        type CounterInstrument = Instrument<number>;
        type IncompleteInstrument = Ref<number> & { read(): number; unsupported: { new(): object } };
        export declare function makeInstrument(): CounterInstrument;
        export declare function passInstrument(value: Instrument<number>): CounterInstrument;
        export declare function passTextInstrument(value: Instrument<string>): Instrument<string>;
        export declare function passIncompleteInstrument(value: IncompleteInstrument): number;
        export declare function makeWriter(): { submit: (data: number | (() => Promise<number | undefined>)) => Promise<void> };
        export declare function makeOptionalWriter(): { submit: (data?: number | (() => Promise<number | undefined>)) => Promise<void> };
        export declare function makeMismatchedWriter(): { submit: (data: number | (() => Promise<string>)) => Promise<void> };
        interface ArchiveFilter<K> { key?: K; exact?: boolean }
        declare const archiveTag: unique symbol;
        type ArchiveKey = readonly unknown[];
        type ArchivePayload<D, K> = K extends { readonly [archiveTag]: infer V } ? V : D;
        type ArchiveUpdater<D> = D | undefined | ((previous: D | undefined) => D | undefined);
        interface ArchiveLoad<D, K> { coordinates: K; loader?: () => D | Promise<D> }
        interface ArchiveController {
            sweep<K extends readonly unknown[] = readonly unknown[]>(filter?: ArchiveFilter<K>): void;
            conflict<K extends readonly unknown[] = readonly unknown[]>(filter?: ArchiveFilter<K>): number;
            conflict<K extends readonly unknown[] = readonly unknown[]>(filter?: ArchiveFilter<K>): void;
            peek<D = unknown, K extends ArchiveKey = ArchiveKey, V = ArchivePayload<D, K>>(key: K): V | undefined;
            wrongDefault<D = unknown, K extends ArchiveKey = ArchiveKey, V = string>(key: K): V | undefined;
            wrongKey<D = unknown, K extends ArchiveKey = ArchiveKey, V = ArchivePayload<D, K>>(key: readonly [K]): V | undefined;
            revise<D = unknown, K extends ArchiveKey = ArchiveKey, V = ArchivePayload<D, K>>(key: K, updater: ArchiveUpdater<NoInfer<V>>): V | undefined;
            wrongUpdater<D = unknown, K extends ArchiveKey = ArchiveKey, V = ArchivePayload<D, K>>(key: K, updater: (previous: V | undefined) => V | null): V | undefined;
            resolve<D, V = D, K extends ArchiveKey = ArchiveKey>(options: ArchiveLoad<D, K>): Promise<V>;
            competingLoad(options: object): number;
            competingLoad<D, V = D, K extends ArchiveKey = ArchiveKey>(options: ArchiveLoad<D, K>): Promise<V>;
            wrongPromise<D, V = D, K extends ArchiveKey = ArchiveKey>(options: ArchiveLoad<D, K>): PromiseLike<V>;
        }
        export declare function openArchive(): ArchiveController;
        export declare function makeLookup(): {
            contains<S extends string, K extends 'reserved' = 'reserved'>(key: S | K, locale?: string): boolean;
            restricted<K extends 'reserved' = 'reserved', S extends K = K>(key: S, locale?: string): boolean;
            competing: {
                <S extends string>(key: S): number;
                <S extends string>(key: S, locale?: string): boolean;
            };
        };
        interface CatalogEntry { readonly id: string; revision: number; describe(): string }
        interface CatalogView { selection(): CatalogEntry | null; setSelection(value: CatalogEntry): void }
        export declare function inspectCatalog(): CatalogView;
        export declare function getCatalogEntry(): CatalogEntry;
        export declare function passVersionedInstrument(left: import('./v1/shape.js').Instrument<number>,
            right: import('./v2/shape.js').Instrument<number>): import('./v2/shape.js').Instrument<number>;
        type ShadowRef<T> = { readonly value: T };
        export declare function receiveExtendedRef<T>(source: ExtendedRef<T>): T;
        export declare function receiveAsymmetricRef<T>(source: Ref<T, T | null>): T;
        export declare function passIndependentRef<TRead, TWrite>(source: Ref<TRead, TWrite>): Ref<TRead, TWrite>;
        export declare function receiveNarrowRef(source: Ref<string, 'initial'>): string;
        export declare function receiveShadowRef<T>(source: ShadowRef<T>): T;
        type Selection<B extends boolean> = B extends true ? { path: string } : number;
        export declare function selectMode<B extends boolean>(label: string, enabled: B, count: number): Selection<B>;
        export declare function selectCollision<B extends boolean>(enabled: B): Selection<B>;
        export declare function selectCollisionWithEnabledTrue(): number;
        export declare function choose<B extends boolean>(detail: B): Selection<B>;
        export declare function chooseWithDetailTrue(): string;
        export declare function chooseWithDetailFalse(): string;
        export declare function selectTwice<B extends boolean>(first: B, second: B): Selection<B>;
        export declare function selectOptional<B extends boolean = false>(enabled?: B): Selection<B>;
        export declare function selectAmbiguous<B extends boolean>(enabled?: B): Selection<B>;
        export declare function selectCorrelated<B extends boolean = false>(options: { enabled: B }, enabled?: B): Selection<B>;
        export declare function selectOptionalTail<B extends boolean = true>(label: string, enabled?: B, count?: number): Selection<B>;
        export declare function countDates(dates: readonly (Date | number | string)[]): number;
        export declare function replaceDate(dates: (Date | number | string)[]): void;
        export declare function flipValue(values: (number | boolean)[]): void;
        export declare function replaceLiteral(values: (Date | "fixed")[]): void;
        export declare function optionalNumbers(): { absent?: number; nullable?: number | null };
        interface OptionalRecord<T> { value?: T; hasValue: string }
        interface OptionalMappedSource<T> { value: T; ignored: string }
        type OptionalMapped<T> = Partial<Pick<OptionalMappedSource<T>, "value">>;
        interface OptionalNullable<T> { value?: T | null }
        interface RequiredNullable<T> { value: T | null }
        interface OptionalUndefined<T> { value?: T | undefined }
        export declare function optionalRecord<T>(initial: T): OptionalRecord<T>;
        export declare function optionalMapped<T>(initial: T): OptionalMapped<T>;
        export declare function optionalNullable<T>(initial: T): OptionalNullable<T>;
        export declare function requiredNullable<T>(initial: T): RequiredNullable<T>;
        export declare function optionalUndefined<T>(initial: T): OptionalUndefined<T>;
        export declare function paint(settings: { tone: "" | "warm-tone" | "warm_tone" }): string;
        """);
    var document = Path.Combine(project, "views", "Widget.jazor");
    await File.WriteAllTextAsync(Path.Combine(project, "Pointer.vue"), """
        <script setup lang="ts">
        interface MouseEvent { label: string }
        defineEmits<{ click: [value: globalThis.MouseEvent]; picked: [value: MouseEvent] }>()
        </script>
        <template><button /></template>
        """);
    var localPointer = (await provider.ReadLocalComponentsAsync(project, ["Pointer.vue"],
        new Dictionary<string, string>(), default, () => new Dictionary<string, string>())).Single();
    Check(localPointer.Component?.Events["click"].Signatures is [[{ WebType: "MouseEvent" }]] &&
        localPointer.Component.Events["picked"].Signatures is [[{ WebType: null }]],
        $"local Vue events preserve standard DOM identity and distinct local namesakes: {localPointer.Error}");
    var pointer = await provider.ReadAsync(document, "component-fixture", "PointerWidget");
    var shadowPointer = await provider.ReadAsync(document, "component-fixture", "ShadowPointerWidget");
    Check(pointer.Events["click"].Signatures is [[{ WebType: "MouseEvent" }]] &&
        shadowPointer.Events["click"].Signatures is [[{ WebType: null }]],
        "installable component event payloads use standard DOM declaration identity");
    Check(pointer.Props["width"] is { InputType: "number|string", LiteralValues: [] } &&
        pointer.Props["fixed"] is { InputType: "boolean|string", LiteralValues: var fixedValues } &&
        fixedValues.Order(StringComparer.Ordinal).SequenceEqual(["left", "right"]) &&
        pointer.Props["pageSizes"].InputType == "number[]" && pointer.Props["showTooltip"].InputType == "boolean",
        "component input projection keeps supported branches of the checked declaration");
    const string pointerSource = """
        @using Npm.ComponentFixture
        <PointerWidget Tone="quiet" Width="140px" Fixed="@Pinned" PageSizes="@(new double[] { 10, 25 })" ShowTooltip="@true" @onclick="@Click" />
        <PointerWidget Width="@(140)" Fixed="left" Rows="@Entries" @onselection="@Selection" />
        @code {
            public sealed record Entry(string id, int rank, string? note, int? score, bool? visible);
            private Entry[] Entries = [new("x", 1, null, null, null)];
            private bool Pinned = true;
            private void Click(ECMAScript.MouseEvent payload) {}
            private void Selection(string[] ids) {}
        }
        """;
    var pointerOutput = await JoltProjectCompiler.CompileAsync([new("Pointer.jazor", pointerSource)],
        new JoltCompileOptions(NpmComponents: [pointer]));
    Check(pointerOutput.Diagnostics.Count == 0 &&
        pointerOutput.Modules.Single().Content.Contains("\"onClick\": this.Click.bind(this)", StringComparison.Ordinal) &&
        pointerOutput.Modules.Single().Content.Contains("\"rows\": this.$jazor$private$Entries", StringComparison.Ordinal) &&
        pointerOutput.Modules.Single().Content.Contains("\"onSelection\": this.Selection.bind(this)", StringComparison.Ordinal) &&
        pointer.Events["snapshot"].Signatures is [[{ ValueDomain: null }]] &&
        pointer.Events["spread"].Signatures is [[{ ValueDomain: null }]],
        "component callbacks forward the original DOM event");
    var wrongRowsOutput = await JoltProjectCompiler.CompileAsync(
        [new("Pointer.jazor", pointerSource.Replace("int rank", "int wrongRank", StringComparison.Ordinal))],
        new JoltCompileOptions(NpmComponents: [pointer]));
    Check(wrongRowsOutput.Diagnostics.Any(item => item.Id == "JOLT1014"),
        "readonly DTO arrays match declared Vue field names, independently of the record name");
    Check(pointer.Props["rows"].InputRecordFields is { } rowFields &&
        rowFields["note"] == "string?" && rowFields["score"] == "number?" && rowFields["visible"] == "boolean?",
        "required nullable primitive fields stay distinct from non-null fields");
    Check(pointer.Props["optionalRows"].InputRecordFields is null &&
        pointer.Props["undefinedRows"].InputRecordFields is null &&
        pointer.Props["mixedRows"].InputRecordFields is null &&
        pointer.Props["literalRows"].InputRecordFields is null,
        "optional fields, undefined, mixed domains and literal restrictions are not nullable primitives");
    var nullableRequired = await JoltProjectCompiler.CompileAsync(
        [new("Pointer.jazor", pointerSource.Replace("int rank", "int? rank", StringComparison.Ordinal))],
        new JoltCompileOptions(NpmComponents: [pointer]));
    Check(nullableRequired.Diagnostics.Any(item => item.Id == "JOLT1014"),
        "a nullable CLR field cannot satisfy a non-null Vue field");
    var shadowPointerOutput = await JoltProjectCompiler.CompileAsync(
        [new("Pointer.jazor", pointerSource.Replace("<PointerWidget", "<ShadowPointerWidget", StringComparison.Ordinal))],
        new JoltCompileOptions(NpmComponents: [shadowPointer]));
    Check(shadowPointerOutput.Diagnostics.Any(item => item.Id == "JOLT1014"), "a namesake event type has no DOM carrier");
    var wrongPointerOutput = await JoltProjectCompiler.CompileAsync(
        [new("Pointer.jazor", pointerSource.Replace("Fixed=\"left\"", "Fixed=\"middle\"", StringComparison.Ordinal))],
        new JoltCompileOptions(NpmComponents: [pointer]));
    Check(wrongPointerOutput.Diagnostics.Any(item => item.Id == "JOLT1006"), "mixed props preserve their string literal domain");
    var optionsWidget = await provider.ReadAsync(document, "component-fixture", "OptionsWidget");
    var recordWidget = await provider.ReadAsync(document, "component-fixture", "RecordWidget");
    Check(optionsWidget.Props["selected"] is { InputType: "number|string", Required: false } &&
        optionsWidget.Props["label"].InputType == "string" &&
        optionsWidget.Slots["default"].Signatures is [[]] &&
        optionsWidget.Events["update:selected"].Signatures is [[{ ValueDomain: "number|string" }]] &&
        optionsWidget.Events["unsafe"].Signatures is [[{ ValueDomain: null }]] &&
        optionsWidget.Events["more"].Signatures is [[{ ValueDomain: null }]] &&
        recordWidget.Props["selected"].InputType is null,
        "only Vue-extracted runtime options reveal their PropType payload and output unions stay closed");
    const string optionsSource = """
        @using Npm.ComponentFixture
        <OptionsWidget Selected="@(2d)" Label="Label" @onupdate:selected="@Change"><span>Default</span></OptionsWidget>
        @code {
            private void Change(ECMAScript.Vue.VueStringNumberValue selected) {}
        }
        """;
    var optionsOutput = await JoltProjectCompiler.CompileAsync([new("Options.jazor", optionsSource)],
        new JoltCompileOptions(NpmComponents: [optionsWidget]));
    Check(optionsOutput.Diagnostics.Count == 0 && optionsOutput.Modules.Single().Content.Contains("\"onUpdate:selected\"", StringComparison.Ordinal),
        $"Vue runtime props and scalar event unions compile: {string.Join(", ", optionsOutput.Diagnostics)}");
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
    // pnpm restores file: dependencies from their source directory before lookup.
    // Change the source of the installed declaration so a restore cannot undo the edit.
    var widgetDeclaration = Path.Combine(package, "index.d.ts");
    var originalWidget = await File.ReadAllTextAsync(widgetDeclaration);
    await File.WriteAllTextAsync(widgetDeclaration, originalWidget.Replace(
        "interface WidgetProps { tone:", "interface WidgetProps { refreshed?: boolean; tone:", StringComparison.Ordinal));
    var second = await provider.ReadAsync(document, "component-fixture/widgets", "default");
    Check(second.Props.ContainsKey("refreshed") && !first.Props.ContainsKey("refreshed"),
        "installed declaration content invalidation");

    var candidates = await provider.ListValueExportsAsync(document, "component-fixture/values");
    Check(candidates.SequenceEqual(["count", "default", "echo", "finiteOptions", "generic", "overloadedCallback", "readonlyShallow"]), "runtime value candidate discovery");
    var finiteOptions = await provider.ReadValueAsync(document, "component-fixture/values", "finiteOptions");
    Check(finiteOptions.Signatures is [{ Parameters: [{ Type.Members: { } finiteMembers }] }] &&
        finiteMembers.Any(member => member is { Name: "ratio", Type: { Kind: "numberLiterals", Name: "Ratio",
            NumberLiterals: [-0.25, 0.5, 1.5] } }) &&
        finiteMembers.Any(member => member is { Name: "mode", Type.Kind: "enum" }),
        "finite fractional inputs preserve their literals and declared enums have a separate projection");
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
    var transfer = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["preserve", "preserveUnknown", "selectFrom", "inspectAndKeep", "nestSelection", "measureSelection",
            "transformSequence", "loadSequence", "transformPair", "transformMutable",
            "constrainedSelection", "chooseSubtype", "indexedSelection", "conditionalSelection", "doublePromise", "resultWithoutSource"]);
    var transferSignatures = transfer.Values.ToDictionary(export => export.ExportName, export => export.Signatures[0]);
    Check(transferSignatures.GetValueOrDefault("preserve") is {
        TypeParameters: ["S"], Parameters: [{ Type: { Kind: "typeParameter", Name: "S" } }],
        ReturnType: { Kind: "typeParameter", Name: "S" } }, "an ordinary generic value preserves its declaration slot");
    Check(transferSignatures.GetValueOrDefault("preserveUnknown") is {
        TypeParameters: ["S"], Parameters: [{ Type: { Kind: "typeParameter", Name: "S" } }],
        ReturnType: { Kind: "typeParameter", Name: "S" } }, "an explicit unknown bound preserves the unrestricted generic slot");
    Check(transferSignatures.GetValueOrDefault("selectFrom") is {
        TypeParameters: ["R", "S"], Parameters: [{ Type.Name: "S" }, { Type.Signatures: [
            { Parameters: [{ Type.Name: "S" }], ReturnType.Name: "R" }] }], ReturnType.Name: "R" },
        "a reordered declaration preserves the independent callback result");
    Check(transferSignatures.GetValueOrDefault("inspectAndKeep") is {
        TypeParameters: ["R", "S"], Parameters: [{ Type.Signatures: [
            { Parameters: [{ Type.Name: "S" }], ReturnType.Name: "R" }] }, { Type.Name: "S" }], ReturnType.Name: "S" },
        "returning the original value preserves its slot while accepting an independent callback result");
    Check(transferSignatures.GetValueOrDefault("nestSelection") is {
        TypeParameters: ["S", "R"], Parameters: [{ Type.Name: "S" }, { Type.Signatures: [
            { Parameters: [{ Type.Name: "S" }], ReturnType: { Kind: "promise", ElementType.Signatures: [{
                Parameters: [{ Type.Name: "S" }], ReturnType: { Kind: "promise", ElementType.Name: "R" } }] } }] }],
        ReturnType: { Kind: "promise", ElementType.Signatures: [{
            Parameters: [{ Type.Name: "S" }], ReturnType: { Kind: "promise", ElementType.Name: "R" } }] } },
        "Promise fulfillment preserves a function with its own Promise result");
    Check(transferSignatures.GetValueOrDefault("measureSelection") is {
        TypeParameters: ["S", "R"], Parameters: [{ Type.Name: "S" }, { Type.Signatures: [
            { Parameters: [{ Type.Name: "S" }], ReturnType.Name: "R" }] }], ReturnType.Kind: "number" },
        "a fixed return retains its declared numeric type despite the independent callback result");
    Check(transferSignatures.GetValueOrDefault("transformSequence") is {
        TypeParameters: ["R", "S"], Parameters: [
            { Type: { Kind: "array", ReadOnly: true, ElementType.Name: "S" } },
            { Type.Signatures: [{ Parameters: [{ Type.Name: "S" }], ReturnType.Name: "R" }] }],
        ReturnType: { Kind: "array", ReadOnly: true, ElementType.Name: "R" } },
        "renamed standard readonly arrays share their element slots with the selector");
    Check(transferSignatures.GetValueOrDefault("loadSequence") is {
        Parameters: [{ Type: { Kind: "array", ReadOnly: true, ElementType.Name: "S" } },
            { Type.Signatures: [{ Parameters: [{ Type: { Kind: "array", ReadOnly: true, ElementType.Name: "S" } }],
                ReturnType: { Kind: "promise", ElementType: { Kind: "array", ReadOnly: true, ElementType.Name: "R" } } }] }],
        ReturnType: { Kind: "promise", ElementType: { Kind: "array", ReadOnly: true, ElementType.Name: "R" } } },
        "readonly arrays preserve their element slots through callbacks and Promise fulfillment");
    Check(transfer.Values.Single(export => export.ExportName == "transformSequence").Signatures is [
        { Parameters: [{ Type.Kind: "array" }, _] },
        { Parameters: [{ Type: { Kind: "recordArray", ElementType.Name: "S" } }, _],
            ReturnType: { Kind: "array", ReadOnly: true, ElementType.Name: "R" } }],
        "a direct readonly array input also accepts a managed array without changing the result type");
    Check(transfer.Errors.Keys.Order(StringComparer.Ordinal).SequenceEqual(
        ["chooseSubtype", "conditionalSelection", "constrainedSelection", "doublePromise", "indexedSelection", "resultWithoutSource",
            "transformMutable", "transformPair"]),
        "the direct transfer projection rejects unsupported constraints, callable state, unresolved conditions, nested fulfillment and unbound results. " +
        $"Actual rejections: {string.Join(", ", transfer.Errors.Keys.Order(StringComparer.Ordinal))}");
    var picks = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["pickLabel", "pickCount", "acceptCount", "createPickSource", "openPick", "symbolPick"]);
    Check(picks.Errors.Keys.Order(StringComparer.Ordinal).SequenceEqual(["openPick", "symbolPick"]),
        "finite generic Picks project and open or symbol keys are rejected: " +
        string.Join(", ", picks.Errors));
    var pickLabel = picks.Values.Single(export => export.ExportName == "pickLabel").Signatures.Single();
    Check(pickLabel.ReturnType.Members is [
        { Name: "data", Type.Name: "T", Optional: false, ReadOnly: false },
        { Name: "label", Type.Kind: "string", Optional: false, ReadOnly: true }],
        "Pick preserves the selected field types and readonly modifiers");
    var pickCount = picks.Values.Single(export => export.ExportName == "pickCount").Signatures.Single();
    var acceptCount = picks.Values.Single(export => export.ExportName == "acceptCount").Signatures.Single();
    Check(pickCount.ReturnType is { TypeIdentity: not null, TypeArguments: [{ Name: "TProduced" }] } &&
        acceptCount.Parameters[1].Type is { TypeArguments: [{ Name: "TRequested" }] } inputPatch &&
        pickCount.ReturnType.TypeIdentity == inputPatch.TypeIdentity, "named Pick aliases retain their resolved type arguments and share declaration identity");
    Check(pickCount.ReturnType.Members is [{ Name: "count", Type.Kind: "number", Optional: true },
        { Name: "nullableCount", Type: { Kind: "nullable", ElementType.Kind: "number" }, Optional: true }] &&
        acceptCount.Parameters[1].Type.Members is [{ Name: "count", Type.Kind: "number", Optional: true },
        { Name: "nullableCount", Type: { Kind: "nullable", ElementType.Kind: "number" }, Optional: true }],
        "Partial Pick preserves optional numeric reads and writes");
    var pickOptions = new JoltCompileOptions { NpmValues = picks.Values };
    const string pickCaller = """
        @using Tool = Npm.ComponentFixture.Other.Exports
        @code {
            public static double Run() {
                var source = Tool.CreatePickSource("payload");
                var output = Tool.PickLabel(source);
                string data = output.Data;
                output.Data = "updated";
                double? count = Tool.PickCount(source).Count;
                return Tool.AcceptCount(source, new() { Count = 0, NullableCount = null }) + Tool.AcceptCount(source, new()) +
                    Tool.AcceptCount(source, Tool.PickCount(source)) + (count ?? 0);
            }
        }
        """;
    var picked = await JoltProjectCompiler.CompileAsync([new("Pick.jazor", pickCaller)], pickOptions);
    Check(picked.Diagnostics.Count == 0, "finite Picks compile in C#: " + string.Join(", ", picked.Diagnostics));
    Check(picked.Modules.Single().Content.Contains("?? null", StringComparison.Ordinal), "optional numeric reads normalize undefined");
    Check(picked.Modules.Single().Content.Contains("count: 0", StringComparison.Ordinal), "optional input preserves zero");
    Check(picked.Modules.Single().Content.Contains("nullableCount: null", StringComparison.Ordinal), "nullable optional input preserves explicit null");
    foreach (var (invalid, diagnostic) in new[] {
        (pickCaller.Replace("output.Data = \"updated\"", "output.Label = \"updated\""), "CS0200"),
        (pickCaller.Replace("output.Data = \"updated\"", "output.Count = 5"), "CS1061"),
        (pickCaller.Replace("Count = 0", "Count = null"), "CS0037"),
    }) {
        var result = await JoltProjectCompiler.CompileAsync([new("Pick.jazor", invalid)], pickOptions);
        Check(result.Diagnostics.Any(item => item.Id == diagnostic), "invalid finite Pick C# use yields " + diagnostic);
    }
    var recordExports = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["reversePair", "packPair", "packTwice", "chooseVariant", "chooseVersion", "fromPhantom", "fromCallbackOnly",
            "fromOptional", "fromIndexed", "fromMethod", "fromConstraint", "fromRecursive", "makeTree", "passTree", "passMutual",
            "makeMixed", "fromExpandingCycle", "fromPhantomCycle", "fromUnknownCycle"]);
    var reversePair = recordExports.Values.Single(export => export.ExportName == "reversePair").Signatures.Single();
    var pairRecord = reversePair.Parameters.Single().Type;
    Check(pairRecord is { Kind: "namedRecord", Name: "PairData", TypeParameters: ["TLeft", "TRight"],
        TypeArguments: [{ Name: "A" }, { Name: "B" }] } &&
        pairRecord.Members!.Single(member => member.Name == "left") is { ReadOnly: true, Type.Name: "TLeft" } &&
        pairRecord.Members!.Single(member => member.Name == "right") is { ReadOnly: false, Type.Name: "TRight" } &&
        reversePair.ReturnType.TypeArguments is [{ Name: "B" }, { Name: "A" }] &&
        pairRecord.TypeIdentity == "[\"component-fixture\",\"1.0.0\",\"other.d.ts\",\"PairData\"]",
        "a substituted interface alias preserves declaration slots, source arguments and inherited mutability");
    var packPair = recordExports.Values.Single(export => export.ExportName == "packPair").Signatures.Single();
    Check(packPair.ReturnType is { Kind: "promise", ElementType: { Kind: "namedRecord", Name: "Envelope",
        TypeParameters: ["T"], Members: [{ Type.Name: "T" }], TypeArguments: [{ Name: "PairData" } pair] } } &&
        pair.TypeDefinition == pairRecord.TypeDefinition && pair.TypeDefinition == packPair.Parameters.Single().Type.TypeDefinition,
        "a named type composes with Promise and reuses the same source declaration across exports");
    Check(recordExports.Values.Single(export => export.ExportName == "packTwice").Signatures.Single().ReturnType is
        { Name: "Envelope", TypeArguments: [{ Name: "Envelope", TypeArguments: [{ Name: "T" }] }] },
        "finite nesting can reuse the same data declaration");
    var chooseVariant = recordExports.Values.Single(export => export.ExportName == "chooseVariant").Signatures.Single();
    Check(chooseVariant.Parameters[0].Type.TypeIdentity == "[\"component-fixture\",\"1.0.0\",\"other.d.ts\",\"LeftVariant\",\"Sample\"]" &&
        chooseVariant.Parameters[1].Type.TypeIdentity == "[\"component-fixture\",\"1.0.0\",\"other.d.ts\",\"RightVariant\",\"Sample\"]" &&
        chooseVariant.ReturnType.TypeIdentity == chooseVariant.Parameters[1].Type.TypeIdentity,
        "an identical record in a different namespace receives a separate stable declaration identity");
    var chooseVersion = recordExports.Values.Single(export => export.ExportName == "chooseVersion").Signatures.Single();
    Check(chooseVersion.Parameters[0].Type.TypeIdentity == "[\"versioned-data\",\"1.0.0\",\"shape.d.ts\",\"Sample\"]" &&
        chooseVersion.Parameters[1].Type.TypeIdentity == "[\"versioned-data\",\"2.0.0\",\"shape.d.ts\",\"Sample\"]" &&
        chooseVersion.ReturnType.TypeIdentity == chooseVersion.Parameters[1].Type.TypeIdentity,
        "an identical record in a different version receives a separate stable declaration identity");
    Check(NpmBindings.GenerateSource([], recordExports.Values).Split("public sealed record Sample_").Length == 5,
        "the public projection emits distinct carriers for each package version and namespace");
    var makeTree = recordExports.Values.Single(export => export.ExportName == "makeTree").Signatures.Single().ReturnType;
    var passTree = recordExports.Values.Single(export => export.ExportName == "passTree").Signatures.Single();
    Check(makeTree is { Kind: "namedRecord", Name: "Tree", Members: [
        { Name: "value", Type.Name: "T" }, { Name: "children", ReadOnly: true, Type: { Kind: "array", ReadOnly: true,
            ElementType: { Kind: "namedRecord", Name: "Tree", Members: null, TypeArguments: [{ Name: "T" }] } child } }] } &&
        child.TypeIdentity == makeTree.TypeIdentity && passTree.Parameters.Single().Type.TypeIdentity == child.TypeIdentity,
        "recursive readonly array elements reuse the same declaration and infer the payload from its data field");
    var mutual = recordExports.Values.Single(export => export.ExportName == "passMutual").Signatures.Single().ReturnType;
    Check(mutual is { Name: "Mutual", Members: [_, { Type: { Name: "MutualBranch", TypeArguments: [{ Name: "B" }, { Name: "A" }],
        Members: [_, { Type.ElementType: { Name: "Mutual", Members: null, TypeArguments: [{ Name: "B" }, { Name: "A" }] } descendant }] } }] } &&
        descendant.TypeIdentity == mutual.TypeIdentity,
        "mutually recursive records preserve the substitution on each edge and both payloads participate in inference");
    Check(recordExports.Values.Single(export => export.ExportName == "makeMixed").Signatures.Single().ReturnType is
        { TypeArguments: [{ Name: "T" }, { Kind: "ref", ElementType.Name: "T" }], Members: [_,
            { Type.ElementType: { Members: null, TypeArguments: [{ Name: "U" }, { Name: "T" }] } }] },
        "a ref supplied as a concrete outer argument can become a data field through a recursive slot swap");
    Check(recordExports.Errors.Keys.Order(StringComparer.Ordinal).SequenceEqual(
        ["fromCallbackOnly", "fromConstraint", "fromExpandingCycle", "fromIndexed", "fromMethod", "fromPhantom",
            "fromPhantomCycle", "fromRecursive", "fromUnknownCycle"]),
        "named records preserve the data-only contract, require inference evidence and reject unbounded or unsupported substitutions. " +
        $"Actual rejections: {string.Join(", ", recordExports.Errors.Keys.Order(StringComparer.Ordinal))}");
    Check(recordExports.Values.Single(export => export.ExportName == "fromOptional").Signatures is [
        { Parameters: [{ Type: { TypeArguments: [{ Name: "T" }], Members: [
            { Optional: true, Type: { Kind: "optionalSlot", ElementType.Name: "T" } }] } }],
            ReturnType.Name: "T" }],
        "a required typed input record provides inference evidence for its optional generic field");
    var batches = await provider.ReadValueAsync(document, "component-fixture/other", "collectBatches");
    var selectedBatches = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["chooseBatchView", "fixedBatchView", "ambiguousBatchView"]);
    var selected = selectedBatches.Single(export => export.ExportName == "chooseBatchView");
    Check(selected.Signatures.Count == 2 &&
        selected.Signatures[0].ReturnType.Members!.Single().Type.ElementType?.ElementType?.Members is [{ Name: "pages" }] &&
        selected.Signatures[1].ReturnType.Members!.Single().Type.ElementType?.ElementType is { Kind: "typeParameter", Name: "TResult" } &&
        selected.Signatures[1].Parameters.Single().Type.Members!.Single(member => member.Name == "projectView") is
            { Optional: false, Type.Signatures: [{ Parameters: [{ Type.Members: [
                { Name: "pages", Type.ElementType.Name: "TPage" },
                { Name: "pageParams", Type.ElementType.Name: "TPageParam" },
            ] }], ReturnType.Name: "TResult" }] } &&
        selectedBatches.Single(export => export.ExportName == "fixedBatchView").Signatures.Count == 1 &&
        selectedBatches.Single(export => export.ExportName == "ambiguousBatchView").Signatures.Count == 1,
        "a unique renamed callback independently determines the result ref");
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
    var pulse = await provider.ReadValuesAsync(document, "component-fixture/other", ["makePulse", "mismatchedPulse"]);
    Check(pulse.Single(value => value.ExportName == "makePulse").Signatures is [{
        TypeParameters: ["TPayload"], ReturnType.Members: [
            { Name: "subscribe", Type.Signatures: [{ ReturnType.Members: [{ Name: "dispose" }] }] },
            { Name: "forget" },
            { Name: "broadcast", Type.Signatures: [{ Parameters: [{ Type.Name: "TPayload" }], ReturnType.Kind: "promise" }] },
            { Name: "reset" }
        ]
    }] && pulse.Where(value => value.ExportName == "mismatchedPulse").SelectMany(value => value.Signatures)
        .All(signature => signature.TypeParameters is not ["TPayload"]),
        "event hook projection follows renamed subscription, removal and dispatch members with one shared payload");
    var queryData = await provider.ReadValueAsync(document, "component-fixture/other", "queryDataProtocol");
    var dataMembers = queryData.Signatures.Single().ReturnType.Members!;
    Check(dataMembers.Single(member => member.Name == "valid").Type.Signatures is
        [{ ReturnType.Kind: "undefinedable" }, { ReturnType.Kind: "undefinedable", ValueTypeParameters: ["T"] }],
        "query cache reads preserve missing reference and value payloads");
    Check(dataMembers.Single(member => member.Name == "reordered").Type.Signatures![0].ReturnType is
        { Kind: "undefinedable", ElementType: { Kind: "typeParameter", Name: "T" } },
        "a keyed conditional alias with reordered type parameters still returns the payload for ordinary keys");
    Check(!dataMembers.Any(member => member.Name is "returnsKey" or "customThis" or "requiredCallbackField" ||
        member.Name == "narrowFirst" && member.Type.Signatures!.Any(signature => signature.TypeParameters is ["T"])),
        "query cache projection rejects key-dependent payloads, earlier overlapping overloads, explicit receivers and callbacks with required fields");
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
    var listeners = subscriptions.Signatures.Single().ReturnType.Members!;
    Check(listeners.Count == 2 && listeners.Single(member => member.Name == "tick").Type.Signatures is [{
        Parameters: [{ Type.Signatures: [{ Parameters.Count: 0, ReturnType.Kind: "void" }] }],
        ReturnType.Members: [{ Name: "release" }] }], "a void event carries no C# listener argument");
    var subscription = listeners.Single(member => member.Name == "observe");
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
    var messages = await provider.ReadValueAsync(document, "component-fixture/other", "messageStore");
    var messageMethods = messages.Signatures.Single().ReturnType.Members!;
    Check(messageMethods.Single(member => member.Name == "retrieve").Type.Signatures is
            [{ Parameters: [{ Type.Kind: "string" }], ReturnType.Kind: "plainRecord" }] &&
        messageMethods.Single(member => member.Name == "replace").Type.Signatures is
            [{ Parameters: [{ Type.Kind: "string" }, { Type.Kind: "plainRecord" }], ReturnType.Kind: "void" }] &&
        messageMethods.Single(member => member.Name == "resolveText").Type is { Kind: "method", Signatures:
            [{ Parameters: [{ Type.Kind: "string" }], ReturnType.Kind: "string" }] } &&
        !messageMethods.Any(member => member.Name is "numeric" or "optionalNumber" or "unrelated" or "wrongConstraint"
            || member.Name == "ambiguousText" && member.Type.Kind == "method"),
        "renamed locale message methods preserve the default record domain and reject narrower or unrelated signatures");
    var errors = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["observerError", "lookalikeObserverError"])).ToDictionary(value => value.ExportName);
    Check(errors["observerError"].Signatures is [{ ReturnType: {
            Kind: "nullable", ElementType.Kind: "error" } }] &&
        (!errors.TryGetValue("lookalikeObserverError", out var lookalikeError) ||
            lookalikeError.Signatures is [{ ReturnType: { Kind: "nullable", ElementType.Kind: "object" } }]),
        "only the standard TypeScript Error symbol projects as the JavaScript Error host");
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
    var jobsByName = forwardedJobs.ToDictionary(value => value.ExportName);
    Check(jobsByName.GetValueOrDefault("schedule")?.Signatures is [{
        TypeParameters: ["R"], ReturnType: { Kind: "promise", ElementType.Name: "R" },
        Parameters: [_, { Type.Signatures: [{ ReturnType: { Kind: "promise", ElementType.Name: "R" } }] }]
    }] && jobsByName.GetValueOrDefault("counterOnly")?.Signatures is [{
        TypeParameters: ["R"], ReturnType: { Kind: "promise", ElementType.Kind: "number" },
        Parameters: [{ Type.Signatures: [{ ReturnType: { Kind: "promise", ElementType.Name: "R" } }] }]
    }] && !jobsByName.ContainsKey("constrainedJob"),
        "Promise callback results retain their own slot and a fixed Promise result keeps its declared fulfillment type");
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
        ["wideRef", "narrowRef", "editWideRef", "localeRef", "narrowLocaleRef", "editComputedRef"])).ToDictionary(value => value.ExportName);
    Check(refs["editComputedRef"].Signatures is [{ Parameters: [{ Type: {
        Kind: "ref", Name: "VueWritableComputedRef", ElementType.Kind: "string",
        WriteType: { Kind: "nullable", ElementType.Kind: "string" }, ReadOnly: false,
    } }] }], "writable computed input keeps Vue identity and the exact setter payload");
    Check(refs["wideRef"].Signatures is [{ ReturnType: {
        Kind: "ref", ElementType.Kind: "string", WriteType: { Kind: "nullable", ElementType.Kind: "string" }, ReadOnly: false,
    } }] && refs["editWideRef"].Signatures is [{ Parameters: [{ Type: {
        Kind: "ref", ElementType.Kind: "string", WriteType: { Kind: "nullable", ElementType.Kind: "string" },
    } }] }], "closed asymmetric Vue refs preserve their exact nullable setter for input and output");
    Check(refs["narrowRef"].Signatures is [{ ReturnType: {
        Kind: "ref", ElementType.Kind: "string", ReadOnly: true, WriteType: null,
    } }] && refs["narrowLocaleRef"].Signatures is [{ ReturnType: {
        Kind: "ref", ElementType.Kind: "string", ReadOnly: true, WriteType: null,
    } }], "literal-only setters preserve readonly Ref and ComputedRef outputs");
    var computedProjections = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["keepClosedComputedRef", "keepGenericComputedRef", "keepIndependentComputedRef", "keepNullableComputedRef",
         "receiveLocalComputedRef", "receiveExtendedComputedRef", "receiveNarrowComputedRef", "receiveUndefinedComputedRef"]);
    var computedRefs = computedProjections.Values.ToDictionary(value => value.ExportName);
    Check(computedRefs["keepClosedComputedRef"].Signatures is [{
        Parameters: [{ Type: { Name: "VueWritableComputedRef", ElementType.Kind: "string", ReadOnly: false, WriteType: null } }],
        ReturnType: { Name: "VueWritableComputedRef", ElementType.Kind: "string", ReadOnly: false, WriteType: null },
    }] && computedRefs["keepGenericComputedRef"].Signatures is [{
        TypeParameters: ["T"],
        Parameters: [{ Type: { Name: "VueWritableComputedRef", ElementType.Name: "T", ReadOnly: false, WriteType: null } }],
        ReturnType: { Name: "VueWritableComputedRef", ElementType.Name: "T", ReadOnly: false, WriteType: null },
    }], "writable computed aliases retain their identity for closed and generic payloads");
    Check(computedRefs["keepIndependentComputedRef"].Signatures is [{
        TypeParameters: ["R", "W"],
        Parameters: [{ Type: { Name: "VueWritableComputedRef", ElementType.Name: "R", WriteType.Name: "W", ReadOnly: true } }],
        ReturnType: { Name: "VueWritableComputedRef", ElementType.Name: "R", WriteType.Name: "W", ReadOnly: true },
    }] && computedRefs["keepNullableComputedRef"].Signatures is [{
        TypeParameters: ["T"],
        Parameters: [{ Type: {
            Name: "VueWritableComputedRef", ElementType.Name: "T",
            WriteType: { Kind: "nullableSlot", ElementType.Name: "T" }, ReadOnly: false,
        } }],
        ReturnType: {
            Name: "VueWritableComputedRef", ElementType.Name: "T",
            WriteType: { Kind: "nullableSlot", ElementType.Name: "T" }, ReadOnly: false,
        },
    }], "computed refs keep the same read/write slots for inputs and outputs");
    Check(computedRefs["receiveExtendedComputedRef"].Signatures is [{
        Parameters: [{ Type: { Kind: "handle", Name: "ExtendedComputedRef" } }],
    }] && computedProjections.Errors.ContainsKey("receiveLocalComputedRef") &&
        computedProjections.Errors.ContainsKey("receiveNarrowComputedRef") &&
        computedProjections.Errors.ContainsKey("receiveUndefinedComputedRef"),
        "extra required members keep their named contract and unsupported computed inputs do not widen");
    var optionalRefs = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["optionalNumberRef", "editOptionalNumberRef"]);
    Check(optionalRefs.Errors.ContainsKey("editOptionalNumberRef") &&
        optionalRefs.Values.Single().Signatures is [{ ReturnType: {
            Kind: "ref", Name: "VueOptionalRef", ElementType: { Kind: "nullable", ElementType.Kind: "number" },
            ReadOnly: false, WriteType: null,
        } }], "undefined-capable setters expose a nullable output view and reject input transfer");
    var optionalRefResult = await JoltProjectCompiler.CompileAsync([new("OptionalRef.jazor", """
        @using Tool = Npm.ComponentFixture.Other.Exports
        @code {
            public static bool Run() {
                var present = Tool.OptionalNumberRef(true);
                var absent = Tool.OptionalNumberRef(false);
                double? zero = present.Value;
                double? missing = absent.Value;
                present.Value = null;
                absent.Value = 0;
                return zero == 0 && missing == null && present.Value == null && absent.Value == 0;
            }
        }
        """)], new JoltCompileOptions { NpmValues = optionalRefs.Values });
    Check(optionalRefResult.Diagnostics.Count == 0, "optional numeric Ref allows nullable reads and writes: " +
        string.Join(", ", optionalRefResult.Diagnostics));
    await File.WriteAllTextAsync(Path.Combine(project, "OptionalRef.mjs"), optionalRefResult.Modules.Single().Content);
    var optionalRefRunner = Path.Combine(project, "verify-optional-ref.mjs");
    await File.WriteAllTextAsync(optionalRefRunner, """
        import assert from 'node:assert/strict';
        import { Run } from './OptionalRef.mjs';
        assert.equal(Run(), true);
        """);
    await NodeModuleRunner.RunAsync(project, optionalRefRunner);
    var gauges = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["buildGauge", "buildConflictingGauge", "acceptGauge", "readFastGauge", "readSlowGauge"]);
    var pulseRefs = (await provider.ReadValueAsync(document, "component-fixture/other", "readPulse")).Signatures.Single().ReturnType.Members;
    Check(pulseRefs is [
        { Name: "mode", Type: { Kind: "ref", Name: "VueShallowRef", ReadOnly: false,
            ElementType: { Kind: "nullable", ElementType: { Kind: "stringEnum", Name: "PulseMode", TypeIdentity: var modeIdentity,
                StringLiterals: ["quick", "slow"] } } } },
        { Name: "equivalentMode", Type.ElementType.ElementType.TypeIdentity: var equivalentIdentity },
        { Name: "link", Type: { Kind: "ref", ReadOnly: true,
            ElementType: { Kind: "undefinedable", ElementType: { Kind: "stringEnum", StringLiterals: ["local", "remote"] } } } },
    ] && modeIdentity is not null && modeIdentity == equivalentIdentity,
        "nullable writable and undefinedable readonly string refs preserve their literal domain and named enum identity");
    var closedDomains = gauges.Values.Where(value => value.ExportName is "readFastGauge" or "readSlowGauge")
        .Select(value => value.Signatures.Single().ReturnType.ElementType).ToArray();
    Check(closedDomains.Length == 2 && closedDomains.All(domain => domain is { Kind: "stringEnum" }) &&
        closedDomains.Select(domain => domain!.TypeIdentity ?? domain.Name).Distinct().Count() == 2,
        "Two instantiations of a finite string alias retain their separate writable enum domains.");
    Check(gauges.Values.Single(value => value.ExportName == "buildGauge").Signatures is [{ ReturnType: {
        Kind: "enrichedRef", ReadOnly: false, ElementType: { Kind: "stringEnum", StringLiterals: var modes },
        Members: var gaugeMembers,
    } }] && modes is not null && modes.Order(StringComparer.Ordinal).SequenceEqual(["disabled", "enabled"]) &&
        gaugeMembers is not null &&
        gaugeMembers.Any(member => member is { Name: "status", ReadOnly: true, Type: { Kind: "ref", ReadOnly: true } }) &&
        gaugeMembers.Any(member => member is { Name: "source", ReadOnly: true, Type: { Kind: "ref", ReadOnly: false } }) &&
        gauges.Values.Single(value => value.ExportName == "acceptGauge").Signatures is
            [{ Parameters: [{ Type.Kind: "enrichedRef" }] }] &&
        gauges.Values.Single(value => value.ExportName == "buildConflictingGauge").Signatures is
            [{ ReturnType: { Kind: "ref", ReadOnly: true, WriteType: null, Members: null } }],
        "complete named enriched Ref inputs and outputs preserve literal writes and nested host members, while conflicting value declarations expose only readonly access");
    var writers = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["makeWriter", "makeOptionalWriter", "makeMismatchedWriter"]);
    foreach (var (writerName, optional) in new[] { ("makeWriter", false), ("makeOptionalWriter", true) })
    {
        var methods = writers.Values.Single(value => value.ExportName == writerName).Signatures.Single().ReturnType.Members!
            .Single(member => member.Name == "submit").Type;
        Check(methods is { Kind: "method", Signatures: { } overloads } &&
            overloads.Count == (optional ? 3 : 2) &&
            overloads.All(overload => overload.ReturnType is { Kind: "promise", ElementType.Kind: "void" }) &&
            overloads.Any(overload => overload.Parameters.Count == 0) == optional &&
            overloads.Any(overload => overload.Parameters is [{ Type.Kind: "number", Optional: false }]) &&
            overloads.Any(overload => overload.Parameters is [{ Type.Kind: "function", Optional: false }]),
            "direct and lazy async submit forms preserve the original omission and return contract");
    }
    Check(writers.Values.Where(value => value.ExportName == "makeMismatchedWriter").All(value =>
        value.Signatures.All(signature => signature.ReturnType.Members?.Any(member =>
            member.Name == "submit" && member.Type.Kind == "method") != true)),
        "a supplier resolving a different payload type does not project as a submit overload");
    var catalogs = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["inspectCatalog", "getCatalogEntry"]);
    var catalog = catalogs.Values.Single(value => value.ExportName == "inspectCatalog").Signatures.Single().ReturnType;
    var catalogEntry = catalogs.Values.Single(value => value.ExportName == "getCatalogEntry").Signatures.Single().ReturnType;
    var catalogSelection = catalog.Members!.Single(member => member.Name == "selection").Type;
    Check(catalog.Kind == "handle" && catalogSelection is { Kind: "method", Signatures: [
        { Parameters.Count: 0, ReturnType: { Kind: "nullable", ElementType: { Kind: "handle", Name: "CatalogEntry" } nestedEntry } },
    ] } && catalogEntry is { Kind: "handle", Name: "CatalogEntry" } &&
        nestedEntry.TypeDefinition == catalogEntry.TypeDefinition &&
        catalogEntry.Members!.Any(member => member is { Name: "id", ReadOnly: true, Type.Kind: "string" }) &&
        catalogEntry.Members!.All(member => member.Name != "revision"),
        "a nullable handle getter preserves the returned declaration and exposes readonly scalar host properties");
    Check(catalog.Members!.Single(member => member.Name == "setSelection").Type.Signatures is
        [{ Parameters: [{ Type: { Kind: "handle", Name: "CatalogEntry" } inputEntry }] }] &&
        inputEntry.TypeDefinition == catalogEntry.TypeDefinition,
        "a host method taking the entry preserves its input declaration identity");
    var archive = (await provider.ReadValueAsync(document, "component-fixture/other", "openArchive"))
        .Signatures.Single().ReturnType.Members!.ToDictionary(member => member.Name);
    Check(archive["sweep"].Type.Signatures!.Any(call => call.ReturnType.Kind == "void" &&
        (call.Parameters.Count == 0 || call.Parameters is [{ Optional: true }])) &&
        archive["sweep"].Type.Signatures!.Any(call => call.ReturnType.Kind == "void" && call.Parameters is [{ Type.Kind: "object" }]) &&
        (!archive.TryGetValue("conflict", out var archiveConflict) ||
            archiveConflict.Type.Signatures!.All(call => call.ReturnType.Kind != "void")),
        "generic optional filters retain void calls at valid arities and preserve competing return contracts");
    var archiveReads = archive["peek"].Type.Signatures!;
    var archiveWrites = archive["revise"].Type.Signatures!;
    Check(archiveReads.Any(call => call.Parameters.Count == 1 &&
        call.ReturnType is { Kind: "undefinedable", ElementType.Name: "D" }) &&
        archiveWrites.Select(call => call.Parameters[1].Type.Kind).Order(StringComparer.Ordinal)
            .SequenceEqual(["boolean", "function", "function", "number", "recordTypeParameter", "string"]) &&
        archiveWrites.All(call => call.ReturnType.Kind == "undefinedable" &&
            (call.Parameters[1].OptionalCallback
                ? call.Parameters[1].Type.Signatures is [{ Parameters: [{ Type.Kind: "undefinedable" }], ReturnType.Kind: "undefinedable" }]
                : call.ReturnType.ElementType?.Kind == call.Parameters[1].Type.Kind)) &&
        !archive.ContainsKey("wrongDefault") && !archive.ContainsKey("wrongKey") && !archive.ContainsKey("wrongUpdater"),
        "generic keyed reads and updates preserve undefined output and require the declared key, default and updater contracts");
    Check(archive["resolve"].Type.Signatures is [{ ReturnType: { Kind: "promise", ElementType.Name: var archiveResultSlot },
        Parameters: [{ Type.Members: [{ Name: "coordinates" }, { Name: "loader", Type.Signatures: [
            { ReturnType: { Kind: "promise", ElementType.Name: var archiveCallbackSlot } },
        ] }] }] }] && archiveResultSlot == archiveCallbackSlot && !archive.ContainsKey("wrongPromise") &&
        (!archive.TryGetValue("competingLoad", out var competingLoad) ||
            competingLoad.Type.Signatures!.All(call => call.ReturnType.Kind != "promise")),
        "generic keyed fetching shares its Promise payload with the callback and excludes nonstandard thenables");
    var lookup = (await provider.ReadValueAsync(document, "component-fixture/other", "makeLookup"))
        .Signatures.Single().ReturnType.Members!;
    var contains = lookup.Single(member => member.Name == "contains").Type.Signatures!;
    var lookupCompetitors = lookup.Single(member => member.Name == "competing").Type.Signatures!;
    Check(contains.Count == 2 && contains.All(call => call.ReturnType.Kind == "boolean" &&
        call.Parameters.All(parameter => parameter.Type.Kind == "string")) &&
        contains.Select(call => call.Parameters.Count).Order().SequenceEqual([1, 2]) &&
        lookup.All(member => member.Name != "restricted") &&
        lookupCompetitors.All(call => call.Parameters.Count != 1 || call.ReturnType.Kind != "boolean"),
        "defaulted schema slots preserve open string inputs and omitted arguments respect competing overload results");
    var instruments = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["makeInstrument", "passInstrument", "passTextInstrument", "passIncompleteInstrument", "passVersionedInstrument"]);
    var instrument = instruments.Values.Single(value => value.ExportName == "makeInstrument").Signatures.Single().ReturnType;
    var numberInstrument = instruments.Values.Single(value => value.ExportName == "passInstrument").Signatures.Single();
    var textInstrument = instruments.Values.Single(value => value.ExportName == "passTextInstrument").Signatures.Single();
    Check(instrument is { Kind: "enrichedRef", ElementType.Name: "T", TypeIdentity: not null,
            TypeArguments: [{ Kind: "number" }], Members: var instrumentMembers } &&
        instrumentMembers is not null &&
        instrumentMembers.Any(member => member is { Name: "label", ReadOnly: true, Type.Kind: "string" }) &&
        instrumentMembers.Any(member => member is { Name: "read", Type: { Kind: "method", Signatures: [{ Parameters.Count: 0, ReturnType.Name: "T" }] } }) &&
        instrumentMembers.Any(member => member is { Name: "replace", Type: { Kind: "method", Signatures: [{ Parameters: [{ Type.Name: "T" }], ReturnType.Kind: "void" }] } }) &&
        numberInstrument.Parameters.Single().Type.TypeIdentity == instrument.TypeIdentity &&
        numberInstrument.ReturnType.TypeIdentity == instrument.TypeIdentity &&
        textInstrument.Parameters.Single().Type is { TypeArguments: [{ Kind: "string" }] } &&
        textInstrument.ReturnType is { TypeArguments: [{ Kind: "string" }] } &&
        instruments.Values.Where(value => value.ExportName == "passIncompleteInstrument")
            .All(value => value.Signatures.All(signature => signature.Parameters.Single().Type.Kind == "handle")),
        "named augmented Ref inputs and outputs keep their closed member types and reject incomplete input contracts");
    var versionedInstrument = instruments.Values.Single(value => value.ExportName == "passVersionedInstrument").Signatures.Single();
    Check(versionedInstrument.Parameters[0].Type.TypeIdentity is { } firstInstrument &&
        versionedInstrument.Parameters[1].Type.TypeIdentity is { } secondInstrument &&
        firstInstrument.Contains("\"versioned-data\",\"1.0.0\"", StringComparison.Ordinal) &&
        secondInstrument.Contains("\"versioned-data\",\"2.0.0\"", StringComparison.Ordinal) &&
        versionedInstrument.ReturnType.TypeIdentity == secondInstrument,
        "same-named augmented refs keep the identity of each declaring package version");
    var genericRefs = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["transferRef", "keepGenericRef", "receiveComputed", "receiveExtendedRef", "receiveAsymmetricRef", "receiveShadowRef",
            "passIndependentRef", "receiveNarrowRef"]))
        .ToDictionary(value => value.ExportName);
    var depth = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["buildDeepPayload", "buildShallowPayload", "buildSymmetricPayload", "buildWiderPayload", "buildImitationPayload", "buildReactivePayload"]);
    Check(depth.Single(value => value.ExportName == "buildDeepPayload").Signatures is [{
        ReturnType: { Kind: "ref", ReadOnly: false, WriteType: { Kind: "object", Name: "VueProjectionPayload", Members: [
            { Name: "title", Type: { Kind: "ref", ElementType.Kind: "string" } },
            { Name: "nested", Type.Members: [{ Name: "count", Type: { Kind: "ref", ElementType.Kind: "number" } }] },
            { Name: "items", Type: { Kind: "array", ElementType: { Kind: "ref", ElementType.Kind: "number" } } },
        ] }, ElementType.Members: [
            { Name: "title", Type.Kind: "string" },
            { Name: "nested", Type.Members: [{ Name: "count", Type.Kind: "number" }] },
            { Name: "items", Type: { Kind: "array", ElementType: { Kind: "ref", ElementType.Kind: "number" } } },
        ] },
    }], "deep Vue records unwrap property reads and preserve raw record writes");
    Check(depth.Single(value => value.ExportName == "buildShallowPayload").Signatures is [{
        ReturnType: { Kind: "ref", Name: "VueShallowRef", ReadOnly: false, ElementType.Members: [
            { Name: "title", Type: { Kind: "ref", ElementType.Kind: "string" } },
            { Name: "nested", Type.Members: [{ Name: "count", Type: { Kind: "ref", ElementType.Kind: "number" } }] },
            { Name: "items", Type: { Kind: "array", ElementType: { Kind: "ref", ElementType.Kind: "number" } } },
        ] },
    }], "shallow Vue records preserve their writable brand and nested ref values");
    Check(depth.Single(value => value.ExportName == "buildSymmetricPayload").Signatures is [{
        ReturnType: { Kind: "ref", ReadOnly: false, WriteType: null, ElementType.Members: [{ Name: "title", Type.Kind: "string" }, ..] },
    }] && depth.Single(value => value.ExportName == "buildWiderPayload").Signatures is [{
        ReturnType: { Kind: "ref", ReadOnly: false, WriteType: null, ElementType.Members: [{ Name: "title", Type.Kind: "string" }, ..] },
    }] && depth.Single(value => value.ExportName == "buildImitationPayload").Signatures is [{
        ReturnType: { Kind: "ref", ReadOnly: true, ElementType.Members: [{ Name: "title", Type.Kind: "ref" }, ..] },
    }], "Vue record unwrapping is identified by its declaration and supports a symmetric setter");
    Check(depth.Single(value => value.ExportName == "buildReactivePayload").Signatures is [{
        ReturnType: { Kind: "object", Name: "VueProjectionPayload", Members: [
            { Name: "title", Type: { Kind: "ref", ElementType.Kind: "string" } },
            { Name: "nested", Type.Members: [{ Name: "count", Type: { Kind: "ref", ElementType.Kind: "number" } }] },
            { Name: "items", Type: { Kind: "array", ElementType: { Kind: "ref", ElementType.Kind: "number" } } },
        ] },
    }], "shallow reactive Vue records preserve inner refs and hide the private brand");
    Check(genericRefs["transferRef"].Signatures is [{ TypeParameters: ["S", "R"], Parameters: [
        { Type: { Kind: "ref", ReadOnly: true, Name: null, ElementType: { Kind: "typeParameter", Name: "S" } } },
        { Type.Signatures: [{ Parameters: [{ Type.Name: "S" }], ReturnType: { Name: "VueComputedRef", ElementType.Name: "R" } }] },
    ], ReturnType: { Kind: "ref", Name: "VueComputedRef", ReadOnly: true, ElementType.Name: "R" } }] &&
        genericRefs["receiveComputed"].Signatures is [{ TypeParameters: ["T"], Parameters: [
            { Type: { Kind: "ref", ReadOnly: true, Name: "VueComputedRef", ElementType.Name: "T" } }],
            ReturnType: { Kind: "typeParameter", Name: "T" } }],
        "generic Vue aliases preserve computed identity in arguments, callbacks, and results");
    Check(genericRefs["keepGenericRef"].Signatures is [{ TypeParameters: ["T"], Parameters: [
        { Type: { Kind: "ref", ReadOnly: false, ElementType.Name: "T" } }],
        ReturnType: { Kind: "ref", ReadOnly: false, ElementType.Name: "T" } }] &&
        !genericRefs.ContainsKey("receiveNarrowRef") &&
        genericRefs.GetValueOrDefault("receiveShadowRef")?.Signatures is [{ TypeParameters: ["T"], Parameters: [
            { Type: { Kind: "namedRecord", Name: "ShadowRef", TypeArguments: [{ Kind: "typeParameter", Name: "T" }],
                Members: [{ Name: "value", ReadOnly: true }] } }], ReturnType: { Kind: "typeParameter", Name: "T" } }],
        "generic writable Vue refs preserve exact writes and require the authentic ref contract");
    Check(genericRefs["receiveExtendedRef"].Signatures is [{
        TypeParameters: ["T"], Parameters: [{ Type: { Kind: "enrichedRef", TypeArguments: [{ Name: "T" }],
            Members: [{ Name: "reset", Type.Kind: "method" }] } }], ReturnType.Name: "T",
    }], "generic augmented Ref inputs retain required methods and the inferred payload type");
    Check(genericRefs["receiveAsymmetricRef"].Signatures is [{
        TypeParameters: ["T"], Parameters: [{ Type: {
            Kind: "ref", ElementType: { Kind: "typeParameter", Name: "T" },
            WriteType: { Kind: "nullableSlot", ElementType: { Kind: "typeParameter", Name: "T" } },
        } }], ReturnType: { Kind: "typeParameter", Name: "T" },
    }], "a generic nullable setter retains the same type parameter as the getter");
    Check(genericRefs["passIndependentRef"].Signatures is [{
        TypeParameters: ["TRead", "TWrite"], Parameters: [{ Type: {
            Kind: "ref", ElementType: { Kind: "typeParameter", Name: "TRead" },
            WriteType: { Kind: "typeParameter", Name: "TWrite" },
        } }], ReturnType: {
            Kind: "ref", ElementType: { Kind: "typeParameter", Name: "TRead" },
            WriteType: { Kind: "typeParameter", Name: "TWrite" }, ReadOnly: true,
        },
    }], "independent generic setters and getters retain their own slots");
    var interior = await provider.ReadValueAsync(document, "component-fixture/other", "interiorOption");
    Check(interior.Signatures.Count(signature => signature.Parameters is [_, { Optional: true }, { Optional: true }]) == 1 &&
        interior.Signatures.Count(signature => signature.Parameters.Count == 3) == 2,
        "only one overlapping overload accepts a target-typed named options argument");
    var competing = await provider.ReadValueAsync(document, "component-fixture/other", "competingOption");
    Check(competing.Signatures.Any(signature => signature.Parameters.Count == 3 && signature.ReturnType.Kind == "string") &&
        competing.Signatures.All(signature => signature.Parameters.All(parameter => !parameter.Optional)),
        "an earlier overload accepting an undefined slot can suppress named omission");
    var callbackResult = await provider.ReadValueAsync(document, "component-fixture/other", "callbackResult");
    var selection = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["selectMode", "selectTwice", "selectOptional", "selectAmbiguous", "selectOptionalTail", "selectCorrelated", "selectCollision", "selectCollisionWithEnabledTrue"]);
    Check(selection.Single(value => value.ExportName == "selectMode").Signatures is [
        { ManagedNameSuffix: "WithEnabledFalse", Parameters: [{ Type.Kind: "string", FixedBoolean: null },
            { FixedBoolean: false }, { Type.Kind: "number", FixedBoolean: null }], ReturnType.Kind: "number" },
        { ManagedNameSuffix: "WithEnabledTrue", Parameters: [{ FixedBoolean: null },
            { FixedBoolean: true }, { FixedBoolean: null }], ReturnType.Members: [{ Name: "path", Type.Kind: "string" }] },
    ] && selection.Where(value => value.ExportName == "selectTwice")
        .All(value => value.Signatures.All(signature => signature.ManagedNameSuffix is null)),
        "literal boolean calls preserve argument position and expose each exact conditional result");
    Check(selection.Single(value => value.ExportName == "selectOptional").Signatures is [
        { ManagedNameSuffix: "WithEnabledFalse", Parameters: [{ FixedBoolean: false }], ReturnType.Kind: "number" },
        { ManagedNameSuffix: "WithEnabledTrue", Parameters: [{ FixedBoolean: true }], ReturnType.Members: [{ Name: "path" }] },
        { ManagedNameSuffix: null, Parameters: [], ReturnType.Kind: "number" },
    ] && selection.Single(value => value.ExportName == "selectAmbiguous").Signatures is [
        { ManagedNameSuffix: "WithEnabledFalse" }, { ManagedNameSuffix: "WithEnabledTrue" },
    ] && selection.Single(value => value.ExportName == "selectOptionalTail").Signatures is [
        { Parameters: [{ Optional: false }, { FixedBoolean: false }, { Optional: true }] },
        { Parameters: [{ Optional: false }, { FixedBoolean: true }, { Optional: true }] },
        { ManagedNameSuffix: null, Parameters: [{ Name: "label" }], ReturnType.Members: [{ Name: "path" }] },
    ], "optional boolean calls expose exact literal results and the projectable result of omitting the parameter: " +
        System.Text.Json.JsonSerializer.Serialize(selection.Where(value => value.ExportName is "selectOptional" or "selectAmbiguous" or "selectOptionalTail")));
    Check(selection.All(value => value.ExportName != "selectCorrelated"),
        "a retained input that can also infer the flag prevents literal and omission specialization");
    Check(selection.Single(value => value.ExportName == "selectCollision").Signatures is
        [{ ManagedNameSuffix: "WithEnabledFalse" }] &&
        selection.Single(value => value.ExportName == "selectCollisionWithEnabledTrue").Signatures is
        [{ ManagedNameSuffix: null, ReturnType.Kind: "number" }],
        "a declared export takes precedence over a synthesized literal entry point");
    await CheckUnsupported("component-fixture/other", "choose", "no callable form has an available C# name");
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
    var opaqueRefs = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["readOpaque", "readUnspecified", "startDeferred"]);
    Check(opaqueRefs.Errors.ContainsKey("readUnspecified") &&
        opaqueRefs.Values.Single(value => value.ExportName == "readOpaque").Signatures is [
            { ReturnType: { Kind: "ref", ReadOnly: true, ElementType: { Kind: "undefinedable", ElementType.Kind: "unknownValue" } } }] &&
        opaqueRefs.Values.Single(value => value.ExportName == "startDeferred").Signatures is [
            { ScalarResultTypeParameter: "T", ReturnType.Members: [
                { Name: "current", Type.ElementType.Kind: "typeParameter" },
                { Name: "rejected", ReadOnly: true, Type: { Kind: "ref", ReadOnly: true, ElementType: { Kind: "undefinedable", ElementType.Kind: "unknownValue" } } }] }],
        "explicit unknown ref results expose the same read-only optional view through ordinary and correlated signatures");
    Check(optionalNumbers.Signatures is [{ ReturnType.Members: [
        { Name: "absent", Optional: true, ReadOnly: true, Type.Kind: "number" },
        { Name: "nullable", Optional: true, ReadOnly: false, Type: { Kind: "nullable", ElementType.Kind: "number" } }] }],
        "an optional numeric result can accept null only if null is in the write domain");
    var genericOptional = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["optionalRecord", "optionalMapped", "optionalNullable", "requiredNullable", "optionalUndefined"]);
    Check(genericOptional.Errors.Keys.Order(StringComparer.Ordinal).SequenceEqual(["optionalUndefined", "requiredNullable"]) &&
        genericOptional.Values.Count == 3,
        "generic optional results project and unsupported union domains report projection errors: " +
        string.Join(", ", genericOptional.Errors));
    var genericOptionalOptions = new JoltCompileOptions { NpmValues = genericOptional.Values };
    const string genericOptionalCaller = """
        @using Tool = Npm.ComponentFixture.Other.Exports
        @code {
            public static bool Run() {
                var named = Tool.OptionalRecord<double>(1);
                var mapped = Tool.OptionalMapped<bool>(true);
                bool namedPresent = named.HasValue_2;
                bool mappedPresent = mapped.HasValue;
                string wire = named.HasValue;
                named.Value = 0;
                mapped.Value = false;
                double number = named.Value;
                bool flag = mapped.Value;
                var nullable = Tool.OptionalMapped<double?>(1);
                nullable.Value = null;
                double? nullableNumber = nullable.Value;
                var nullableBranch = Tool.OptionalNullable<double>(1);
                bool nullableBranchPresent = nullableBranch.HasValue;
                nullableBranch.Value = 0;
                double unwrapped = nullableBranch.Value!.Value;
                nullableBranch.Value = null;
                return !namedPresent && !mappedPresent && wire == "wire" &&
                    named.HasValue_2 && mapped.HasValue && nullable.HasValue &&
                    number == 0 && !flag && nullableNumber == null && !nullableBranchPresent &&
                    unwrapped == 0 && nullableBranch.HasValue && nullableBranch.Value is null;
            }
        }
        """;
    var projectedOptional = await JoltProjectCompiler.CompileAsync([new("Optional.jazor", genericOptionalCaller)], genericOptionalOptions);
    Check(projectedOptional.Diagnostics.Count == 0,
        "generic optional result members retain their instantiated C# types: " + string.Join(", ", projectedOptional.Diagnostics));
    var optionalModule = projectedOptional.Modules.Single().Content;
    Check(optionalModule.Contains("named.hasValue", StringComparison.Ordinal) &&
        optionalModule.Contains("named.value = 0", StringComparison.Ordinal) &&
        optionalModule.Contains("mapped.value = false", StringComparison.Ordinal) &&
        optionalModule.Contains("nullable.value = null", StringComparison.Ordinal) &&
        !optionalModule.Contains("?? null", StringComparison.Ordinal) &&
        !optionalModule.Contains("hasValue_2", StringComparison.Ordinal),
        "generic optional access uses the upstream wire properties and raw instantiated values: " + optionalModule);
    var invalidOptional = await JoltProjectCompiler.CompileAsync(
        [new("Optional.jazor", genericOptionalCaller.Replace("named.Value = 0", "named.Value = null", StringComparison.Ordinal))],
        genericOptionalOptions);
    Check(invalidOptional.Diagnostics.Any(item => item.Id == "CS0037"),
        "a generic optional numeric result rejects null writes");
    await File.WriteAllTextAsync(Path.Combine(project, "Optional.mjs"), optionalModule);
    var optionalRunner = Path.Combine(project, "verify-optional.mjs");
    await File.WriteAllTextAsync(optionalRunner, """
        import assert from 'node:assert/strict';
        import { Run } from './Optional.mjs';
        assert.equal(Run(), true);
        """);
    await NodeModuleRunner.RunAsync(project, optionalRunner);
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
    var optionalValidators = validators["assessOptional"].Signatures;
    Check(optionalValidators.Any(signature => signature.TypeParameters is null &&
            signature.Parameters.Count >= 2 && signature.Parameters[1].Type.Kind == "string") &&
        optionalValidators.Any(signature => signature is { TypeParameters: ["TValue"], Parameters.Count: 1 } &&
            signature.ReturnType.Members!.Any(member => member is { Name: "current", Type.ElementType.Kind: "nullableSlot" }) &&
            signature.ReturnType.Members!.Any(member => member is { Name: "label", Type.Kind: "union" })) &&
        optionalValidators.Any(signature => signature.Parameters is [_, { OptionalCallback: true }] &&
            signature.Parameters[1].Type.Signatures!.Single().Parameters[0].Type.Kind == "nullableSlot") &&
        optionalValidators.Any(signature => signature.ManagedNameSuffix == "WithOptionalValue" &&
            signature.Parameters is [_, { OptionalCallback: true }, { Type.Kind: "object" }]) &&
        optionalValidators.Any(signature => signature.Parameters is [_, _, { Type.Kind: "object" }] &&
            signature.ReturnType.Members!.Any(member => member is { Name: "current", Type.ElementType.Kind: "typeParameter" })),
        "the renamed validator preserves the string-or-ref label and distinguishes missing-value and initialized calls: " +
        string.Join(" | ", optionalValidators.Select(signature => $"{signature.ManagedNameSuffix}<{string.Join(",", signature.TypeParameters ?? [])}> " +
            $"({string.Join(",", signature.Parameters.Select(p => $"{p.Type.Kind}/{p.OptionalCallback}"))}) " +
            string.Join(",", signature.ReturnType.Members!.Select(member => $"{member.Name}:{member.Type.Kind}/{member.Type.ElementType?.Kind}")))));

    static NpmValueType GetResult(NpmValueSignature signature) =>
        signature.Parameters[1].Type.Signatures!.Single().ReturnType;

    var draftExports = await provider.ReadValuesAsync(document, "component-fixture/other", ["assembleDraft", "mismatchedDraft", "restrictedDraft"]);
    Check(draftExports is [{ ExportName: "assembleDraft", Signatures: [var draft] }] &&
        draft.Parameters is [{ Type.Members: [
            { Name: "startingPoint", Type.Kind: "recordTypeParameter" },
            { Name: "adapter", Type.Members: [
                { Name: "marker", Type.StringLiterals: ["transformed-draft"] },
                { Name: "decode", Type.Signatures: [{ Parameters: [{ Type: { Kind: "partialRecord", ElementType.Name: "TInput" } }], ReturnType.ElementType.Members: [
                    { Name: "entity", Type.Kind: "recordTypeParameter", Optional: false },
                    { Name: "issues", Type.ElementType.Members: [{ Name: "trail" }, { Name: "notes" }] }] }] }] }] }] &&
        draft.ReturnType.Members is [
            { Name: "snapshot", Type: { Kind: "partialRecord", ElementType.Name: "TInput" } },
            { Name: "notices", ReadOnly: true, Type: { Kind: "ref", Name: "VueComputedRef", ReadOnly: true,
                ElementType: { Kind: "readonlyDictionary", ElementType.Kind: "string" } } },
            { Name: "groupedNotices", Type: { Kind: "ref", ReadOnly: true,
                ElementType: { Kind: "readonlyDictionary", ElementType: { Kind: "array", ElementType.Kind: "string" } } } },
            { Name: "commit", Type.Kind: "method", Type.Signatures: [{ Parameters: [{ Name: "accept" }] }, { Parameters: [
                { Name: "accept" }, { Name: "reject", Type.Signatures: [{ Parameters: [
                    { Type.Members: [{ Name: "draft", Type.Kind: "partialRecord" }, { Name: "problems", Type.Kind: "readonlyDictionary" }] }] }] }] }] },
            { Name: "replace", Type.Kind: "method" },
            { Name: "revise", Type.Signatures: [{
                TypeParameters: [var fieldType],
                Parameters: [
                    { Type: { Kind: "propertyPath", VeeValidatePath: false, ElementType: { Kind: "recordTypeParameter", Name: "TInput" },
                        Variants: [{ Kind: "typeParameter", Name: var selectorType }] } },
                    { Type: { Kind: "typeParameter", Name: var valueType } },
                    { Optional: true, Type.Kind: "boolean" }] }] },
            { Name: "attach", Type: { Kind: "method", Signatures: [{
                TypeParameters: [var attachedField],
                Parameters: [{ Type: { Kind: "propertyPath", VeeValidatePath: false, ArrayIndices: false,
                    ElementType: { Kind: "recordTypeParameter", Name: "TInput" },
                    Variants: [{ Kind: "typeParameter", Name: var attachedPath }] } }],
                ReturnType: { Kind: "tupleView", Members: [
                    { Name: "Item1", ReadOnly: true, Type: { Kind: "ref",
                        ElementType: { Kind: "nullableSlot", ElementType.Name: var modelRead },
                        WriteType: { Kind: "typeParameter", Name: var modelWrite } } },
                    { Name: "Item2", ReadOnly: true, Type: { Kind: "ref", ReadOnly: true, WriteType: null, ElementType.Members: var callbacks } }
                ] }
            }] } },
            { Name: "attachConfigured" },
            { Name: "attachProps" },
            { Name: "attachWrongState" },
            { Name: "attachWrongProps" },
            { Name: "attachRestrictedSetup" }
        ] &&
        attachedField == attachedPath && attachedPath == modelRead && modelRead == modelWrite &&
        callbacks is { Count: 3 } &&
        callbacks.Select(member => member.Name).Order(StringComparer.Ordinal).SequenceEqual(["onBlur", "onChange", "onInput"]) &&
        callbacks.All(member => member.Type.Signatures is [{ Parameters: [], ReturnType.Kind: "void" }]) &&
        fieldType == selectorType && selectorType == valueType,
        "renamed form members preserve computed errors and tie nested selectors to replacements and control refs. " +
        $"Members: {string.Join(", ", draftExports.SelectMany(value => value.Signatures)
            .SelectMany(signature => signature.ReturnType.Members ?? []).Select(member => member.Name))}");
    Check(draftExports.Single(value => value.ExportName == "assembleDraft").Signatures.SelectMany(signature => signature.ReturnType.Members ?? [])
        .Any(member => member is { Name: "attachConfigured", Type.Signatures: [
            { Parameters: [{ Type.Kind: "propertyPath" }] },
            { Parameters: [{ Type.Kind: "propertyPath" }, { Optional: false, Type.Members: [
                { Name: "caption", Optional: true, Type.Kind: "string" },
                { Name: "check", Optional: true, Type.Kind: "boolean" }] }] }
        ] }), "renamed field binding accepts optional static string and boolean options");
    var draftFields = draftExports.Single(value => value.ExportName == "assembleDraft").Signatures.Single().ReturnType.Members!;
    Check(draftFields.SingleOrDefault(member => member.Name == "attachProps")?.Type.Signatures is [_, _, {
        TypeParameters: ["TField", "TProps"],
        Parameters: [{ Type.Kind: "propertyPath" }, { Type.Members: [
            { Name: "check", Optional: true, Type.Kind: "boolean" },
            { Name: "enrich", Optional: false, Type.Signatures: [{
                Parameters: [{ Name: "snapshot", Type.Members: [
                    { Name: "current", ReadOnly: true, Optional: true,
                        Type: { Kind: "nullableSlot", ElementType.Name: "TField" } },
                    { Name: "changed", ReadOnly: true, Type.Kind: "boolean" },
                    { Name: "notes", ReadOnly: true, Type: { Kind: "array", ReadOnly: true, ElementType.Kind: "string" } }
                ] }],
                ReturnType: { Kind: "recordTypeParameter", Name: "TProps" }
            }] }
        ] }],
        ReturnType.Members: [_, { Type: { Kind: "ref", ReadOnly: true,
            ElementType: { Kind: "recordTypeParameter", Name: "TProps" } } }]
    }, _, _] && new[] { "attachWrongState", "attachWrongProps" }.All(name =>
        draftFields.SingleOrDefault(member => member.Name == name)?.Type.Signatures?.Take(2).ToArray() is [
            { Parameters: [{ Type.Kind: "propertyPath" }] },
            { Parameters: [{ Type.Kind: "propertyPath" }, { Type.Members: [{ Name: "check", Type.Kind: "boolean" }] }] }]),
        "renamed field props callbacks preserve the selected field state and correlate the result with the readonly ref only for matching slots");
    Check(draftFields.Single(member => member.Name == "attachProps").Type.Signatures is [_, _, _, {
        TypeParameters: ["TField"], OverloadPriority: -1, Parameters: [_, { Type.Signatures: [{
            Parameters: [{ Type.Members: [{ Name: "current", Type.ElementType.Name: "TField" }, { Name: "changed" }] }],
            ReturnType.Members: [{ Name: "refresh", Type.Kind: "boolean", Optional: true, ReadOnly: false }]
        }] }], ReturnType.Members: [_, { Type: { Kind: "ref", ReadOnly: true, ElementType.Members: { Count: 3 } } }]
    }, {
        TypeParameters: ["TField", "TProps"], OverloadPriority: -1, Parameters: [_, { Type.Signatures: [{
            ReturnType.Members: [
                { Name: "refresh", Type.Kind: "boolean", Optional: true },
                { Name: "attributes", Type: { Kind: "recordTypeParameter", Name: "TProps" }, Optional: false }
            ]
        }] }], ReturnType.Members: [_, { Type: { Kind: "ref", ReadOnly: true, ElementType.Name: "TProps" } }]
    }] && draftFields.Single(member => member.Name == "attachWrongState").Type.Signatures is [_, _] &&
        draftFields.Single(member => member.Name == "attachWrongProps").Type.Signatures is [_, _, {
            TypeParameters: ["TField"], Parameters: [_, { Type.Signatures: [{
                ReturnType.Members: [{ Name: "refresh", Type.Kind: "boolean" }]
            }] }]
        }] && draftFields.Single(member => member.Name == "attachRestrictedSetup").Type.Signatures is [
            { Parameters: [{ Type.Kind: "propertyPath" }] }
        ], "renamed lazy config ties its state to the selector and supplies typed props only when the return object can be constructed");
    var draftBindings = NpmBindings.GenerateSource([], draftExports);
    Check(draftBindings.Contains("[global::ECMAScript.ECMAScriptPropertyPath] global::System.Func<@TInput, ", StringComparison.Ordinal) &&
        draftBindings.Contains("@Revise<", StringComparison.Ordinal),
        "the standalone C# facade marks the selector parameter and retains its type relationship");
    var indexedHandles = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["columnHandle", "observedColumn", "mixedHandle", "reorderedHandle", "requiredHandleTail", "changedHandleIndex", "specializedHandle"]);
    Check(indexedHandles.Values.Single(value => value.ExportName == "columnHandle").Signatures is [{
        TypeParameters: ["TRecord", "TValue"],
        Parameters: [{ Type.Name: "TRecord", Optional: false },
            { Type: { Kind: "propertyKey", Variants: [{ Name: "TValue" }] }, Optional: false }],
        ReturnType: { Kind: "ref", Name: "VueWritableComputedRef", ElementType.Name: "TValue", ReadOnly: false },
    }] && indexedHandles.Values.Single(value => value.ExportName == "observedColumn").Signatures is [
        { ReturnType: { Name: "VueComputedRef", ReadOnly: true, ElementType.Name: "TValue" } },
    ] && indexedHandles.Values.Single(value => value.ExportName == "mixedHandle").Signatures is [
        { Parameters.Count: 2, ReturnType: { Kind: "ref", ElementType.Name: "TValue" } },
        { Parameters.Count: 3, ReturnType.Kind: "number" },
    ] && indexedHandles.Errors.Keys.Order(StringComparer.Ordinal).SequenceEqual(
        ["changedHandleIndex", "reorderedHandle", "requiredHandleTail", "specializedHandle"]),
        "indexed Vue results require the exact selected field and preserve two-argument overload order");
    var explicitCells = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["connectCell", "spreadCell", "wrongCell", "narrowNotice", "wrongRest", "wrongResult", "shadowedCell", "functionShadowedCell"]))
        .ToDictionary(value => value.ExportName);
    Check(explicitCells["connectCell"].Signatures is [{
        Parameters: [_, { Type.Kind: "propertyKey" }, { Name: "announce", RestCallback: false, Type.Signatures: [{
            Parameters: [{ Type.Kind: "string" }, { Type: { Kind: "typeParameter", Name: "TValue" } }],
            ReturnType.Kind: "void",
        }] }],
        ReturnType: { Name: "VueWritableComputedRef", ElementType.Name: "TValue" },
    }], "required renamed callbacks use the exact selected property domain");
    Check(explicitCells["spreadCell"].Signatures is [{ Parameters.Count: 2 }, {
        Parameters: [_, { Type.Kind: "propertyKey" }, { Name: "announce", RestCallback: true, Type.Signatures: [{
            Parameters: [{ Type.Kind: "string" }, { Type: {
                Kind: "recordArray", ElementType: { Kind: "undefinedable", ElementType.Kind: "unknownValue" },
            } }],
            ReturnType.Kind: "void",
        }] }],
    }], "rest callbacks preserve omission and represent unknown JavaScript arguments");
    Check(new[] { "wrongCell", "narrowNotice", "wrongRest", "wrongResult", "shadowedCell", "functionShadowedCell" }
        .All(name => !explicitCells.TryGetValue(name, out var value) ||
            value.Signatures.All(signature => signature.Parameters.Count == 2)),
        "incompatible callbacks and shadowed return types do not project explicit callbacks");
    var columnBehaviors = await provider.ReadValueProjectionsAsync(document, "component-fixture/other",
        ["trackColumn", "wrongBehaviorSlot", "requiredBehaviorHole", "changedBehaviorReturn", "competingBehavior"]);
    var tracked = columnBehaviors.Values.Single(value => value.ExportName == "trackColumn").Signatures;
    Check(tracked.Single(signature => signature.Parameters.Count == 2).ReturnType is
        { Kind: "ref", Name: null, ElementType.Name: "TValue" },
        "omitting behavior preserves declaration order even when the true overload comes first");
    foreach (var (fixedValue, refName) in new[] { (false, "VueWritableComputedRef"), (true, (string?)null) })
    {
        Check(tracked.Any(signature => signature.Parameters is [
            { Type.Name: "TRecord" }, { Type.Kind: "propertyKey" }, { FixedUndefined: true },
            { Type.Members: var members },
        ] && members is not null &&
            members.Any(member => member is { Name: "deferred", FixedBoolean: var flag } && flag == fixedValue) &&
            members.Any(member => member is { Name: "fallback", Type: { Kind: "optionalSlot", ElementType.Name: "TValue" } }) &&
            members.Any(member => member is { Name: "accept", Type.Signatures: [
                { Parameters: [{ Type.Name: "TValue" }], ReturnType.Kind: "boolean" },
            ] }) &&
            signature.ReturnType is { Kind: "ref", Name: var name, ElementType.Name: "TValue" } && name == refName),
            $"fixed deferred={fixedValue} keeps the omitted argument slot and ties options and result to the selected value");
    }
    Check(columnBehaviors.Values.Where(value => value.ExportName != "trackColumn")
        .All(value => value.Signatures.All(signature => signature.Parameters.All(parameter => !parameter.FixedUndefined))),
        "mismatched indexed slots, required holes, changed results, and competing earlier overloads reject fixed behavior");
    var proxyColumns = await provider.ReadValuesAsync(document, "component-fixture/other", ["watchColumn", "watchLookalike"]);
    var views = proxyColumns.Single(value => value.ExportName == "watchColumn").Signatures;
    Check(views.Any(signature => signature is { ManagedNameSuffix: "WithObserveTrueRecordView",
        ReturnType: { Kind: "ref", ReadOnly: true, ElementType: { Kind: "unwrappedRecord", ElementType.Name: "TValue" } },
        Parameters: [_, { Type.RequireRecordView: true }, { Type.Members: var settings }] } &&
            settings!.Any(member => member is { Name: "subtree", Type.Kind: "boolean" })) &&
        views.Any(signature => signature.Parameters.Any(parameter => parameter.Type.RequirePlainRecord)) &&
        proxyColumns.Where(value => value.ExportName == "watchLookalike").SelectMany(value => value.Signatures)
            .All(signature => signature.Parameters.All(parameter => !parameter.Type.RequireRecordView && !parameter.Type.RequirePlainRecord)),
        "the Vue UnwrapRef of the selected slot yields a stable DTO and a record view, retaining independent options");
    var indexedAccess = (await provider.ReadValuesAsync(document, "component-fixture/other",
        ["readColumn", "putCell", "readRequired", "readTextKeys", "readMismatched", "putUnrelated", "readExtra", "readNested"]))
        .ToDictionary(value => value.ExportName);
    Check(indexedAccess.Count == 2 && indexedAccess.TryGetValue("readColumn", out var reader) &&
        reader.Signatures is [{ TypeParameters: ["TRecord", "TValue"], Parameters: [
            { Name: "entry", Type: { Kind: "typeParameter", Name: "TRecord" } },
            { Name: "column", Type: { Kind: "propertyKey", ElementType: { Kind: "typeParameter", Name: "TRecord" },
                Variants: [{ Kind: "typeParameter", Name: "TValue" }] } }], ReturnType: { Kind: "typeParameter", Name: "TValue" } }] &&
        indexedAccess.TryGetValue("putCell", out var writer) && writer.Signatures is [{ TypeParameters: ["TRecord", "TValue"], Parameters: [
            { Name: "row", Type: { Kind: "typeParameter", Name: "TRecord" } },
            { Name: "index", Type: { Kind: "propertyKey", ElementType: { Kind: "typeParameter", Name: "TRecord" },
                Variants: [{ Kind: "typeParameter", Name: "TValue" }] } },
            { Name: "cell", Type: { Kind: "typeParameter", Name: "TValue" } }], ReturnType.Kind: "void" }],
        $"renamed readers and writers preserve the same object and key slot, while restricted keys and unrelated slots stay unsupported ({string.Join(", ", indexedAccess.Keys)})");
    var draftContext = await provider.ReadValuesAsync(document, "component-fixture/other",
        ["collectNotices", "collectSnapshot", "writeNotices", "markReview", "patchUnchecked", "markNarrow",
            "restoreDraft", "restoreRequired", "inspectDraft", "inspectOpaque", "trackRows", "trackSeededRows",
            "currentDraft", "narrowDraft", "startDraft", "startRequiredDraft", "startFilteredDraft", "startUnspecifiedDraft",
            "replaceRecord", "replaceSelected", "replaceRequired", "selectCurrent", "selectConstrained", "selectDefault"]);
    var draftByName = draftContext.ToDictionary(value => value.ExportName);
    foreach (var name in new[] { "startDraft", "startRequiredDraft" })
    {
        var seeded = draftByName[name].Signatures.Single(signature => signature is
            { Parameters: [{ Type.Members: [{ Name: "startingPoint", Optional: false, Type.Name: "TInput" }] }], TypeParameters: ["TInput"] });
        Check(seeded.ReturnType.Members!.Any(member => member is { Name: "snapshot", Type: { Kind: "partialRecord", ElementType.Name: "TInput" } }),
            $"{name} requires a complete initial DTO and keeps live fields sparse");
    }
    Check(!draftByName.TryGetValue("startFilteredDraft", out var filteredSeed) ||
        filteredSeed.Signatures.All(signature => signature.TypeParameters is null || signature.Parameters.Count == 0),
        "conditional inputs and maps of flags cannot initialize an arbitrary DTO");
    var startedForm = draftByName["startDraft"].Signatures.Single(signature =>
        signature is { Parameters: [], TypeParameters: ["TInput"] }).ReturnType.Members!;
    var ambientForm = draftByName["currentDraft"].Signatures.Single(signature =>
        signature is { Parameters: [], TypeParameters: ["TInput"] }).ReturnType.ElementType!.Members!;
    Check(startedForm.Concat(ambientForm).Count(member => member is {
        Name: "groupedNotices", Type: { Kind: "ref", ReadOnly: true,
            ElementType: { Kind: "readonlyDictionary", ElementType: { Kind: "array", ElementType.Kind: "string" } } }
    }) == 2 &&
        startedForm.Concat(ambientForm).Count(member => member is {
            Name: "replace", Type.Signatures: [{ Parameters: [{ Type.Name: "TInput" }, ..] }]
        }) == 2 &&
        startedForm.Concat(ambientForm).All(member => member.Name is not
            ("conditionalGroups" or "fieldGroups" or "conditionalScalarNotices" or "fieldScalarNotices")) &&
        ambientForm.All(member => member.Name is not ("commit" or "transformedGroups" or "transformedNotices")) &&
        HasSparseSubmission(startedForm, "commit", "TInput", "draft") &&
        HasSparseSubmission(draftByName["currentDraft"].Signatures.Single(signature =>
            signature.TypeParameters is ["TInput", "TOutput"]).ReturnType.ElementType!.Members!,
            "commit", "TOutput", "draft"),
        "renamed unseeded forms retain fixed bags and input patches without inventing ambient submission output. " +
        $"Created members: {string.Join(", ", startedForm.Select(member => member.Name))}. " +
        $"Ambient members: {string.Join(", ", ambientForm.Select(member => member.Name))}");
    Check(draftByName.TryGetValue("startDraft", out var startDraft) && startDraft.Signatures.Any(signature =>
        signature is { Parameters: [], TypeParameters: ["TInput"], ReturnType.Kind: "object" } &&
        signature.ReturnType.Members is { } members &&
        members.Any(member => member is { Name: "snapshot", Type.Kind: "partialRecord" }) &&
        members.Any(member => member is { Name: "revise", Type.Signatures: [{
            Parameters: [{ Type.Kind: "propertyPath" }, { Type.Kind: "typeParameter" }]
        }] })) &&
        new[] { "startRequiredDraft", "startUnspecifiedDraft" }.All(name =>
            !draftByName.TryGetValue(name, out var candidate) ||
            candidate.Signatures.All(signature => signature.Parameters.Count != 0 || signature.TypeParameters is null)),
        "renamed form creation requires optional options and defaulted type parameters");
    Check(draftByName["currentDraft"].Signatures.All(signature => signature.ReturnType.Kind == "undefinedable") &&
        ambientForm.Any(member => member is { Name: "snapshot", Type: { Kind: "partialRecord", ElementType.Name: "TInput" } }) &&
        ambientForm.Any(member => member is { Name: "revise", Type.Signatures: [{ Parameters: [{ Type.Kind: "propertyPath" }, _, _] }] }) &&
        ambientForm.Any(member => member is { Name: "recheck", Type.Signatures: [{ Parameters: [], ReturnType.ElementType.Members: [{ Name: "accepted", Type.Kind: "boolean" }] }] }) &&
        ambientForm.Any(member => member is { Name: "review", Type.Signatures: [{ Parameters: [{ Type: { Kind: "propertyPath", ArrayIndices: false } }],
                ReturnType.ElementType.Members: [{ Name: "accepted", Type.Kind: "boolean" }, { Name: "notes", Type.ElementType.Kind: "string" }] }] }) &&
        (!draftByName.TryGetValue("narrowDraft", out var narrowDraft) ||
            narrowDraft.Signatures.All(signature => signature.TypeParameters is null)),
        "a renamed ambient form preserves absence, sparse values and correlated validation, and rejects unmapped record constraints");
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
        { Name: "drop" }] }, { TypeParameters: ["TData"], Parameters: [{ Name: "key", Type.Kind: "string" }],
        ReturnType.Members: [
            { Name: "rows", Type: { Kind: "ref", ElementType: { Kind: "recordArray", ElementType.Members: [
                { Name: "payload", ReadOnly: true, Type: { Kind: "typeParameter", Name: "TData" } },
                { Name: "isHead", ReadOnly: true }, { Name: "isTail", ReadOnly: true }] } } },
            { Name: "drop", Type.Signatures: [{ Parameters: [{ Type.Kind: "number" }] }] },
            { Name: "add", Type.Signatures: [{ Parameters: [{ Type: { Kind: "typeParameter", Name: "TData" } }] }] }] }],
        "the inferred unknown collection omits payload operations and the explicit overload correlates readonly entries and typed writes");
    Check(!draftByName.TryGetValue("trackSeededRows", out var seededRows) ||
        seededRows.Signatures.All(signature => signature.TypeParameters is null && signature.ReturnType.Members is [
            { Name: "rows", Type: { Kind: "ref", ReadOnly: true, ElementType.ReadOnly: true } }, { Name: "drop" },
        ]), "a selector that depends on the selected payload cannot reuse the inferred unknown input projection");
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
    var urlParams = await provider.ReadValueAsync(document, "@vueuse/core", "useUrlSearchParams");
    Check(urlParams.Signatures is [
        { Parameters: [], TypeParameters: null, ReturnType: { Kind: "writableDictionary", ElementType.Kind: "union" } },
        { Parameters: [{ Type: { Kind: "stringEnum", StringLiterals: ["hash", "hash-params", "history"] } }],
            TypeParameters: null, ReturnType: { Kind: "writableDictionary", ElementType: { Kind: "union", Variants:
                [{ Kind: "array", ElementType.Kind: "string" }, { Kind: "string" }] } } },
    ], "URL parameters expose a writable sparse string or repeated-string result for both supported arities");
    var windowFocus = await provider.ReadValueAsync(document, "@vueuse/core", "useWindowFocus");
    var vueModel = await provider.ReadValueAsync(document, "@vueuse/core", "useVModel");
    Check(vueModel.Signatures.Any(signature => signature is {
        Parameters: [_, { Type.Kind: "propertyKey" }, { RestCallback: true }],
        ReturnType.Name: "VueWritableComputedRef",
        OverloadPriority: -1,
    }) && vueModel.Signatures.Any(signature => signature is {
        Parameters: [_, { Type.RequirePlainRecord: true }, { RestCallback: true }, { Type.Kind: "object" }],
    }) && vueModel.Signatures.Any(signature => signature is {
        Parameters: [_, { Type.RequireRecordView: true }, { RestCallback: true }, { Type.Kind: "object" }],
    }) && vueModel.Signatures.Any(signature => signature is {
        Parameters: [_, { Type.Kind: "propertyKey" }, { RestCallback: true }, { Type.Kind: "object" }],
        ManagedNameSuffix: "WithCloneCallback",
    }), "VueUse projects explicit model emits with computed, clone and passive options");
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
            "useIsFieldTouched", "useFieldArray", "useResetForm", "useValidateField", "useValidateForm", "useSubmitForm", "useSetFieldValue", "useFormContext"]);
    var formOptions = new JoltCompileOptions { NpmValues = formExports };
    var formContextSignatures = formExports.Single(value => value.ExportName == "useFormContext").Signatures;
    var typedContext = formContextSignatures.SingleOrDefault(signature => signature.TypeParameters is ["TInput"])?.ReturnType.ElementType?.Members ?? [];
    var explicitContext = formContextSignatures.Single(signature => signature.TypeParameters is ["TInput", "TOutput"]);
    Check(formContextSignatures is [
        { Parameters: [], TypeParameters: null, ReturnType: { Kind: "undefinedable", ElementType.Members: { } status } },
        { Parameters: [], TypeParameters: ["TInput"], ReturnType.Kind: "undefinedable" },
        { Parameters: [], TypeParameters: ["TInput", "TOutput"], ReturnType.Kind: "undefinedable" },
    ] && status.Any(member => member is { Name: "name", Type.Kind: "string" }) &&
        status.Any(member => member is { Name: "submitCount", Type: { Kind: "ref", ElementType.Kind: "number" } }) &&
        status.Any(member => member is { Name: "isSubmitting", Type: { Kind: "ref", ElementType.Kind: "boolean" } }) &&
        status.Any(member => member is { Name: "errors", Type: { Kind: "ref", ElementType.Kind: "readonlyDictionary" } }) &&
        typedContext.Any(member => member is { Name: "values", Type: { Kind: "partialRecord", ElementType.Name: "TInput" } }) &&
        typedContext.Any(member => member is { Name: "validateField", Type.Signatures: [{ Parameters: [{ Type.Kind: "propertyPath" }] }] }),
        "both ambient context overloads preserve provider absence, declared status and the input DTO path");
    var formSignatures = formExports.Single(value => value.ExportName == "useForm").Signatures;
    var typedForm = formSignatures.Single(signature => signature.TypeParameters is ["TInput", "TOutput"]).ReturnType;
    var unseededForm = formSignatures.Single(signature => signature is { Parameters: [], TypeParameters: ["TInput"] }).ReturnType;
    foreach (var commonMembers in formSignatures.Where(signature => signature.TypeParameters?.Contains("TInput") == true)
        .Select(signature => signature.ReturnType.Members!)
        .Concat(formContextSignatures.Where(signature => signature.TypeParameters?.Contains("TInput") == true)
            .Select(signature => signature.ReturnType.ElementType!.Members!)))
    {
        Check(commonMembers.Any(member => member is { Name: "name", Type.Kind: "string" }) &&
            commonMembers.Any(member => member is { Name: "submitCount", Type: { Kind: "ref", ReadOnly: true, ElementType.Kind: "number" } }) &&
            commonMembers.Any(member => member is { Name: "isSubmitting", Type: { Kind: "ref", ReadOnly: true, ElementType.Kind: "boolean" } }) &&
            commonMembers.Any(member => member is { Name: "isValidating", Type: { Kind: "ref", ReadOnly: true, ElementType.Kind: "boolean" } }) &&
            commonMembers.Any(member => member is { Name: "meta", Type: { Kind: "ref", ReadOnly: true, ElementType.Members: { } meta } } &&
            meta.Any(state => state is { Name: "dirty", Type.Kind: "boolean" })) &&
            commonMembers.Any(member => member is { Name: "setFieldTouched", Type.Signatures: [{ TypeParameters: ["TField"],
                Parameters: [{ Type: { Kind: "propertyPath", VeeValidatePath: true, ArrayIndices: true } }, { Type.Kind: "boolean" }] }] }) &&
            commonMembers.Any(member => member is { Name: "setFieldError", Type.Signatures: [{ TypeParameters: ["TField"],
                Parameters: [{ Type.Kind: "propertyPath" }, { Optional: false, Type: { Kind: "undefinedable", ElementType.Kind: "union" } }] }] }) &&
            commonMembers.Any(member => member is { Name: "setErrors", Type.Signatures: [{
                Parameters: [{ Type: { Kind: "formPathMap", TypeArguments: [{ Name: "TInput" }],
                    ElementType: { Kind: "undefinedable", ElementType.Kind: "union" }, ArrayIndices: true } }] }] }) &&
            commonMembers.Any(member => member is { Name: "resetForm", Type.Signatures: [{
                Parameters: [{ Optional: true, Type.Members: [{ Name: "submitCount", Optional: true, Type.Kind: "number" }] },
                    { Optional: true, Type.Members: [{ Name: "force", Optional: true, Type.Kind: "boolean" }] }] }] }),
            "typed forms preserve common live state, selector-based mutations, checked error paths and optional reset options");
    }
    var seededForm = formSignatures.Single(signature => signature is { Parameters: [{ Type.Members:
        [{ Name: "initialValues", Optional: false, Type.Name: "TInput" }] }], TypeParameters: ["TInput"] }).ReturnType;
    Check(HasSparseSubmission(seededForm.Members!, "handleSubmit", "TInput", "values"),
        "seeded forms keep raw submission values sparse after fields unregister");
    foreach (var formMembers in new[] { unseededForm.Members!, typedContext, explicitContext.ReturnType.ElementType!.Members! })
    {
        Check(formMembers.Any(member => member is { Name: "errorBag", Type: { Kind: "ref", ReadOnly: true,
            ElementType: { Kind: "readonlyDictionary", ElementType: { Kind: "array", ElementType.Kind: "string" } } } }) &&
            formMembers.Any(member => member is { Name: "setValues", Type.Signatures: [{
                Parameters: [{ Type.Name: "TInput" }, { Optional: true, Type.Kind: "boolean" }]
            }] }), "unseeded and ambient forms preserve readonly error bags and typed input patches");
    }
    Check(typedContext.All(member => member.Name != "handleSubmit") &&
        HasSparseSubmission(unseededForm.Members!, "handleSubmit", "TInput", "values") &&
        HasSparseSubmission(explicitContext.ReturnType.ElementType!.Members!, "handleSubmit", "TOutput", "values"),
        "submission keeps input values sparse and separates an explicit ambient output");
    Check(formSignatures.Any(signature => signature is { Parameters: [], TypeParameters: ["TInput"], ReturnType.Kind: "object" } &&
        signature.ReturnType.Members is { } members &&
        members.Any(member => member is { Name: "values", Type: { Kind: "partialRecord", ElementType.Name: "TInput" } }) &&
        members.Any(member => member is { Name: "setFieldValue", Type.Signatures: [{ Parameters: [{ Type.Kind: "propertyPath" }, ..] }] }) &&
        members.Any(member => member is { Name: "validateField", Type.Signatures: [{ Parameters: [{ Type.Kind: "propertyPath" }],
            ReturnType.Kind: "promise" }] }) &&
        members.Any(member => member is { Name: "defineField", Type.Signatures: { } signatures } &&
            signatures.Any(call => call is { Parameters: [_], ReturnType.Kind: "tupleView" }))),
        "typed no-options form creation supplies a non-null result with sparse values and correlated field operations");
    Check(typedForm.Members?.SingleOrDefault(member => member.Name == "defineField")?.Type.Signatures is [{
        Parameters: [{ Type: { Kind: "propertyPath", VeeValidatePath: true, ArrayIndices: true } }],
        ReturnType.Kind: "tupleView"
    }, { Parameters: [{ Type.Kind: "propertyPath" }, { Type.Kind: "object" }], ReturnType.Kind: "tupleView" },
    {
        TypeParameters: ["TField", "TProps"],
        Parameters: [{ Type.Kind: "propertyPath" }, { Type.Members: var callbackConfig }],
        ReturnType.Members: [_, { Name: "Item2", Type.ElementType: { Kind: "recordTypeParameter", Name: "TProps" } }]
    }, {
        TypeParameters: ["TField"],
        Parameters: [{ Type.Kind: "propertyPath" }, { Type.Signatures: [{ ReturnType.Members: var lazyScalars }] }]
    }, {
        TypeParameters: ["TField", "TProps"],
        Parameters: [{ Type.Kind: "propertyPath" }, { Type.Signatures: [{ ReturnType.Members: var lazyProps }] }],
        ReturnType.Members: [_, { Name: "Item2", Type: { ReadOnly: true,
            ElementType: { Kind: "recordTypeParameter", Name: "TProps" } } }]
    }] && lazyScalars?.Select(member => member.Name).Order(StringComparer.Ordinal).SequenceEqual(
        ["validateOnBlur", "validateOnChange", "validateOnInput", "validateOnModelUpdate"]) == true &&
        lazyProps?.SingleOrDefault(member => member.Name == "props") is
            { Optional: false, Type: { Kind: "recordTypeParameter", Name: "TProps" } } &&
        callbackConfig!.Any(member => member is {
        Name: "props", Optional: false, Type.Signatures: [{
            Parameters: [{ Type.Members: var stateMembers }],
            ReturnType: { Kind: "recordTypeParameter", Name: "TProps" }
        }]
    } && stateMembers!.Any(state => state is {
        Name: "value", Type: { Kind: "nullableSlot", ElementType.Name: "TField" }
    })) &&
        typedForm.Members?.SingleOrDefault(member => member.Name == "setFieldValue")?.Type.Signatures is [{
        Parameters: [{ Type: { Kind: "propertyPath", VeeValidatePath: true, ArrayIndices: false } }, ..]
    }] && typedForm.Members?.SingleOrDefault(member => member.Name == "validateField")?.Type.Signatures is [{
        Parameters: [{ Type: { Kind: "propertyPath", VeeValidatePath: true, ArrayIndices: true } }],
        ReturnType.Kind: "promise"
    }] && formExports.Single(value => value.ExportName == "useFormContext").Signatures.Any(signature =>
        signature.ReturnType.ElementType?.Members?.SingleOrDefault(member => member.Name == "validateField")?.Type.Signatures is [{
        Parameters: [{ Type: { Kind: "propertyPath", VeeValidatePath: true, ArrayIndices: true } }]
    }]), "locked field binding and validation support indexed paths while the setter preserves its narrower path domain");
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
        (formSource.Replace("var errors = form.Errors;", "var invalidAddress = form.Values.Address; var errors = form.Errors;", StringComparison.Ordinal), "CS1061"),
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
        (formSource.Replace("""value.Contact!.EmailAddress, "updated@example.test", false""",
            "value.Contact!.EmailAddress, false, false", StringComparison.Ordinal), "CS0411"),
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

var toolingStore = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.GetFullPath(".tmp/nodeapi-store");
var toolingLock = Path.GetFullPath("src/Jazor.Npm/tooling/pnpm-lock.yaml");
var toolingVersion = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(toolingLock)));
var writerCheck = Directory.CreateTempSubdirectory("jolt-locale-writer-");
try
{
    var script = Path.Combine(writerCheck.FullName, "verify.mjs");
    await File.WriteAllTextAsync(script, """
        import assert from 'node:assert/strict';
        import { pathToFileURL } from 'node:url';
        const [typescript, recognizer] = globalThis.__jazorArguments;
        const { default: ts } = await import(pathToFileURL(typescript));
        const { readStaticLocaleWriter } = await import(pathToFileURL(recognizer));
        const declarations = `
            declare const marker: unique symbol;
            type Empty<T> = T extends { [marker]?: never } ? true : false;
            type Absent<T> = [T] extends [never] ? true : false;
            type Languages<T, K = keyof T> = K extends string ? K : never;
            interface Composer<M> {
                setLocaleMessage<S = never, L extends string = string,
                    K extends Languages<NonNullable<M>> = Languages<NonNullable<M>>,
                    P = Absent<S> extends true ? Empty<M> extends true ? {} : NonNullable<M>[K] : S,
                    V extends P = P>(locale: L | K, message: V): void;
            }
            declare const concrete: Composer<{ en: { title: string } }>;
        `;
        function accepts(text, locale = 'en') {
            const options = { strict: true };
            const host = ts.createCompilerHost(options);
            const original = host.getSourceFile;
            host.getSourceFile = (file, version, ...args) => file === 'composer.ts' ?
                ts.createSourceFile(file, text, version, true) : original(file, version, ...args);
            const program = ts.createProgram(['composer.ts'], options, host);
            assert.deepEqual(program.getSemanticDiagnostics(), []);
            const checker = program.getTypeChecker();
            const file = program.getSourceFile('composer.ts');
            const declaration = file.statements.at(-1).declarationList.declarations[0];
            const type = checker.getTypeAtLocation(declaration);
            const method = checker.getTypeOfPropertyOfType(type, 'setLocaleMessage');
            const acceptsLocale = readStaticLocaleWriter(ts, checker, method.getCallSignatures(), type.target.getSymbol(), 'setLocaleMessage', checker.getTypeArguments(type)[0]);
            return Boolean(acceptsLocale && acceptsLocale(locale));
        }
        assert.equal(accepts(declarations), true);
        const mixedKeys = declarations.replace('Composer<{ en: { title: string } }>', 'Composer<{ 42: { title: string }; en: { title: string } }>');
        assert.equal(accepts(mixedKeys), true);
        assert.equal(accepts(mixedKeys, '42'), false);
        assert.equal(accepts(declarations.replace('Languages<T, K = keyof T> = K extends string ? K : never', 'Languages<T> = Extract<keyof T, string>')), true);
        assert.equal(accepts(declarations.replace('T extends { [marker]?: never } ? true : false', 'T extends unknown ? true : false')), false);
        """);
    await NodeModuleRunner.RunAsync(writerCheck.FullName, script, [
        Path.Combine(toolingStore, "npm-metadata", toolingVersion, "node_modules", "typescript", "lib", "typescript.js"),
        Path.GetFullPath("src/Jazor.Npm/locale-message-projection.mjs"),
    ]);
}
finally
{
    writerCheck.Delete(recursive: true);
}

static bool HasSparseSubmission(IReadOnlyList<NpmValueMember> members, string name, string output, string valuesName) =>
    members.SingleOrDefault(member => member.Name == name)?.Type.Signatures is [_, {
        Parameters: [
            { Type.Signatures: [{ Parameters: [{ Type: { Kind: "partialRecord", ElementType.Name: var success } }] }] },
            { Type.Signatures: [{ Parameters: [{ Type.Members: var failure }] }] }
        ],
        ReturnType.Signatures: [{ Parameters: [], ReturnType: { Kind: "promise", ElementType.Kind: "void" } }]
    }] && success == output && failure?.Any(member => member.Name == valuesName &&
        member.Type is { Kind: "partialRecord", ElementType.Name: "TInput" }) == true;

static void Check(bool value, string description)
{
    if (!value)
        throw new InvalidOperationException($"Verification failed for {description}.");
}
