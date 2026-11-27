import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<main></main>', { url: 'https://jolt.example/' });
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement', 'Event', 'StorageEvent', 'CustomEvent', 'Storage'])
        globalThis[name] = window[name];
    const { createApp, nextTick } = await import('vue');
    const { default: Storage } = await import('./.jazor/Storage.mjs');
    const mount = window.document.querySelector('main');
    const key = 'jolt-storage-options';
    const app = createApp(Storage);
    app.mount(mount);
    await nextTick();
    assert.equal(mount.querySelector('output').textContent, '1');
    assert.equal(window.localStorage.getItem(key), null);
    const notifications = [];
    window.addEventListener('storage', event => {
        if (event.key === key)
            notifications.push(event);
    });
    const [save, clear] = mount.querySelectorAll('button');
    save.click();
    await nextTick();
    assert.equal(mount.querySelector('output').textContent, '0');
    assert.equal(window.localStorage.getItem(key), '0');
    assert.equal(notifications.length, 1);
    assert.equal(notifications[0].newValue, '0');

    clear.click();
    await nextTick();
    assert.equal(mount.querySelector('#clear-result').textContent, 'null');
    assert.equal(mount.querySelector('output').textContent, '1');
    assert.equal(window.localStorage.getItem(key), null);
    assert.equal(notifications.length, 2);
    assert.equal(notifications[1].oldValue, '0');
    assert.equal(notifications[1].newValue, null);
    app.unmount();

    const reset = createApp(Storage);
    reset.mount(mount);
    await nextTick();
    assert.equal(mount.querySelector('output').textContent, '1');
    assert.equal(window.localStorage.getItem(key), null);
    mount.querySelector('button').click();
    await nextTick();
    assert.equal(window.localStorage.getItem(key), '0');
    reset.unmount();

    const remounted = createApp(Storage);
    remounted.mount(mount);
    await nextTick();
    assert.equal(mount.querySelector('output').textContent, '0');
    remounted.unmount();
    assert.equal(window.localStorage.getItem(key), '0');
    console.log('VueUse skipped writing defaults, removed the stored value on a null write, and restored a later zero after remount.');
} finally {
    window.close();
}
