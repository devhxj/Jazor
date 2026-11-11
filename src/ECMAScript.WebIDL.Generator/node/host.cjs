const fs = require('node:fs/promises');
const path = require('node:path');
const { pathToFileURL } = require('node:url');
const { createRequire } = require('node:module');

exports.restore = async (bootstrapRoot, projectPath, storeDir) => {
    const pnpm = createRequire(path.join(bootstrapRoot, 'package.json'))('@pnpm/napi');
    const manifest = JSON.parse(await fs.readFile(projectPath, 'utf8'));
    const dir = path.dirname(projectPath);
    await fs.mkdir(storeDir, { recursive: true });
    const result = await pnpm.install({
        dir,
        projects: [{ rootDir: dir, manifest }],
        storeDir,
        cacheDir: path.join(storeDir, 'cache'),
        nodeVersion: process.versions.node,
        // The Rust engine's Windows junction targets are not resolved correctly
        // by this libnode 20 build. Use pnpm's supported flat layout for the pilot.
        nodeLinker: 'hoisted',
        frozenLockfile: await fs.access(path.join(dir, 'pnpm-lock.yaml')).then(() => true, () => false),
        ignoreScripts: true,
        registries: { default: 'https://registry.npmjs.org/' },
        reporter: { appendOnly: true },
    });
    return JSON.stringify({ pid: process.pid, node: process.version, pnpm: pnpm.engineVersion(), ...result });
};

exports.loadCollector = async (projectPath, workerPath) => {
    const ts = createRequire(projectPath)('typescript');
    const source = await fs.readFile(workerPath, 'utf8');
    const compiled = ts.transpileModule(source, {
        fileName: workerPath,
        compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
    });
    // Keep ESM package resolution under the project's node_modules, without
    // launching tsc/Node or executing authored code during transpilation.
    const compiledDir = path.join(path.dirname(projectPath), 'obj', 'collector');
    await fs.mkdir(compiledDir, { recursive: true });
    const modulePath = path.join(compiledDir, path.basename(workerPath, '.ts') + '.mjs');
    await fs.writeFile(modulePath, compiled.outputText);
    return import(pathToFileURL(modulePath).href);
};

exports.collect = async (projectPath, workerPath, skipProse) => {
    const collector = await exports.loadCollector(projectPath, workerPath);
    // The emitter also uses raw JSON defaults in XML docs. Preserve the old
    // inventory's indentation so migrating its transport does not rewrite docs.
    return JSON.stringify(await collector.collectInventory(skipProse), null, 2);
};

exports.test = async (projectPath, workerPath, bootstrapRoot) => {
    await exports.loadCollector(projectPath, workerPath);
    const tests = await exports.loadCollector(projectPath, path.join(path.dirname(workerPath), 'collect.test.ts'));
    for (const test of Object.values(tests)) await test();

    const testRoot = await fs.mkdtemp(path.join(path.dirname(projectPath), 'obj', 'frozen-test-'));
    try {
        const lock = await fs.readFile(path.join(path.dirname(projectPath), 'pnpm-lock.yaml'));
        await fs.writeFile(path.join(testRoot, 'pnpm-lock.yaml'), lock);
        const manifest = JSON.parse(await fs.readFile(projectPath, 'utf8'));
        manifest.dependencies.webidl2 = '0.0.0-webidl-frozen-mismatch';
        const pnpm = createRequire(path.join(bootstrapRoot, 'package.json'))('@pnpm/napi');
        try {
            await pnpm.install({
                dir: testRoot,
                projects: [{ rootDir: testRoot, manifest }],
                storeDir: path.join(testRoot, 'store'),
                nodeVersion: process.versions.node,
                nodeLinker: 'hoisted',
                frozenLockfile: true,
                ignoreScripts: true,
            });
            throw new Error('Frozen restoration unexpectedly accepted an outdated lockfile.');
        } catch (error) {
            if (error.code !== 'ERR_PNPM_OUTDATED_LOCKFILE') throw error;
        }
        if (!lock.equals(await fs.readFile(path.join(testRoot, 'pnpm-lock.yaml')))) {
            throw new Error('Failed frozen restoration changed the lockfile.');
        }
    } finally {
        await fs.rm(testRoot, { recursive: true, force: true });
    }
    return JSON.stringify({ tests: Object.keys(tests).length + 1, pid: process.pid });
};
