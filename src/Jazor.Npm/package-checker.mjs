import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { resolveRuntimeModule } from './runtime-module.mjs';
import { readTypeScriptInput } from './typescript-input.mjs';
import { declarationDocumentation } from './declaration-documentation.mjs';

export async function packageChecker(entryUrl, typescript, moduleSpecifier, sourceText, validateRuntime) {
    const { default: ts } = await import(pathToFileURL(typescript));
    const inputs = new Set();
    const reads = new Map();
    function readInput(file) {
        const key = path.resolve(file);
        inputs.add(key);
        if (!reads.has(key))
            reads.set(key, readTypeScriptInput(key));
        return reads.get(key)?.text;
    }
    const modulePath = resolveRuntimeModule(moduleSpecifier, entryUrl, import.meta.resolve, inputs, readInput);
    const root = path.resolve(path.dirname(fileURLToPath(entryUrl)), '../..');
    const entry = path.join(root, 'jazor-metadata.mts');
    inputs.add(modulePath);
    const options = {
        target: ts.ScriptTarget.ES2022,
        module: ts.ModuleKind.ESNext,
        moduleResolution: ts.ModuleResolutionKind.Bundler,
        strict: true,
        exactOptionalPropertyTypes: true,
        skipLibCheck: true,
        types: [],
    };
    // Runtime conditions can change even when TypeScript follows the same types entry.
    let moduleType;
    for (let directory = path.dirname(modulePath); directory !== path.dirname(directory); directory = path.dirname(directory)) {
        const manifest = path.join(directory, 'package.json');
        inputs.add(manifest);
        if (!fs.existsSync(manifest))
            continue;
        const metadata = JSON.parse(readInput(manifest));
        if (moduleType === undefined)
            moduleType = metadata.type ?? null;
        if (metadata.name)
            break;
    }
    validateRuntime?.(ts, modulePath, options, moduleType, readInput);
    const host = ts.createCompilerHost(options);
    const realpaths = new Map();
    const realpath = host.realpath.bind(host);
    // A package link can move while the original target and the files probed
    // through the link still have the same contents.
    host.realpath = file => {
        const resolved = realpath(file);
        realpaths.set(path.resolve(file), path.resolve(resolved));
        return resolved;
    };
    host.readFile = readInput;
    const getSourceFile = host.getSourceFile.bind(host);
    // TypeScript passes normalized slashes even when the host constructs a Windows path.
    const entryKey = host.getCanonicalFileName(path.normalize(entry));
    const isEntry = file => host.getCanonicalFileName(path.normalize(file)) === entryKey;
    host.getSourceFile = (file, ...args) => isEntry(file) ? source : getSourceFile(file, ...args);
    const fileExists = host.fileExists.bind(host);
    host.fileExists = file => {
        if (isEntry(file))
            return true;
        inputs.add(path.resolve(file));
        return fileExists(file);
    };
    const directoryExists = host.directoryExists.bind(host);
    host.directoryExists = directory => {
        inputs.add(path.resolve(directory));
        return directoryExists(directory);
    };
    // Resolving optional peer imports also contributes inputs to a portable signature.
    let source = ts.createSourceFile(entry, sourceText(ts, entry, options, host), options.target, true);
    const program = ts.createProgram([entry], options, host);
    const diagnostics = ts.getPreEmitDiagnostics(program);
    if (diagnostics.length)
        throw new Error(ts.formatDiagnostics(diagnostics, host));

    const definitions = new WeakMap();
    const definition = node => {
        if (!node || isEntry(node.getSourceFile().fileName))
            return null;
        if (definitions.has(node))
            return definitions.get(node);
        const target = node.name ?? node;
        const result = {
            path: path.resolve(target.getSourceFile().fileName),
            start: target.getStart(),
            length: target.getWidth(),
            documentation: declarationDocumentation(ts, node),
        };
        // A callable variable or property can document every use of its call
        // type. A function declaration's JSDoc belongs to that overload alone.
        if (result.documentation && (ts.isVariableDeclaration(node) || ts.isPropertySignature(node) || ts.isPropertyDeclaration(node)))
            result.documentationFromCallableOwner = true;
        definitions.set(node, result);
        return result;
    };
    function packageIdentity() {
        const resolution = ts.resolveModuleName(moduleSpecifier, entry, options, host).resolvedModule;
        if (!resolution)
            throw new Error(`Cannot resolve TypeScript declarations for '${moduleSpecifier}'.`);
        const packageId = resolution.packageId;
        if (!packageId)
            throw new Error(`Cannot resolve npm package metadata for '${moduleSpecifier}'.`);
        return {
            moduleSpecifier, packageName: packageId.name, version: packageId.version,
            modulePath, declarationPath: path.resolve(resolution.resolvedFileName),
        };
    }

    function snapshotInputs() {
        return {
            inputs: [...inputs], modulePath, realpaths: Object.fromEntries(realpaths),
            observedInputs: Object.fromEntries([...reads].map(([file, input]) => [file, input?.hash ?? null])),
        };
    }
    return { ts, modulePath, entry, options, host, snapshotInputs, program,
        source, definition, replaceSource: value => { source = value; }, packageIdentity };
}
