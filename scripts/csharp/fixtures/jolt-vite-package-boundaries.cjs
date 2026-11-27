const assert = require('node:assert/strict');
const fs = require('node:fs/promises');
const path = require('node:path');
const { EventEmitter } = require('node:events');

exports.run = async (root, candidate) => {
    const project = await fs.mkdtemp(path.join(root, 'fixture-'));
    const modules = path.join(project, '.jazor');
    const directory = path.join(modules, 'one', 'two', 'three', 'four', 'five');
    const importer = path.join(directory, 'entry.mjs');
    const boundary = path.join(modules, 'package.json');
    const nearer = path.join(modules, 'one', 'package.json');
    const originalRead = fs.readFile;
    let reads = 0;
    fs.readFile = (file, ...args) => {
        if (path.basename(file) === 'package.json') reads++;
        return originalRead(file, ...args);
    };
    try {
        await fs.mkdir(directory, { recursive: true });
        const manifest = name => JSON.stringify({ name: '@jazor/generated', jazor: { packages: {
            [name]: { source: 'npm', specifier: name, canonicalName: name },
        } } });
        const watcher = new EventEmitter();
        watcher.add = () => {};
        const server = { watcher, config: { root: project, base: '/' }, middlewares: { use() {} } };
        const context = { resolve(source, from) { return { id: from, source }; } };
        await fs.writeFile(boundary, manifest('first'));
        const [plugin] = await require(candidate).plugins(project, true, modules);
        plugin.configureServer(server);
        const resolve = name => plugin.resolveId.call(context, name, importer, {});
        const applicationManifest = path.join(project, 'package.json');
        assert.equal((await resolve('first')).id, applicationManifest);
        assert.equal(reads, 6);
        assert.equal((await resolve('first')).id, applicationManifest);
        assert.equal(reads, 6, 'A repeated resolution should reuse the nearest package boundary');

        await fs.writeFile(boundary + '.new', manifest('replacement'));
        await fs.rename(boundary + '.new', boundary);
        watcher.emit('all', 'change', boundary);
        assert.equal(await resolve('first'), undefined);
        assert.equal((await resolve('replacement')).id, applicationManifest);

        await fs.writeFile(nearer, manifest('nearer'));
        watcher.emit('all', 'add', nearer);
        assert.equal(await resolve('replacement'), undefined);
        assert.equal((await resolve('nearer')).id, applicationManifest);
        await fs.unlink(nearer);
        watcher.emit('all', 'unlink', nearer);
        assert.equal((await resolve('replacement')).id, applicationManifest);

        await fs.unlink(boundary);
        watcher.emit('all', 'unlink', boundary);
        assert.equal(await resolve('replacement'), undefined);
        reads = 0;
        assert.equal(await resolve('replacement'), undefined);
        assert.equal(reads, 0, 'A repeated resolution should reuse the missing boundary');
        await fs.writeFile(boundary, manifest('restored'));
        watcher.emit('all', 'add', boundary);
        assert.equal((await resolve('restored')).id, applicationManifest);
        return 'Vite package boundaries reuse positive and negative lookups and follow manifest changes.';
    } finally {
        fs.readFile = originalRead;
        await fs.rm(project, { recursive: true, force: true });
    }
};
