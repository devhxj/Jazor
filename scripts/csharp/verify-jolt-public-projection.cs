#:property AssemblyName=Jazor.NodeApiVerification
#:property CETCompat=false
#:property PublishAot=false
#:project ../../src/Jazor.Npm/Jazor.Npm.csproj
#:project ../../src/Jazor.NodeHost/Jazor.NodeHost.csproj

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Jazor.NodeHost;
using Jazor.Npm;

if (args is not [var executable])
    throw new ArgumentException("Usage: verify-jolt-public-projection.cs <Jolt CLI apphost>");
var cli = Path.GetFullPath(executable);
var root = Environment.CurrentDirectory;
var fixture = Path.Combine(root, "scripts", "csharp", "fixtures", "jolt-npm-runtime");
var scratch = Directory.CreateDirectory(Path.Combine(root, ".tmp", $"public-projection-{Guid.NewGuid():N}"));
try
{
    foreach (var name in new[] { "package.json", "pnpm-lock.yaml" })
        File.Copy(Path.Combine(fixture, name), Path.Combine(scratch.FullName, name));
    var manifestPath = Path.Combine(scratch.FullName, "package.json");
    var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!;
    manifest["dependencies"]!["projection-fixture"] = "link:./fixture";
    manifest["dependencies"]!["projection-transfer"] = "link:./transfer";
    var referenceManifest = JsonNode.Parse(File.ReadAllText(Path.Combine(fixture, "..", "jolt-binding-projection", "package.json")))!;
    manifest["dependencies"]!["vue-i18n"] = referenceManifest["dependencies"]!["vue-i18n"]!.DeepClone();
    File.WriteAllText(manifestPath, manifest.ToJsonString());
    var localPackage = scratch.CreateSubdirectory("fixture");
    File.WriteAllText(Path.Combine(localPackage.FullName, "package.json"),
        """{"name":"projection-fixture","version":"1.0.0","type":"module","exports":"./fixture.js"}""");
    File.WriteAllText(Path.Combine(localPackage.FullName, "fixture.js"), """
        export function createEnvelope(value) { return { payload: value, version: 3 }; }
        export function createDraft(seed, include) { return include ? { seed, value: seed } : { seed }; }
        export function hasDraftValue(draft) { return Object.hasOwn(draft, "value"); }
        export function hasNullDraftValue(draft) { return Object.hasOwn(draft, "value") && draft.value === null; }
        export const hasNullNullableDraftValue = hasNullDraftValue;
        import { computed, ref } from "vue";
        export function observeField(props, key, emit, options) {
            if (key !== "count" || options !== undefined)
                throw new Error("Expected the selected model key and omitted options");
            return computed({
                get: () => props[key],
                set: () => {
                    if (props[key] > 0)
                        emit("update:count", null, undefined, 0, false);
                    else
                        emit("update:count");
                },
            });
        }
        export function watchField(props, key, publish) {
            return computed({ get: () => props[key], set: value => publish('changed', value) });
        }
        export function makeMeter(initial) {
            const meter = ref(initial);
            meter.replace = function(next) {
                if (this !== meter)
                    throw new Error("Expected the original meter receiver");
                const previous = this.value;
                this.value = next;
                return previous;
            };
            return meter;
        }
        export function switchSignal() {
            const source = ref("enabled");
            return Object.assign(source, { source, status: computed(() => source.value) });
        }
        export function persistCurrent(value) { return value; }
        export function computedLabel() {
            const number = ref(3);
            return computed({ get: () => String(number.value), set: value => { number.value = value; } });
        }
        export function numberLabel() {
            let label = "initial";
            return { get value() { return label; }, set value(number) { label = String(number); } };
        }
        export function booleanLabel() {
            let label = "initial";
            return { get value() { return label; }, set value(flag) { label = String(flag); } };
        }
        export function nullableWriter(initial) {
            let current = initial;
            return { get value() { return current; }, set value(next) { current = next === null ? initial : next; } };
        }
        export function buildTree(value) { return { value, children: [{ value, children: [] }] }; }
        export function pickCount(source) {
            return typeof source.payload === "number" ? { count: source.payload } : {};
        }
        export function acceptCount(source, patch) {
            return Object.hasOwn(patch, "count") ? patch.count : source.version;
        }
        export default function format(label, options) {
            if (!Object.hasOwn(options, 'nullable') || options.nullable !== null)
                throw new Error('Expected an explicit null option');
            return { label: label.toUpperCase(), nullable: 1 };
        }
        export function unprojectable() { return Symbol(); }
        let reads = 0;
        export function tryRead(present) { reads++; return present ? 0 : undefined; }
        export function readBox(present) {
            return { get value() { reads++; return present ? 0 : undefined; } };
        }
        export function takeReads() { const count = reads; reads = 0; return count; }
        export const absent = undefined;
        export const zero = 0;
        export function optionalArguments(count = 7, options = {}, label = "unset") {
            return [count, options === null ? "null" : "object", label, arguments.length].join("|");
        }
        export function transform(value, project) { return project(value); }
        export function compareProjection(value, project) {
            if (typeof value === "string")
                return project(value) + 1;
            return project(value * 2);
        }
        export function lookup(target, key) { return target[key]; }
        export function assign(target, key, value) { target[key] = value; }
        const mappedArrays = new WeakSet();
        let lastCatalogItems;
        export function mapItems(items, select) {
            if (typeof items[0] === 'string' && !mappedArrays.has(items))
                throw new Error('Mapping the projected result must preserve array identity');
            const result = items.map(select);
            if (items[0]?.Name === 'Notebook') {
                if (typeof result[0] === 'string')
                    lastCatalogItems = items;
                else if (lastCatalogItems !== items)
                    throw new Error('Mapping a C# array must preserve array identity');
            }
            mappedArrays.add(result);
            return result;
        }
        """);
    File.WriteAllText(Path.Combine(localPackage.FullName, "fixture.d.ts"), """
        import type { ComputedRef, Ref, WritableComputedRef } from "vue";
        import type { UseVModelOptions } from "@vueuse/core";
        export declare function observeField<P extends object, K extends keyof P, Name extends string>(
            props: P, key?: K, emit?: (name: Name, ...args: any[]) => void,
            options?: UseVModelOptions<P[K], false>): WritableComputedRef<P[K]>;
        export declare function watchField<Row extends object, Field extends keyof Row>(
            row: Row, field: Field, publish: (event: string, value: Row[Field]) => void): WritableComputedRef<Row[Field]>;
        export type SwitchState = "enabled" | "disabled";
        export type SwitchSignal = Ref<SwitchState> & {
            readonly source: Ref<SwitchState>;
            readonly status: ComputedRef<SwitchState>;
        };
        export declare function switchSignal(): SwitchSignal;
        export interface Meter<T> extends Ref<T> { replace(next: T): T; }
        export declare function makeMeter<T>(initial: T): Meter<T>;
        export interface Envelope<T> { payload: T; readonly version: number; }
        export declare function createEnvelope<T>(value: T): Envelope<T>;
        export interface Draft<T> { seed: T; value?: T; }
        export declare function createDraft<T>(seed: T, include: boolean): Draft<T>;
        export declare function hasDraftValue<T>(draft: Draft<T>): boolean;
        export declare function hasNullDraftValue<T>(draft: Draft<T>): boolean;
        export interface NullableDraft<T> { seed: T; value?: T | null; }
        export declare function hasNullNullableDraftValue<T>(draft: NullableDraft<T>): boolean;
        export declare function persistCurrent<T>(value: Ref<T>): Ref<T>;
        export declare function computedLabel(): WritableComputedRef<string, number>;
        export declare function numberLabel(): Ref<string, number>;
        export declare function booleanLabel(): Ref<string, boolean>;
        export declare function nullableWriter<T>(initial: T): Ref<T, T | null>;
        export interface Tree<T> { value: T; readonly children: readonly Tree<T>[]; }
        export declare function buildTree<T>(value: T): Tree<T>;
        export interface CountFields<T> { payload: T; count: number; }
        export type CountPatch<T> = Partial<Pick<CountFields<T>, "count">>;
        export declare function pickCount<TSource>(source: Envelope<TSource>): CountPatch<TSource>;
        export declare function acceptCount<TInput>(source: Envelope<TInput>, patch: CountPatch<TInput>): number;
        export default function format(label: string, options: { nullable?: number | null }): {
            label: string;
            nullable?: number | null;
            missing?: number;
        };
        export declare function unprojectable(): symbol;
        export declare function unprojectableInput(value: symbol): string;
        export declare function tryRead(present: boolean): number | undefined;
        export declare function readBox(present: boolean): { readonly value: number | undefined };
        export declare function takeReads(): number;
        export declare const absent: number | undefined;
        export declare const zero: number | undefined;
        export declare function optionalArguments(count?: number, options?: string | null, label?: string): string;
        export declare function transform<TInput, TOutput>(value: TInput, project: (value: TInput) => TOutput): TOutput;
        export declare function compareProjection(value: string, project: (value: string) => number): number;
        export declare function compareProjection<T>(value: number, project: (value: number) => T): T;
        export declare function lookup<T, K extends keyof T>(target: T, key: K): T[K];
        export declare function assign<T, K extends keyof T>(target: T, key: K, value: T[K]): void;
        export declare function mapItems<T, U>(items: readonly T[], select: (value: T) => U): readonly U[];
        """);
    var transferPackage = scratch.CreateSubdirectory("transfer");
    File.WriteAllText(Path.Combine(transferPackage.FullName, "package.json"),
        """{"name":"projection-transfer","version":"1.0.0","type":"module","exports":"./transfer.js"}""");
    File.WriteAllText(Path.Combine(transferPackage.FullName, "transfer.js"), """
        export function forwardEnvelope(value) { return value; }
        export function forwardDraft(draft) { return draft; }
        export function forwardNullableDraft(draft) { return draft; }
        export function forwardTree(value) { return value; }
        export function viewCurrent(value) { return value; }
        export function forwardSlot(value) { return value; }
        export function persistSignal(value) { return value; }
        export function forwardMeter(value) { return value; }
        export function forwardComputed(value) { return value; }
        export function selectOptional(enabled = false) {
            return enabled ? { path: "true/" + arguments.length } : arguments.length;
        }
        export function selectMode(label, enabled, count) { return enabled ? { path: label + "/" + count } : count; }
        """);
    File.WriteAllText(Path.Combine(transferPackage.FullName, "transfer.d.ts"), """
        import type { Ref, WritableComputedRef } from "vue";
        import type { Draft, Envelope, Meter, NullableDraft, SwitchState, Tree } from "projection-fixture";
        export declare function forwardEnvelope<T>(value: Envelope<T>): Envelope<T>;
        export declare function forwardDraft<T>(draft: Draft<T>): Draft<T>;
        export declare function forwardNullableDraft<T>(draft: NullableDraft<T>): NullableDraft<T>;
        export declare function viewCurrent<T>(value: Ref<T>): Readonly<Ref<T>>;
        export declare function forwardSlot<R, W>(value: Ref<R, W>): Ref<R, W>;
        export declare function persistSignal(value: Ref<SwitchState>): Ref<SwitchState>;
        export declare function forwardMeter<T>(value: Meter<T>): Meter<T>;
        export declare function forwardComputed<R, W>(value: WritableComputedRef<R, W>): WritableComputedRef<R, W>;
        export declare function forwardTree<T>(value: Tree<T>): Tree<T>;
        export declare function selectOptional<B extends boolean = false>(enabled?: B): B extends true ? { path: string } : number;
        export declare function selectMode<B extends boolean>(label: string, enabled: B, count: number): B extends true ? { path: string } : number;
        """);
    var store = Environment.GetEnvironmentVariable("JazorPnpmStoreDirectory") ?? Path.Combine(root, ".tmp", "nodeapi-store");
    await NodeToolchain.RestoreAsync(scratch.FullName, store, CancellationToken.None, Console.Out);
    var library = scratch.CreateSubdirectory("Library");
    var consumer = scratch.CreateSubdirectory("Consumer");
    var source = Path.Combine(library.FullName, "Dates.g.cs");
    string[] dateExports = ["add", "formatDistance", "formatDuration", "formatISO", "isWithinInterval", "intervalToDuration"];
    var dates = ("date-fns", dateExports);
    (string, string[]) locale = ("date-fns/locale", ["fr"]);
    await GenerateAsync(source, [dates, locale]);
    var first = await File.ReadAllTextAsync(source);
    if (first.Contains('\r'))
        throw new InvalidOperationException("Generated bindings must use LF line endings.");
    File.SetLastWriteTimeUtc(source, new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    var timestamp = File.GetLastWriteTimeUtc(source);
    await GenerateAsync(source, [locale, ("date-fns", dateExports.Reverse().ToArray())]);
    if (await File.ReadAllTextAsync(source) != first || File.GetLastWriteTimeUtc(source) != timestamp)
        throw new InvalidOperationException("Reordered exports rewrote an identical public projection.");
    await GenerateAsync(source, [dates, ("date-fns/locale", ["fr", "noSuchExport"])], expectedError: "'date-fns/locale' has no export named 'noSuchExport'");
    await GenerateAsync(source, [dates, ("projection-fixture", ["unprojectable"])],
        expectedError: "Unsupported npm value export 'unprojectable' from 'projection-fixture': type 'symbol'. Failed at return type. TypeScript declaration:");
    var diagnosticProvider = new NpmValueMetadataProvider(store);
    var diagnosticResult = await diagnosticProvider.ReadValueProjectionsAsync(manifestPath, "projection-fixture",
        ["unprojectable", "unprojectableInput"]);
    if (diagnosticResult.Errors.Count != 2 ||
        !diagnosticResult.Errors["unprojectableInput"].Contains("Failed at parameter 'value'.", StringComparison.Ordinal) ||
        diagnosticResult.Errors.Values.Any(error => !Regex.IsMatch(error, @"fixture\.d\.ts\([1-9]\d*,[1-9]\d*\)")))
        throw new InvalidOperationException(string.Join(Environment.NewLine, diagnosticResult.Errors.Values));
    Console.WriteLine("Verified unsupported return and parameter diagnostics include the original TypeScript declaration location.");
    if (await File.ReadAllTextAsync(source) != first || File.GetLastWriteTimeUtc(source) != timestamp)
        throw new InvalidOperationException("A failed generation changed the existing public projection.");
    await GenerateAsync(Path.Combine(library.FullName, "Fixture.g.cs"), [("projection-fixture", ["default", "optionalArguments", "tryRead", "readBox", "takeReads", "absent", "zero", "transform", "mapItems", "lookup", "assign", "createEnvelope", "persistCurrent", "pickCount", "acceptCount", "buildTree", "createDraft", "hasDraftValue", "hasNullDraftValue", "numberLabel", "booleanLabel", "nullableWriter", "computedLabel", "switchSignal", "makeMeter", "observeField", "watchField", "hasNullNullableDraftValue"]),
        ("projection-transfer", ["forwardEnvelope", "viewCurrent", "selectMode", "forwardTree", "selectOptional", "forwardDraft", "forwardNullableDraft", "forwardSlot", "forwardComputed", "persistSignal", "forwardMeter"])]);
    await GenerateAsync(Path.Combine(library.FullName, "VueUse.g.cs"), [("@vueuse/core", ["isClient", "useStorage", "useVModel", "useColorMode", "createEventHook"])]);
    await GenerateAsync(Path.Combine(library.FullName, "Query.g.cs"), [("@tanstack/vue-query", ["useQuery", "VueQueryPlugin", "useMutation", "useInfiniteQuery", "useQueryClient"])]);
    await GenerateAsync(Path.Combine(library.FullName, "Form.g.cs"), [("vee-validate", ["useForm", "useField"])]);
    await GenerateAsync(Path.Combine(library.FullName, "Messages.g.cs"), [("vue-i18n", ["createI18n"])]);
    await GenerateAsync(Path.Combine(library.FullName, "Text.g.cs"), [("es-toolkit/compat", ["truncate"])]);
    await GenerateAsync(Path.Combine(library.FullName, "Async.g.cs"), [("es-toolkit/promise", ["withTimeout"]),
        ("es-toolkit/function", ["retry"])]);

    // Use the selected tool build's host contracts and source generator. The consumer
    // only references the projection through the compiled library's public metadata.
    File.WriteAllText(Path.Combine(scratch.FullName, "Directory.Build.props"), """
        <Project>
          <PropertyGroup>
            <TargetFramework>net11.0</TargetFramework>
            <LangVersion>preview</LangVersion>
            <Nullable>enable</Nullable>
          </PropertyGroup>
          <ItemGroup>
            <Reference Include="ECMAScript" HintPath="$(JoltToolDirectory)/ECMAScript.dll" />
            <Reference Include="ECMAScript.Contract" HintPath="$(JoltToolDirectory)/ECMAScript.Contract.dll" />
            <Reference Include="ECMAScript.Vue" HintPath="$(JoltToolDirectory)/ECMAScript.Vue.dll" />
          </ItemGroup>
        </Project>
        """);
    File.WriteAllText(Path.Combine(library.FullName, "Library.csproj"), """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup><TreatWarningsAsErrors>true</TreatWarningsAsErrors></PropertyGroup>
        </Project>
        """);
    File.WriteAllText(Path.Combine(consumer.FullName, "Consumer.csproj"), """
        <Project Sdk="Microsoft.NET.Sdk">
          <ItemGroup>
            <ProjectReference Include="../Library/Library.csproj" />
            <Analyzer Include="$(JoltToolDirectory)/Jazor.Compiler.dll;$(JoltToolDirectory)/Jazor.Common.dll;$(JoltToolDirectory)/ECMAScript.Contract.dll;$(JoltToolDirectory)/Acornima*.dll;$(JoltToolDirectory)/ICSharpCode.Decompiler.dll" />
          </ItemGroup>
        </Project>
        """);
    const string optionalResults = """
        public static bool OptionalResults()
        {
            System.Func<bool, double?> read = Fixture.TryRead;
            return Fixture.TryRead(false) == null && Fixture.TryRead(true) == 0 &&
                read(false) == null && read(true) == 0 && Fixture.Absent == null && Fixture.Zero == 0 &&
                Fixture.ReadBox(false).Value == null && Fixture.ReadBox(true).Value == 0 &&
                Fixture.TakeReads() == 6;
        }
        """;
    File.WriteAllText(Path.Combine(scratch.FullName, "Optional.jazor"),
        "@using Fixture = Npm.ProjectionFixture.Exports\n\n@code {\n" + optionalResults + "\n}\n");
    const string genericTransform = """
        private sealed record TransformInput(string Label, double Count);

        public static string TransformResult()
        {
            var input = new TransformInput("typed", 3);
            string label = Fixture.Transform(input, value => value.Label);
            double count = Fixture.Transform(input, value => value.Count);
            return label + "/" + count;
        }
        """;
    File.WriteAllText(Path.Combine(scratch.FullName, "Transform.jazor"), """
        @using Fixture = Npm.ProjectionFixture.Exports

        @code {
            public static string TransformResult()
            {
                double length = Fixture.Transform("typed", value => (double)value.Length);
                return Fixture.Transform(length, value => value == 5 ? "typed/5" : "unexpected");
            }
        }
        """);
    const string mapItems = """
        public record CatalogItem(string Name, double Price) : ECMAScript.Vue.VueProps;

        public static string MapCatalog()
        {
            var items = new[] { new CatalogItem("Notebook", 6), new CatalogItem("Pen", 2) };
            ECMAScript.IArray<string> names = Fixture.MapItems(items, item => item.Name);
            ECMAScript.IArray<double> prices = Fixture.MapItems(items, item => item.Price);
            ECMAScript.IArray<double> lengths = Fixture.MapItems(names, name => (double)name.Length);
            return names[0] + "/" + prices[1] + "/" + lengths[0];
        }
        """;
    File.WriteAllText(Path.Combine(scratch.FullName, "Catalog.jazor"),
        "@using Fixture = Npm.ProjectionFixture.Exports\n\n<p>@MapCatalog()</p>\n\n@code {\n" +
        mapItems.Replace("public static string MapCatalog()", "private string MapCatalog()") + "\n}\n");
    var switchState = Regex.Match(File.ReadAllText(Path.Combine(library.FullName, "Fixture.g.cs")),
        @"public enum (SwitchState_[0-9A-F]{12})\b");
    if (!switchState.Success)
        throw new InvalidOperationException("Expected the shared SwitchState enum in the combined package bindings.");
    var directGenerics = """
        public static bool AugmentedRef()
        {
            var meter = Fixture.MakeMeter(5d);
            var forwarded = Transfer.ForwardMeter(meter);
            double previous = forwarded.Replace(0d);
            var flag = Transfer.ForwardMeter(Fixture.MakeMeter(true));
            bool original = flag.Replace(false);
            return object.ReferenceEquals(meter, forwarded) && previous == 5d && meter.Value == 0d
                && original && !flag.Value;
        }

        public static bool EnrichedRef()
        {
            var signal = Fixture.SwitchSignal();
            var forwarded = Transfer.PersistSignal(signal);
            var initial = signal.Value;
            signal.Value = SWITCH_STATE.Disabled;
            return object.ReferenceEquals(signal, forwarded) && object.ReferenceEquals(signal, signal.Source)
                && initial == SWITCH_STATE.Enabled
                && forwarded.Value == SWITCH_STATE.Disabled
                && signal.Source.Value == SWITCH_STATE.Disabled
                && signal.Status.Value == SWITCH_STATE.Disabled;
        }

        public static bool WritableComputed()
        {
            var label = Fixture.ComputedLabel();
            var forwardedLabel = Transfer.ForwardComputed(label);
            string before = forwardedLabel.Value;
            forwardedLabel.WriteValue = 0d;

            var state = ECMAScript.Vue.Ref(true);
            var flag = ECMAScript.Vue.Computed(new ECMAScript.Vue.VueWritableComputedOptions<bool> {
                Get = () => state.Value,
                Set = value => { state.Value = value; },
            });
            var forwardedFlag = Transfer.ForwardComputed(flag);
            forwardedFlag.WriteValue = false;
            return object.ReferenceEquals(label, forwardedLabel) && object.ReferenceEquals(flag, forwardedFlag)
                && before == "3" && label.Value == "0" && !state.Value && !forwardedFlag.Value;
        }

        public static bool AsymmetricRefs()
        {
            var number = Fixture.NumberLabel();
            var forwardedNumber = Transfer.ForwardSlot(number);
            forwardedNumber.WriteValue = 0d;
            string zero = number.Value;
            var flag = Fixture.BooleanLabel();
            var forwardedFlag = Transfer.ForwardSlot(flag);
            forwardedFlag.WriteValue = false;
            string disabled = flag.Value;
            var nullable = Fixture.NullableWriter(3d);
            var forwardedNullable = Transfer.ForwardSlot(nullable);
            forwardedNullable.WriteValue = 0d;
            double count = nullable.Value;
            forwardedNullable.WriteValue = null;
            return object.ReferenceEquals(number, forwardedNumber) && object.ReferenceEquals(flag, forwardedFlag)
                && object.ReferenceEquals(nullable, forwardedNullable)
                && zero == "0" && disabled == "false" && count == 0d && nullable.Value == 3d;
        }

        public static string RoundTrip()
        {
            var source = ECMAScript.Vue.Ref(2d);
            var writable = Fixture.PersistCurrent(source);
            var observed = Transfer.ViewCurrent(writable);
            writable.Value = 7d;
            double count = observed.Value;
            var envelope = Fixture.CreateEnvelope("initial");
            var forwarded = Transfer.ForwardEnvelope(envelope);
            forwarded.Payload = "updated";
            string label = envelope.Payload;
            double version = forwarded.Version;
            return object.ReferenceEquals(source, writable) && object.ReferenceEquals(writable, observed)
                && object.ReferenceEquals(envelope, forwarded) ? label + "/" + count + "/" + version : "copied";
        }

        public static string LiteralSelection()
        {
            string path = Transfer.SelectModeWithEnabledTrue(count: 4, label: "catalog").Path;
            System.Func<string, double, double> selectCount = Transfer.SelectModeWithEnabledFalse;
            double count = selectCount("catalog", 7);
            return path + "/" + count;
        }

        public static string OptionalSelection()
        {
            double omitted = Transfer.SelectOptional();
            double disabled = Transfer.SelectOptionalWithEnabledFalse();
            string enabled = Transfer.SelectOptionalWithEnabledTrue().Path;
            return omitted + "/" + disabled + "/" + enabled;
        }

        public static string RecursiveTree()
        {
            var tree = Fixture.BuildTree("root");
            var forwarded = Transfer.ForwardTree(tree);
            var child = forwarded.Children[0];
            var forwardedChild = Transfer.ForwardTree(child);
            forwardedChild.Value = "leaf";
            return object.ReferenceEquals(tree, forwarded) && object.ReferenceEquals(child, forwardedChild)
                && forwardedChild.Children.Length == 0 ? tree.Value + "/" + tree.Children[0].Value : "copied";
        }

        public static double ConstructEnvelope()
        {
            var input = Transfer.ForwardEnvelope<double>(new() { Payload = 4d, Version = 2d });
            var output = Transfer.ForwardEnvelope(input);
            return object.ReferenceEquals(input, output) ? output.Payload + output.Version : -1;
        }

        public static bool InspectDrafts()
        {
            var omitted = Fixture.CreateDraft(0d, false);
            var initialized = Transfer.ForwardDraft<bool>(new() { Seed = true, Value = false });
            var nullable = Transfer.ForwardDraft<double?>(new() { Seed = null, Value = null });
            var forwarded = Transfer.ForwardDraft(omitted);
            bool absent = !forwarded.HasValue && !Fixture.HasDraftValue(forwarded);
            forwarded.Value = 0d;
            var record = Fixture.CreateDraft(Fixture.CreateEnvelope("initial"), true);
            var update = Fixture.CreateEnvelope("changed");
            Transfer.ForwardDraft(record).Value = update;
            return absent && object.ReferenceEquals(omitted, forwarded)
                && omitted.HasValue && Fixture.HasDraftValue(omitted) && omitted.Value == 0d
                && initialized.HasValue && Fixture.HasDraftValue(initialized) && !initialized.Value
                && nullable.HasValue && Fixture.HasNullDraftValue(nullable) && nullable.Value == null
                && record.HasValue && object.ReferenceEquals(record.Value, update)
                && record.Value.Payload == "changed";
        }

        public static double ReadOmittedDraft() => Fixture.CreateDraft(0d, false).Value;


        public static bool InspectNullableDrafts()
        {
            var absent = Transfer.ForwardNullableDraft<double>(new() { Seed = 1d });
            var empty = Transfer.ForwardNullableDraft<double>(new() { Seed = 1d, Value = null });
            var zero = Transfer.ForwardNullableDraft<double>(new() { Seed = 1d, Value = 0d });
            var disabled = Transfer.ForwardNullableDraft<bool>(new() { Seed = true, Value = false });
            return !absent.HasValue && !Fixture.HasNullNullableDraftValue(absent)
                && empty.HasValue && Fixture.HasNullNullableDraftValue(empty) && empty.Value is null
                && zero.HasValue && zero.Value!.Value == 0d
                && disabled.HasValue && !disabled.Value!.Value;
        }

        public static bool MappedRoundTrip()
        {
            var source = Transfer.ForwardEnvelope<double>(new() { Payload = 0d, Version = 3d });
            var patch = Fixture.PickCount(source);
            double? count = patch.Count;
            var missingSource = Transfer.ForwardEnvelope<string>(new() { Payload = "missing", Version = 5d });
            var missing = Fixture.PickCount(missingSource);
            double? missingCount = missing.Count;
            return count == 0d && missingCount == null
                && Fixture.AcceptCount(source, new()) == 3d
                && Fixture.AcceptCount(source, new() { Count = 0d }) == 0d
                && Fixture.AcceptCount(source, patch) == 0d
                && Fixture.AcceptCount(missingSource, missing) == 5d;
        }
        """.Replace("SWITCH_STATE", "global::Jolt.Generated.Npm.Exports." + switchState.Groups[1].Value);
    File.WriteAllText(Path.Combine(scratch.FullName, "DirectGenerics.jazor"),
        "@using Fixture = Npm.ProjectionFixture.Exports\n@using Transfer = Npm.ProjectionTransfer.Exports\n\n@code {\n" +
        directGenerics + "\n}\n");
    File.WriteAllText(Path.Combine(scratch.FullName, "Mode.jazor"), """
        @using Messages = Npm.VueI18n.Exports
        @code {
            public static string Locale()
            {
                var plugin = Messages.CreateI18nWithLegacyFalse(new() { Locale = "fr" });
                var locale = plugin.Global.Locale.Value;
                plugin.Dispose();
                return locale;
            }
        }
        """);
    const string keyedProperties = """
        public sealed record Nested(double Total);
        public sealed record Entry(
            [property: ECMAScript.ECMAScriptName("pricing.total")] double FlatTotal,
            [property: ECMAScript.ECMAScriptName("pricing")] Nested Pricing,
            string Label);

        public static string ReadAndWriteKeys()
        {
            var entry = new Entry(7, new Nested(9), "before");
            double initial = Fixture.Lookup(entry, item => item.FlatTotal);
            Fixture.Assign(entry, item => item.FlatTotal, 0);
            Fixture.Assign(entry, item => item.Label, "after");
            double count = Fixture.Lookup(entry, item => item.FlatTotal);
            string label = Fixture.Lookup(entry, item => item.Label);
            return initial + "/" + count + "/" + label + "/" + entry.Pricing.Total;
        }
        """;
    File.WriteAllText(Path.Combine(scratch.FullName, "Keys.jazor"),
        "@using Fixture = Npm.ProjectionFixture.Exports\n\n<p>@ReadAndWriteKeys()</p>\n\n@code {\n" +
        keyedProperties.Replace("public static string ReadAndWriteKeys()", "private string ReadAndWriteKeys()") + "\n}\n");
    // Compile the identical typed calls through both authoring entry points.
    const string compareProjection = """
        public static string ReadProjection()
        {
            double length = Fixture.CompareProjection("typed", (string value) => (double)value.Length);
            string result = Fixture.CompareProjection(3d, (double value) => value == 6d ? "doubled" : "unexpected");
            return result + "/" + length;
        }
        """;
    File.WriteAllText(Path.Combine(scratch.FullName, "ProjectionParity.jazor"),
        "@using Fixture = Npm.ProjectionFixture.Exports\n\n<p>@ReadProjection()</p>\n\n@code {\n" +
        compareProjection + "\n}\n");
    await RunAsync(cli, scratch.FullName, ["build"]);
    VerifyProjectionImport(File.ReadAllText(Path.Combine(scratch.FullName, ".jolt", "ProjectionParity.mjs")));
    var caller = Path.Combine(consumer.FullName, "Calendar.cs");
    const string calendar = """
        using Dates = Npm.DateFns.Exports;
        using Locales = Npm.DateFns.Locale.Exports;
        using Fixture = Npm.ProjectionFixture.Exports;
        using VueUse = Npm.Vueuse.Core.Exports;
        using Transfer = Npm.ProjectionTransfer.Exports;
        using Text = Npm.EsToolkit.Compat.Exports;
        using Timers = Npm.EsToolkit.Promise.Exports;
        using Functions = Npm.EsToolkit.Function.Exports;
        [ECMAScript.ECMAScriptModule("calendar.mjs")]
        public static class Calendar
        {
            public static string Label()
            {
                var duration = new Dates.Duration { Years = 1, Days = 2 };
                return Dates.FormatDuration(duration, new() { Delimiter = ", " });
            }

            public static string Upper()
            {
                var result = Fixture.Default(Label(), new() { Nullable = null });
                result.Nullable = null;
                return result.Missing == null && result.Nullable == null ? result.Label : "missing";
            }

            public static bool IsClient() => VueUse.IsClient;

            public static string CheckEventHook()
            {
                var hook = VueUse.CreateEventHook<string>();
                var result = "";
                System.Action<string> record = value => { result += value; };
                hook.On(record);
                hook.Trigger("a");
                hook.Off(record);
                hook.Trigger("b");
                var registration = hook.On(record);
                hook.Trigger("c");
                registration.Off();
                hook.Trigger("d");
                return result;
            }

            public sealed record ModelProps(double Count, string Label) : ECMAScript.Vue.VueProps;

            public static double ReadModel()
            {
                var model = VueUse.UseVModel(new ModelProps(4d, "label"), props => props.Count);
                double count = model.Value;
                return count;
            }

            public static bool CheckModelOptions()
            {
                var props = new ModelProps(4d, "label");
                double observed = -1;
                var direct = VueUse.UseVModel(props, value => value.Count, new() {
                    DefaultValue = 0d,
                    ShouldEmit = value => { observed = value; return false; },
                });
                var passive = VueUse.UseVModelWithPassiveTrue(props, value => value.Count, new() {
                    DefaultValue = 0d,
                    ShouldEmit = value => value >= 0d,
                });
                direct.Value = 0d;
                var forwarded = Transfer.ForwardComputed(direct);
                double current = forwarded.Value;
                double passiveValue = passive.Value;
                return object.ReferenceEquals(direct, forwarded)
                    && observed == 0d && current == 4d && passiveValue == 4d;
            }

            public sealed record RefProperty(ECMAScript.Vue.IVueRef<double> Count) : ECMAScript.Vue.VueProps;
            public sealed record ArrayRefs(ECMAScript.Vue.IVueRef<double>[] Items,
                ECMAScript.Vue.VueShallowRef<RefProperty>[] Shallow) : ECMAScript.Vue.VueProps;
            public sealed record ArrayModel(ArrayRefs Rows, RefProperty Direct) : ECMAScript.Vue.VueProps;

            public static bool CheckPassiveModelRefs()
            {
                ECMAScript.Vue.IVueRef<double> count = ECMAScript.Vue.Ref(4d);
                var direct = new RefProperty(count);
                var shallow = ECMAScript.Vue.ShallowRef(direct);
                var props = new ArrayModel(new ArrayRefs([count], [shallow]), direct);
                var passive = VueUse.UseVModelWithPassiveTrueRecord(props, value => value.Rows, new());
                passive.Value.Items[0].Value = 7d;
                return object.ReferenceEquals(passive.Value.Items[0], count)
                    && object.ReferenceEquals(passive.Value.Shallow[0], shallow)
                    && object.ReferenceEquals(passive.Value.Shallow[0].Value, direct)
                    && object.ReferenceEquals(passive.Value.Shallow[0].Value.Count, count)
                    && count.Value == 7d;
            }

            public static bool CheckPassiveModelView()
            {
                ECMAScript.Vue.IVueRef<double> count = ECMAScript.Vue.Ref(4d);
                var direct = new RefProperty(count);
                var props = new ArrayModel(new ArrayRefs([], []), direct);
                var view = VueUse.UseVModelWithPassiveTrueRecordView(props, value => value.Direct, new()).Value;
                ECMAScript.Vue.IVueRef<double> selected = ECMAScript.Vue.ToRef(view, value => value.Count);
                bool read = selected.Value == 4d;
                selected.Value = 8d;
                return read && count.Value == 8d;
            }

            public static bool CheckColorMode()
            {
                var mode = VueUse.UseColorMode(new() { Attribute = "data-theme" });
                mode.Value = VueUse.StringChoiceOfAutoOrDarkOrLight.Dark;
                return mode.Value == VueUse.StringChoiceOfAutoOrDarkOrLight.Dark
                    && mode.Store.Value == VueUse.StringChoiceOfAutoOrDarkOrLight.Dark;
            }

            public static string OptionalArguments() =>
                Fixture.OptionalArguments() + ";" + Fixture.OptionalArguments(label: "named") + ";" +
                Fixture.OptionalArguments(count: 0, options: null, label: "explicit");

            // OPTIONAL_RESULTS
            // GENERIC_TRANSFORM
            // MAP_ITEMS

            public static string FrenchDistance() =>
                Dates.FormatDistance(new ECMAScript.Date(2026, 0, 17), new ECMAScript.Date(2026, 0, 15),
                    new() { Locale = Locales.Fr });

            public static string CalendarDate() =>
                Dates.FormatISO(new ECMAScript.Date(2026, 0, 15), new() {
                    Format = Dates.ISOStringFormat.Basic,
                    Representation = Dates.ISOStringRepresentation.Date,
                });

            public static string Preview() =>
                Text.Truncate("generated projection", new Text.TruncateOptions { Length = 12 });

            private sealed record Row(string Title);

            public static ECMAScript.IPromise<string> LoadTitle() =>
                Timers.WithTimeout(() => Functions.Retry(() =>
                    ECMAScript.Promise<Row>.Resolve(new Row("loaded")), 2), 1000).Then(row => row.Title);

            public static string BookingLength()
            {
                var start = new ECMAScript.Date(2026, 0, 15);
                var interval = new Dates.Interval { Start = start, End = new ECMAScript.Date(2026, 0, 17) };
                if (!Dates.IsWithinInterval(start, interval))
                    return "outside";
                return Dates.FormatDuration(Dates.IntervalToDuration(interval));
            }
        }
        """;
    var calendarSource = calendar.Replace("// OPTIONAL_RESULTS", optionalResults)
        .Replace("// GENERIC_TRANSFORM", genericTransform)
        .Replace("// MAP_ITEMS", mapItems);
    File.WriteAllText(caller, calendarSource);
    File.WriteAllText(Path.Combine(consumer.FullName, "MessagesProbe.cs"), """
        using Messages = Npm.VueI18n.Exports;

        [ECMAScript.ECMAScriptModule("messages-probe.mjs")]
        public static class MessagesProbe
        {
            public static string Composition()
            {
                var plugin = Messages.CreateI18nWithLegacyFalse(new() {
                    Locale = "fr",
                    Messages = new() { ["en"] = new() { ["greet"] = "Hello" } },
                });
                var global = plugin.Global;
                var before = global.Locale.Value;
                global.Locale.Value = "en";
                var result = before + "/" + global.T("greet");
                plugin.Dispose();
                return result;
            }

            public static string Legacy()
            {
                var plugin = Messages.CreateI18nWithLegacyTrue(new() {
                    Locale = "en",
                    Messages = new() { ["fr"] = new() { ["greet"] = "Salut" } },
                });
                var global = plugin.Global;
                var before = global.Locale;
                global.Locale = "fr";
                var result = before + "/" + global.T("greet");
                plugin.Dispose();
                return result;
            }
        }
        """);
    var queryPath = Path.Combine(consumer.FullName, "QueryProbe.cs");
    const string querySource = """
        using Query = Npm.Tanstack.VueQuery.Exports;
        using ECMAScript;
        using System;

        [ECMAScriptModule("query-probe.mjs")]
        public static class QueryProbe
        {
            private sealed record Payload(string Label) : Vue.VueProps;
            private sealed record Filter(double Page) : Vue.VueProps;

            public static void Install(Vue.VueApp app) => app.Use(Query.VueQueryPlugin);

            public static IPromise<string> FetchCache()
            {
                var client = Query.UseQueryClient();
                var seed = new Payload("seed");
                client.SetQueryData(["public-cache-seed"], seed);
                if (!object.ReferenceEquals(seed, client.GetQueryData<Payload>(["public-cache-seed"])))
                    return Promise<string>.Resolve("lost seed identity");
                return client.FetchQuery<Payload>(new() {
                    QueryKey = ["public-cache", 0, false],
                    QueryFn = () => Promise<Payload>.Resolve(new Payload("cache result")),
                }).Then(data => {
                    Payload? cached = client.GetQueryData<Payload>(["public-cache", 0, false]);
                    if (!object.ReferenceEquals(data, cached))
                        return "lost identity";
                    client.SetQueryData<Payload>(["public-cache", 0, false],
                        (Payload? previous) => new Payload((previous?.Label ?? "missing") + " updated"));
                    return client.GetQueryData<Payload>(["public-cache", 0, false])?.Label ?? "missing";
                });
            }

            public static void RemoveCache() =>
                Query.UseQueryClient().RemoveQueries(new() { QueryKey = ["public-cache", 0, false], Exact = true });

            public static void Start(Action<string> receive)
            {
                var query = Query.UseQuery(new Query.UseQueryAsyncOptionsInput<Payload>
                {
                    QueryKey = ["public-projection", 0, false, new Filter(3)],
                    QueryFn = () => Promise<Payload>.Resolve(new Payload("query result")),
                });
                string? initial = query.Data?.Value?.Label;
                receive(initial ?? "pending");
                Vue.OnServerPrefetch(() => query.Refetch().Then(_ =>
                {
                    string? fetched = query.Data?.Value?.Label;
                    receive(fetched ?? "missing");
                }));
            }
        }
        """;
    File.WriteAllText(queryPath, querySource);
    var fieldPath = Path.Combine(consumer.FullName, "FieldProbe.cs");
    var fieldSource = """
        using Validation = Npm.VeeValidate.Exports;

        [ECMAScript.ECMAScriptModule("field-probe.mjs")]
        public static class FieldProbe
        {
            public static string ReadFields()
            {
                var count = Validation.UseField<double>("rows[0].count");
                var enabled = Validation.UseField<bool>("enabled", "projection_rule");
                var configured = Validation.UseField<double>("configured", options: new() { InitialValue = 0 });
                var named = Validation.UseField<bool>("named", "projection_rule", options: new() { InitialValue = false });
                var name = ReadName(count.Name);
                if (count.Value.Value is not null || enabled.Value.Value is not null)
                    return "unexpected initial value";
                count.SetValue(0, false);
                enabled.SetValue(false, false);
                return count.Value.Value?.Value == 0 && enabled.Value.Value?.Value == false &&
                    configured.Value.Value?.Value == 0 && named.Value.Value?.Value == false ? name : "missing";
            }

            public static string ReadName(Validation.RefOrString name)
            {
                var value = name.Value;
                return value is string text ? text : ((ECMAScript.Vue.VueReadonlyRef<string>)value!).Value;
            }

            public static ECMAScript.IPromise<string> ValidateMissing()
            {
                var field = Validation.UseField<double>("checked", value => value is null ? "required" : true);
                return field.Validate().Then(result => result.Valid ? "valid" : result.Errors[0]);
            }
        }
        """;
    File.WriteAllText(fieldPath, fieldSource);
    var correlatedPath = Path.Combine(consumer.FullName, "CorrelatedConsumer.cs");
    var correlatedSource = File.ReadAllText(Path.Combine(fixture, "..", "jolt-binding-projection", "CorrelatedConsumer.cs"))
        .Replace("    public static string ReadStorage()", """
                public static UnknownValue?[] ReadRestEmit(bool multiple)
                {
                    UnknownValue?[] captured = [null];
                    var props = new UnmountInput(multiple ? 4d : 0d, true);
                    var model = Npm.ProjectionFixture.Exports.ObserveField(props, p => p.Count, (name, payload) =>
                    {
                        if (name == "update:count")
                            captured = payload;
                    });
                    model.Value = 7d;
                    return captured;
                }

                public static double ReadTypedEmit()
                {
                    double observed = -1d;
                    var model = Npm.ProjectionFixture.Exports.WatchField(new EmptyInput(4d, true), p => p.Count,
                        (name, value) => { if (name == "changed") observed = value; });
                    model.Value = 0d;
                    return observed;
                }

                public static string ReadStorage()
            """, StringComparison.Ordinal);
    File.WriteAllText(correlatedPath, correlatedSource);
    var keyedPath = Path.Combine(consumer.FullName, "KeyedConsumer.cs");
    var keyedSource = """
        using Fixture = Npm.ProjectionFixture.Exports;

        [ECMAScript.ECMAScriptModule("keyed-consumer.mjs")]
        public static class KeyedConsumer
        {
            // KEYED_PROPERTIES
        }
        """.Replace("// KEYED_PROPERTIES", keyedProperties);
    File.WriteAllText(keyedPath, keyedSource);
    var directPath = Path.Combine(consumer.FullName, "DirectGenerics.cs");
    var directSource = """
        using Fixture = Npm.ProjectionFixture.Exports;
        using Transfer = Npm.ProjectionTransfer.Exports;

        [ECMAScript.ECMAScriptModule("direct-generics.mjs")]
        public static class DirectGenerics
        {
            // DIRECT_GENERICS
        }
        """.Replace("// DIRECT_GENERICS", directGenerics);
    File.WriteAllText(directPath, directSource);
    var paritySdk = scratch.CreateSubdirectory("ParitySdk");
    File.WriteAllText(Path.Combine(paritySdk.FullName, "ParitySdk.csproj"), """
        <Project Sdk="Microsoft.NET.Sdk">
          <ItemGroup>
            <ProjectReference Include="REPOSITORY/src/Jazor.Emit/Jazor.Emit.csproj" ReferenceOutputAssembly="false" />
            <Analyzer Include="$(JoltToolDirectory)/Jazor.Compiler.dll;$(JoltToolDirectory)/Jazor.Common.dll;$(JoltToolDirectory)/ECMAScript.Contract.dll;$(JoltToolDirectory)/Acornima*.dll;$(JoltToolDirectory)/ICSharpCode.Decompiler.dll" />
          </ItemGroup>
          <Import Project="REPOSITORY/src/Jazor/build/Jazor.targets" />
        </Project>
        """.Replace("REPOSITORY", System.Security.SecurityElement.Escape(root), StringComparison.Ordinal));
    File.WriteAllText(Path.Combine(paritySdk.FullName, "ProjectionParity.cs"), """
        using Fixture = Npm.ProjectionFixture.Exports;

        [ECMAScript.ECMAScriptModule("projection-parity.mjs")]
        public static class ProjectionParity
        {
            // SAME_CALLS
        }
        """.Replace("// SAME_CALLS", compareProjection, StringComparison.Ordinal));
    await BuildAsync(paritySdk);
    await BuildAsync(library);
    await BuildAsync(consumer);
    var assembly = Assembly.Load(File.ReadAllBytes(Path.Combine(consumer.FullName, "bin", "Debug", "net11.0", "Consumer.dll")));
    var catalog = assembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: true)!;
    var modules = (IEnumerable)catalog.GetMethod("GetModules", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;
    var parityAssembly = Assembly.Load(File.ReadAllBytes(Path.Combine(paritySdk.FullName, "bin", "Debug", "net11.0", "ParitySdk.dll")));
    var parityCatalog = parityAssembly.GetType("Jazor.Generated.ModuleCatalog", throwOnError: true)!;
    var parityModules = (IEnumerable)parityCatalog.GetMethod("GetModules", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, null)!;
    foreach (var module in modules.Cast<object>().Concat(parityModules.Cast<object>()))
    {
        var js = (string)module.GetType().GetProperty("Content")!.GetValue(module)!;
        var name = (string)module.GetType().GetProperty("RelativePath")!.GetValue(module)!;
        if (name == "projection-parity.mjs")
            VerifyProjectionImport(js);
        if (name == "query-probe.mjs" && !js.Contains("from \"@tanstack/vue-query\"", StringComparison.Ordinal))
            throw new InvalidOperationException("The generated query module is missing its Vue Query import.");
        if (name == "messages-probe.mjs" && new[] { "false", "true" }.Any(mode =>
            !Regex.IsMatch(js, $"""["']?\blegacy["']?\s*:\s*{mode}\b""")))
            throw new InvalidOperationException($"Expected both Vue I18n option literals from assembly metadata.\n{js}");
        if (name == "correlated-consumer.mjs" &&
            (!js.Contains("label: \"Contact count\"", StringComparison.Ordinal) ||
             !js.Contains("validateOnBlur: false", StringComparison.Ordinal) ||
             !js.Contains("validateOnModelUpdate: false", StringComparison.Ordinal)))
            throw new InvalidOperationException($"Expected the field binding label and disabled validation flags in the generated module.\n{js}");
        if (name == "correlated-consumer.mjs" &&
            (!Regex.IsMatch(js, """useStorage\(\((?<key>[\w$]+)\s*=\s*["']named-storage["'],\s*(?<initial>[\w$]+)\s*=\s*["']configured["'],\s*(?<options>[\w$]+)\s*=\s*\{\s*["']?writeDefaults["']?\s*:\s*false\s*\},\s*(?<gap>[\w$]+)\s*=\s*(?:void\s+0|undefined),\s*\k<key>\),\s*\k<initial>,\s*\k<gap>,\s*\k<options>\)""") ||
             !Regex.IsMatch(js, """\(["']default-storage["'],\s*["']default["']\)""")))
            throw new InvalidOperationException($"Expected the VueUse storage gap and a separate two-argument call.\n{js}");
        File.WriteAllText(Path.Combine(scratch.FullName, name), js);
    }
    var verify = Path.Combine(scratch.FullName, "verify.mjs");
    File.WriteAllText(verify, """
        import assert from "node:assert/strict";
        import { createRenderer, createSSRApp, h, nextTick, ref } from "vue";
        import { renderToString } from "vue/server-renderer";
        import { ReadProjection } from "./projection-parity.mjs";
        import ProjectionParity, { ReadProjection as ReadJoltProjection } from "./.jolt/ProjectionParity.mjs";
        assert.equal(ReadProjection(), "doubled/6");
        assert.equal(ReadJoltProjection(), "doubled/6");
        assert.match(await renderToString(createSSRApp(ProjectionParity)), /<p>doubled\/6<\/p>/);
        import { BookingLength, CalendarDate, CheckColorMode, CheckModelOptions, CheckPassiveModelRefs, CheckPassiveModelView, FrenchDistance, IsClient, Label, LoadTitle, MapCatalog, OptionalArguments, OptionalResults, Preview, TransformResult, Upper } from "./calendar.mjs";
        import { FetchCache, Install, RemoveCache, Start } from "./query-probe.mjs";
        import { ReadFields, ReadName, ValidateMissing } from "./field-probe.mjs";
        import { Composition, Legacy } from "./messages-probe.mjs";
        import { ReadPageParams, ReadRestEmit, ReadStorage, ReadTypedEmit, ReadVariables, ReadVueUseEmit, TrackSeededField, WriteEmptyForm, WriteForm, WriteSeededForm } from "./correlated-consumer.mjs";
        assert.deepEqual(ReadRestEmit(false), []);
        assert.deepEqual(ReadRestEmit(true), [null, null, 0, false]);
        assert.deepEqual(ReadVueUseEmit(), [0]);
        assert.equal(ReadTypedEmit(), 0);
        import { OptionalResults as JoltOptionalResults } from "./.jolt/Optional.mjs";
        import { Locale } from "./.jolt/Mode.mjs";
        import { TransformResult as JoltTransformResult } from "./.jolt/Transform.mjs";
        import Catalog from "./.jolt/Catalog.mjs";
        import { ConstructEnvelope, LiteralSelection, MappedRoundTrip, RoundTrip } from "./direct-generics.mjs";
        import { ConstructEnvelope as JoltConstructEnvelope, LiteralSelection as JoltLiteralSelection, MappedRoundTrip as JoltMappedRoundTrip, RoundTrip as JoltRoundTrip } from "./.jolt/DirectGenerics.mjs";
        import { OptionalSelection, RecursiveTree } from "./direct-generics.mjs";
        import { OptionalSelection as JoltOptionalSelection, RecursiveTree as JoltRecursiveTree } from "./.jolt/DirectGenerics.mjs";
        import { InspectDrafts, InspectNullableDrafts, ReadOmittedDraft } from "./direct-generics.mjs";
        import { AsymmetricRefs, AugmentedRef, EnrichedRef, WritableComputed } from "./direct-generics.mjs";
        import { AsymmetricRefs as JoltAsymmetricRefs, AugmentedRef as JoltAugmentedRef, EnrichedRef as JoltEnrichedRef, WritableComputed as JoltWritableComputed } from "./.jolt/DirectGenerics.mjs";
        import { InspectDrafts as JoltInspectDrafts, InspectNullableDrafts as JoltInspectNullableDrafts, ReadOmittedDraft as JoltReadOmittedDraft } from "./.jolt/DirectGenerics.mjs";
        import { ReadAndWriteKeys } from "./keyed-consumer.mjs";
        import Keys from "./.jolt/Keys.mjs";
        import { useQueryClient } from "@tanstack/vue-query";
        import { defineRule, useField } from "vee-validate";
        const field = { setup() {
            useField("count");
            return () => null;
        } };
        const showField = ref(true);
        let readSeededField;
        const parent = { setup() {
            readSeededField = TrackSeededField();
            return () => showField.value ? h(field) : null;
        } };
        // Only comment nodes are rendered. Vue owns the component lifecycle.
        const renderer = createRenderer({
            createComment: () => ({}),
            insert(node, parent) { node.parent = parent; },
            remove(node) { delete node.parent; },
            parentNode: node => node.parent,
            nextSibling: () => null,
        });
        const mountedForm = renderer.createApp(parent);
        mountedForm.mount({});
        try {
            assert.equal(await readSeededField(), "0/false");
            showField.value = false;
            await nextTick();
            await nextTick();
            assert.equal(await readSeededField(), "missing/false");
        } finally {
            mountedForm.unmount();
        }
        assert.equal(Label(), "1 year, 2 days");
        assert.equal(Upper(), "1 YEAR, 2 DAYS");
        assert.equal(IsClient(), false);
        assert.equal((await import("./calendar.mjs")).CheckEventHook(), "ac");
        assert.equal(BookingLength(), "2 days");
        assert.equal(FrenchDistance(), "2 jours");
        assert.equal(ReadName("plain name"), "plain name");
        defineRule("projection_rule", () => true);
        const fields = createSSRApp({
            setup() {
                assert.equal(ReadFields(), "rows[0].count");
                return () => h("div");
            },
        });
        await renderToString(fields);
        const validated = createSSRApp({
            async setup() {
                assert.equal(await ValidateMissing(), "required");
                return () => h("div");
            },
        });
        await renderToString(validated);
        assert.equal(OptionalArguments(), "7|object|unset|0;7|object|named|3;0|null|explicit|3");
        assert.equal(OptionalResults(), true);
        assert.equal(JoltOptionalResults(), true);
        assert.equal(CalendarDate(), "20260115");
        assert.equal(Preview(), "generated...");
        assert.equal(Composition(), "fr/Hello");
        assert.equal(Legacy(), "en/Salut");
        assert.equal(Locale(), "fr");
        assert.equal(await LoadTitle(), "loaded");
        assert.equal(TransformResult(), "typed/3");
        assert.equal(JoltTransformResult(), "typed/5");
        assert.equal(MapCatalog(), "Notebook/2/8");
        assert.equal(await renderToString(createSSRApp(Catalog)), "<p>Notebook/2/8</p>");
        assert.equal(RoundTrip(), "updated/7/3");
        assert.equal(JoltRoundTrip(), "updated/7/3");
        assert.equal(LiteralSelection(), "catalog/4/7");
        assert.equal(JoltLiteralSelection(), "catalog/4/7");
        assert.equal(OptionalSelection(), "0/1/true/1");
        assert.equal(JoltOptionalSelection(), "0/1/true/1");
        assert.equal(RecursiveTree(), "root/leaf");
        assert.equal(JoltRecursiveTree(), "root/leaf");
        assert.equal(ConstructEnvelope(), 6);
        assert.equal(JoltConstructEnvelope(), 6);
        assert.equal(MappedRoundTrip(), true);
        assert.equal(JoltMappedRoundTrip(), true);
        assert.equal(InspectDrafts(), true);
        assert.equal(JoltInspectDrafts(), true);
        assert.equal(InspectNullableDrafts(), true);
        assert.equal(JoltInspectNullableDrafts(), true);
        assert.equal(AsymmetricRefs(), true);
        assert.equal(JoltAsymmetricRefs(), true);
        assert.equal(EnrichedRef(), true);
        assert.equal(AugmentedRef(), true);
        assert.equal(JoltAugmentedRef(), true);
        assert.equal(JoltEnrichedRef(), true);
        assert.equal(WritableComputed(), true);
        assert.equal(JoltWritableComputed(), true);
        assert.throws(ReadOmittedDraft);
        assert.throws(JoltReadOmittedDraft);
        let emptyForm;
        const emptyFormApp = createSSRApp({
            setup() {
                emptyForm = WriteEmptyForm();
                return () => h("p");
            },
        });
        await renderToString(emptyFormApp);
        assert.equal(await emptyForm, "missing/0/2");
        let seededForm;
        await renderToString(createSSRApp({
            setup() {
                seededForm = WriteSeededForm();
                return () => h("p");
            },
        }));
        assert.equal(await seededForm, "0/false/5");
        assert.equal(ReadAndWriteKeys(), "7/0/after/9");
        assert.equal(await renderToString(createSSRApp(Keys)), "<p>7/0/after/9</p>");
        const observations = [];
        const variables = [];
        const pages = [];
        const pageStates = [];
        const app = createSSRApp({
            setup() {
                Start(value => observations.push(value));
                ReadVariables(value => variables.push(value));
                ReadPageParams((parameter, label) => pages.push([parameter, label]),
                    (pending, success) => pageStates.push([pending, success]));
                assert.equal(WriteForm(), "updated/0/3/4/custom-blur");
                assert.equal(ReadStorage(), "configured/default");
                assert.equal(CheckModelOptions(), true);
                assert.equal(CheckPassiveModelRefs(), true);
                assert.equal(CheckPassiveModelView(), true);
                assert.equal(CheckColorMode(), true);
                return () => h("p");
            },
        });
        app.config.errorHandler = error => { throw error; };
        Install(app);
        await renderToString(app);
        assert.deepEqual(observations, ["pending", "query result"]);
        assert.deepEqual(variables, [null, 0]);
        assert.deepEqual(pages, [[0, "first"]]);
        assert.deepEqual(pageStates, [[true, false], [false, true]]);
        const client = app.runWithContext(() => useQueryClient());
        assert.equal(await app.runWithContext(FetchCache), "cache result updated");
        assert.deepEqual(client.getQueryData(["public-cache", 0, false]), { Label: "cache result updated" });
        client.setQueryData(["public-cache", 0, false, "child"], "retained");
        app.runWithContext(RemoveCache);
        assert.equal(client.getQueryData(["public-cache", 0, false]), undefined);
        assert.equal(client.getQueryData(["public-cache", 0, false, "child"]), "retained");
        assert.ok(client.getQueryData(["public-projection", 0, false, { Page: 3 }]));
        client.clear();
        """);
    await NodeModuleRunner.RunAsync(scratch.FullName, verify);
    Console.WriteLine("The SDK and Jolt selected the same typed npm overloads, emitted the same named import, and executed both calls. Vue also rendered their result.");

    File.WriteAllText(caller, calendar.Replace(
        "var passive = VueUse.UseVModelWithPassiveTrueRecord(props, value => value.Rows, new());",
        "var rejected = VueUse.UseVModelWithPassiveTrueRecord(props, value => value.Direct, new());\n" +
        "                var passive = VueUse.UseVModelWithPassiveTrueRecord(props, value => value.Rows, new());"));
    await BuildAsync(consumer, expectedError: "must preserve its authored type under Vue's deep proxy reads");
    File.WriteAllText(caller, calendar.Replace("Days = 2", """Days = "two" """));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(caller, calendar.Replace("Dates.IntervalToDuration(interval)", "Dates.IntervalToDuration(start)"));
    await BuildAsync(consumer, expectedError: "CS1503");
    File.WriteAllText(caller, calendar.Replace("Length = 12", """Length = "twelve" """));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(caller, calendar.Replace("row => row.Title", "row => row.Missing"));
    await BuildAsync(consumer, expectedError: "CS1061");
    File.WriteAllText(caller, calendar.Replace("Dates.ISOStringFormat.Basic", "Dates.ISOStringRepresentation.Date"));
    await BuildAsync(consumer, expectedError: "CS0266");
    File.WriteAllText(caller, calendar.Replace("Locale = Locales.Fr", "Locale = \"fr\""));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(caller, calendarSource.Replace("input, value => value.Label", "input, (string value) => value"));
    await BuildAsync(consumer, expectedError: "CS0411");
    File.WriteAllText(caller, calendarSource);
    File.WriteAllText(correlatedPath, correlatedSource.Replace("EmailAddress, \"updated\"", "EmailAddress, false"));
    await BuildAsync(consumer, expectedError: "CS0411");
    File.WriteAllText(correlatedPath, correlatedSource.Replace("count.WriteValue = 3", """count.WriteValue = "three" """));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(correlatedPath, correlatedSource.Replace("ValidateOnBlur = false,", """ValidateOnBlur = "off","""));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(correlatedPath, correlatedSource.Replace("ValidateOnBlur = false", """ValidateOnBlur = "false" """));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(correlatedPath, correlatedSource.Replace("double? variables =", "double variables ="));
    await BuildAsync(consumer, expectedError: "CS0266");
    File.WriteAllText(correlatedPath, correlatedSource.Replace("double pageParam =", "string pageParam ="));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(correlatedPath, correlatedSource.Replace("SelectedPage result =", "Page result ="));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(correlatedPath, correlatedSource);
    File.WriteAllText(caller, calendarSource.Replace("return names[0]", "names[0] = \"changed\"; return names[0]"));
    await BuildAsync(consumer, expectedError: "CS0200");
    File.WriteAllText(caller, calendarSource);
    File.WriteAllText(keyedPath, keyedSource.Replace("item => item.FlatTotal, 0", "item => item.FlatTotal, false"));
    await BuildAsync(consumer, expectedError: "CS0411");
    File.WriteAllText(keyedPath, keyedSource.Replace("item => item.FlatTotal, 0", "item => item.Pricing.Total, 0"));
    await BuildAsync(consumer, expectedError: "A JavaScript property key requires a lambda selecting exactly one structural record data property.");
    File.WriteAllText(keyedPath, keyedSource);
    File.WriteAllText(directPath, directSource.Replace("writable.Value = 7d", "observed.Value = 7d"));
    await BuildAsync(consumer, expectedError: "CS0200");
    File.WriteAllText(directPath, directSource.Replace("forwarded.Payload = \"updated\"", "forwarded.Payload = 7d"));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(directPath, directSource.Replace("double version = forwarded.Version", "forwarded.Version = 7d; double version = forwarded.Version"));
    await BuildAsync(consumer, expectedError: "CS8852");
    File.WriteAllText(directPath, directSource.Replace("Payload = 4d, Version = 2d", "Payload = 4d"));
    await BuildAsync(consumer, expectedError: "CS9035");
    File.WriteAllText(directPath, directSource.Replace("double omitted = Transfer.SelectOptional()", "string omitted = Transfer.SelectOptional()"));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(directPath, directSource.Replace("forwardedChild.Value = \"leaf\"", "forwardedChild.Value = 7d"));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(directPath, directSource.Replace("forwardedChild.Value = \"leaf\"", "forwarded.Children[0] = forwardedChild"));
    await BuildAsync(consumer, expectedError: "CS0200");
    File.WriteAllText(directPath, directSource.Replace("forwarded.Value = 0d", "forwarded.Value = null"));
    await BuildAsync(consumer, expectedError: "CS0037");
    File.WriteAllText(directPath, directSource.Replace("forwardedNumber.WriteValue = 0d", "forwardedNumber.WriteValue = \"wrong\""));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(directPath, directSource.Replace("Transfer.ForwardComputed(flag)", "Transfer.ForwardComputed(state)"));
    await BuildAsync(consumer, expectedError: "CS0411");
    File.WriteAllText(directPath, directSource);
    File.WriteAllText(caller, calendarSource.Replace("DefaultValue = 0d", "DefaultValue = \"wrong\""));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(caller, calendarSource.Replace("ShouldEmit = value => value >= 0d", "ShouldEmit = (string value) => value.Length > 0"));
    await BuildAsync(consumer, expectedError: "CS1661");
    File.WriteAllText(caller, calendarSource);
    File.WriteAllText(directPath, directSource.Replace("signal.Value = global::Jolt.Generated.Npm.Exports." + switchState.Groups[1].Value + ".Disabled",
        "signal.Value = \"outside-domain\""));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(directPath, directSource);
    File.WriteAllText(caller, calendarSource.Replace("mode.Value = VueUse.StringChoiceOfAutoOrDarkOrLight.Dark", "mode.Value = \"outside-domain\""));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(caller, calendarSource);
    File.WriteAllText(correlatedPath, correlatedSource.Replace("captured = payload;", "{ double number = payload[0]!; }", StringComparison.Ordinal));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(correlatedPath, correlatedSource);
    File.WriteAllText(directPath, directSource.Replace("Transfer.ForwardMeter(meter)", "Transfer.ForwardMeter<double>(ECMAScript.Vue.Ref(5d))"));
    await BuildAsync(consumer, expectedError: "CS1503");
    File.WriteAllText(directPath, directSource.Replace("Transfer.ForwardMeter(meter)", "Transfer.ForwardMeter<string>(meter)"));
    await BuildAsync(consumer, expectedError: "CS1503");
    File.WriteAllText(directPath, directSource);
    File.WriteAllText(queryPath, querySource.Replace("Promise<Payload>.Resolve(new Payload(\"cache result\"))", "Promise<string>.Resolve(\"wrong\")"));
    await BuildAsync(consumer, expectedError: "CS0266");
    File.WriteAllText(queryPath, querySource.Replace("Exact = true", "Exact = \"wrong\""));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(queryPath, querySource.Replace("Payload? cached =", "string? cached ="));
    await BuildAsync(consumer, expectedError: "CS0029");
    Console.WriteLine("The independent C# consumer verified QueryClient DTO cache reads, writes, updates, fetches, and exact removal.");
    File.WriteAllText(queryPath, querySource);
    File.WriteAllText(fieldPath, fieldSource.Replace("count.SetValue(0, false)", "count.SetValue(\"wrong\", false)"));
    await BuildAsync(consumer, expectedError: "CS1503");
    File.WriteAllText(fieldPath, fieldSource.Replace("InitialValue = 0", "InitialValue = \"wrong\""));
    await BuildAsync(consumer, expectedError: "CS0029");
    File.WriteAllText(fieldPath, fieldSource);
    Console.WriteLine("C# and Jolt preserved augmented refs and their method receiver across packages. C# rejected an ordinary ref and an incompatible generic instantiation.");
    Console.WriteLine("C# and Jolt preserved split Vue ref identity across packages and wrote zero, false, and null through the typed setter.");
    Console.WriteLine("C# and Jolt verified generic optional field presence, writes, forwarding, and absent reads.");
    Console.WriteLine("C# and Jolt verified the omitted and explicit boolean calls, and preserved recursive tree identity across packages.");
    Console.WriteLine("C# and Jolt preserved Vue ref and named record identity across both packages. C# rejected readonly writes and an incompatible payload.");
    Console.WriteLine("C# and Jolt passed a named mapped result back as input and preserved both a missing count and zero.");
    Console.WriteLine("C# and Jolt read and wrote a literal dotted key and preserved zero. C# rejected an incompatible setter value and a nested selector.");
    Console.WriteLine("C# and Jolt mapped catalog DTOs to typed readonly arrays, and the C# compiler rejected an index assignment.");
    Console.WriteLine("The independent C# consumer verified nested form writes, optional mutation variables, and selected infinite-query page parameters.");
    Console.WriteLine("The independent C# consumer ran both Vue I18n modes, Vue Query plugin registration, date-fns with a shared locale, and the inferred DTO through es-toolkit retry and withTimeout. The generic transform returned string and numeric results from C# and Jolt, and the C# compiler rejected an incompatible callback.");

    async Task GenerateAsync(string output, (string Module, string[] Exports)[] requests, string? expectedError = null)
    {
        var provider = new NpmValueMetadataProvider(store);
        var exports = new List<NpmValueExport>();
        foreach (var (module, names) in requests)
        {
            var projection = await provider.ReadValueProjectionsAsync(manifestPath, module, names);
            if (projection.Errors.Count != 0)
            {
                if (expectedError is not null && projection.Errors.Values.Any(error => error.Contains(expectedError, StringComparison.Ordinal)))
                    return;
                throw new InvalidOperationException(string.Join(Environment.NewLine, projection.Errors.Values));
            }
            exports.AddRange(projection.Values);
        }

        if (expectedError is not null)
            throw new InvalidOperationException($"Expected a projection error containing {expectedError}.");
        var text = NpmBindings.GenerateCachedSource(scratch.FullName, [], exports);
        if (!File.Exists(output) || await File.ReadAllTextAsync(output) != text)
            await File.WriteAllTextAsync(output, text);
    }

    Task BuildAsync(DirectoryInfo project, string? expectedError = null) => RunAsync("dotnet", project.FullName,
        ["build", "-v:q", "-p:JoltToolDirectory=" + Path.GetDirectoryName(cli)], expectedError);
}
finally
{
    scratch.Delete(recursive: true);
}

static void VerifyProjectionImport(string js)
{
    if (!Regex.IsMatch(js, """import\s*\{[^}]*\bcompareProjection\b[^}]*\}\s*from\s*"projection-fixture";"""))
        throw new InvalidOperationException($"Expected the named compareProjection import from projection-fixture.\n{js}");
}

static async Task RunAsync(string executable, string directory, string[] arguments, string? expectedError = null)
{
    var start = new ProcessStartInfo(executable)
    {
        WorkingDirectory = directory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start)!;
    var output = Task.WhenAll(process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
    await process.WaitForExitAsync();
    var log = string.Join(Environment.NewLine, await output);
    if (expectedError is null ? process.ExitCode != 0 :
        process.ExitCode == 0 || !log.Contains(expectedError, StringComparison.Ordinal))
        throw new InvalidOperationException($"{executable} {string.Join(' ', arguments)} exited {process.ExitCode}.{Environment.NewLine}{log}");
}
