import assert from 'node:assert/strict';
import { JSDOM } from 'jsdom';

const { window } = new JSDOM('<button></button><aside></aside><main></main>');
try {
    for (const name of ['window', 'document', 'Element', 'SVGElement'])
        globalThis[name] = window[name];
    const { createApp, nextTick } = await import('vue');
    const { default: Behavior } = await import('./.jolt/Behavior.mjs');
    const target = document.querySelector('button');
    const outside = document.querySelector('aside');
    const container = document.querySelector('main');
    const app = createApp(Behavior, { Target: target });
    app.mount(container);
    const click = element => element.dispatchEvent(new window.MouseEvent('click', { bubbles: true, clientX: 42 }));
    click(target);
    const count = () => container.querySelector('output').textContent;
    for (let i = 0; i < 100 && count() !== '28'; i++)
        await new Promise(resolve => setTimeout(resolve, 10));
    assert.equal(count(), '28');
    click(outside);
    await nextTick();
    assert.equal(count(), '36');
    app.unmount();
    assert.equal(container.querySelector('output'), null);
    console.log('VueUse debounce cancellation and flush, throttle results, timeout arguments and outside clicks ran with Vue and JSDOM.');
} finally {
    window.close();
}
