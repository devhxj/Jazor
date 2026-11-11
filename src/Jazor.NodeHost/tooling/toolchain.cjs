const fs = require('node:fs/promises');
const path = require('node:path');
const crypto = require('node:crypto');
const { createRequire } = require('node:module');
const { pathToFileURL } = require('node:url');
const { inspect } = require('node:util');

const exists = (file) => fs.access(file).then(() => true, () => false);
const hash = (value) => crypto.createHash('sha256').update(value).digest('hex');

exports.render = async (requestJson) => {
    const { projectDirectory, modulePath, state } = JSON.parse(requestJson);
    const module = await import(pathToFileURL(path.join(projectDirectory, 'ssr-entry.js')).href);
    return module.render({ modulePath, state });
};

exports.executeModule = async (requestJson) => {
    const { projectDirectory, modulePath, arguments: args, imports } = JSON.parse(requestJson);
    if (imports) {
        // Only test fixtures need an import map for their local host doubles.
        // Register an environment-owned ESM hook; production modules retain
        // standard package resolution and are never rewritten by this runner.
        const loader = `
            let imports, baseURL;
            export function initialize(data) { ({ imports, baseURL } = data); }
            export async function resolve(specifier, context, nextResolve) {
                const key = Object.keys(imports).sort((a, b) => b.length - a.length)
                    .find(key => key === specifier || key.endsWith('/') && specifier.startsWith(key));
                if (!key) return nextResolve(specifier, context);
                const target = imports[key] + specifier.slice(key.length);
                return nextResolve(target, { ...context, parentURL: baseURL });
            }
        `;
        const { register } = require('node:module');
        register('data:text/javascript,' + encodeURIComponent(loader), {
            data: { imports, baseURL: pathToFileURL(path.join(projectDirectory, 'package.json')).href }
        });
    }
    const tests = [];
    const output = [];
    const { format } = require('node:util');
    console.log = (...values) => output.push(format(...values));
    globalThis.__jazorArguments = args;
    // Runtime fixtures register their existing assertions here. This is a
    // test-owned function, not a Deno compatibility object or a production API.
    globalThis.__jazorTest = (name, body) => tests.push({ name, body });
    await import(pathToFileURL(modulePath).href);
    for (const test of tests) {
        try { await test.body(); }
        catch (error) { throw new Error(test.name + ': ' + (error.stack ?? error)); }
    }
    return JSON.stringify({ tests: tests.length, pid: process.pid, output: output.join('\n') });
};

exports.restore = async (requestJson) => {
    const request = JSON.parse(requestJson);
    const root = request.projectDirectory;
    const pnpm = createRequire(path.join(request.bootstrap, 'package.json'))('@pnpm/napi');
    const manifestText = await fs.readFile(path.join(root, 'package.json'), 'utf8');
    const manifest = JSON.parse(manifestText);
    const lockPath = path.join(root, 'pnpm-lock.yaml');
    const statePath = path.join(root, 'node_modules', '.jazor-restore-state');
    const graph = JSON.stringify({ dependencies: manifest.dependencies, devDependencies: manifest.devDependencies, optionalDependencies: manifest.optionalDependencies });
    const identity = hash(JSON.stringify({ graph, engine: request.engineVersion, node: process.version, platform: process.platform, arch: process.arch, linker: 'hoisted' }));
    const lock = await exists(lockPath) ? await fs.readFile(lockPath, 'utf8') : '';
    const expected = identity + '\n' + hash(lock);
    const local = Object.values({ ...manifest.dependencies, ...manifest.devDependencies }).some(value => value.startsWith('file:'));
    if (!local && await exists(statePath) && await fs.readFile(statePath, 'utf8') === expected) {
        return JSON.stringify({ operation: 'restore', status: 'skipped', pid: process.pid });
    }
    const frozen = !!lock && await rootMatchesLock(pnpm, root, manifest);
    await fs.rm(statePath, { force: true });
    const config = pnpm.readConfig({ dir: root });
    await fs.mkdir(request.storeDirectory, { recursive: true });
    const result = await pnpm.install({
        dir: root,
        projects: [{ rootDir: root, manifest }],
        registries: Object.fromEntries(config.registries.map(registry => [registry.name, registry.url])),
        authHeaderByUri: config.authHeaderByUri,
        proxyConfig: { httpProxy: config.httpProxy, httpsProxy: config.httpsProxy, noProxy: config.noProxy },
        networkConfig: { ca: config.ca, cert: config.cert, key: config.key, strictSsl: config.strictSsl },
        storeDir: request.storeDirectory,
        cacheDir: path.join(request.storeDirectory, 'cache'),
        nodeLinker: 'hoisted',
        nodeVersion: process.versions.node,
        autoInstallPeers: true,
        frozenLockfile: frozen,
        ignoreScripts: true,
        reporter: { appendOnly: true },
    });
    if (!local) await fs.writeFile(statePath, identity + '\n' + hash(await fs.readFile(lockPath, 'utf8')));
    return JSON.stringify({ operation: 'restore', status: 'completed', pid: process.pid, frozen, ...result });
};

async function rootMatchesLock(pnpm, root, manifest) {
    // The engine owns the format. Inspect its parsed root importer rather than
    // parsing YAML or trying a failed frozen install followed by a retry.
    const lock = await pnpm.readLockfile({ dir: root });
    const importer = lock?.importers?.['.'];
    if (!importer) return false;
    for (const group of ['dependencies', 'devDependencies', 'optionalDependencies']) {
        const expected = manifest[group] ?? {};
        const actual = importer[group] ?? {};
        if (Object.keys(expected).length !== Object.keys(actual).length) return false;
        for (const [name, specifier] of Object.entries(expected)) if (actual[name]?.specifier !== specifier) return false;
    }
    return true;
}

async function loadVite(projectDirectory) {
        const requireFromProject = createRequire(path.join(projectDirectory, 'package.json'));
        if (process.platform === 'win32' && process.arch === 'x64' && process.config.variables.node_target_type === 'shared_library') {
            // napi-rs mistakes Microsoft's MSVC libnode shared_library for MinGW.
            // Use its explicit native-binding contract only while loading rolldown.
            // Replacing this environment's env object avoids writing the OS-wide
            // environment through Node's special process.env setter properties.
            const inheritedEnvironment = process.env;
            process.env = { ...inheritedEnvironment, NAPI_RS_NATIVE_LIBRARY_PATH: requireFromProject.resolve('@rolldown/binding-win32-x64-msvc') };
            try {
                await import(pathToFileURL(requireFromProject.resolve('rolldown')).href);
            } finally {
                process.env = inheritedEnvironment;
            }
        }
        return import(pathToFileURL(requireFromProject.resolve('vite')).href);
}

exports.build = async (requestJson) => {
    const { projectDirectory, ssr } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    await vite.build({ root: projectDirectory, configFile: path.join(projectDirectory, ssr ? 'ssr.vite.config.js' : 'vite.config.js') });
    return JSON.stringify({ operation: 'build', ssr, status: 'completed', pid: process.pid });
};

exports.check = async (requestJson) => {
    const { projectDirectory, entryPaths } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    const server = await vite.createServer({
        root: projectDirectory,
        appType: 'custom',
        server: { middlewareMode: true, watch: null, hmr: false },
        optimizeDeps: { noDiscovery: true, include: [] }
    });
    try {
        const visited = new Set();
        async function visit(url) {
            if (visited.has(url)) return;
            visited.add(url);
            await server.transformRequest(url);
            const module = await server.moduleGraph.getModuleByUrl(url);
            for (const dependency of module?.importedModules ?? []) {
                // Transform authored local modules without evaluating their
                // initializers. Package exports resolve through Vite; package
                // implementations are checked by the eventual build.
                if (dependency.file && !dependency.file.includes('/node_modules/'))
                    await visit(dependency.url);
            }
        }
        for (const entry of entryPaths)
            await visit('/' + path.relative(projectDirectory, entry).split(path.sep).join('/'));
        return JSON.stringify({ operation: 'check', modules: visited.size, pid: process.pid });
    } finally {
        await server.close();
    }
};

let ownedViteServer;
exports.startVite = async (requestJson) => {
    const { projectDirectory, host, port, base } = JSON.parse(requestJson);
    const vite = await loadVite(projectDirectory);
    ownedViteServer = await vite.createServer({ root: projectDirectory, base, server: { host, port, strictPort: true, open: false } });
    try {
        await ownedViteServer.listen();
        return JSON.stringify({ operation: 'vite', status: 'listening', pid: process.pid, origin: ownedViteServer.resolvedUrls.local[0] });
    } catch (error) {
        await ownedViteServer.close();
        ownedViteServer = undefined;
        throw error;
    }
};

exports.closeVite = async () => {
    if (ownedViteServer) await ownedViteServer.close();
    ownedViteServer = undefined;
    return JSON.stringify({ operation: 'vite', status: 'closed' });
};

exports.verifyVite = async (projectDirectory) => {
    try {
        const vite = await loadVite(projectDirectory);
        const server = await vite.createServer({ root: projectDirectory, configFile: false, server: { port: 0, host: '127.0.0.1' } });
        try {
            await server.listen();
            const response = await fetch(server.resolvedUrls.local[0] + '@vite/client');
            if (!response.ok) throw new Error('Vite client failed: ' + response.status);
        } finally {
            await server.close();
        }
        await vite.build({ root: projectDirectory, configFile: false, build: { outDir: path.join(projectDirectory, 'dist'), rolldownOptions: { input: path.join(projectDirectory, 'entry.js') } } });
        return JSON.stringify({ operation: 'vite', status: 'passed', pid: process.pid, node: process.version });
    } catch (error) {
        throw new Error(inspect(error, { depth: 8 }));
    }
};
