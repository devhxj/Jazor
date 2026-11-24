import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

export async function packageChecker(entryUrl, typescript, moduleSpecifier, sourceText, validateRuntime) {
    const { default: ts } = await import(pathToFileURL(typescript));
    const modulePath = fileURLToPath(import.meta.resolve(moduleSpecifier));
    const root = path.resolve(path.dirname(fileURLToPath(entryUrl)), '../..');
    const entry = path.join(root, 'jazor-metadata.mts');
    const inputs = new Set([modulePath]);
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
        const metadata = JSON.parse(fs.readFileSync(manifest, 'utf8'));
        if (moduleType === undefined)
            moduleType = metadata.type ?? null;
        if (metadata.name)
            break;
    }
    validateRuntime?.(ts, modulePath, options, moduleType);
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
    let source = ts.createSourceFile(entry, sourceText(ts, entry, options, host), options.target, true);
    const readFile = host.readFile.bind(host);
    host.readFile = file => {
        inputs.add(path.resolve(file));
        const result = readFile(file);
        return result;
    };
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
    const program = ts.createProgram([entry], options, host);
    const diagnostics = ts.getPreEmitDiagnostics(program);
    if (diagnostics.length)
        throw new Error(ts.formatDiagnostics(diagnostics, host));

    const definition = node => {
        if (!node || isEntry(node.getSourceFile().fileName))
            return null;
        const target = node.name ?? node;
        return {
            path: path.resolve(target.getSourceFile().fileName),
            start: target.getStart(),
            length: target.getWidth(),
        };
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

    return { ts, modulePath, entry, options, host, realpaths, inputs, program,
        source, definition, replaceSource: value => { source = value; }, packageIdentity };
}
